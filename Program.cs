﻿using System;
using AGame;

namespace AGame
{
    class Program
    {
        private const string TITLE = "Hello Triangle!";

        static void Main(string[] args)
        {
            Game game = new ImplGame();
            game.Run(1280, 720, TITLE);
        }
    }
}