using System.Collections.Generic;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;
using AGame.Engine.Assets.Scripting;

namespace DefaultMod;

[SystemRunsOn(SystemRunner.Client), ScriptType(Name = "character_animation_system")]
public class CharacterAnimationSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<TransformComponent>();
        this.RegisterComponentType<AnimatorComponent>();
        this.RegisterComponentType<CharacterComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        foreach (Entity entity in entities)
        {
            TransformComponent ppc = entity.GetComponent<TransformComponent>();
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
}