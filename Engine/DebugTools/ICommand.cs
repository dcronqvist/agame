using System.Collections.Generic;
using System.CommandLine;

namespace AGame.Engine.DebugTools
{
    public interface ICommand
    {
        Command GetConfiguration();
    }
}