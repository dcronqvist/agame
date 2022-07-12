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
        this.RegisterComponentType<AnimatorComponent>();
    }

    public override void Render(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity entity in entities.OrderBy(e => e.GetComponent<PlayerPositionComponent>().Position.Y))
        {
            PlayerPositionComponent ppc = entity.GetComponent<PlayerPositionComponent>();
            CoordinateVector pos = ppc.Position;

            AnimatorComponent pac = entity.GetComponent<AnimatorComponent>();
            //Vector2 middleOfRec = pac.GetAnimator().GetCurrentAnimation();

            pac.GetAnimator().Render(pos.ToWorldVector().ToVector2());
        }
    }
}