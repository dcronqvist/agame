using System;

namespace AGame.Engine.Items;

[AttributeUsage(AttributeTargets.Class)]
public class ItemComponentPropsAttribute : Attribute
{
    public string TypeName { get; set; }
}

public abstract class ItemComponentDefinition
{
    public abstract ItemComponent CreateComponent();
}