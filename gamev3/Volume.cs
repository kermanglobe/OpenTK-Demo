using OpenTK.Mathematics;

namespace gamev3
{
    public abstract class Volume
    {
        public int VAO;
        public int VBO;
        public int EBO;

        public string Name = "";

        public int VertCount;
        public int IndCount;
        public int ColorDataCount;

        public Dictionary<string, int> Textures = new Dictionary<string, int>();

        public abstract void CalculateNormals();

        public abstract Vector3[] GetVerts();
        public abstract Vector3[] GetNormals();
        public abstract Vector2[] GetTextureCoords();
        public abstract int[] GetIndices();
    }
}
