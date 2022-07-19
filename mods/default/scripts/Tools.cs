using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Items;
using AGame.Engine.World;

namespace DefaultMod
{
    [ScriptClass(Name = "no_use_tool")]
    public class NoUseTool : IUseTool
    {
        public bool CanUse(Tool tool, Entity playerEntity, CoordinateVector mouseWorldPosition, ECS ecs)
        {
            return true;
        }

        public bool UseTool(Tool tool, Entity playerEntity, CoordinateVector mouseWorldPosition, ECS ecs, float deltaTime, float totalTimeUsed)
        {
            if (totalTimeUsed > 1f)
            {
                tool.PlaceEntity(playerEntity, "default.entity.placeable", mouseWorldPosition.ToTileAligned());

                Logging.Log(LogLevel.Debug, $"no_use_tool created entity");
                return true;
            }
            return false;
        }
    }
}