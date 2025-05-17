using OpenTK.Mathematics;
using System.Globalization;

namespace gamev3
{
    public class Mesh : Volume
    {
        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> texcoords = new List<Vector2>();
        private List<Vector3> normals = new List<Vector3>();
        private List<int> indices = new List<int>();
        private List<List<Tuple<int, int, int>>> faces = new List<List<Tuple<int, int, int>>>();

        public Mesh(string name) { Name = name; }
        public override void CalculateNormals()
        {
            var computedNormals = new List<Vector3>(new Vector3[vertices.Count]);

            foreach (var face in faces)
            {
                if (face.Count < 3) continue;

                var v1 = vertices[face[0].Item1];
                var v2 = vertices[face[1].Item1];
                var v3 = vertices[face[2].Item1];

                Vector3 edge1 = v2 - v1;
                Vector3 edge2 = v3 - v1;
                Vector3 normal = Vector3.Cross(edge1, edge2).Normalized();

                foreach (var vertex in face)
                {
                    int vInd = vertex.Item1;
                    computedNormals[vInd] += normal;
                }
            }

            for (int i = 0; i < computedNormals.Count(); i++)
                computedNormals[i] = computedNormals[i].Normalized();

            normals = computedNormals;
        }

        public override Vector2[] GetTextureCoords() { return texcoords.ToArray(); }
        public override Vector3[] GetVerts() { return vertices.ToArray(); }
        public override Vector3[] GetNormals() { return normals.ToArray(); }
        public override int[] GetIndices() { return indices.ToArray(); }

        public void Load(string path)
        {
            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "v":
                        vertices.Add(new Vector3(
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture)
                            ));
                        break;
                    case "vt":
                        texcoords.Add(new Vector2(
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture)
                            ));
                        break;
                    case "vn":
                        normals.Add(new Vector3(
                            float.Parse(parts[1], CultureInfo.InvariantCulture),
                            float.Parse(parts[2], CultureInfo.InvariantCulture),
                            float.Parse(parts[3], CultureInfo.InvariantCulture)
                            ));
                        break;
                    case "f":
                        var face = new List<Tuple<int, int, int>>();
                        for (int i = 1; i < parts.Length; i++)
                        {
                            var indices = parts[i].Split('/');

                            int v = (indices.Length >= 1 && indices[0] != "") ? int.Parse(indices[0]) - 1 : -1;
                            int vt = (indices.Length >= 2 && indices[1] != "") ? int.Parse(indices[1]) - 1 : -1;
                            int vn = (indices.Length >= 3 && indices[2] != "") ? int.Parse(indices[2]) - 1 : -1;

                            face.Add(Tuple.Create(v, vt, vn));
                        }
                        faces.Add(face);
                        break;
                }
            }

            if (normals.Count == 0)
            {
                CalculateNormals();

                foreach (var face in faces)
                {
                    for (int i = 0; i < face.Count; i++)
                    {
                        var vertex = face[i];
                        face[i] = Tuple.Create(vertex.Item1, vertex.Item2, vertex.Item1);
                    }
                }
            }

            var vertexMap = new Dictionary<Tuple<int, int, int>, int>();
            var combinedVertices = new List<Vector3>();
            var combinedTexcoords = new List<Vector2>();
            var combinedNormals = new List<Vector3>();

            // При экспорте моделей из Blender'а число нормалей или текстурный координат может оказаться меньше, чем число вершин
            // Если этот момент не обработать, то модель нормально не загрузится
            // Этот код обрабатывает недостающие данные
            foreach (var face in faces)
            {
                foreach (var vertex in face)
                {
                    int v = vertex.Item1;
                    int vt = vertex.Item2;
                    int vn = vertex.Item3;

                    var key = Tuple.Create(v, vt, vn);
                    if (!vertexMap.TryGetValue(key, out int index))
                    {
                        Vector3 pos = (v != -1) ? vertices[v] : Vector3.Zero;
                        Vector2 tex = (vt != -1 && vt < texcoords.Count) ? texcoords[vt] : Vector2.Zero;
                        Vector3 norm = (vn != -1 && vn < normals.Count) ? normals[vn] : Vector3.Zero;

                        combinedVertices.Add(pos);
                        combinedTexcoords.Add(tex);
                        combinedNormals.Add(norm);

                        index = combinedVertices.Count - 1;
                        vertexMap[key] = index;
                    }

                    indices.Add(index);
                }
            }

            vertices = combinedVertices;
            texcoords = combinedTexcoords;
            normals = combinedNormals;

            VertCount = vertices.Count;
            IndCount = indices.Count;
        }
    }
}
