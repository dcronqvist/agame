using System;
using System.Collections.Generic;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using GameUDPProtocol;

namespace AGame.Engine.Items;

public abstract class ItemComponent : IPacketable
{
    public abstract int Populate(byte[] data, int offset);
    public abstract byte[] ToBytes();
    public abstract string GetTypeName();

    /// <summary>
    /// Called to test if the item can be used in the current state that it is being used
    /// </summary>
    public abstract bool CanBeUsed(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs);

    /// <summary>
    /// Called when CanBeUsed is true and the USE_ITEM key is being held. Return true when the item has been successfully used.
    /// </summary>
    public abstract bool OnUse(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed, out bool resetUseTime);

    /// <summary>
    /// Called when the selected item on the player's hotbar is this item. For rendering purposes
    /// </summary>
    public virtual void OnHoldRender(Entity playerEntity, ItemInstance item, ECS ecs, float deltaTime) { }

    /// <summary>
    /// Called when the selected item on the player's hotbar is this item and is being used. For rendering purposes
    /// </summary>
    public virtual void OnUseRender(Entity playerEntity, UserCommand userCommand, ItemInstance item, ECS ecs, float deltaTime, float totalTimeUsed) { }

    public virtual void RenderInSlot(ItemInstance item, Vector2 topLeftOfSlot) { }

    public virtual void OnHoverInContainerRender(ItemInstance item, Vector2 mousePosition) { }

    public abstract bool ShouldItemBeConsumed();
    public abstract void OnConsumed(Entity playerEntity, ItemInstance item, ECS ecs);
    public abstract ulong GetHash();
}

public abstract class ItemComponent<T> : ItemComponent where T : ItemComponentDefinition
{
    public T Definition { get; set; }

    public ItemComponent(T definition)
    {
        this.Definition = definition;
    }

    public override string GetTypeName()
    {
        return ItemManager.GetComponentTypeNameByType(typeof(T));
    }
}