using AGame.Engine.Assets;

namespace AGame.Engine.Items;

public abstract class Item
{
    public string ItemName { get; set; }
    public Texture2D Texture { get; set; }
    public ItemType ItemType { get; set; }
    public int MaxStack { get; set; }

    public Item(string itemName, Texture2D texture, ItemType itemType, int maxStack)
    {
        ItemName = itemName;
        Texture = texture;
        ItemType = itemType;
        MaxStack = maxStack;
    }
}

public class Tool : Item
{
    public int Durability { get; set; }
    public string OnUse { get; set; }

    public Tool(string itemName, Texture2D texture, ItemType itemType, int maxStack, int durability, string onUse) : base(itemName, texture, itemType, maxStack)
    {
        Durability = durability;
        OnUse = onUse;
    }
}

public class Consumable : Item
{
    public string OnConsume { get; set; }
    public bool ConsumesOnUse { get; set; }

    public Consumable(string itemName, Texture2D texture, ItemType itemType, int maxStack, string onConsume, bool consumesOnUse) : base(itemName, texture, itemType, maxStack)
    {
        OnConsume = onConsume;
        ConsumesOnUse = consumesOnUse;
    }
}

public class Equipable : Item
{
    public string Effect { get; set; }

    public Equipable(string itemName, Texture2D texture, ItemType itemType, int maxStack, string effect) : base(itemName, texture, itemType, maxStack)
    {
        Effect = effect;
    }
}

public class Placeable : Item
{
    public string PlaceEntity { get; set; }
    public bool ConsumesOnPlace { get; set; }

    public Placeable(string itemName, Texture2D texture, ItemType itemType, int maxStack, string placeEntity, bool consumesOnPlace) : base(itemName, texture, itemType, maxStack)
    {
        PlaceEntity = placeEntity;
        ConsumesOnPlace = consumesOnPlace;
    }
}