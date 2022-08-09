using System.Collections.Generic;
using System.Linq;
using AGame.Engine.ECSys;

namespace AGame.Engine.Assets;

public class EntityDescription : Asset
{
    public string Extends { get; set; }
    public List<Component> Components { get; set; }

    public override bool InitOpenGL()
    {
        // Do nothing
        return true;
    }

    public bool HasComponent(string componentName)
    {
        return Components.Any(c => c.ComponentType == componentName);
    }

    public bool HasComponent<T>()
    {
        return Components.Any(c => c is T);
    }

    public T GetComponent<T>() where T : Component
    {
        return (T)Components.FirstOrDefault(c => c is T);
    }
}