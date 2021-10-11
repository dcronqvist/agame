using System;
using System.Collections.Generic;
using AGame.Engine;
using AGame.Engine.DebugTools;
using AGame.Engine.Graphics;
using AGame.Engine.World;
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

    class SetDebugCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            if (!Debug.PropertyExists(args[0]))
            {
                return CommandResult.CreateError($"Debug property '{args[0]}' does not exist.");
            }

            Type t = Debug.GetDebugPropertyType(args[0]);

            try
            {
                object val = Convert.ChangeType(args[1], t);
                Debug.SetDebugProperty(args[0], val);

                return CommandResult.CreateOk($"Set '{args[0]}' to {args[1]}");
            }
            catch (Exception ex)
            {
                return CommandResult.CreateError($"{ex.Message}");
            }

        }
        public string GetHandle()
        {
            return "debug";
        }
    }
}