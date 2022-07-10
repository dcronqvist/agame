using AGame.Engine.Configuration;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client)]
public class MovementSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<PlayerPositionComponent>();
    }

    public override void Render(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity entity in entities)
        {
            PlayerPositionComponent ppc = entity.GetComponent<PlayerPositionComponent>();
            CoordinateVector velocity = ppc.Velocity;

            CoordinateVector start = ppc.Position;
            CoordinateVector end = ppc.Position + velocity * 0.2f;

            Renderer.Primitive.RenderLine(start.ToWorldVector().ToVector2(), end.ToWorldVector().ToVector2(), 2, ColorF.White);
        }
    }
}