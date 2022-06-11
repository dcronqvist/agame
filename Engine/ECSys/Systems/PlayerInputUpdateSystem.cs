using AGame.Engine.ECSys.Components;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Server)]
public class PlayerInputUpdateSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<PlayerInputComponent>();
    }

    public override void AfterUpdate(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity e in entities)
        {
            PlayerInputComponent playerInput = e.GetComponent<PlayerInputComponent>();
            playerInput.PreviousKeyBitmask = playerInput.KeyBitmask;
        }
    }

    public override void BeforeUpdate(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity e in entities)
        {
            PlayerInputComponent playerInput = e.GetComponent<PlayerInputComponent>();
            playerInput.KeyBitmask = playerInput.NewBitmask;

            if (playerInput.NewBitmask != 0)
            {
                int x = 2;
            }
        }
    }

    public override void Render(List<Entity> entity, WorldContainer gameWorld)
    {
        base.Render(entity, gameWorld);
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld)
    {
        base.Update(entities, gameWorld);
    }
}