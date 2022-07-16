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
    }

    public override void Render(List<Entity> entities, WorldContainer gameWorld)
    {
        // All entities with animators
        List<Entity> animators = entities.FindAll(e => e.HasComponent<AnimatorComponent>());

        // All found entities
        List<Entity> foundEntities = animators;

        foreach (Entity e in foundEntities.OrderBy(e => e.GetComponent<TransformComponent>().Position.Y))
        {
            var transform = e.GetComponent<TransformComponent>();

            if (e.HasComponent<AnimatorComponent>())
            {
                // Animator entity
                var animator = e.GetComponent<AnimatorComponent>();
                animator.GetAnimator().Render(transform.Position.ToWorldVector().ToVector2(), ColorF.White);
            }

            if (e.HasComponent<CharacterComponent>() && e.ID != this.GameClient.GetPlayerEntity().ID)
            {
                // Render name above entity
                var character = e.GetComponent<CharacterComponent>();
                var font = ModManager.GetAsset<Font>("default.font.rainyhearts");
                var name = character.Name;
                var scale = 1f;
                var nameSize = font.MeasureString(name, scale);
                var renderOffset = character.NameRenderOffset;

                var namePosition = transform.Position.ToWorldVector().ToVector2() + renderOffset;

                Renderer.Text.RenderText(font, name, namePosition - nameSize / 2f, scale, ColorF.Black, Renderer.Camera);
                Renderer.Text.RenderText(font, name, namePosition - nameSize / 2f - Vector2.One * 1, scale, ColorF.White, Renderer.Camera);
            }
        }
    }
}