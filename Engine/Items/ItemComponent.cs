using System;
using System.Collections.Generic;
using AGame.Engine.Assets;
using GameUDPProtocol;

namespace AGame.Engine.Items;

public abstract class ItemComponent : IPacketable
{
    public abstract int Populate(byte[] data, int offset);
    public abstract byte[] ToBytes();
    public abstract string GetTypeName();
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