using System;
using AGame.Engine.DebugTools;
using AGame.Engine.Graphics;

namespace MyMod
{

    class MyCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            Console.WriteLine("Did nothing");
            Console.WriteLine(args);
            return CommandResult.CreateOk("I have done nothing");
        }

        public string GetHandle()
        {
            return "bitch";
        }
    }

    class AnotherCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            string argsString = "";
            foreach (string arg in args)
            {
                argsString += " " + arg;
            }
            return CommandResult.CreateWarning($"You are most likely dcronqvist. Run with args: {argsString}");
        }

        public string GetHandle()
        {
            return "whoami";
        }
    }

    class NewCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            Console.WriteLine("I am a new command.");
            return CommandResult.CreateOk($"Default command.");
        }
        public string GetHandle()
        {
            return "newcommand";
        }
    }

    class ExitCommand : ICommand
    {
        public CommandResult Execute(string[] args)
        {
            DisplayManager.SetWindowShouldClose(true);
            return CommandResult.CreateOk($"Default command.");
        }
        public string GetHandle()
        {
            return "exit";
        }
    }
}