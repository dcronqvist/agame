using System;
using AGame.Engine;
using AGame.Engine.DebugTools;
using System.CommandLine;

namespace AGame
{
    class Program
    {
        private const string TITLE = "Hello Triangle!";

        static void Main(string[] args)
        {
            Game game = new ImplGame();
            game.Run(1536, 768, TITLE, args);
        }
    }
}