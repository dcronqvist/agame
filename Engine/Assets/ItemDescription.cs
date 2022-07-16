using AGame.Engine.Items;

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

    public abstract Item CreateItem();
}

public class ToolDescription : ItemDescription
{
    public int Durability { get; set; }
    public string OnUse { get; set; }

    public override Item CreateItem()
    {
        return new Tool(ItemName, ModManager.GetAsset<Texture2D>(Texture), ItemType, MaxStack, Durability, OnUse);
    }
}

public class ConsumableDescription : ItemDescription
{
    public string OnConsume { get; set; }
    public bool ConsumesOnUse { get; set; }

    public override Item CreateItem()
    {
        return new Consumable(ItemName, ModManager.GetAsset<Texture2D>(Texture), ItemType, MaxStack, OnConsume, ConsumesOnUse);
    }
}

public class EquipableDescription : ItemDescription
{
    public string Effect { get; set; }

    public override Item CreateItem()
    {
        return new Equipable(ItemName, ModManager.GetAsset<Texture2D>(Texture), ItemType, MaxStack, Effect);
    }
}

public class PlaceableDescription : ItemDescription
{
    public string PlaceEntity { get; set; }
    public bool ConsumesOnPlace { get; set; }

    public override Item CreateItem()
    {
        return new Placeable(ItemName, ModManager.GetAsset<Texture2D>(Texture), ItemType, MaxStack, PlaceEntity, ConsumesOnPlace);
    }
}