using System;
using System.Collections.Generic;
using AGame.Engine;
using AGame.Engine.DebugTools;
using AGame.Engine.Graphics;
using static AGame.Engine.OpenGL.GL;

namespace MyMod
{
    class MyCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            return CommandResult.CreateOk("I have done nothing");
        }

        public string GetHandle()
        {
            return "bitch";
        }
    }

    class AnotherCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            string argsString = "";
            foreach (string arg in args)
            {
                argsString += " " + arg;
            }
            return CommandResult.CreateWarning($"You are most likely dcronqvist. Run with args: {argsString}");
        }

        public string GetHandle()
        {
            return "whoami";
        }
    }

    class NewCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            return CommandResult.CreateOk($"Default command.");
        }
        public string GetHandle()
        {
            return "newcommand";
        }
    }

    class ExitCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            DisplayManager.SetWindowShouldClose(true);
            return CommandResult.CreateOk($"Default command.");
        }
        public string GetHandle()
        {
            return "exit";
        }
    }

    class CommandsCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            Dictionary<string, ICommand> commands = GameConsole.AvailableCommands;

            foreach (KeyValuePair<string, ICommand> kvp in commands)
            {
                GameConsole.WriteLine(this, kvp.Key);
            }

            return CommandResult.CreateOk($"Listed all commands.");
        }

        public string GetHandle()
        {
            return "commands";
        }
    }

    class ToggleWireframeCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
            return CommandResult.CreateOk($"Set to wireframe.");
        }
        public string GetHandle()
        {
            return "wireframe";
        }
    }

    class NormalMode : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            return CommandResult.CreateOk($"Removed wireframe.");
        }
        public string GetHandle()
        {
            return "normal";
        }
    }

    class WindowSizeCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            int x = int.Parse(args[0]);
            int y = int.Parse(args[1]);

            DisplayManager.SetWindowSizeInPixels(new System.Numerics.Vector2(x, y));

            return CommandResult.CreateOk($"Set window size to: {new System.Numerics.Vector2(x, y)}");
        }
        public string GetHandle()
        {
            return "windowsize";
        }
    }

    class RowHeightCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            int height = int.Parse(args[0]);
            GameConsole.RowHeight = height;
            return CommandResult.CreateOk($"Set GameConsole's rowheight to {height} pixels.");
        }
        public string GetHandle()
        {
            return "rowheight";
        }
    }

    class FPSCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            float fps = 1.0f / GameTime.DeltaTime;
            return CommandResult.CreateOk($"FPS is {MathF.Round(fps)}");
        }
        public string GetHandle()
        {
            return "checkfps";
        }
    }

    class ClearConsoleCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            GameConsole.ConsoleLines.Clear();
            return null;
        }
        public string GetHandle()
        {
            return "clear";
        }
    }
}