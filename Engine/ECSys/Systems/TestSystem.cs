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
        this.RegisterComponentType<TransformComponent>();
        this.RegisterComponentType<KeyboardInputComponent>();
        this.RegisterComponentType<MouseInputComponent>();
    }

    public override void Render(List<Entity> entities, WorldContainer gameWorld)
    {
        foreach (Entity e in entities)
        {
            //Renderer.Primitive.RenderCircle(e.GetComponent<TransformComponent>().Position.ToWorldVector().ToVector2(), 5f, ColorF.Blue, false);
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
            Vector2i tilePos = transform.Position.ToTileAligned();
            float speed = 5f;

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
                if (keyboard.IsKeyDown(KeyboardInputComponent.KEY_SHIFT))
                {
                    gameWorld.UpdateTile((int)tilePos.X, (int)tilePos.Y, "game:grass");
                }
                else
                {
                    Entity e = base.ParentECS.CreateEntityFromAsset("entity_weird");
                    e.GetComponent<TransformComponent>().Position = transform.Position;
                }
            }
        }
    }
}