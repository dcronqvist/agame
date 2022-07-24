using System.Collections.Generic;
using AGame.Engine.Assets;
using AGame.Engine.Networking;

namespace AGame.Engine.Items;

public class ItemDefinition : Asset
{
    public string ItemID { get; set; }
    public string ItemName { get; set; }
    public int MaxStack { get; set; }
    public string Texture { get; set; }
    public List<ItemComponentDefinition> Definitions { get; set; }

    internal GameServer _gameServer;
    internal GameClient _gameClient;

    public ItemDefinition(string name, List<ItemComponentDefinition> definitions)
    {
        this.Name = name;
        this.Definitions = definitions;
    }

    public ItemInstance CreateItem()
    {
        List<ItemComponent> components = new List<ItemComponent>();

        foreach (ItemComponentDefinition def in Definitions)
        {
            components.Add(def.CreateComponent());
        }

        return new ItemInstance(this, components);
    }

    public override bool InitOpenGL()
    {
        // Don't need to do anything
        return true;
    }
}