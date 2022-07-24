using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace AGame.Engine.Configuration;

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Fatal = 4
}

public interface ILogStream
{
    void WriteLine(string s);
}

public class FileLogger : ILogStream
{
    private string _file;

    public FileLogger(string file)
    {
        this._file = file;
    }

    public void WriteLine(string s)
    {
        using (StreamWriter writer = new StreamWriter(this._file, true))
        {
            writer.WriteLine(s);
        }
    }
}

public class ConsoleLogger : ILogStream
{
    public void WriteLine(string s)
    {
        Console.WriteLine(s);
    }
}

public static class Logging
{
    private static List<ILogStream> _logStreams = new List<ILogStream>();
    private static BufferBlock<(LogLevel, string)> _logQueue = new BufferBlock<(LogLevel, string)>();

    public static void AddLogStream(ILogStream stream)
    {
        _logStreams.Add(stream);
    }

    public static void Log(LogLevel level, string message)
    {
        string logMessage = $"[{DateTime.Now.ToString()} - {level.ToString().ToUpper().PadRight(7)}] {message}";
        _logQueue.SendAsync((level, logMessage));
    }

    public static void StartLogging()
    {
        Task.Run(() =>
        {
            while (true)
            {
                (LogLevel level, string message) = _logQueue.Receive();

                foreach (ILogStream stream in _logStreams)
                {
                    stream.WriteLine(message);
                }
            }
        });
    }
}