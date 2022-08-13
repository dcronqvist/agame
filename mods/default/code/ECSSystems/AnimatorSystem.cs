using System.Collections.Generic;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.World;
using GameUDPProtocol;
using AGame.Engine.Assets.Scripting;

namespace DefaultMod;

[SystemRunsOn(SystemRunner.Client), ScriptType(Name = "animator_system")]
public class AnimatorSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<AnimatorComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        ECS parent = this.ParentECS;

        foreach (Entity entity in entities)
        {
            AnimatorComponent ac = entity.GetComponent<AnimatorComponent>();
            ac.GetAnimator().Update(deltaTime);
        }
    }
}