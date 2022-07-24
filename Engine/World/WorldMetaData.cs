using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AGame.Engine.Assets;
using AGame.Engine.ECSys;

namespace AGame.Engine.World;

public class WorldMetaData
{
    [JsonIgnore]
    public string Directory { get; set; }

    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastPlayedAt { get; set; }
    public string Generator { get; set; }

    public async Task<WorldContainer> GetAsContainerAsync(bool rendering)
    {
        WorldContainer wc = new WorldContainer(rendering, Utilities.GetGeneratorFromTypeName(Generator));

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
                AllowTrailingCommas = true,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true
            };

            string json = JsonSerializer.Serialize(entities, options);
            sw.Write(json);
        }

        this.SaveWorldMeta();
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

    public PlayerInfo GetPlayerInfo(string playerName, CoordinateVector newPositionIfNoExist)
    {
        List<PlayerInfo> players = GetPlayerInfos();
        return players.FirstOrDefault(p => p.PlayerName == playerName, new PlayerInfo(playerName, newPositionIfNoExist));
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

        this.SaveWorldMeta();
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

    public void SaveWorldMeta()
    {
        // Update relevant meta data
        this.LastPlayedAt = DateTime.Now;

        using (StreamWriter sw = new StreamWriter(Directory + "/meta.json"))
        {
            // Assume a JSON description of the entity
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                AllowTrailingCommas = true
            };

            string json = JsonSerializer.Serialize(this, options);
            sw.Write(json);
        }
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

        this.SaveWorldMeta();
    }

    public void CreateInitialFiles()
    {
        // Create the world directory
        System.IO.Directory.CreateDirectory(Directory);

        // Create the world.json file
        using (StreamWriter sw = new StreamWriter(Directory + "/world.json"))
        {
            // Assume a JSON description of the entity
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                AllowTrailingCommas = true
            };

            string json = JsonSerializer.Serialize(new WorldContainer(false, Utilities.GetGeneratorFromTypeName(Generator)), options);
            sw.Write(json);
        }

        // Create the entities.json file
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

            string json = JsonSerializer.Serialize(new List<Entity>(), options);
            sw.Write(json);
        }

        // Create the players.json file
        using (StreamWriter sw = new StreamWriter(Directory + "/players.json"))
        {
            // Assume a JSON description of the entity
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                AllowTrailingCommas = true
            };

            string json = JsonSerializer.Serialize(new List<PlayerInfo>(), options);
            sw.Write(json);
        }

        // Create the meta.json file
        using (StreamWriter sw = new StreamWriter(Directory + "/meta.json"))
        {
            // Assume a JSON description of the entity
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
                AllowTrailingCommas = true
            };

            string json = JsonSerializer.Serialize(this, options);
            sw.Write(json);
        }
    }
}
