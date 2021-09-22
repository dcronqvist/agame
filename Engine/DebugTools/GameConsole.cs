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
    static class GameConsole
    {
        static Dictionary<string, ICommand> AvailableCommands { get; set; }
        public static List<CommandResult> ExecutedCommands { get; private set; }

        private static StringBuilder currentLine;

        static GameConsole()
        {
            ExecutedCommands = new List<CommandResult>();
            AvailableCommands = new Dictionary<string, ICommand>();
        }

        public static void Initialize()
        {
            currentLine = new StringBuilder();
            LoadCommands();
            Input.OnChar += (sender, c) =>
            {
                currentLine.Append(c);
            };
            Input.OnBackspace += (sender, e) =>
            {
                if (currentLine.Length > 0)
                    currentLine.Remove(currentLine.Length - 1, 1);
            };
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
                ExecutedCommands.Add(CommandResult.CreateError("Invalid command."));
                return;
            }

            ICommand ic = AvailableCommands[commandHandle];
            CommandResult cr = ic.Execute(splitLine.Skip(1).ToArray());
            ExecutedCommands.Add(cr);
        }

        public static void Update()
        {
            if (Input.IsKeyPressed(GLFW.Keys.Enter))
            {
                RunLine(currentLine.ToString());
                currentLine.Clear();
            }
        }

        public static void Render(Font font, TextRenderer renderer, Camera2D cam)
        {
            Vector2 basePosition = Vector2.Zero;
            float rowHeight = 16f;

            for (int i = 0; i < ExecutedCommands.Count; i++)
            {
                Vector2 offset = new Vector2(0, i * rowHeight);
                renderer.RenderText(font, GameConsole.ExecutedCommands[i].ToString(), basePosition + offset, 1f, ColorF.White, cam);
            }
            Vector2 curroffset = new Vector2(0, ExecutedCommands.Count * rowHeight);
            renderer.RenderText(font, currentLine.ToString(), basePosition + curroffset, 1f, ColorF.White, cam);
        }
    }
}