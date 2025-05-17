using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gamev3
{
    public class Game : GameWindow
    {
        private int width, height;

        // для теней
        private float near = 1f;
        private float far = 25f;

        private Shader shader;
        private Shader single_color;
        private Shader shadow_shader;

        private Dictionary<string, Volume> models = new Dictionary<string, Volume>();
        private Dictionary<UsedShader, List<Object>> objects = new Dictionary<UsedShader, List<Object>>();
        private List<PointLight> lights = new List<PointLight>();

        private Dictionary<string, float[]> vertices = new Dictionary<string, float[]>();
        private Dictionary<string, int[]> indices = new Dictionary<string, int[]>();
        private Dictionary<int, Texture> textures = new Dictionary<int, Texture>();

        private Camera cam;

        private double time;
        private float camSpeed = 1.5f;
        private float sensitivity = 0.2f;
        private Vector2 lastMousePos;
        private bool mouseMoved = false;

        private Vector3 lightPos = Vector3.UnitX;

        private int shadow_width = 384;
        private int shadow_height = 384;
        private int light_sources_count = 8;

        private int[] shadowFBO;
        private int[] depthCubeMaps;

        public Game(int width, int height, string title) : base(
            GameWindowSettings.Default,
            new NativeWindowSettings() { ClientSize = (width, height), Title = title }
            )
        {
            this.CenterWindow(new Vector2i(width, height));
            this.height = height;
            this.width = width;
            this.time = 0.0;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.Enable(EnableCap.DepthTest);

            cam = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            // Загрузка моделей

            models.Add("cube", new Cube());

            Cube cube_textured = new Cube();
            cube_textured.Name = "cube_textured";
            models.Add(cube_textured.Name, cube_textured);

            Mesh monkey = new Mesh("monkey");
            monkey.Load("Models/monkey.obj");
            models.Add(monkey.Name, monkey);

            foreach (var (name, v) in models)
            {
                List<float> verts = new List<float>();

                var vVertCount = v.VertCount;
                var vverts = v.GetVerts();
                var vnorms = v.GetNormals();
                var vtext = v.GetTextureCoords();
                for (int i = 0; i < vVertCount; i++)
                    verts.AddRange(
                        vverts[i].X, vverts[i].Y, vverts[i].Z,
                        vnorms[i].X, vnorms[i].Y, vnorms[i].Z,
                        vtext[i].X, vtext[i].Y
                        );

                vertices[name] = verts.ToArray();
                indices[name] = models[name].GetIndices();

                models[name].VBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, models[name].VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices[name].Length * sizeof(float), vertices[name], BufferUsageHint.StaticDraw);

                models[name].EBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, models[name].EBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices[name].Length * sizeof(int), indices[name], BufferUsageHint.StaticDraw);

                models[name].VAO = GL.GenVertexArray();
                GL.BindVertexArray(models[name].VAO);

                //вершины
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                //нормали
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(1);
                //координаты текстур
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
                GL.EnableVertexAttribArray(2);

                GL.BindVertexArray(0);
            }

            // Загрузка текстур

            Texture diffuseMap = Texture.LoadFromFile("Textures/container2.png", TextureUnit.Texture7);
            Texture specularMap = Texture.LoadFromFile("Textures/container2_specular.png", TextureUnit.Texture8);

            Texture monkeyDiffuse = Texture.LoadFromFile("Textures/Suzanne_diffuse.png", TextureUnit.Texture9);
            Texture monkeySpecular = Texture.LoadFromFile("Textures/Suzanne_specular.png", TextureUnit.Texture10);

            models["cube_textured"].Textures["diffuse"] = diffuseMap.Handle;
            models["cube_textured"].Textures["specular"] = specularMap.Handle;

            models["monkey"].Textures["diffuse"] = monkeyDiffuse.Handle;
            models["monkey"].Textures["specular"] = monkeySpecular.Handle;

            textures[7] = diffuseMap;
            textures[8] = specularMap;
            textures[9] = monkeyDiffuse;
            textures[10] = monkeySpecular;

            // Добавление объектов в сцену

            foreach (UsedShader us in Enum.GetValues(typeof(UsedShader)))
                objects.Add(us, new List<Object>());

            objects[UsedShader.LightShader].Add(new Object("cube", 
                                                           lightPos,
                                                           new Vector3(0f, 0f, 0f),
                                                           new Vector3(0.25f, 0.25f, 0.25f)));
            objects[UsedShader.LightShader].Add(new Object("cube",
                                                           -lightPos,
                                                           new Vector3(0f, 0f, 0f),
                                                           new Vector3(0.25f, 0.25f, 0.25f)));
            objects[UsedShader.LightShader][0].FlatColor = Vector3.UnitY+Vector3.UnitZ;
            objects[UsedShader.LightShader][1].FlatColor = Vector3.UnitX;

            for (int i = 0; i < 1000; i++)
            {
                Random rnd = new Random();
                Vector3 randRot = new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                Vector3 randPos = new Vector3((float)rnd.NextDouble() * 15f - 7.5f, (float)rnd.NextDouble() * 15f - 7.5f, (float)rnd.NextDouble() * 15f - 7.5f);
                objects[UsedShader.DiffuseShader].Add(new Object("cube_textured",
                                                                    randPos,
                                                                    randRot,
                                                                    0.5f * Vector3.One));
            }

            for (int i = 0; i < 100; i++)
            {
                Random rnd = new Random();
                Vector3 randRot = new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                Vector3 randPos = new Vector3((float)rnd.NextDouble() * 15f - 7.5f, (float)rnd.NextDouble() * 15f - 7.5f, (float)rnd.NextDouble() * 15f - 7.5f);
                objects[UsedShader.DiffuseShader].Add(new Object("monkey",
                                                                    randPos,
                                                                    randRot,
                                                                    0.5f * Vector3.One));
            }

            shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag", fromFile: true);
            single_color = new Shader("Shaders/shader.vert", "Shaders/single_color.frag", fromFile: true);
            shadow_shader = new Shader("Shaders/shadow.vert", "Shaders/shadow.frag", "Shaders/shadow.geom", true);

            // Обнуление массива источников света
            for(int i = 0; i < light_sources_count; i++)
            {
                shader.SetUniformVector3($"pointLights[{i}].position", Vector3.Zero);
                shader.SetUniformVector3($"pointLights[{i}].ambient", new Vector3(1f, 1f, 1f));
                shader.SetUniformVector3($"pointLights[{i}].diffuse", Vector3.Zero);
                shader.SetUniformVector3($"pointLights[{i}].specular", Vector3.Zero);
                shader.SetUniformFloat($"pointLights[{i}].constant", 1.0f);
                shader.SetUniformFloat($"pointLights[{i}].linear", 0.09f);
                shader.SetUniformFloat($"pointLights[{i}].quadratic", 0.032f);
                shader.SetUniformFloat($"pointLights[{i}].strength", 0f);
            }

            // Создание cubemap
            depthCubeMaps = new int[light_sources_count];
            shadowFBO = new int[light_sources_count];

            for (int i = 0; i < light_sources_count; i++)
            {
                depthCubeMaps[i] = GL.GenTexture();
                GL.BindTexture(TextureTarget.TextureCubeMap, depthCubeMaps[i]);

                shadowFBO[i] = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO[i]);

                for (int j = 0; j < 6; j++)
                {
                    GL.TexImage2D(
                        TextureTarget.TextureCubeMapPositiveX + j,
                        0,
                        PixelInternalFormat.DepthComponent,
                        shadow_width,
                        shadow_height,
                        0,
                        PixelFormat.DepthComponent,
                        PixelType.Float,
                        IntPtr.Zero);
                }

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthCubeMaps[i], 0);
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }

            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            //time += 4.0 * e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Заполнение информации о существующих источниках света
            lights.Clear();

            foreach (var o in objects[UsedShader.LightShader])
            {
                lights.Add(new PointLight(o.Position,
                                          1f,
                                          1f,
                                          0.09f,
                                          0.032f,
                                          Vector3.Zero,
                                          o.FlatColor,
                                          o.FlatColor));
            }

            // Рендер теней
            ShadowRender();

            // Рендер сцены
            RenderScene();

            SwapBuffers();
        }

        private void ShadowRender()
        {
            GL.Viewport(0, 0, shadow_width, shadow_height);
            Matrix4 shadowProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, shadow_width / (float)shadow_height, near, far);

            shadow_shader.Use();
            GL.CullFace(TriangleFace.Front);

            int l = 0;
            foreach (var light in lights)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO[l]);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                List<Matrix4> shadowTransforms = new List<Matrix4>
                {
                     Matrix4.LookAt(light.position, light.position + Vector3.UnitX, -Vector3.UnitY) * shadowProj,
                     Matrix4.LookAt(light.position, light.position - Vector3.UnitX, -Vector3.UnitY) * shadowProj,
                     Matrix4.LookAt(light.position, light.position + Vector3.UnitY, Vector3.UnitZ)  * shadowProj,
                     Matrix4.LookAt(light.position, light.position - Vector3.UnitY, -Vector3.UnitZ) * shadowProj,
                     Matrix4.LookAt(light.position, light.position + Vector3.UnitZ, -Vector3.UnitY) * shadowProj,
                     Matrix4.LookAt(light.position, light.position - Vector3.UnitZ, -Vector3.UnitY) * shadowProj,
                };

                for (int i = 0; i < 6; i++)
                    shadow_shader.SetUniformMatrix4($"matrices[{i}].matrix", shadowTransforms[i]);

                shadow_shader.SetUniformFloat("far_plane", far);
                shadow_shader.SetUniformVector3("lightPos", light.position);

                foreach(var o in objects[UsedShader.DiffuseShader])
                {
                    o.CalculateModelMatrix();
                    var model_matrix = o.ModelMatrix;
                    var model_name = o.Model;
                    var model = models[model_name];
                    GL.BindVertexArray(model.VAO);

                    shadow_shader.SetUniformMatrix4("model", model_matrix);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, model.VBO);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, model.EBO);

                    GL.DrawElements(PrimitiveType.Triangles, model.IndCount, DrawElementsType.UnsignedInt, 0);
                    GL.BindVertexArray(0);
                }

                l++;
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }

            GL.CullFace(TriangleFace.Back);
        }

        private void RenderScene()
        {
            GL.Viewport(0, 0, width, height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            foreach (var (group, objectsList) in objects)
            {
                switch (group)
                {
                    // Поведение такое же, как и у SingleColorShader,
                    // Но мы буфферизуем положение, цвет и т.п. имеющихся на сцене источников света
                    case UsedShader.LightShader:
                        {
                            single_color.Use();

                            foreach (var o in objectsList)
                            {
                                o.CalculateModelMatrix();
                                var model_matrix = o.ModelMatrix;
                                var model_name = o.Model;
                                var model = models[model_name];
                                GL.BindVertexArray(model.VAO);

                                single_color.SetUniformMatrix4("model", model_matrix);
                                single_color.SetUniformMatrix4("view", cam.GetViewMatrix());
                                single_color.SetUniformMatrix4("projection", cam.GetProjectionMatrix());
                                single_color.SetUniformVector3("lightColor", o.FlatColor);

                                GL.BindBuffer(BufferTarget.ArrayBuffer, model.VBO);
                                GL.BindBuffer(BufferTarget.ElementArrayBuffer, model.EBO);

                                GL.DrawElements(PrimitiveType.Triangles, model.IndCount, DrawElementsType.UnsignedInt, 0);
                                GL.BindVertexArray(0);
                            }

                            continue;
                        }
                    case UsedShader.SingleColorShader:
                        {
                            single_color.Use();

                            foreach (var o in objectsList)
                            {
                                o.CalculateModelMatrix();
                                var model_matrix = o.ModelMatrix;
                                var model_name = o.Model;
                                var model = models[model_name];

                                GL.BindVertexArray(model.VAO);
                                single_color.SetUniformMatrix4("model", model_matrix);
                                single_color.SetUniformMatrix4("view", cam.GetViewMatrix());
                                single_color.SetUniformMatrix4("projection", cam.GetProjectionMatrix());
                                single_color.SetUniformVector3("lightColor", o.FlatColor);

                                GL.BindBuffer(BufferTarget.ArrayBuffer, model.VBO);
                                GL.BindBuffer(BufferTarget.ElementArrayBuffer, model.EBO);

                                GL.DrawElements(PrimitiveType.Triangles, model.IndCount, DrawElementsType.UnsignedInt, 0);
                                GL.BindVertexArray(0);
                            }

                            continue;
                        }

                    case UsedShader.DiffuseShader:
                        {
                            shader.Use();

                            foreach (var o in objectsList)
                            {
                                o.CalculateModelMatrix();
                                var model_matrix = o.ModelMatrix;
                                var model_name = o.Model;
                                var model = models[model_name];

                                GL.BindVertexArray(model.VAO);

                                if (model.Textures.Count != 0)
                                {
                                    var diffuseMapHandle = model.Textures["diffuse"] + 6;
                                    var specularMapHandle = model.Textures["specular"] + 6;

                                    textures[diffuseMapHandle].Use();
                                    textures[specularMapHandle].Use();

                                    shader.SetUniformInt("material.diffuse", diffuseMapHandle);
                                    shader.SetUniformInt("material.specular", specularMapHandle);
                                    shader.SetUniformVector3("material.specular", 0.5f * Vector3.One);
                                    shader.SetUniformFloat("material.shininess", 32f);
                                }

                                shader.SetUniformMatrix4("model", model_matrix);
                                shader.SetUniformMatrix4("view", cam.GetViewMatrix());
                                shader.SetUniformMatrix4("projection", cam.GetProjectionMatrix());

                                shader.SetUniformVector3("viewPos", cam.Position);
                                shader.SetUniformFloat("far_plane", far);

                                for (int i = 0; i < lights.Count; i++)
                                {
                                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                                    GL.BindTexture(TextureTarget.TextureCubeMap, depthCubeMaps[i]);

                                    shader.SetUniformVector3($"pointLights[{i}].position", lights[i].position);
                                    shader.SetUniformVector3($"pointLights[{i}].ambient", lights[i].ambient);
                                    shader.SetUniformVector3($"pointLights[{i}].diffuse", lights[i].diffuse);
                                    shader.SetUniformVector3($"pointLights[{i}].specular", lights[i].specular);
                                    shader.SetUniformFloat($"pointLights[{i}].strength", 1f);
                                    shader.SetUniformInt($"shadowMap[{i}].shadowMap", i);
                                }

                                GL.BindBuffer(BufferTarget.ArrayBuffer, model.VBO);
                                GL.BindBuffer(BufferTarget.ElementArrayBuffer, model.EBO);

                                GL.DrawElements(PrimitiveType.Triangles, model.IndCount, DrawElementsType.UnsignedInt, 0);
                                GL.BindVertexArray(0);
                            }

                            continue;
                        }

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var keybodardInput = KeyboardState;

            if (keybodardInput.IsKeyDown(Keys.Escape))
                Close();
            if (keybodardInput.IsKeyDown(Keys.W))
                cam.Position += cam.Front * camSpeed * (float)e.Time;
            if (keybodardInput.IsKeyDown(Keys.S))
                cam.Position -= cam.Front * camSpeed * (float)e.Time;
            if (keybodardInput.IsKeyDown(Keys.A))
                cam.Position -= cam.Right * camSpeed * (float)e.Time;
            if (keybodardInput.IsKeyDown(Keys.D))
                cam.Position += cam.Right * camSpeed * (float)e.Time;

            if (keybodardInput.IsKeyDown(Keys.I))
                lightPos += 1.25f * Vector3.UnitX * (float)e.Time;
            if (keybodardInput.IsKeyDown(Keys.K))
                lightPos -= 1.25f * Vector3.UnitX * (float)e.Time;
            if (keybodardInput.IsKeyDown(Keys.J))
                lightPos -= 1.25f * Vector3.UnitY * (float)e.Time;
            if (keybodardInput.IsKeyDown(Keys.L))
                lightPos += 1.25f * Vector3.UnitY * (float)e.Time;
            if (keybodardInput.IsKeyDown(Keys.U))
                lightPos += 1.25f * Vector3.UnitZ * (float)e.Time;
            if (keybodardInput.IsKeyDown(Keys.O))
                lightPos -= 1.25f * Vector3.UnitZ * (float)e.Time;

            objects[UsedShader.LightShader][0].Position = lightPos;
            objects[UsedShader.LightShader][1].Position = -lightPos;

            var mouseState = MouseState;

            if (!mouseMoved)
            {
                lastMousePos = new Vector2(mouseState.X, mouseState.Y);
                mouseMoved = true;
            }

            if (IsFocused)
            {
                var deltaX = mouseState.X - lastMousePos.X;
                var deltaY = mouseState.Y - lastMousePos.Y;
                lastMousePos = new Vector2(mouseState.X, mouseState.Y);
                cam.Yaw += deltaX * sensitivity;
                cam.Pitch -= deltaY * sensitivity;
            }

        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnFocusedChanged(FocusedChangedEventArgs e)
        {
            base.OnFocusedChanged(e);

            if (!IsFocused)
                mouseMoved = false;
        }
    }
}
