using System;
using AGame.Engine;
using AGame.Engine.GLFW;
using System.Drawing;
using AGame.Engine.OpenGL;
using System.Numerics;
using AGame.Engine.Assets;

namespace AGame.Engine.Graphics
{
    public static class DisplayManager
    {
        public static Window WindowHandle { get; set; }
        public static event EventHandler<Vector2> OnFramebufferResize;
        private static bool manuallySetClose;
        public static int TargetFPS { get; set; }
        public static float FrameTime
        {
            get
            {
                return 1.0f / TargetFPS;
            }
        }

        private static void PrepareContext()
        {
            // Set some common hints for the OpenGL profile creation
            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.Doublebuffer, true);
            Glfw.WindowHint(Hint.Decorated, true);

            manuallySetClose = false;
        }

        public static void SetTargetFPS(int fps)
        {
            TargetFPS = fps;
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

            Glfw.SetFramebufferSizeCallback(WindowHandle, (Window, width, height) =>
            {
                OnFramebufferResize?.Invoke(null, new Vector2(width, height));
            });
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

        public unsafe static void SetWindowIcon(Texture2D tex)
        {
            byte[] pixelData = tex.GetPixelData();
            fixed (byte* pix = &pixelData[0])
            {
                IntPtr ip = new IntPtr(pix);
                Glfw.SetWindowIcon(WindowHandle, 1, new Image[] { new Image(tex.Width, tex.Height, ip) });
            }
        }

        public static void SetWindowShouldClose(bool value)
        {
            manuallySetClose = value;
        }

        public static bool GetWindowShouldClose()
        {
            return Glfw.WindowShouldClose(WindowHandle) || manuallySetClose;
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

        public static void SetWindowTitle(string title)
        {
            Glfw.SetWindowTitle(WindowHandle, title);
        }
    }
}