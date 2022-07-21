using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Items;
using AGame.Engine.World;
using System.Linq;

namespace DefaultMod
{
    [ScriptClass(Name = "no_use_tool")] // default.script.no_use_tool
    public class NoUseTool : IUseTool
    {
        public bool CanUse(Tool tool, Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
        {
            return true;
        }

        public void UseTool(Tool tool, Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
        {
            Logging.Log(LogLevel.Debug, $"no_use_tool used");
        }
    }

    [ScriptClass(Name = "rock_tool")] // default.script.rock_tool
    public class HarvestRockTool : IUseTool
    {
        public bool CanUse(Tool tool, Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
        {
            return true;
        }

        public void UseTool(Tool tool, Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs)
        {
            // Do nothing though
            Logging.Log(LogLevel.Debug, $"HarvestRockTool used tool");

            tool.CreateEntity("default.entity.ground_item", ecs, (e) =>
            {
                e.GetComponent<TransformComponent>().Position = new CoordinateVector(mouseWorldPosition.X, mouseWorldPosition.Y) + new CoordinateVector(1, 1);
                e.GetComponent<GroundItemComponent>().Item = "default.item.pebble";
            });
        }
    }
}