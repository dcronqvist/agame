namespace AGame.Engine.World;

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