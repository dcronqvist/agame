using System;
using System.Collections.Generic;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.Items;
using AGame.Engine.Networking;
using AGame.Engine.World;
using AGame.Engine.Assets;
using System.Numerics;
using System.Drawing;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Graphics;
using System.Linq;

namespace DefaultMod
{
    [ScriptType(Name = "tool_def")]
    public class ToolDef : ItemComponentDefinition
    {
        public int MaxEnergyCharge { get; set; }

        public override ItemComponent CreateComponent()
        {
            return new Tool(this);
        }
    }

    public class Tool : ItemComponent<ToolDef>
    {
        public int CurrentEnergyCharge { get; set; }

        public Tool(ToolDef definition) : base(definition)
        {
            this.CurrentEnergyCharge = definition.MaxEnergyCharge;
        }

        public override int Populate(byte[] data, int offset)
        {
            int start = offset;
            this.CurrentEnergyCharge = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            return offset - start;
        }

        public override byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(this.CurrentEnergyCharge));
            return bytes.ToArray();
        }

        public override bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed, out bool resetUseTime)
        {
            Entity entity = ScriptingAPI.GetEntityAtPosition(ecs, new Vector2i(userCommand.MouseTileX, userCommand.MouseTileY));

            if (entity.TryGetComponent<HarvestableComponent>(out var harvest))
            {
                if (totalTimeUsed > 1f)
                {
                    if (!userCommand.HasBeenRun)
                    {
                        this.CurrentEnergyCharge -= 1;

                        harvest.BreaksAfter -= 1;

                        if (harvest.BreaksAfter < 1)
                        {
                            ScriptingAPI.DestroyEntity(playerEntity, ecs, entity);
                        }

                        foreach (var def in harvest.Yields)
                        {
                            int amount = Utilities.GetRandomInt(def.MinAmount, def.MaxAmount);
                            string newItem = def.Item;

                            foreach (var i in Enumerable.Range(0, amount))
                            {
                                ScriptingAPI.CreateEntity(playerEntity, ecs, "default.entity.ground_item", onCreateClientSide: null, (entity) =>
                                {
                                    entity.GetComponent<TransformComponent>().Position = new CoordinateVector(userCommand.MouseTileX, userCommand.MouseTileY);
                                    entity.GetComponent<GroundItemComponent>().Item = ItemManager.GetItemDef(newItem).CreateItem();
                                    entity.GetComponent<BouncingComponent>().VerticalVelocity = Utilities.GetRandomFloat(-200f, -100f);
                                    entity.GetComponent<BouncingComponent>().Velocity = Vector2.Normalize(Utilities.GetRandomVector2(-1f, 1f, -1f, 1f)) * Utilities.GetRandomFloat(0.1f, 1f);
                                });
                            }
                        }

                        ScriptingAPI.PlayAudioFromPlayerAction(playerEntity, ecs, userCommand, harvest.HarvestSound);

                        resetUseTime = true;
                        return true;
                    }
                }
            }

            resetUseTime = false;
            return false;
        }

        public override bool ShouldItemBeConsumed()
        {
            return false;
        }

        public override void OnConsumed(Entity playerEntity, ItemInstance item, ECS ecs)
        {
            Logging.Log(LogLevel.Debug, "Tool was consumed");
        }

        public override ulong GetHash()
        {
            return this.CurrentEnergyCharge.Hash();
        }

        public override bool CanBeUsed(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs)
        {
            Entity entity = ScriptingAPI.GetEntityAtPosition(ecs, new Vector2i(userCommand.MouseTileX, userCommand.MouseTileY));

            if (entity is null || this.CurrentEnergyCharge <= 0)
            {
                return false;
            }
            else
            {
                if (entity.TryGetComponent<HarvestableComponent>(out var harvest))
                {
                    if (harvest.HasTag("rock"))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public override void RenderInSlot(ItemInstance item, Vector2 slotTopLeft)
        {
            var durability = this.Definition.MaxEnergyCharge;
            var currDur = this.CurrentEnergyCharge;
            var perc = ((float)currDur / durability).ToString("0.00");

            var font = ModManager.GetAsset<Font>("default.font.rainyhearts");
            var scale = 1f;
            var size = new Vector2(ContainerSlot.WIDTH, ContainerSlot.HEIGHT);

            var durabilitySize = font.MeasureString(perc, scale);

            var textSize = font.MeasureString(perc, scale);
            var durabilityPosition = slotTopLeft + new Vector2(size.X - durabilitySize.X, size.Y - durabilitySize.Y - textSize.Y * 2);
            Renderer.Text.RenderText(font, perc, durabilityPosition.PixelAlign(), scale, ColorF.White, Renderer.Camera);
        }
    }

    [ScriptType(Name = "resource_def")]
    public class ResourceDef : ItemComponentDefinition
    {
        public int Quality { get; set; }

        public override ItemComponent CreateComponent()
        {
            return new Resource(this);
        }
    }

    public class Resource : ItemComponent<ResourceDef>
    {
        public Resource(ResourceDef definition) : base(definition)
        {

        }

        public override bool CanBeUsed(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs)
        {
            return false;
        }

        public override ulong GetHash()
        {
            return Utilities.Hash(this.Definition.Quality);
        }

        public override void OnConsumed(Entity playerEntity, ItemInstance item, ECS ecs)
        {
            // Do nothing
            Logging.Log(LogLevel.Debug, "Resource was consumed");
        }

        public override bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed, out bool resetUseTime)
        {
            resetUseTime = false;
            return false;
        }

        public override int Populate(byte[] data, int offset)
        {
            return 0;
        }

        public override bool ShouldItemBeConsumed()
        {
            return false;
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }

    [ScriptType(Name = "placeable_def")]
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

                ScriptingAPI.PlayAudioFromPlayerAction(playerEntity, ecs, userCommand, "default.audio.click");
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

    [ScriptType(Name = "rock_crusher_yield_def")]
    public class RockCrusherYieldDef : ItemComponentDefinition
    {
        public string Item { get; set; }
        public int Amount { get; set; }
        public float TimeToCrush { get; set; }

        public override ItemComponent CreateComponent()
        {
            return new RockCrusherYield(this);
        }
    }

    public class RockCrusherYield : ItemComponent<RockCrusherYieldDef>
    {
        public RockCrusherYield(RockCrusherYieldDef definition) : base(definition)
        {

        }

        public override bool CanBeUsed(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs)
        {
            return false;
        }

        public override ulong GetHash()
        {
            return Utilities.CombineHash(Utilities.Hash(this.Definition.Item), Utilities.Hash(this.Definition.Amount), this.Definition.TimeToCrush.Hash());
        }

        public override void OnConsumed(Entity playerEntity, ItemInstance item, ECS ecs)
        {

        }

        public override bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed, out bool resetUseTime)
        {
            resetUseTime = false;
            return false;
        }

        public override int Populate(byte[] data, int offset)
        {
            return 0;
        }

        public override bool ShouldItemBeConsumed()
        {
            return false;
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }
}