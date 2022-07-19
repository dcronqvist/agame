using AGame.Engine.ECSys.Components;
using AGame.Engine.Items;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client)]
public class PlayerUseItemSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<HotbarComponent>();
        this.RegisterComponentType<PlayerStateComponent>();
        this.RegisterComponentType<InventoryComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        foreach (var entity in entities)
        {
            var hotbar = entity.GetComponent<HotbarComponent>();
            var playerState = entity.GetComponent<PlayerStateComponent>();
            var inventory = entity.GetComponent<InventoryComponent>();

            InventorySlot slot = inventory.GetInventory().GetSlot(hotbar.SelectedSlot, 2);

            if (slot is not null)
            {
                Item item = slot.GetItem();

                if (playerState.HoldingUseItem)
                {
                    item.OnHoldLeftClick(entity, new Vector2i(playerState.MouseTileX, playerState.MouseTileY), ParentECS, deltaTime);
                }
                else
                {
                    // Some items might do stuff just when holding the item, without "using it"
                    item.OnReleaseLeftClick(entity, new Vector2i(playerState.MouseTileX, playerState.MouseTileY), ParentECS);
                }
            }
        }
    }
}