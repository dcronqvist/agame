using System;
using AGame.Engine;
using AGame.Engine.DebugTools;
using System.CommandLine;
using OpenTK.Audio;

namespace AGame
{
    class Program
    {
        private const string TITLE = "Hello Triangle!";

        static void Main(string[] args)
        {
            Game game = new NewGame();//new ImplGame();
            game.Run(1536, 768, TITLE, args);
        }
    }
}