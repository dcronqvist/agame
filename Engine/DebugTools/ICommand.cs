using System.Collections.Generic;

namespace AGame.Engine.DebugTools
{
    public interface ICommand
    {
        CommandConfig GetConfiguration();
        CommandResult Execute(Dictionary<string, object> args);
    }
}