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
        public CommandResult Execute(Dictionary<string, object> args)
        {
            return CommandResult.CreateOk("I have done nothing");
        }

        public CommandConfig GetConfiguration()
        {
            return new CommandConfig().SetHandle("nocommand");
        }
    }

    class ExitCommand : ICommand
    {
        public CommandResult Execute(Dictionary<string, object> args)
        {
            DisplayManager.SetWindowShouldClose(true);
            return CommandResult.CreateOk($"Default command.");
        }

        public CommandConfig GetConfiguration()
        {
            return new CommandConfig().SetHandle("exit");
        }
    }

    class CommandsCommand : ICommand
    {
        public CommandResult Execute(Dictionary<string, object> args)
        {
            Dictionary<string, ICommand> commands = GameConsole.AvailableCommands;

            foreach (KeyValuePair<string, ICommand> kvp in commands)
            {
                GameConsole.WriteLine(this, $"{kvp.Value.GetConfiguration().GetUsageMessage()}");
            }

            return CommandResult.CreateOk($"Listed all commands.");
        }

        public CommandConfig GetConfiguration()
        {
            return new CommandConfig().SetHandle("commands");
        }
    }

    class WindowSizeCommand : ICommand
    {
        public CommandResult Execute(Dictionary<string, object> args)
        {
            int x = (int)args["width"];
            int y = (int)args["height"];

            DisplayManager.SetWindowSizeInPixels(new System.Numerics.Vector2(x, y));

            return CommandResult.CreateOk($"Set window size.");
        }

        public CommandConfig GetConfiguration()
        {
            return new CommandConfig().SetHandle("windowsize").AddParameter(
                new CommandConfig.Parameter(CommandConfig.ParameterType.Integer, "width", 0)
            ).AddParameter(
                new CommandConfig.Parameter(CommandConfig.ParameterType.Integer, "height", 1)
            );
        }
    }

    class FPSCommand : ICommand
    {
        public CommandResult Execute(Dictionary<string, object> args)
        {
            float fps = 1.0f / GameTime.DeltaTime;
            return CommandResult.CreateOk($"FPS is {MathF.Round(fps)}");
        }

        public CommandConfig GetConfiguration()
        {
            return new CommandConfig().SetHandle("checkfps");
        }
    }

    class ClearConsoleCommand : ICommand
    {
        public CommandResult Execute(Dictionary<string, object> args)
        {
            GameConsole.ConsoleLines.Clear();
            return null;
        }

        public CommandConfig GetConfiguration()
        {
            return new CommandConfig().SetHandle("clear");
        }
    }

    class SetDebugCommand : ICommand
    {
        public CommandResult Execute(Dictionary<string, object> args)
        {
            string prop = (string)args["property"];
            string va = (string)args["value"];

            if (!Debug.PropertyExists(prop))
            {
                return CommandResult.CreateError($"Debug property '{prop}' does not exist.");
            }

            Type t = Debug.GetDebugPropertyType(prop);

            try
            {
                object val = Convert.ChangeType(va, t);
                Debug.SetDebugProperty(prop, val);

                return CommandResult.CreateOk($"Set '{prop}' to {va}");
            }
            catch (Exception ex)
            {
                return CommandResult.CreateError($"{ex.Message}");
            }
        }

        public CommandConfig GetConfiguration()
        {
            return new CommandConfig().SetHandle("debug").AddParameter(
                new CommandConfig.Parameter(CommandConfig.ParameterType.String, "property", 0)
            ).AddParameter(
                new CommandConfig.Parameter(CommandConfig.ParameterType.String, "value", 1)
            );
        }
    }
}