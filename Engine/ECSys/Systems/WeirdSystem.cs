using System.Numerics;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Server)]
public class WeirdSystem : BaseSystem
{
    public override void BeforeUpdate(List<Entity> entities, WorldContainer gameWorld)
    {
        base.BeforeUpdate(entities, gameWorld);
    }

    float interval = 0.1f;
    float counter = 0f;

    public override void Update(List<Entity> entities, WorldContainer gameWorld)
    {
        if (counter >= interval)
        {
            Entity n = this.ParentECS.CreateEntityFromAsset("entity_weird");

            WeirdComponent wc = n.GetComponent<WeirdComponent>();

            wc.Direction = MathF.PI * 2f;

            counter = 0f;
        }

        counter += GameTime.DeltaTime;

        foreach (Entity e in entities)
        {
            TransformComponent tc = e.GetComponent<TransformComponent>();
            WeirdComponent wc = e.GetComponent<WeirdComponent>();

            Vector2 move = new Vector2(MathF.Sin(wc.Direction), MathF.Cos(wc.Direction));

            tc.Position += move * 20f * GameTime.DeltaTime;

            if (wc.TimeAlive > 10f)
            {
                this.ParentECS.DestroyEntity(e.ID);
            }

            wc.TimeAlive += GameTime.DeltaTime;
        }
    }

    public override void AfterUpdate(List<Entity> entities, WorldContainer gameWorld)
    {
        base.AfterUpdate(entities, gameWorld);
    }

    public override void Initialize()
    {
        this.RegisterComponentType<WeirdComponent>();
        this.RegisterComponentType<TransformComponent>();
        this.RegisterComponentType<SpriteComponent>();
    }

    public override void Render(List<Entity> entity, WorldContainer gameWorld)
    {
        foreach (var e in entity)
        {
            var sprite = e.GetComponent<SpriteComponent>();
            var transform = e.GetComponent<TransformComponent>();
            WeirdComponent wc = e.GetComponent<WeirdComponent>();

            sprite.Sprite.ColorTint = ColorF.White * (1f - (wc.TimeAlive / 5f));

            sprite.Sprite.Render(transform.Position);
        }
    }
}