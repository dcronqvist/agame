using System;
using AGame.Engine;

namespace AGame
{
    class Program
    {
        private const string TITLE = "Hello Triangle!";

        static void Main(string[] args)
        {
            Game game = new ImplGame();
            game.Run(1664, 936, TITLE, args);
        }
    }
}