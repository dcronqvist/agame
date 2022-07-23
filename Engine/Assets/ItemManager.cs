using AGame.Engine.Items;
using AGame.Engine.Networking;

namespace AGame.Engine.Assets;

public static class ItemManager
{
    private static Dictionary<string, Item> _items = new Dictionary<string, Item>();

    public static void Initialize(GameServer gameServer, GameClient gameClient)
    {
        _items.Clear();
        var descriptions = ModManager.GetAssetsOfType<ItemDescription>();

        foreach (var description in descriptions)
        {
            RegisterItem(description, gameServer, gameClient);
        }
    }

    private static void RegisterItem(ItemDescription description, GameServer gameServer, GameClient gameClient)
    {
        var item = description.CreateItem(gameServer, gameClient);
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