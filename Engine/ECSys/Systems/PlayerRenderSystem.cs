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
        this.RegisterComponentType<ColorComponent>();
    }

    public override void Render(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity entity in entities)
        {
            PlayerPositionComponent ppc = entity.GetComponent<PlayerPositionComponent>();
            CoordinateVector pos = ppc.Position;

            ColorComponent cc = entity.GetComponent<ColorComponent>();
            Renderer.Primitive.RenderCircle(pos.ToWorldVector().ToVector2(), 50f, cc.Color, false);
        }
    }
}