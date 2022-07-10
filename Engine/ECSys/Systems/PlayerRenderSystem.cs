using System.Numerics;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client)]
public class PlayerRenderSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<PlayerPositionComponent>();
        this.RegisterComponentType<SpriteComponent>();
    }

    public override void Render(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity entity in entities)
        {
            PlayerPositionComponent ppc = entity.GetComponent<PlayerPositionComponent>();
            CoordinateVector pos = ppc.Position;

            SpriteComponent cc = entity.GetComponent<SpriteComponent>();
            Vector2 middleOfRec = cc.GetSprite().MiddleOfSourceRectangle;

            cc.GetSprite().Render(pos.ToWorldVector().ToVector2() - middleOfRec * cc.RenderScale);
        }
    }
}