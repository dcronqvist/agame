using AGame.Engine.Assets;
using AGame.Engine.ECSys.Components;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client)]
public class AudioSystem : BaseSystem
{
    public override void AfterUpdate(List<Entity> entities, WorldContainer gameWorld)
    {
        base.AfterUpdate(entities, gameWorld);
    }

    public override void BeforeUpdate(List<Entity> entities, WorldContainer gameWorld)
    {
        base.BeforeUpdate(entities, gameWorld);
    }

    public override void Initialize()
    {
        base.RegisterComponentType<AudioComponent>();
        base.RegisterComponentType<TransformComponent>();
    }

    public override void Render(List<Entity> entity, WorldContainer gameWorld)
    {
        base.Render(entity, gameWorld);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity e in entities)
        {
            AudioComponent audioComponent = e.GetComponent<AudioComponent>();
            if (audioComponent.HasAudio())
            {
                string audio = audioComponent.DequeueAudio();
                Audio asset = AssetManager.GetAsset<Audio>(audio);
                TransformComponent tc = e.GetComponent<TransformComponent>();
                asset.Play(tc.Position, refDistance: 1f, maxDistance: 100f);
            }
        }
    }
}