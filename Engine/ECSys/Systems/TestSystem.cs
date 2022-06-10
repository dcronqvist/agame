using System.Numerics;
using AGame.Engine.ECSys.Components;

namespace AGame.Engine.ECSys.Systems;

public class TestSystem : BaseSystem
{
    float counter = 0f;
    float interval = 5f;

    public override void Initialize()
    {
        this.RegisterComponentType<SpriteComponent>();
        this.RegisterComponentType<TransformComponent>();
        this.RegisterComponentType<PlayerInputComponent>();
    }

    public override void Render(List<Entity> entity)
    {
        foreach (var e in entity)
        {
            var sprite = e.GetComponent<SpriteComponent>();
            var transform = e.GetComponent<TransformComponent>();

            sprite.Sprite.Render(transform.Position);
        }
    }

    public override void Update(List<Entity> entities)
    {
        // Every 2 seconds,
        // Change the sprite's texture
        if (counter >= interval)
        {
            foreach (var e in entities)
            {
                var sprite = e.GetComponent<SpriteComponent>();

                sprite.Texture = sprite.Texture == "tex_player" ? "tex_krobus" : "tex_player";
            }

            counter = 0f;
        }

        counter += GameTime.DeltaTime;

        foreach (Entity entity in entities)
        {
            TransformComponent transform = entity.GetComponent<TransformComponent>();
            PlayerInputComponent input = entity.GetComponent<PlayerInputComponent>();

            Vector2 movement = new Vector2(0, 0);
            float speed = 150f;

            if (input.IsKeyDown(PlayerInputComponent.KEY_W))
            {
                movement += new Vector2(0, -1);
            }
            if (input.IsKeyDown(PlayerInputComponent.KEY_A))
            {
                movement += new Vector2(-1, 0);
            }
            if (input.IsKeyDown(PlayerInputComponent.KEY_S))
            {
                movement += new Vector2(0, 1);
            }
            if (input.IsKeyDown(PlayerInputComponent.KEY_D))
            {
                movement += new Vector2(1, 0);
            }

            if (movement != Vector2.Zero)
            {
                transform.Position += Vector2.Normalize(movement) * speed * GameTime.DeltaTime;
            }
        }
    }
}