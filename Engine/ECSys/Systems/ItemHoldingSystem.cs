using AGame.Engine.ECSys.Components;
using AGame.Engine.Items;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Server)]
public class ItemHoldingSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<InventoryComponent>();
        this.RegisterComponentType<HotbarComponent>();
        this.RegisterComponentType<PlayerStateComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        foreach (var entity in entities)
        {
            var inventory = entity.GetComponent<InventoryComponent>();
            var hotbar = entity.GetComponent<HotbarComponent>();
            var playerState = entity.GetComponent<PlayerStateComponent>();

            InventorySlot slot = inventory.GetInventory().GetSlot(hotbar.SelectedSlot, 2);

            if (slot != null)
            {
                playerState.HoldingItem = slot.Item;
            }
            else
            {
                playerState.HoldingItem = "";
            }
        }
    }
}