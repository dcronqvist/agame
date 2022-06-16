using System.Drawing;
using System.Numerics;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Server)]
public class TestSystem : BaseSystem
{
    float counter = 0f;
    float interval = 5f;

    public override void Initialize()
    {
        this.RegisterComponentType<SpriteComponent>();
        this.RegisterComponentType<TransformComponent>();
        this.RegisterComponentType<KeyboardInputComponent>();
        this.RegisterComponentType<MouseInputComponent>();
    }

    public override void Render(List<Entity> entity, WorldContainer gameWorld)
    {
        foreach (var e in entity)
        {
            var sprite = e.GetComponent<SpriteComponent>();
            var transform = e.GetComponent<TransformComponent>();
            var mouse = e.GetComponent<MouseInputComponent>();

            Vector2 spriteSize = sprite.Sprite.Texture.Middle;
            sprite.Sprite.Render(transform.Position - spriteSize / 2f);
        }
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity entity in entities)
        {
            TransformComponent transform = entity.GetComponent<TransformComponent>();
            KeyboardInputComponent keyboard = entity.GetComponent<KeyboardInputComponent>();
            MouseInputComponent mouse = entity.GetComponent<MouseInputComponent>();

            Vector2 movement = new Vector2(0, 0);
            Vector2i tilePos = transform.GetTilePosition();
            float speed = 150f;

            if (keyboard.IsKeyDown(KeyboardInputComponent.KEY_W))
            {
                movement += new Vector2(0, -1);
            }
            if (keyboard.IsKeyDown(KeyboardInputComponent.KEY_A))
            {
                movement += new Vector2(-1, 0);
            }
            if (keyboard.IsKeyDown(KeyboardInputComponent.KEY_S))
            {
                movement += new Vector2(0, 1);
            }
            if (keyboard.IsKeyDown(KeyboardInputComponent.KEY_D))
            {
                movement += new Vector2(1, 0);
            }

            if (movement != Vector2.Zero)
            {
                transform.Position += Vector2.Normalize(movement) * speed * GameTime.DeltaTime;
            }

            if (keyboard.IsKeyPressed(KeyboardInputComponent.KEY_SPACE))
            {
                gameWorld.UpdateTile(tilePos.X, tilePos.Y, "game:grass");
            }
        }
    }
}