using System;
using AGame.Engine;
using AGame.Engine.DebugTools;
using System.CommandLine;
using OpenTK.Audio;
using System.Collections.Generic;
using System.Linq;

namespace AGame
{
    class Program
    {
        private const string TITLE = "Hello Triangle!";

        static void Main(string[] args)
        {
            Game game = new ImplGame();
            game.Run(1536, 768, TITLE, args);

            // List<string> updatedProps = new List<string>();

            // TestComponent comp1 = new TestComponent();
            // comp1.TestValue = 0;
            // comp1.AnotherTestValue = "";

            // TestComponent comp = new TestComponent();
            // comp.PropertyChanged += (sender, e) =>
            // {
            //     updatedProps.Add(e.PropertyName);
            // };

            // comp.TestValue = 5;
            // comp.AnotherTestValue = "Hello";

            // var bytes = comp.GetBytes(updatedProps.ToArray());

            // comp1.FromBytes(bytes, 0);

            // Console.ReadLine();
        }
    }
}