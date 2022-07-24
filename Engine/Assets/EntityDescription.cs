using System.Collections.Generic;
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
}