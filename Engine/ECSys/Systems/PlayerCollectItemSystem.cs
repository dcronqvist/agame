using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AGame.Engine.ECSys.Components;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Server)]
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
            var itemTransform = entity.GetComponent<TransformComponent>();
            var middleOfItem = (itemTransform.Position + new CoordinateVector(0.5f, 0.5f));

            if (entity.GetComponent<GroundItemComponent>().PickedUpBy == -1)
            {
                // Get closest player that is at most 2 blocks away from this entity
                var player = players.Where(e =>
                {
                    var playerTransform = e.GetComponent<TransformComponent>();
                    var middleOfPlayer = (playerTransform.Position + new CoordinateVector(0.5f, 1f));
                    return middleOfPlayer.DistanceTo(middleOfItem) <= 1.5f;
                }).FirstOrDefault();

                if (player != null)
                {
                    entity.GetComponent<GroundItemComponent>().PickedUpBy = player.ID;
                }
            }

            if (entity.GetComponent<GroundItemComponent>().PickedUpBy != -1)
            {
                var pickedUpBy = ParentECS.GetEntityFromID(entity.GetComponent<GroundItemComponent>().PickedUpBy);
                var pickedUpByTransform = pickedUpBy.GetComponent<TransformComponent>();
                var middleOfPickedUpBy = pickedUpByTransform.Position + pickedUpBy.GetComponent<AnimatorComponent>().GetAnimator().GetCurrentAnimation().GetMiddleOfCurrentFrameScaled().ToCoordinateVector();

                var container = pickedUpBy.GetComponent<ContainerComponent>();
                if (container.GetContainer().AddItem(entity.GetComponent<GroundItemComponent>().Item))
                {
                    ParentECS.DestroyEntity(entity.ID);
                    this.GameServer.SendContainerContentsToViewers(pickedUpBy);
                }
                else
                {
                    entity.GetComponent<GroundItemComponent>().PickedUpBy = -1;
                }
            }
        }
    }
}