using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.World;
using GameUDPProtocol;

namespace AGame.Engine.ECSys.Systems;

[SystemRunsOn(SystemRunner.Client)]
public class RenderSystem : BaseSystem
{
    public override void Initialize()
    {
        this.RegisterComponentType<TransformComponent>();
        this.RegisterComponentType<RenderComponent>();
    }

    public override void Render(List<Entity> entities, WorldContainer gameWorld)
    {
        // Different rendering passes
        // 1. Find all different render layers
        byte[] layers = entities.Select(e => e.GetComponent<RenderComponent>().RenderLayer).Distinct().ToArray();

        // sort layers in descending order, so that the lowest layer is rendered first
        layers = layers.OrderByDescending(l => l).ToArray();

        // 2. For each render layer, sort entities by their y position if sortByY is true, and render those

        foreach (byte layer in layers)
        {
            List<Entity> layerEntities = entities.Where(e => e.GetComponent<RenderComponent>().RenderLayer == layer).ToList();

            // Get all those with sortByY set to true
            List<Entity> sortedEntities = layerEntities.Where(e => e.GetComponent<RenderComponent>().SortByY).ToList();

            // Sort by y position
            sortedEntities = sortedEntities.OrderBy(e => (e.GetComponent<TransformComponent>().Position.ToWorldVector().ToVector2() + e.GetComponent<RenderComponent>().FeetOffset).Y).ToList();

            // Render all entities with sortByY set to true
            foreach (Entity entity in sortedEntities)
            {
                RenderEntity(entity);
            }

            // Get all those with sortByY set to false
            List<Entity> unsortedEntities = layerEntities.Where(e => !e.GetComponent<RenderComponent>().SortByY).ToList();

            // Render all entities with sortByY set to false
            foreach (Entity entity in unsortedEntities)
            {
                RenderEntity(entity);
            }
        }
    }

    private void RenderEntity(Entity entity)
    {
        var render = entity.GetComponent<RenderComponent>();
        var transform = entity.GetComponent<TransformComponent>();

        if (entity.HasComponent<AnimatorComponent>())
        {
            // Animator entity
            var animator = entity.GetComponent<AnimatorComponent>();
            animator.GetAnimator().Render(transform.Position.ToWorldVector().ToVector2(), ColorF.White);
        }

        if (entity.HasComponent<CharacterComponent>() && entity.ID != this.GameClient.GetPlayerEntity().ID)
        {
            // Render name above entity
            var character = entity.GetComponent<CharacterComponent>();
            var font = ModManager.GetAsset<Font>("default.font.rainyhearts");
            var name = character.Name;
            var scale = 1f;
            var nameSize = font.MeasureString(name, scale);
            var renderOffset = character.NameRenderOffset;

            var namePosition = transform.Position.ToWorldVector().ToVector2() + renderOffset;

            Renderer.Text.RenderText(font, name, namePosition - nameSize / 2f, scale, ColorF.Black, Renderer.Camera);
            Renderer.Text.RenderText(font, name, namePosition - nameSize / 2f - Vector2.One * 1, scale, ColorF.White, Renderer.Camera);
        }

        if (entity.HasComponent<PlayerStateComponent>())
        {
            var state = entity.GetComponent<PlayerStateComponent>();

            if (state.HoldingItem != "")
            {
                // var item = ItemManager.GetItem(state.HoldingItem);
                // if (state.HoldingUseItem)
                // {
                //     Renderer.Text.RenderText(ModManager.GetAsset<Font>("default.font.rainyhearts"), "using...", transform.Position.ToWorldVector().ToVector2(), 1f, ColorF.DeepBlue, Renderer.Camera);
                //     item.OnHoldLeftClickRender(entity, new Vector2i(state.MouseTileX, state.MouseTileY), ParentECS, state.ItemUsedTime);
                // }
            }

            // var collider = entity.GetComponent<ColliderComponent>();

            // Renderer.Primitive.RenderRectangle(collider.Box, ColorF.Blue * 0.3f);
        }

        if (entity.HasComponent<SpriteComponent>())
        {
            var sprite = entity.GetComponent<SpriteComponent>();
            sprite.GetSprite().Render(transform.Position.ToWorldVector().ToVector2());
        }

        if (entity.HasComponent<GroundItemComponent>())
        {
            var itemComponent = entity.GetComponent<GroundItemComponent>();
            var item = itemComponent.Item;

            Renderer.Texture.Render(item.GetTexture(), transform.Position.ToWorldVector().ToVector2(), Vector2.One * 2f, 0f, ColorF.White);
        }

        if (entity.HasComponent<InteractableComponent>() && entity.TryGetComponent<ColliderComponent>(out var c))
        {
            // var interactableCollider = entity.GetComponent<ColliderComponent>();
            // var interactable = entity.GetComponent<InteractableComponent>();
            // var interactBox = interactableCollider.Box.Inflate(interactable.InteractDistance * TileGrid.TILE_SIZE);

            // Renderer.Primitive.RenderRectangle(interactBox, ColorF.Green * 0.3f);
        }
    }
}