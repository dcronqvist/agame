using System;
using System.Collections.Generic;
using AGame.Engine;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Items;
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

        public override int Populate(byte[] data, int offset)
        {
            return 0;
        }

        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }
}