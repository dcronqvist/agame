using System;

namespace AGame.Engine.Items;

public abstract class ItemComponentDefinition
{
    public abstract ItemComponent CreateComponent();
}