using System.Collections.Generic;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client | SystemRunner.Server)]
public class CollisionSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<TransformComponent>();
        this.RegisterComponentType<ColliderComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        foreach (Entity entity in entities)
        {
            var transform = entity.GetComponent<TransformComponent>();
            var collider = entity.GetComponent<ColliderComponent>();

            WorldVector position = transform.Position.ToWorldVector();
            collider.UpdateBox(position);
        }
    }
}