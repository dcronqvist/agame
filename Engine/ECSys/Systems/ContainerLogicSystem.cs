using System.Collections.Generic;
using AGame.Engine.ECSys.Components;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Server)]
public class ContainerLogicSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<ContainerComponent>();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld, float deltaTime)
    {
        foreach (var entity in entities)
        {
            var container = entity.GetComponent<ContainerComponent>();
            if (container.GetContainer().UpdateLogic(deltaTime))
            {
                // Container has had an update, so we need to send it to the clients that are viewing it
                this.GameServer.SendContainerContentsToViewers(entity);
            }
        }
    }
}