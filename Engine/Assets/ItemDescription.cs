using AGame.Engine.Assets.Scripting;
using AGame.Engine.Items;
using AGame.Engine.Networking;

namespace AGame.Engine.Assets;

public enum ItemType
{
    Tool,
    Consumable,
    Equipable,
    Placeable
}

public abstract class ItemDescription : Asset
{
    public string ItemName { get; set; }
    public string Texture { get; set; }
    public ItemType ItemType { get; set; }
    public int MaxStack { get; set; }

    public override bool InitOpenGL()
    {
        // Nothing needed
        return true;
    }

    public abstract Item CreateItem(GameClient gameClient);
}

public class ToolDescription : ItemDescription
{
    public int Durability { get; set; }
    public string OnUse { get; set; }
    public int Reach { get; set; }

    public override Item CreateItem(GameClient gameClient)
    {
        return new Tool(this.Name, ItemName, ModManager.GetAsset<Texture2D>(Texture), ItemType, MaxStack, Durability, (IUseTool)ScriptingManager.CreateInstance(OnUse), Reach, gameClient);
    }
}

public class ConsumableDescription : ItemDescription
{
    public string OnConsume { get; set; }
    public bool ConsumesOnUse { get; set; }

    public override Item CreateItem(GameClient gameClient)
    {
        return new Consumable(this.Name, ItemName, ModManager.GetAsset<Texture2D>(Texture), ItemType, MaxStack, OnConsume, ConsumesOnUse, gameClient);
    }
}

public class EquipableDescription : ItemDescription
{
    public string Effect { get; set; }

    public override Item CreateItem(GameClient gameClient)
    {
        return new Equipable(this.Name, ItemName, ModManager.GetAsset<Texture2D>(Texture), ItemType, MaxStack, Effect, gameClient);
    }
}

public class PlaceableDescription : ItemDescription
{
    public string PlaceEntity { get; set; }
    public bool ConsumesOnPlace { get; set; }

    public override Item CreateItem(GameClient gameClient)
    {
        return new Placeable(this.Name, ItemName, ModManager.GetAsset<Texture2D>(Texture), ItemType, MaxStack, PlaceEntity, ConsumesOnPlace, gameClient);
    }
}