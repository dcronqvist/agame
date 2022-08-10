using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using AGame.Engine.Assets;
using AGame.Engine.Assets.Scripting;
using AGame.Engine.ECSys;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.Items;

public class ItemInstance
{
    public ItemDefinition Definition { get; set; }
    public List<ItemComponent> Components { get; set; }

    public ItemInstance(ItemDefinition definition, IEnumerable<ItemComponent> comps)
    {
        this.Definition = definition;
        this.Components = new List<ItemComponent>(comps);
    }

    public ulong GetHash()
    {
        return Utilities.CombineHash(
            this.Components.Select(c => c.GetHash()).ToArray()
        );
    }

    public bool HasComponent<T>() where T : ItemComponent
    {
        return this.Components.Any(c => c is T);
    }

    public bool HasComponent(string componentTypeName)
    {
        return this.Components.Any(c => c.GetTypeName() == componentTypeName);
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
        return ModManager.GetAsset<Texture2D>(this.Definition.Texture);
    }

    public void RenderInSlot(Vector2 position)
    {
        var itemTargetSize = new Vector2(ContainerSlot.WIDTH, ContainerSlot.HEIGHT);
        var itemTextureSize = new Vector2(this.GetTexture().Width, this.GetTexture().Height);
        var itemScale = itemTargetSize / itemTextureSize;

        Renderer.Texture.Render(this.GetTexture(), position, itemScale, 0f, ColorF.White);

        foreach (var comp in Components)
        {
            comp.RenderInSlot(this, position);
        }
    }

    public void OnHoverInContainerRender(Vector2 mousePosition)
    {
        var itemName = this.Definition.ItemName;
        var font = ModManager.GetAsset<Font>("default.font.rainyhearts");

        Renderer.Text.RenderText(font, itemName, (mousePosition + new Vector2(22, 2)).PixelAlign(), 2f, ColorF.Black, Renderer.Camera);
        Renderer.Text.RenderText(font, itemName, (mousePosition + new Vector2(20, 0)).PixelAlign(), 2f, ColorF.White, Renderer.Camera);

        foreach (var comp in Components)
        {
            comp.OnHoverInContainerRender(this, mousePosition);
        }
    }

    private bool TryGetCanBeUsedComponent(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, out ItemComponent canBeUsed)
    {
        foreach (var component in this.Components)
        {
            if (component.CanBeUsed(playerEntity, userCommand, item, ecs))
            {
                canBeUsed = component;
                return true;
            }
        }

        canBeUsed = null;
        return false;
    }

    public bool CanBeUsed(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs)
    {
        ItemComponent canBeUsed;
        return this.TryGetCanBeUsedComponent(playerEntity, userCommand, item, ecs, out canBeUsed);
    }

    public bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed, out bool resetUseTime)
    {
        resetUseTime = false;

        if (TryGetCanBeUsedComponent(playerEntity, userCommand, item, ecs, out var canBeUsed))
        {
            return canBeUsed.OnUse(playerEntity, userCommand, item, ecs, deltaTime, totalTimeUsed, out resetUseTime);
        }

        return false;
    }

    public void OnHoldRender(Entity playerEntity, ItemInstance item, ECS ecs, float deltaTime)
    {
        foreach (var component in this.Components)
        {
            component.OnHoldRender(playerEntity, item, ecs, deltaTime);
        }
    }

    public void OnUseRender(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed)
    {
        if (TryGetCanBeUsedComponent(playerEntity, userCommand, item, ecs, out var canBeUsed))
        {
            canBeUsed.OnUseRender(playerEntity, userCommand, item, ecs, deltaTime, totalTimeUsed);
        }
    }

    public bool ShouldItemBeConsumed()
    {
        foreach (var component in this.Components)
        {
            if (component.ShouldItemBeConsumed())
            {
                return true;
            }
        }
        return false;
    }

    public void OnConsumed(Entity playerEntity, ItemInstance item, ECS ecs)
    {
        foreach (var component in this.Components)
        {
            component.OnConsumed(playerEntity, item, ecs);
        }
    }
}