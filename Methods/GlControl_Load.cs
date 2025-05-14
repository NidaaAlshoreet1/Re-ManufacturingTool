using OpenTK.Graphics.OpenGL4;
// using OpenTK.Graphics.OpenGL;
using System;
using OpenTK.WinForms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.WinForms;
using g3;
using System.Collections.Generic;
namespace ManufacturingTool;

// This class is responsible for loading and managing OpenGL shaders
// It handles shader compilation, linking, and error checking
public partial class GlControl
{

    private int _shaderProgram;

    // This method is called when the OpenGL control is loaded
    // It initializes the OpenGL context and loads shaders
    private void InitializeOpenGL()
    {
        GL.ClearColor(0.95f, 0.95f, 0.97f, 1.0f);
        GL.Enable(EnableCap.DepthTest);
    }

        // This method is called when the OpenGL control is loaded
        // It sets up the shaders and checks for compilation errors

    private void GlControl_Load(object sender, EventArgs e)
    {
        GL.ClearColor(0.95f, 0.95f, 0.97f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        _shaderProgram = LoadShaders("vertex.glsl", "fragment.glsl");

        int LoadShaders(string vertexPath, string fragmentPath)
        {
            string vertexShaderSource = File.ReadAllText(vertexPath);
            string fragmentShaderSource = File.ReadAllText(fragmentPath);

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompileStatus(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompileStatus(fragmentShader);

            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            CheckProgramLinkStatus(shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }

        void CheckShaderCompileStatus(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Shader compilation failed: {infoLog}");
            }
        }

        void CheckProgramLinkStatus(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                throw new Exception($"Program linking failed: {infoLog}");
            }
        }
    }
}