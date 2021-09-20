using System;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Shaders;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Textures;
using static AGame.Engine.OpenGL.GL;
using System.IO;
using System.Numerics;

namespace AGame
{
    class ImplGame : Game
    {
        Shader basicShader;
        Shader textureShader;

        Camera2D cam;

        Texture2D tex;
        private uint texVAO;
        uint vao, vbo;
        private uint texVBO;

        public override void Initialize()
        {

        }

        public unsafe override void LoadContent()
        {
            glEnable(GL_BLEND);
            glDisable(GL_DEPTH_TEST);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            glViewport(0, 0, (int)DisplayManager.GetWindowSizeInPixels().X, (int)DisplayManager.GetWindowSizeInPixels().Y);

            string vertexShader = @"#version 330 core
                                    layout (location = 0) in vec2 aPosition;
                                    layout (location = 1) in vec3 aColor;
                                    out vec4 vertexColor;
    
                                    uniform mat4 projection;
                                    uniform mat4 model;

                                    void main() 
                                    {
                                        vertexColor = vec4(aColor.rgb, 1.0);
                                        gl_Position = projection * model * vec4(aPosition.xy, 0, 1.0);
                                    }";

            string fragmentShader = @"#version 330 core
                                    out vec4 FragColor;
                                    in vec4 vertexColor;

                                    void main() 
                                    {
                                        FragColor = vertexColor;
                                    }";


            basicShader = new Shader(vertexShader, fragmentShader);
            basicShader.Load();

            vertexShader = @"#version 330 core
                             layout (location = 0) in vec2 aPosition;
                             layout (location = 1) in vec2 aTexCoords;
                             
                             out vec2 TexCoords;
    
                             uniform mat4 projection;
                             uniform mat4 model;

                             void main() 
                             {
                                 TexCoords = aTexCoords.xy;
                                 gl_Position = projection * model * vec4(aPosition.xy, 0, 1.0);
                             }";

            fragmentShader = @"#version 330 core
                               out vec4 FragColor;
                               in vec2 TexCoords;

                               uniform sampler2D tex;
                               uniform vec4 textureColor;

                               void main() 
                               {
                                   FragColor = textureColor * texture(tex, TexCoords);
                               }";

            textureShader = new Shader(vertexShader, fragmentShader);
            textureShader.Load();

            // Creating square.
            vao = glGenVertexArray();
            vbo = glGenBuffer();

            glBindVertexArray(vao);

            glBindBuffer(GL_ARRAY_BUFFER, vbo);

            float[] vertices =
            {
                -0.5f, 0.5f, 1f, 0f, 0f, // top left
                0.5f, 0.5f, 0f, 1f, 0f,// top right
                -0.5f, -0.5f, 0f, 0f, 1f, // bottom left

                0.5f, 0.5f, 0f, 1f, 0f,// top right
                0.5f, -0.5f, 0f, 1f, 1f, // bottom right
                -0.5f, -0.5f, 0f, 0f, 1f, // bottom left
            };

            fixed (float* v = &vertices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STATIC_DRAW);
            }

            glVertexAttribPointer(0, 2, GL_FLOAT, false, 5 * sizeof(float), (void*)0);
            glEnableVertexAttribArray(0);

            glVertexAttribPointer(1, 3, GL_FLOAT, false, 5 * sizeof(float), (void*)(sizeof(float) * 2));
            glEnableVertexAttribArray(1);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);

            cam = new Camera2D(DisplayManager.GetWindowSizeInPixels() / 2f, 2.5f);

            tex = new Texture2D(Directory.GetCurrentDirectory() + @"/res/pine_tree.png");

            ///// TEXTURE MAKING

            texVAO = glGenVertexArray();
            glBindVertexArray(texVAO);

            texVBO = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, texVBO);

            float[] texVertexData =
            {
                // pos      // tex
                -0.5f, 0.5f, 0.0f, 1.0f, //downLeft
                0.5f, -0.5f, 1.0f, 0.0f, //topRight
                -0.5f, -0.5f, 0.0f, 0.0f, //topLeft

                -0.5f, 0.5f, 0.0f, 1.0f, //downLeft
                0.5f, 0.5f, 1.0f, 1.0f, //downRight
                0.5f, -0.5f, 1.0f, 0.0f  //
            };

            fixed (float* v = &texVertexData[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * texVertexData.Length, v, GL_STATIC_DRAW);
            }

            glEnableVertexAttribArray(0);
            glVertexAttribPointer(0, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)0);

            glEnableVertexAttribArray(1);
            glVertexAttribPointer(1, 2, GL_FLOAT, false, 4 * sizeof(float), (void*)(sizeof(float) * 2));

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);
        }

        public override void Update()
        {
            DisplayManager.SetWindowTitle(Input.GetMousePosition().ToString());
        }

        public override void Render()
        {
            glClearColor(1, 1, 1, 1);
            glClear(GL_COLOR_BUFFER_BIT);

            basicShader.Use();
            basicShader.SetMatrix4x4("projection", cam.GetProjectionMatrix());

            Vector2 position = new Vector2(1280 / 2, 720 / 2);
            Vector2 scale = new Vector2(150, 100);
            float rotation = GameTime.TotalElapsedSeconds;

            Matrix4x4 trans = Matrix4x4.CreateTranslation(position.X, position.Y, 0);
            Matrix4x4 scaling = Matrix4x4.CreateScale(scale.X, scale.Y, 1);
            Matrix4x4 rot = Matrix4x4.CreateRotationZ(rotation);

            basicShader.SetMatrix4x4("model", scaling * rot * trans);

            glBindVertexArray(vao);
            //glDrawArrays(GL_TRIANGLES, 0, 6);
            glBindVertexArray(0);

            ////////////////////////////////////////

            textureShader.Use();
            textureShader.SetMatrix4x4("projection", cam.GetProjectionMatrix());

            textureShader.SetMatrix4x4("model", scaling * rot * trans);

            tex.BindTexture(GL_TEXTURE0);
            textureShader.SetInt("tex", 0);

            textureShader.SetVec4("textureColor", 1, 1, 1, 0.5f);

            glBindVertexArray(texVAO);
            glDrawArrays(GL_TRIANGLES, 0, 6);
            glBindVertexArray(0);

            DisplayManager.SwapBuffers();
        }

        public override void Unload()
        {

        }
    }
}