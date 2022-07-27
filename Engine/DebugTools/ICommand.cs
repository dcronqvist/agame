using System.Collections.Generic;
using System.CommandLine;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;
using AGame.Engine.World;

namespace AGame.Engine.DebugTools;

public interface ICommand
{
    IEnumerable<string> GetAliases();
    Command GetConfiguration(Entity callingEntity, ECS ecs);
}

public abstract class ClientSideCommand : ICommand
{
    private GameClient _gameClient;

    public void Initialize(GameClient gameClient)
    {
        this._gameClient = gameClient;
    }

    public abstract Command GetCommand(Entity callingEntity, ECS ecs, GameClient gameClient);

    public Command GetConfiguration(Entity callingEntity, ECS ecs)
    {
        return GetCommand(callingEntity, ecs, this._gameClient);
    }

    public abstract IEnumerable<string> GetAliases();
}

public abstract class ServerSideCommand : ICommand
{
    private GameServer _gameServer;

    public void Initialize(GameServer gameServer)
    {
        this._gameServer = gameServer;
    }

    public abstract IEnumerable<string> GetAliases();

    public abstract Command GetCommand(Entity callingEntity, ECS ecs, GameServer gameServer);

    public Command GetConfiguration(Entity callingEntity, ECS ecs)
    {
        return GetCommand(callingEntity, ecs, this._gameServer);
    }
}
