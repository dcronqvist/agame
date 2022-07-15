using AGame.Engine.Configuration;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client)]
public class PlayerMovementSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<PlayerPositionComponent>();
        this.RegisterComponentType<AnimatorComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        foreach (Entity entity in entities)
        {
            PlayerPositionComponent ppc = entity.GetComponent<PlayerPositionComponent>();
            AnimatorComponent ac = entity.GetComponent<AnimatorComponent>();

            if (ppc.Velocity.Length() > 1f)
            {

                if (ppc.Velocity.X > 0f)
                {
                    ac.GetAnimator().SetNextAnimation("run_right");
                }
                else
                {
                    ac.GetAnimator().SetNextAnimation("run_left");
                }
            }
            else
            {
                ac.GetAnimator().SetNextAnimation("idle");
            }
        }
    }

    public override void Render(List<Entity> entities, WorldContainer gameWorld)
    {
        // foreach (Entity entity in entities)
        // {
        //     PlayerPositionComponent ppc = entity.GetComponent<PlayerPositionComponent>();
        //     CoordinateVector velocity = ppc.Velocity;

        //     CoordinateVector start = ppc.Position;
        //     CoordinateVector end = ppc.Position + velocity * 0.4f;

        //     Renderer.Primitive.RenderLine(start.ToWorldVector().ToVector2(), end.ToWorldVector().ToVector2(), 2, ColorF.White);
        // }
    }
}