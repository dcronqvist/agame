using System.Collections.Generic;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.World;

namespace DefaultMod;

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

            if (container.GetContainer().Provider.ShouldSendProviderData())
            {
                var packet = container.GetContainer().Provider.GetContainerProviderData(entity.ID);
                this.GameServer.SendContainerProviderDataToViewers(packet, entity);
            }
        }
    }
}