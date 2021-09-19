using System;
using GLFW;
using System.Drawing;
using OpenGL;
using System.Numerics;

namespace AGame.Graphics
{
    class DisplayManager
    {
        public static Window WindowHandle { get; set; }

        private static void PrepareContext()
        {
            // Set some common hints for the OpenGL profile creation
            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.Doublebuffer, true);
            Glfw.WindowHint(Hint.Decorated, true);
        }

        private static Window CreateWindow(int width, int height, string title)
        {
            // Create window, make the OpenGL context current on the thread, and import graphics functions
            Window window = Glfw.CreateWindow(width, height, title, Monitor.None, Window.None);

            // Center window
            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            int x = (screen.Width - width) / 2;
            int y = (screen.Height - height) / 2;
            Glfw.SetWindowPosition(window, x, y);

            Glfw.MakeContextCurrent(window);
            GL.Import(Glfw.GetProcAddress);

            return window;
        }

        public static void InitWindow(int width, int height, string title)
        {
            PrepareContext();
            WindowHandle = CreateWindow(width, height, title);

            Input.Init();
        }

        public static void CloseWindow()
        {
            Glfw.Terminate();
        }

        public static Vector2 GetWindowSizeInPixels()
        {
            Glfw.GetFramebufferSize(WindowHandle,
                                    out int width,
                                    out int height);

            return new Vector2(width, height);
        }

        public static void SetWindowSizeInPixels(Vector2 size)
        {
            Glfw.SetWindowSize(WindowHandle, (int)size.X, (int)size.Y);
        }

        public static bool GetWindowShouldClose()
        {
            return Glfw.WindowShouldClose(WindowHandle);
        }

        public static void SwapBuffers(int swapInterval = 0)
        {
            Glfw.SwapInterval(swapInterval);
            Glfw.SwapBuffers(WindowHandle);
        }

        public static void PollEvents()
        {
            Glfw.PollEvents();
        }
    }
}