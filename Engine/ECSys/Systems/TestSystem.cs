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
        this.RegisterComponentType<PlayerInputComponent>();
    }

    public override void Render(List<Entity> entity, WorldContainer gameWorld)
    {
        foreach (var e in entity)
        {
            var sprite = e.GetComponent<SpriteComponent>();
            var transform = e.GetComponent<TransformComponent>();

            sprite.Sprite.Render(transform.Position);

            // Vector2i tilePos = transform.GetTilePosition();

            // RectangleF rect = new RectangleF(tilePos.X * TileGrid.TILE_SIZE, tilePos.Y * TileGrid.TILE_SIZE, TileGrid.TILE_SIZE, TileGrid.TILE_SIZE);

            // Renderer.Primitive.RenderRectangle(rect, ColorF.Red * 0.5f);

            // Vector2i chunkPos = transform.GetChunkPosition();

            // RectangleF rect2 = new RectangleF(chunkPos.X * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE, chunkPos.Y * Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE, Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE, Chunk.CHUNK_SIZE * TileGrid.TILE_SIZE);

            // Renderer.Primitive.RenderRectangle(rect2, ColorF.Green * 0.5f);
        }
    }

    public override void Update(List<Entity> entities, WorldContainer gameWorld)
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
            Vector2i tilePos = transform.GetTilePosition();
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

            if (input.IsKeyPressed(PlayerInputComponent.KEY_SPACE))
            {
                gameWorld.UpdateTile(tilePos.X, tilePos.Y, "game:grass");
            }
        }
    }
}