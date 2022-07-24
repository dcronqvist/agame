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

namespace DefaultMod
{
    [ItemComponentProps(TypeName = "default.item_component.tool")]
    public class ToolDef : ItemComponentDefinition
    {
        public int Durability { get; set; }
        public string ScriptOnUse { get; set; }

        public override ItemComponent CreateComponent()
        {
            return new Tool(this);
        }
    }

    public class Tool : ItemComponent<ToolDef>
    {
        public int CurrentDurability { get; set; }

        public Tool(ToolDef definition) : base(definition)
        {
            this.CurrentDurability = definition.Durability;
        }

        public override int Populate(byte[] data, int offset)
        {
            int start = offset;
            this.CurrentDurability = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            return offset - start;
        }

        public override byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(this.CurrentDurability));
            return bytes.ToArray();
        }

        public override bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed)
        {
            if (totalTimeUsed > 1f)
            {
                if (!userCommand.HasBeenRun)
                {
                    this.CurrentDurability -= 20;
                    item.CreateEntity(playerEntity, ecs, "default.entity.test_rock", (entity) =>
                    {
                        Vector2i tilepos = playerEntity.GetComponent<TransformComponent>().Position.ToTileAligned();
                        entity.GetComponent<TransformComponent>().Position = new CoordinateVector(tilepos.X, tilepos.Y);
                    });
                }
                return false;
            }

            return true;
        }

        public override bool ShouldItemBeConsumed()
        {
            return this.CurrentDurability <= 0;
        }

        public override void OnConsumed(Entity playerEntity, ItemInstance item, ECS ecs)
        {
            Logging.Log(LogLevel.Debug, "Tool was consumed");
        }

        public override ulong GetHash()
        {
            return this.CurrentDurability.Hash();
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
}