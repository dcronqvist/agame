using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Items;

public class ItemInstance
{
    public string ItemID { get; set; }
    public string Name { get; set; }
    public int MaxStack { get; set; }
    public string Texture { get; set; }
    public List<ItemComponent> Components { get; set; }

    public ItemInstance(string itemID, string name, int maxStack, string texture, IEnumerable<ItemComponent> comps)
    {
        this.ItemID = itemID;
        this.Name = name;
        this.MaxStack = maxStack;
        this.Texture = texture;
        this.Components = new List<ItemComponent>(comps);
    }

    public bool HasComponent<T>() where T : ItemComponent
    {
        return this.Components.Any(c => c is T);
    }

    public T GetComponent<T>() where T : ItemComponent
    {
        return (T)this.Components.FirstOrDefault(c => c is T);
    }

    public bool TryGetComponent<T>(out T component) where T : ItemComponent
    {
        component = (T)this.Components.FirstOrDefault(c => c is T);
        return component != null;
    }

    public bool TryGetComponent(string componentType, out ItemComponent component)
    {
        component = this.Components.FirstOrDefault(c => c.GetTypeName() == componentType);
        return component != null;
    }

    public PackedItem ToPackedItem()
    {
        return new PackedItem(this);
    }

    public Texture2D GetTexture()
    {
        return ModManager.GetAsset<Texture2D>(this.Texture);
    }

    public void Render(Vector2 position)
    {
        var itemTargetSize = new Vector2(ContainerSlot.WIDTH, ContainerSlot.HEIGHT);
        var itemTextureSize = new Vector2(this.GetTexture().Width, this.GetTexture().Height);
        var itemScale = itemTargetSize / itemTextureSize;

        Renderer.Texture.Render(this.GetTexture(), position, itemScale, 0f, ColorF.White);
    }
}

/*
{
    "definitions": [
        {
            "type": "tool",
            "durability": 100
        },
        {
            "type": "resource",
            "quality": 5
        }
    ],
    "name": "A Very Cool Item"
}
*/