using System;
using System.Collections.Generic;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.World;
using AGame.Engine.Assets;
using System.Numerics;
using System.Drawing;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Graphics;

namespace ModTemplate
{
    [ScriptClass(Name = "placeable_def")]
    public class PlaceableDef : ItemComponentDefinition
    {
        public string EntityToPlace { get; set; }
        public Vector2 PlaceOffset { get; set; }

        public override ItemComponent CreateComponent()
        {
            return new Placeable(this);
        }
    }

    public class Placeable : ItemComponent<PlaceableDef>
    {
        public Placeable(PlaceableDef definition) : base(definition)
        {

        }

        public override bool CanBeUsed(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs)
        {
            Entity entity = ScriptingAPI.GetEntityAtPosition(ecs, new Vector2i(userCommand.MouseTileX + (int)this.Definition.PlaceOffset.X, userCommand.MouseTileY + (int)this.Definition.PlaceOffset.Y));

            if (entity is null)
            {
                return true;
            }

            return false;
        }

        public override ulong GetHash()
        {
            return Utilities.Hash(this.Definition.EntityToPlace);
        }

        public override void OnConsumed(Entity playerEntity, ItemInstance item, ECS ecs)
        {

        }

        public override bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed, out bool resetUseTime)
        {
            Entity entity = ScriptingAPI.GetEntityAtPosition(ecs, new Vector2i(userCommand.MouseTileX + (int)this.Definition.PlaceOffset.X, userCommand.MouseTileY + (int)this.Definition.PlaceOffset.Y));

            if (entity is null)
            {
                // Can place entity
                ScriptingAPI.CreateEntity(playerEntity, ecs, this.Definition.EntityToPlace, (entity) =>
                {
                    entity.GetComponent<TransformComponent>().Position = new CoordinateVector(userCommand.MouseTileX + (int)this.Definition.PlaceOffset.X, userCommand.MouseTileY + (int)this.Definition.PlaceOffset.Y);
                });

                ScriptingAPI.PlayAudioFromPlayerAction(playerEntity, ecs, userCommand, "default.audio.test_sound");
            }

            resetUseTime = true;
            return true;
        }

        public override int Populate(byte[] data, int offset)
        {
            return 0;
        }

        public override bool ShouldItemBeConsumed()
        {
            return true;
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }

        public override void OnHoldRender(Entity playerEntity, ItemInstance item, ECS ecs, float deltaTime)
        {
            var playerState = playerEntity.GetComponent<PlayerStateComponent>();
            var pos = new Vector2i(playerState.MouseTileX + (int)this.Definition.PlaceOffset.X, playerState.MouseTileY + (int)this.Definition.PlaceOffset.Y);
            var texture = item.GetTexture();

            var worldPos = new Vector2(pos.X * TileGrid.TILE_SIZE, pos.Y * TileGrid.TILE_SIZE);

            Renderer.Texture.Render(texture, worldPos, Vector2.One * 2f, 0f, ColorF.Green * 0.2f, TextureRenderEffects.None);
        }
    }
}