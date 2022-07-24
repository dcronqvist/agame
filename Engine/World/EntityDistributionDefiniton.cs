using System.Collections.Generic;
using AGame.Engine.Assets.Scripting;

namespace AGame.Engine.World;

public struct SpawnEntityDefinition
{
    public SpawnEntityDefinition(string entityAsset, Vector2i tileAlignedPosition)
    {
        EntityAsset = entityAsset;
        TileAlignedPosition = tileAlignedPosition;
    }

    public string EntityAsset { get; set; }
    public Vector2i TileAlignedPosition { get; set; }
}

public interface IDistributor
{
    public List<SpawnEntityDefinition> GetDistribution(string entityAsset, float size, Vector2i startTile);
}

public class EntityDistributionDefinition
{
    public string EntityAsset { get; set; }
    public float Frequency { get; set; }
    public float Size { get; set; }
    public string Distributor { get; set; }

    public EntityDistributionDefinition(string entityAsset, float frequency, float size, string distributor)
    {
        EntityAsset = entityAsset;
        Frequency = frequency;
        Size = size;
        Distributor = distributor;
    }

    public List<SpawnEntityDefinition> GetDistribution(Vector2i startTile)
    {
        var distributor = (IDistributor)ScriptingManager.CreateInstance(Distributor);
        return distributor.GetDistribution(this.EntityAsset, this.Size, startTile);
    }
}