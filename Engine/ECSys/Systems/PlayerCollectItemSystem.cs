using System.Numerics;
using AGame.Engine.ECSys.Components;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client)]
public class PlayerCollectItemSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<GroundItemComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        // Find all entities with a player state component
        var players = this.ParentECS.GetAllEntities(e => e.HasComponent<PlayerStateComponent>());

        foreach (var entity in entities)
        {
            // Get closest player that is at most 2 blocks away from this entity
            var player = players.Where(e =>
            {
                var playerTransform = e.GetComponent<TransformComponent>();
                var middleOfPlayer = (playerTransform.Position + new CoordinateVector(0.5f, 1f));
                var itemTransform = entity.GetComponent<TransformComponent>();
                var middleOfItem = (itemTransform.Position + new CoordinateVector(0.5f, 0.5f));
                return middleOfPlayer.DistanceTo(middleOfItem) <= 1f;
            }).FirstOrDefault();

            // Pick up item
            if (player != null)
            {
                var inventory = player.GetComponent<InventoryComponent>();
                inventory.GetInventory().AddItem(entity.GetComponent<GroundItemComponent>().Item, 1);
                this.ParentECS.DestroyEntity(entity.ID);
            }
        }

    }
}