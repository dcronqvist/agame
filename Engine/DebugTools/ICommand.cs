using System.Collections.Generic;
using System.CommandLine;
using AGame.Engine.ECSys;
using AGame.Engine.World;

namespace AGame.Engine.DebugTools
{
    public interface ICommand
    {
        Command GetConfiguration(ECS ecs, WorldContainer world);
    }
}