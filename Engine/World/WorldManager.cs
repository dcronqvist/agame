using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using AGame.Engine.Assets;
using AGame.Engine.ECSys;
using AGame.Engine.ECSys.Components;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Networking;
using AGame.Engine.Screening;

namespace AGame.Engine.World;

public class WorldManager
{
    private static WorldManager _instance;
    public static WorldManager Instance => _instance ?? (_instance = new WorldManager());

    private string _worldDirectory = "./worlds";

    private List<WorldMetaData> _worlds = new List<WorldMetaData>();

    private WorldManager()
    {

    }

    private bool TryLoadWorld(string directory, out WorldMetaData metaData)
    {
        try
        {
            string metaDataFile = directory + "/meta.json";

            JsonSerializerOptions jso = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            using (StreamReader sr = new StreamReader(metaDataFile))
            {
                string json = sr.ReadToEnd();
                metaData = JsonSerializer.Deserialize<WorldMetaData>(json, jso);
                metaData.Directory = directory;

                return true;
            }
        }
        catch
        {
            metaData = null;
            return false;
        }
    }

    public void LoadWorlds()
    {
        this._worlds.Clear();

        foreach (string world in Directory.GetDirectories(_worldDirectory))
        {
            if (TryLoadWorld(world, out WorldMetaData meta))
            {
                _worlds.Add(meta);
            }
        }
    }

    public WorldMetaData[] GetAllWorlds()
    {
        return _worlds.ToArray();
    }

    public WorldMetaData CreateNewWorld(string worldName, string generator)
    {
        WorldMetaData meta = new WorldMetaData()
        {
            CreatedAt = DateTime.Now,
            Name = worldName,
            Generator = generator,
            LastPlayedAt = DateTime.Now,
            Directory = _worldDirectory + "/" + worldName
        };

        meta.CreateInitialFiles();

        return meta;
    }
}
