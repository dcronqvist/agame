using System.Drawing;
using System.Numerics;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client)]
public class SpriteSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<SpriteComponent>();
        this.RegisterComponentType<TransformComponent>();
    }

    public override void Render(List<Entity> entity, WorldContainer gameWorld)
    {
        foreach (var e in entity)
        {
            var sprite = e.GetComponent<SpriteComponent>();
            var transform = e.GetComponent<TransformComponent>();

            Vector2 spriteSize = sprite.Sprite.MiddleOfSourceRectangle * sprite.RenderScale;
            sprite.Sprite.Render(transform.Position.ToWorldVector().ToVector2() - spriteSize);
        }
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld)
    {

    }
}