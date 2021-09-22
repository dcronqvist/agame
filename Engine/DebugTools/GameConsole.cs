using System;
using System.Collections.Generic;
using AGame.Engine.Assets.Scripting;
using System.Linq;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using System.Text;

namespace AGame.Engine.DebugTools
{
    public static class GameConsole
    {
        public static Dictionary<string, ICommand> AvailableCommands { get; set; }
        public static List<ConsoleLine> ConsoleLines { get; private set; }

        private static StringBuilder currentLine;
        private static RenderTexture canvas;
        private static bool enabled;

        static GameConsole()
        {
            ConsoleLines = new List<ConsoleLine>();
            AvailableCommands = new Dictionary<string, ICommand>();
            enabled = false;
        }

        public static void Initialize()
        {
            currentLine = new StringBuilder();
            Input.OnChar += (sender, c) =>
            {
                if (enabled)
                {
                    currentLine.Append(c);
                }
            };
            Input.OnBackspace += (sender, e) =>
            {
                if (enabled)
                {
                    if (currentLine.Length > 0)
                        currentLine.Remove(currentLine.Length - 1, 1);
                }
            };
            canvas = new RenderTexture(DisplayManager.GetWindowSizeInPixels());
        }

        public static void SetEnabled(bool val)
        {
            enabled = val;
        }

        public static void WriteLine(ICommand sender, string message)
        {
            ConsoleLines.Add(new ConsoleLine(sender.GetHandle(), message));
        }

        public static void WriteLine(string sender, string message)
        {
            ConsoleLines.Add(new ConsoleLine(sender, message));
        }

        public static void LoadCommands()
        {
            Type[] commandTypes = ScriptingManager.GetAllTypesWithBaseType<ICommand>();

            foreach (Type commandType in commandTypes)
            {
                ICommand ic = ScriptingManager.CreateInstance<ICommand>(commandType.FullName);

                AvailableCommands.Add(ic.GetHandle(), ic);
            }
        }

        public static void RunLine(string line)
        {
            string[] splitLine = line.Split(char.Parse(" "));
            string commandHandle = splitLine[0];

            if (!AvailableCommands.ContainsKey(commandHandle))
            {
                ConsoleLines.Add(CommandResult.CreateError("Invalid command."));
                return;
            }

            ICommand ic = AvailableCommands[commandHandle];
            CommandResult cr = ic.Execute(splitLine.Skip(1).ToArray());
            ConsoleLines.Add(cr);
        }

        public static void Update()
        {
            if (Input.IsKeyPressed(GLFW.Keys.Enter))
            {
                RunLine(currentLine.ToString());
                currentLine.Clear();
            }
        }

        public static RenderTexture Render(Font font)
        {
            Renderer.SetRenderTarget(canvas, null);
            Renderer.Clear(ColorF.Black * 0.2f);

            Vector2 basePosition = Vector2.Zero;
            float rowHeight = 16f;

            for (int i = 0; i < ConsoleLines.Count; i++)
            {
                Vector2 offset = new Vector2(0, i * rowHeight);
                Renderer.Text.RenderText(font, GameConsole.ConsoleLines[i].ToString(), basePosition + offset, 1f, ColorF.White, Renderer.Camera);
            }
            Vector2 curroffset = new Vector2(0, ConsoleLines.Count * rowHeight);
            Renderer.Text.RenderText(font, currentLine.ToString(), basePosition + curroffset, 1f, ColorF.White, Renderer.Camera);

            Renderer.SetRenderTarget(null, null);

            return canvas;
        }
    }
}