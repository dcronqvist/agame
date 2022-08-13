using System.Collections.Generic;
using AGame.Engine.ECSys;
using AGame.Engine.Items;
using AGame.Engine.World;
using AGame.Engine.Assets.Scripting;

namespace DefaultMod;

[SystemRunsOn(SystemRunner.Server), ScriptType(Name = "item_holding_system")]
public class ItemHoldingSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<ContainerComponent>();
        this.RegisterComponentType<HotbarComponent>();
        this.RegisterComponentType<PlayerStateComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        foreach (var entity in entities)
        {
            var container = entity.GetComponent<ContainerComponent>();
            var hotbar = entity.GetComponent<HotbarComponent>();
            var playerState = entity.GetComponent<PlayerStateComponent>();

            var slot = container.GetContainer().GetSlot(hotbar.SelectedSlot);

            // if (slot.Item != null && slot.Item != "")
            // {
            //     playerState.HoldingItem = slot.Item;
            // }
            // else
            // {
            //     playerState.HoldingItem = "";
            // }
        }
    }
}