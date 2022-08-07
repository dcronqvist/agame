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

namespace DefaultMod
{
    [ItemComponentProps(TypeName = "default.item_component.tool")]
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

        public override bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed)
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

                                    ScriptingAPI.CreateEntity(playerEntity, ecs, "default.entity.ground_item", (entity) =>
                                    {
                                        entity.GetComponent<TransformComponent>().Position = new CoordinateVector(userCommand.MouseTileX, userCommand.MouseTileY);
                                        entity.GetComponent<GroundItemComponent>().Item = ItemManager.GetItemDef(newItem).CreateItem();
                                    });
                                }

                                return false;
                            }
                        }

                        return true;
                    }
                }

                return false;
            }
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
    }

    [ItemComponentProps(TypeName = "default.item_component.resource")]
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

        public override ulong GetHash()
        {
            return Utilities.Hash(this.Definition.Quality);
        }

        public override void OnConsumed(Entity playerEntity, ItemInstance item, ECS ecs)
        {
            // Do nothing
            Logging.Log(LogLevel.Debug, "Resource was consumed");
        }

        public override bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed)
        {
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

    [ItemComponentProps(TypeName = "default.item_component.placeable")]
    public class PlaceableDef : ItemComponentDefinition
    {
        public string EntityToPlace { get; set; }

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

        public override ulong GetHash()
        {
            return Utilities.Hash(this.Definition.EntityToPlace);
        }

        public override void OnConsumed(Entity playerEntity, ItemInstance item, ECS ecs)
        {

        }

        public override bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed)
        {
            Entity entity = ScriptingAPI.GetEntityAtPosition(ecs, new Vector2i(userCommand.MouseTileX, userCommand.MouseTileY));

            if (entity is null)
            {
                // Can place entity
                ScriptingAPI.CreateEntity(playerEntity, ecs, this.Definition.EntityToPlace, (entity) =>
                {
                    entity.GetComponent<TransformComponent>().Position = new CoordinateVector(userCommand.MouseTileX, userCommand.MouseTileY);
                });
            }

            return false;
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
    }

    [ItemComponentProps(TypeName = "default.item_component.rock_crusher_yield")]
    public class RockCrusherYieldDef : ItemComponentDefinition
    {
        public string Item { get; set; }
        public int Amount { get; set; }

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

        public override ulong GetHash()
        {
            return Utilities.CombineHash(Utilities.Hash(this.Definition.Item), Utilities.Hash(this.Definition.Amount));
        }

        public override void OnConsumed(Entity playerEntity, ItemInstance item, ECS ecs)
        {

        }

        public override bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed)
        {
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