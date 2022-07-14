using System;
using static AGame.Engine.OpenGL.GL;
using System.IO;
using System.Numerics;
using System.Diagnostics;
using AGame.Engine.Graphics.Cameras;
using System.Threading;
using AGame.Engine.DebugTools;

namespace AGame.Engine.Assets
{
    public class Shader : Asset
    {
        // internal fields for the shader GLSL code
        string vertexCode;
        string fragmentCode;

        // Which shader program ID this specific shader has
        public uint ProgramID { get; set; }

        public Shader(string vertexCode, string fragmentCode)
        {
            this.vertexCode = vertexCode;
            this.fragmentCode = fragmentCode;
        }

        public void Load()
        {
            uint vs, fs;

            // Create and compile vertex shader
            vs = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(vs, vertexCode);
            glCompileShader(vs);

            int[] status = glGetShaderiv(vs, GL_COMPILE_STATUS, 1);
            if (status[0] == 0)
            {
                // Compile failed
                string error = glGetShaderInfoLog(vs, 1024);
                // TODO: This shouldn't be debug.writeline

                throw new Exception("ERROR COMPILING VERTEX SHADER: " + error);
            }

            // Create and compile fragment shader
            fs = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(fs, fragmentCode);
            glCompileShader(fs);

            status = glGetShaderiv(fs, GL_COMPILE_STATUS, 1);
            if (status[0] == 0)
            {
                // Compile failed
                string error = glGetShaderInfoLog(fs, 1024);
                throw new Exception("ERROR COMPILING FRAGMENT SHADER: " + error);
            }

            // Link the shaders together into a single shader program
            ProgramID = glCreateProgram();
            glAttachShader(ProgramID, vs);
            glAttachShader(ProgramID, fs);
            glLinkProgram(ProgramID);

            status = glGetProgramiv(ProgramID, GL_LINK_STATUS, 1);
            if (status[0] == 0)
            {
                // link failed failed
                string error = glGetProgramInfoLog(ProgramID, 1024);
                throw new Exception("ERROR LINKING SHADER PROGRAM: " + error);
            }

            // Delete unnecessary shader objects after linking
            glDetachShader(ProgramID, vs);
            glDetachShader(ProgramID, fs);
            glDeleteShader(vs);
            glDeleteShader(fs);
        }

        public static Shader LoadFromFiles(string vertexFile, string fragmentFile)
        {
            string vertexShaderCode = "";
            string fragmentShaderCode = "";

            using (StreamReader sr = new StreamReader(vertexFile))
            {
                vertexShaderCode = sr.ReadToEnd();
            }
            using (StreamReader sr = new StreamReader(fragmentFile))
            {
                fragmentShaderCode = sr.ReadToEnd();
            }

            Shader s = new Shader(vertexShaderCode, fragmentShaderCode);
            s.Load();
            return s;
        }

        public void Use()
        {
            glUseProgram(ProgramID);
        }

        public void SetMatrix4x4(string uniformName, Matrix4x4 mat)
        {
            glUniformMatrix4fv(glGetUniformLocation(ProgramID, uniformName), 1, false, GetMatrix4x4Values(mat));
        }

        public void SetInt(string uniformName, int value)
        {
            glUniform1i(glGetUniformLocation(ProgramID, uniformName), value);
        }

        public void SetFloat(string uniformName, float f)
        {
            glUniform1f(glGetUniformLocation(ProgramID, uniformName), f);
        }

        public void SetVec2(string uniformName, float f1, float f2)
        {
            glUniform2f(glGetUniformLocation(ProgramID, uniformName), f1, f2);
        }

        public void SetVec4(string uniformName, float f1, float f2, float f3, float f4)
        {
            glUniform4f(glGetUniformLocation(ProgramID, uniformName), f1, f2, f3, f4);
        }

        public void SetFloatArray(string uniformName, float[] values)
        {
            glUniform1fv(glGetUniformLocation(ProgramID, uniformName), values.Length, values);
        }

        public void SetBool(string uniformName, bool val)
        {
            glUniform1i(glGetUniformLocation(ProgramID, uniformName), Convert.ToInt32(val));
        }

        private float[] GetMatrix4x4Values(Matrix4x4 m)
        {
            return new float[]
            {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            };
        }

        public override bool InitOpenGL()
        {
            Load();
            return true;
        }
    }
}