using OpenTK.Mathematics;

namespace gamev3
{
    // Все типы шейдеров, с помощью которых можно рисовать объекты (кроме теневого!)
    public enum UsedShader
    {
        SingleColorShader,
        LightShader,
        DiffuseShader,
    }

    public class PointLight
    {
        public Vector3 position;

        public float strength;

        public float constant;
        public float linear;
        public float quadratic;

        public Vector3 ambient;
        public Vector3 diffuse;
        public Vector3 specular;

        public PointLight(Vector3 position,
            float strength,
            float constant,
            float linear,
            float quadratic,
            Vector3 ambient,
            Vector3 diffuse,
            Vector3 specular)
        {
            this.position = position;
            this.strength = strength;
            this.constant = constant;
            this.linear = linear;
            this.quadratic = quadratic;
            this.ambient = ambient;
            this.diffuse = diffuse;
            this.specular = specular;
        }
    }

    public class Object
    {
        public string Model = "";

        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public Vector3 Scale = Vector3.One;

        public Vector3 FlatColor = new Vector3(1f, 1f, 1f);

        public Matrix4 ModelMatrix = Matrix4.Identity;
        public Matrix4 ViewMatrix = Matrix4.Identity;
        public Matrix4 ProjectionMatrix = Matrix4.Identity;

        public void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) *
                Matrix4.CreateTranslation(Position) *
                Matrix4.CreateRotationX(Rotation.X) *
                Matrix4.CreateRotationY(Rotation.Y) *
                Matrix4.CreateRotationZ(Rotation.Z);
        }

        public Object(string model,
            Vector3 pos,
            Vector3 rot,
            Vector3 scale)
        {
            this.Model = model;
            this.Position = pos;
            this.Rotation = rot;
            this.Scale = scale;
        }
    }
}
