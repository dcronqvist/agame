using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.DebugTools;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.World;
using static AGame.Engine.OpenGL.GL;
using System.CommandLine;

namespace MyMod
{
    class MyCommand : ICommand
    {
        public Command GetConfiguration()
        {
            var c = new Command("nocommand");

            c.SetHandler(() =>
            {
                GameConsole.WriteLine(this, "This command does nothing, but it works!");
            });

            return c;
        }
    }

    class ExitCommand : ICommand
    {
        public Command GetConfiguration()
        {
            var c = new Command("exit");

            c.SetHandler(() =>
            {
                GameConsole.WriteLine(this, "Exiting...");
                DisplayManager.SetWindowShouldClose(true);
            });

            return c;
        }
    }

    class CommandsCommand : ICommand
    {
        public Command GetConfiguration()
        {
            var c = new Command("commands");

            c.SetHandler(() =>
            {
                foreach (KeyValuePair<string, ICommand> kvp in GameConsole.AvailableCommands)
                {
                    GameConsole.WriteLine(this, $"{kvp.Key}");
                }
            });

            return c;
        }
    }

    class WindowSizeCommand : ICommand
    {
        public Command GetConfiguration()
        {
            var c = new Command("windowsize");

            var width = new Argument<int>("width", "the desired width of the window");
            var height = new Argument<int>("height", "the desired height of the window");

            c.SetHandler((w, h) =>
            {
                DisplayManager.SetWindowSizeInPixels(new Vector2(w, h));
            }, width, height);

            c.AddArgument(width);
            c.AddArgument(height);

            return c;
        }
    }

    class ClearConsoleCommand : ICommand
    {
        public Command GetConfiguration()
        {
            var c = new Command("clear");

            c.SetHandler(() =>
            {
                GameConsole.ConsoleLines.Clear();
            });

            return c;
        }
    }

    // class SetDebugCommand : ICommand
    // {
    //     public CommandResult Execute(Dictionary<string, object> args)
    //     {
    //         string prop = (string)args["property"];
    //         string va = (string)args["value"];

    //         if (!Debug.PropertyExists(prop))
    //         {
    //             return CommandResult.CreateError($"Debug property '{prop}' does not exist.");
    //         }

    //         Type t = Debug.GetDebugPropertyType(prop);

    //         try
    //         {
    //             object val = Convert.ChangeType(va, t);
    //             Debug.SetDebugProperty(prop, val);

    //             return CommandResult.CreateOk($"Set '{prop}' to {va}");
    //         }
    //         catch (Exception ex)
    //         {
    //             return CommandResult.CreateError($"{ex.Message}");
    //         }
    //     }

    //     public CommandConfig GetConfiguration()
    //     {
    //         return new CommandConfig().SetHandle("debug").AddParameter(
    //             new CommandConfig.Parameter(CommandConfig.ParameterType.String, "property", 0)
    //         ).AddParameter(
    //             new CommandConfig.Parameter(CommandConfig.ParameterType.String, "value", 1)
    //         );
    //     }
    // }

    // class ECSEntityCount : ICommand
    // {
    //     public CommandResult Execute(Dictionary<string, object> args)
    //     {
    //         return CommandResult.CreateOk($"Entity Count: {ECS.Instance.Value.GetAllEntities().Count}");
    //     }

    //     public CommandConfig GetConfiguration()
    //     {
    //         return new CommandConfig().SetHandle("ecsentitycount");
    //     }
    // }

    // class ECSNewEntityAtMouse : ICommand
    // {
    //     public CommandResult Execute(Dictionary<string, object> args)
    //     {
    //         string name = (string)args["name"];

    //         Entity e = ECS.CreateEntityFromAsset(name);

    //         Vector2 pos = Input.GetMousePosition(WorldManager.PlayerCamera);

    //         e.GetComponent<TransformComponent>().Position = pos;

    //         return CommandResult.CreateOk($"Created entity '{name}' at x={pos.X}, y={pos.Y}");
    //     }

    //     public CommandConfig GetConfiguration()
    //     {
    //         return new CommandConfig().SetHandle("newentity").AddParameter(
    //             new CommandConfig.Parameter(CommandConfig.ParameterType.String, "name", 0)
    //         );
    //     }
    // }

    // class ECSGetEntityComponents : ICommand
    // {
    //     public CommandResult Execute(Dictionary<string, object> args)
    //     {
    //         if (!ECS.Instance.Value.EntityExists((int)args["entity"]))
    //         {
    //             return CommandResult.CreateError($"Entity does not exist.");
    //         }

    //         Entity e = ECS.Instance.Value.GetEntityFromID((int)args["entity"]);

    //         string s = e.Components.Select(x => x.ComponentType + ": " + x.ToString()).Aggregate((x, y) => x + "," + y);

    //         return CommandResult.CreateOk(s);
    //     }

    //     public CommandConfig GetConfiguration()
    //     {
    //         return new CommandConfig().SetHandle("ecscomponents").AddParameter(
    //             new CommandConfig.Parameter(CommandConfig.ParameterType.Integer, "entity", 0)
    //         );
    //     }
    // }

    // class MyCoolComponent : Component
    // {
    //     public override Component Clone()
    //     {
    //         return new MyCoolComponent();
    //     }

    //     public override string ToString()
    //     {
    //         return "MyCoolComponent";
    //     }
    // }
}