using System;
using AGame.Engine.DebugTools;

class MyCommand : ICommand
{
    public CommandResult Execute()
    {
        Console.WriteLine("Did nothing");
        return new CommandResult()
        {
            Message = "FAK",
            Type = CommandResultType.Ok
        };
    }

    public string GetHandle()
    {
        return "bitch";
    }
}