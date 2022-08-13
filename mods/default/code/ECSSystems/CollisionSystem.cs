using System.Collections.Generic;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;
using AGame.Engine.Assets.Scripting;

namespace DefaultMod;

[SystemRunsOn(SystemRunner.Client | SystemRunner.Server), ScriptType(Name = "collision_system")]
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