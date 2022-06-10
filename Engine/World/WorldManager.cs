using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.World;

public class WorldManager
{
    private static WorldManager _instance;
    public static WorldManager Instance => _instance ?? (_instance = new WorldManager());

    public WorldContainer World { get; set; }

    public WorldManager()
    {

    }

    public void Init()
    {
        this.World = new WorldContainer(new TestWorldGenerator());
    }
}
