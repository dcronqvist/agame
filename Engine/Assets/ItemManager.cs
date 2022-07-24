using System;
using System.Collections.Generic;
using System.Linq;
using AGame.Engine.Items;
using AGame.Engine.Networking;

namespace AGame.Engine.Assets;

public static class ItemManager
{
    private static Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();
    private static Dictionary<string, Type> _itemComponentTypes = new Dictionary<string, Type>();

    public static void Initialize(GameServer gameServer, GameClient gameClient)
    {
        _items.Clear();
        var descriptions = ModManager.GetAssetsOfType<ItemDefinition>();

        foreach (var description in descriptions)
        {
            description.ItemID = description.Name;
            RegisterItem(description, gameServer, gameClient);
        }
    }

    private static void RegisterItem(ItemDefinition definition, GameServer gameServer, GameClient gameClient)
    {
        _items.Add(definition.ItemID, definition);
    }

    public static ItemDefinition GetItemDef(string name)
    {
        return _items[name];
    }

    public static Type GetComponentTypeByName(string name)
    {
        return _itemComponentTypes[name];
    }

    public static string GetComponentTypeNameByType(Type type)
    {
        return _itemComponentTypes.FirstOrDefault(kvp => kvp.Value == type).Key;
    }

    public static void RegisterComponentTypes()
    {
        Type[] componentTypes = Utilities.FindDerivedTypes(typeof(ItemComponentDefinition)).Where(x => x != typeof(ItemComponentDefinition) && !x.ContainsGenericParameters).ToArray();
        componentTypes = componentTypes.OrderBy(t => t.Name).DistinctBy(x => x.Name).ToArray();

        for (int i = 0; i < componentTypes.Length; i++)
        {
            var attribute = componentTypes[i].GetCustomAttributes(typeof(ItemComponentPropsAttribute), false).FirstOrDefault() as ItemComponentPropsAttribute;
            if (attribute is not null)
            {
                var typename = attribute.TypeName;
                if (!_itemComponentTypes.ContainsKey(typename))
                {
                    _itemComponentTypes.Add(typename, componentTypes[i]);
                }
            }
        }
    }
}