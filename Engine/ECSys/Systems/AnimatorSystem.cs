using AGame.Engine.Configuration;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client | SystemRunner.Server)]
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
            if (ac.GetAnimator().Update(deltaTime) && base.Runner == SystemRunner.Server)
            {
                base.GameServer.EnqueueBroadcastPacket(new AnimationStateChangePacket() { EntityID = entity.ID, NewState = ac.GetAnimator().CurrentState }, true, false);
            }
        }
    }
}

public class AnimationStateChangePacket : Packet
{
    public int EntityID { get; set; }
    public string NewState { get; set; }
}