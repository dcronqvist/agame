using AGame.Engine.Configuration;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client | SystemRunner.Server)]
public class PlayerMovementSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<PlayerPositionComponent>();
        this.RegisterComponentType<AnimatorComponent>();
    }

    private Dictionary<int, float> _timeIdle = new Dictionary<int, float>();

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        foreach (Entity entity in entities)
        {
            PlayerPositionComponent ppc = entity.GetComponent<PlayerPositionComponent>();
            AnimatorComponent ac = entity.GetComponent<AnimatorComponent>();

            if (!this._timeIdle.ContainsKey(entity.ID))
            {
                this._timeIdle.Add(entity.ID, 0);
            }

            if (ppc.Velocity.Length() > 1f)
            {
                this._timeIdle[entity.ID] = 0;
                ac.GetAnimator().SetNextState("walk");
            }
            else
            {
                this._timeIdle[entity.ID] += deltaTime;
                ac.GetAnimator().SetNextState("idle");
            }

            if (this._timeIdle[entity.ID] > 5f)
            {
                ac.GetAnimator().SetNextState("green");
                this._timeIdle[entity.ID] = 0;
            }
        }
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