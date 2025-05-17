using OpenTK.Mathematics;

namespace gamev3
{
    public class Cube : Volume
    {
        public Cube()
        {
            VertCount = 24;
            IndCount = 36;
            ColorDataCount = 8;
            Name = "cube";
        }

        public override Vector3[] GetNormals()
        {
            return new Vector3[] {
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, 1f),

                new Vector3(0f, 0f, -1f),
                new Vector3(0f, 0f, -1f),
                new Vector3(0f, 0f, -1f),
                new Vector3(0f, 0f, -1f),

                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 0f),
                new Vector3(0f, 1f, 0f),

                new Vector3(0f, -1f, 0f),
                new Vector3(0f, -1f, 0f),
                new Vector3(0f, -1f, 0f),
                new Vector3(0f, -1f, 0f),

                new Vector3(1f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(1f, 0f, 0f),

                new Vector3(-1f, 0f, 0f),
                new Vector3(-1f, 0f, 0f),
                new Vector3(-1f, 0f, 0f),
                new Vector3(-1f, 0f, 0f),
            };
        }

        public override Vector3[] GetVerts()
        {
            return new Vector3[] {
                new Vector3(-0.5f, -0.5f, 0.5f), 
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),

                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f), 
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f), 

                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f), 

                new Vector3(-0.5f, -0.5f, 0.5f), 
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f), 
                new Vector3(-0.5f, -0.5f, -0.5f),

                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f), 
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),

                new Vector3(-0.5f, -0.5f, 0.5f), 
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f), 
                new Vector3(-0.5f, 0.5f, 0.5f)
            };
        }

        public override int[] GetIndices()
        {
            int[] inds = new int[] {
                // Front
                0, 1, 2, 0, 2, 3,
                // Back
                4, 5, 6, 4, 6, 7,
                // Top
                8, 9, 10, 8, 10, 11,
                // Bottom
                12, 13, 14, 12, 14, 15,
                // Right
                16, 17, 18, 16, 18, 19,
                // Left
                20, 21, 22, 20, 22, 23
            };

            return inds;
        }

        public override Vector2[] GetTextureCoords()
        {
            return new Vector2[] {
                // Front face (0-3)
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),

                // Back face (4-7)
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),

                // Top face (8-11)
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),

                // Bottom face (12-15)
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),

                // Right face (16-19)
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),

                // Left face (20-23)
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
        }

        // костыль (потому что нормали ужО посчитаны)
        public override void CalculateNormals() {}
    }
}
