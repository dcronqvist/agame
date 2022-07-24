using System;

namespace AGame.Engine.Networking;

[AttributeUsage(AttributeTargets.Class)]
public class ComponentNetworkingAttribute : Attribute
{
    public bool UpdateTriggersNetworkUpdate { get; set; } = true;
    public bool CreateTriggersNetworkUpdate { get; set; } = true;

    public ComponentNetworkingAttribute()
    {

    }
}