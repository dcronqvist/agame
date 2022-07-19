using AGame.Engine.Items;
using AGame.Engine.Networking;

namespace AGame.Engine.Assets;

public static class ItemManager
{
    private static Dictionary<string, Item> _items = new Dictionary<string, Item>();

    public static void Initialize(GameClient gameClient)
    {
        var descriptions = ModManager.GetAssetsOfType<ItemDescription>();

        foreach (var description in descriptions)
        {
            RegisterItem(description, gameClient);
        }
    }

    private static void RegisterItem(ItemDescription description, GameClient gameClient)
    {
        var item = description.CreateItem(gameClient);
        _items.Add(description.Name, item);
    }

    public static Item GetItem(string name)
    {
        return _items[name];
    }

    public static T GetItem<T>(string name) where T : Item
    {
        return (T)_items[name];
    }
}