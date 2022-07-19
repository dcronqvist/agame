using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.Items;

public abstract class Item
{
    public string ItemID { get; set; }
    public string ItemName { get; set; }
    public Texture2D Texture { get; set; }
    public ItemType ItemType { get; set; }
    public int MaxStack { get; set; }

    // References to game client to perform actions on the world etc.
    private GameClient _gameClient;

    public Item(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, GameClient gameClient)
    {
        this.ItemID = itemID;
        ItemName = itemName;
        Texture = texture;
        ItemType = itemType;
        MaxStack = maxStack;
        _gameClient = gameClient;
    }

    public virtual void OnHoldLeftClick(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs, float deltaTime) { }
    public virtual void OnReleaseLeftClick(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs) { }

    public void PlaceEntity(Entity placingEntity, string assetName, Vector2i tileAlignedPos)
    {
        if (placingEntity.ID == _gameClient.GetPlayerEntity().ID)
        {
            // Only if the placing entity is the local player is it allowed to place entities
            _gameClient?.PlaceEntity(assetName, tileAlignedPos);
        }
    }

    public virtual void OnHoldLeftClickRender(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs) { }
    public virtual void OnReleaseLeftClickRender(Entity playerEntity, Vector2i mouseWorldPosition, ECS ecs) { }
}

public class Consumable : Item
{
    public string OnConsume { get; set; }
    public bool ConsumesOnUse { get; set; }

    public Consumable(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, string onConsume, bool consumesOnUse, GameClient gameClient) : base(itemID, itemName, texture, itemType, maxStack, gameClient)
    {
        OnConsume = onConsume;
        ConsumesOnUse = consumesOnUse;
    }
}

public class Equipable : Item
{
    public string Effect { get; set; }

    public Equipable(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, string effect, GameClient gameClient) : base(itemID, itemName, texture, itemType, maxStack, gameClient)
    {
        Effect = effect;
    }
}

public class Placeable : Item
{
    public string PlacesEntity { get; set; }
    public bool ConsumesOnPlace { get; set; }

    public Placeable(string itemID, string itemName, Texture2D texture, ItemType itemType, int maxStack, string placesEntity, bool consumesOnPlace, GameClient gameClient) : base(itemID, itemName, texture, itemType, maxStack, gameClient)
    {
        PlacesEntity = placesEntity;
        ConsumesOnPlace = consumesOnPlace;
    }
}