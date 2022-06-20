using System;
using System.Collections.Generic;
using System.Drawing;
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

public class WorldMetaData
{
    [JsonIgnore]
    public string Directory { get; set; }

    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastPlayedAt { get; set; }
    public string Generator { get; set; }

    public async Task<WorldContainer> GetAsContainerAsync()
    {
        WorldContainer wc = new WorldContainer(Utilities.GetGeneratorFromTypeName(Generator), false);

        using (StreamReader sr = new StreamReader(Directory + "/world.json"))
        {
            string json = await sr.ReadToEndAsync();
            wc.Deserialize(json);
        }

        //TODO: Remove this fake time increase
        await Task.Delay(2000);

        return wc;
    }

    public async Task<List<Entity>> GetEntitiesAsync()
    {
        using (StreamReader sr = new StreamReader(Directory + "/entities.json"))
        {
            string json = await sr.ReadToEndAsync();

            // Assume a JSON description of the entity
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new ComponentConverter() },
                IncludeFields = true,
                AllowTrailingCommas = true
            };

            return JsonSerializer.Deserialize<List<Entity>>(json, options);
        }
    }

    public void SaveEntities(List<Entity> entities)
    {
        using (StreamWriter sw = new StreamWriter(Directory + "/entities.json"))
        {
            // Assume a JSON description of the entity
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new ComponentConverter() },
                IncludeFields = true,
                AllowTrailingCommas = true
            };

            string json = JsonSerializer.Serialize(entities, options);
            sw.Write(json);
        }
    }

    public async Task<List<PlayerInfo>> GetPlayerInfosAsync()
    {
        using (StreamReader sr = new StreamReader(Directory + "/players.json"))
        {
            string json = await sr.ReadToEndAsync();

            // Assume a JSON description of the entity
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                AllowTrailingCommas = true
            };

            List<PlayerInfo> infos = JsonSerializer.Deserialize<List<PlayerInfo>>(json, options);
            return infos;
        }
    }

    public List<PlayerInfo> GetPlayerInfos()
    {
        using (StreamReader sr = new StreamReader(Directory + "/players.json"))
        {
            string json = sr.ReadToEnd();

            // Assume a JSON description of the entity
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                AllowTrailingCommas = true
            };

            List<PlayerInfo> infos = JsonSerializer.Deserialize<List<PlayerInfo>>(json, options);
            return infos;
        }
    }

    public async Task<PlayerInfo> GetPlayerInfoAsync(string playerName)
    {
        List<PlayerInfo> players = await GetPlayerInfosAsync();
        return players.FirstOrDefault(p => p.PlayerName == playerName);
    }

    public PlayerInfo GetPlayerInfo(string playerName)
    {
        List<PlayerInfo> players = GetPlayerInfos();
        return players.FirstOrDefault(p => p.PlayerName == playerName);
    }

    private void SavePlayerInfos(List<PlayerInfo> playerInfos)
    {
        using (StreamWriter sw = new StreamWriter(Directory + "/players.json"))
        {
            // Assume a JSON description of the entity
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                AllowTrailingCommas = true
            };

            string json = JsonSerializer.Serialize(playerInfos, options);
            sw.Write(json);
        }
    }

    public async Task UpdatePlayerInfoAsync(string playerName, PlayerInfo info)
    {
        List<PlayerInfo> players = await GetPlayerInfosAsync();
        players.Remove(players.FirstOrDefault(p => p.PlayerName == playerName));

        players.Add(info);
        SavePlayerInfos(players);
    }

    public void UpdatePlayerInfo(string playerName, PlayerInfo info)
    {
        List<PlayerInfo> players = GetPlayerInfos();
        players.Remove(players.FirstOrDefault(p => p.PlayerName == playerName));

        players.Add(info);
        SavePlayerInfos(players);
    }

    public void SaveWorld(WorldContainer container)
    {
        using (StreamWriter sw = new StreamWriter(Directory + "/world.json"))
        {
            // Assume a JSON description of the entity
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                AllowTrailingCommas = true
            };

            string json = container.Serialize();
            sw.Write(json);
        }
    }
}

public class PlayerInfo
{
    public string PlayerName { get; set; }
    public CoordinateVector Position { get; set; }

    public PlayerInfo(string playerName, CoordinateVector position)
    {
        PlayerName = playerName;
        Position = position;
    }
}

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
}
