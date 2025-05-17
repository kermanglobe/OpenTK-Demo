using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace gamev3
{
    public class Shader
    {
        public readonly int Handle;
        private readonly Dictionary<string, int> uniformLocations;

        public Shader(string vShader, string fShader, string gShader = "", bool fromFile = false)
        {
            Handle = GL.CreateProgram();
            uniformLocations = new Dictionary<string, int>();

            if (fromFile)
            {
                readFromFile(vShader, fShader, gShader);
            }

            else
            {
                readFromString(vShader, fShader, gShader);
            }
        }

        private void readFromString(string vShader, string fShader, string gShader = "")
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vShader);
            CompileShader(vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fShader);
            CompileShader(fragmentShader);

            if (gShader != "")
            {
                var geometryShader = GL.CreateShader(ShaderType.GeometryShader);
                GL.ShaderSource(geometryShader, gShader);
                CompileShader(geometryShader);
                GL.AttachShader(Handle, geometryShader);
            }

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // кэширование переменных uniform
            for (int i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                var location = GL.GetUniformLocation(Handle, key);
                uniformLocations.Add(key, location);
            }
        }

        private void readFromFile(string vShader, string fShader, string gShader = "")
        {
            var shaderSource = File.ReadAllText(vShader);
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSource);
            CompileShader(vertexShader);

            shaderSource = File.ReadAllText(fShader);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            if (gShader != "")
            {
                shaderSource = File.ReadAllText(gShader);
                var geometryShader = GL.CreateShader(ShaderType.GeometryShader);
                GL.ShaderSource(geometryShader, shaderSource);
                CompileShader(geometryShader);
                GL.AttachShader(Handle, geometryShader);
            }

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            // caching uniforms
            for (int i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);
                var location = GL.GetUniformLocation(Handle, key);
                uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);

            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occured when compiling shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);

            if (code != (int)All.True)
            {
                var infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Error occured when linking program({program}).\n\n{infoLog}");
            }
        }

        public void Use() { GL.UseProgram(Handle); }

        public int GetAttribLocation(string attribName) { return GL.GetAttribLocation(Handle, attribName); }

        // установка Uniform

        public void SetUniformInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniformLocations[name], data);
        }

        public void SetUniformFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniformLocations[name], data);
        }

        public void SetUniformVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(uniformLocations[name], data);
        }

        public void SetUniformMatrix3(string name, Matrix3 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix3(uniformLocations[name], true, ref data);
        }

        public void SetUniformMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(uniformLocations[name], true, ref data);
        }
    }
}
