using System;
using System.Collections.Generic;

namespace AGame.DebugTools
{
    class GameConsole
    {
        List<CommandResult> ExecutedCommands { get; set; }

        public GameConsole()
        {
            this.ExecutedCommands = new List<CommandResult>();
        }
    }
}