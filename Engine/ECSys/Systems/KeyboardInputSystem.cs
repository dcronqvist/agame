using AGame.Engine.ECSys.Components;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Server)]
public class KeyboardInputSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<KeyboardInputComponent>();
    }

    public override void AfterUpdate(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity e in entities)
        {
            KeyboardInputComponent playerInput = e.GetComponent<KeyboardInputComponent>();
            playerInput.PreviousKeyBitmask = playerInput.KeyBitmask;
        }
    }

    public override void BeforeUpdate(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity e in entities)
        {
            KeyboardInputComponent playerInput = e.GetComponent<KeyboardInputComponent>();
            playerInput.KeyBitmask = playerInput.NewBitmask;
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