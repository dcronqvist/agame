using AGame.Engine.Configuration;
using AGame.Engine.ECSys.Components;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Server)]
public class MovementSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<TransformComponent>();
        this.RegisterComponentType<MovementComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld)
    {
        ECS parent = this.ParentECS;

        foreach (Entity entity in entities)
        {
            TransformComponent transform = entity.GetComponent<TransformComponent>();
            MovementComponent movement = entity.GetComponent<MovementComponent>();

            transform.Position += movement.Movement.GetVelocity(entity) * GameTime.DeltaTime;
        }
    }
}