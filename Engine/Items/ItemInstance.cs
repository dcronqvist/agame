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
    }

    public bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed)
    {
        bool beingUsed = false;
        foreach (var component in this.Components)
        {
            if (component.OnUse(playerEntity, userCommand, item, ecs, deltaTime, totalTimeUsed))
            {
                beingUsed = true;
            }
        }
        return beingUsed;
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

    // API STUFF
    public void CreateEntity(Entity playerEntity, ECS ecs, string entity, Action<Entity> onCreate)
    {
        if (ecs.IsRunner(SystemRunner.Server))
        {
            // IF WE ARE ON THE SERVER
            this.Definition._gameServer.CreateEntityAsClient(playerEntity.ID, entity, onCreate);
        }
        else if (ecs.IsRunner(SystemRunner.Client))
        {
            // IF WE ARE ON THE CLIENT
            this.Definition._gameClient.AttemptCreateEntity(entity, onCreate);
        }
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