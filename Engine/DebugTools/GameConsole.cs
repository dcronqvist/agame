using System;
using System.Collections.Generic;
using AGame.Engine.Assets.Scripting;
using System.Linq;
using System.Numerics;
using AGame.Engine;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.Assets;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Cameras;
using System.Text;
using static AGame.Engine.OpenGL.GL;
using AGame.Engine.GLFW;
using System.CommandLine;
using System.CommandLine.IO;
using AGame.Engine.ECSys;
using AGame.Engine.Networking;

namespace AGame.Engine.DebugTools;

public static class GameConsole
{
    public static Dictionary<string, ClientSideCommand> AvailableCommands { get; set; }
    public static List<ConsoleLine> ConsoleLines { get; private set; }

    private static int historyIndex;
    private static List<string> history;
    private static StringBuilder currentLine;
    private static RenderTexture canvas;
    private static bool enabled;

    public static float RowHeight = 14.0f;
    private static string caret = "";
    private static float caretInterval = 0.6f;
    private static float currentCaretTime = 0f;

    static GameConsole()
    {
        ConsoleLines = new List<ConsoleLine>();
        AvailableCommands = new Dictionary<string, ClientSideCommand>();
        enabled = false;
    }

    public unsafe static void Initialize()
    {
        currentLine = new StringBuilder();
        history = new List<string>();
        historyIndex = 0;
        Input.OnChar += (sender, c) =>
        {
            if (enabled)
            {
                currentLine.Append(c);
            }
        };

        Input.OnBackspace += (sender, e) =>
        {
            if (enabled)
            {
                if (currentLine.Length > 0)
                    currentLine.Remove(currentLine.Length - 1, 1);
            }
        };

        Input.OnCharMods += (sender, t) =>
        {
            if (enabled)
            {
                char c = t.Item1;
                ModifierKeys mk = t.Item2;

                if (c == char.Parse("v") && mk == ModifierKeys.Control)
                {
                    string s = Glfw.GetClipboardString(DisplayManager.WindowHandle);
                    currentLine.Append(s);
                }
            }
        };

        glEnable(GL_DEBUG_OUTPUT);
        glDebugMessageCallback((s, t, i, sev, l, msg, up) =>
        {
            if (t == GL_DEBUG_TYPE_ERROR)
            {
                Console.WriteLine(s);
                Console.WriteLine(msg);
                GameConsole.WriteLine("GL", msg);
            }
        }, (void*)0);

        canvas = new RenderTexture(DisplayManager.GetWindowSizeInPixels());
    }

    public static void SetEnabled(bool val)
    {
        enabled = val;
    }

    public static void WriteLine(ICommand command, string message)
    {
        //ConsoleLine line = new ConsoleLine(command.GetConfiguration().Aliases.First(), message);
        ///ConsoleLines.Add(line);
    }

    public static void WriteLine(string sender, string message)
    {
        ConsoleLines.Add(new ConsoleLine(sender, message));
    }

    public static void InitializeCommands(GameClient gameClient)
    {
        ScriptType[] commandTypes = ScriptingManager.GetAllScriptTypesWithBaseType<ClientSideCommand>();

        foreach (ScriptType commandType in commandTypes)
        {
            ClientSideCommand ic = commandType.CreateInstance<ClientSideCommand>();
            var aliases = ic.GetAliases();
            ic.Initialize(gameClient);

            foreach (string alias in aliases)
            {
                AvailableCommands.Add(alias, ic);
            }
        }
    }

    public static void RunClientSideCommand(Entity callingEntity, ECS ecs, GameClient gameClient, ClientSideCommand command, string commandLine)
    {
        command.GetCommand(callingEntity, ecs, gameClient).Invoke(commandLine.Split(char.Parse(" ")).Skip(1).ToArray());
    }

    public static void RunLine(Entity callingEntity, ECS ecs, GameClient gameClient, string line)
    {
        string[] splitLine = line.Split(char.Parse(" "));
        string commandHandle = splitLine[0];

        if (!AvailableCommands.ContainsKey(commandHandle))
        {
            // If the client doesn't have the command, then it might be a server command, send it to the server and await response
            gameClient.EnqueuePacket(new RunServerCommandPacket() { LineToRun = line }, true, false);
            return;
        }

        ClientSideCommand csc = AvailableCommands[commandHandle];
        RunClientSideCommand(callingEntity, ecs, gameClient, csc, line);
    }

    public static void Update(Entity callingEntity, ECS ecs, GameClient gameClient)
    {
        if (Input.IsKeyPressed(GLFW.Keys.Enter))
        {
            if (currentLine.ToString() != "")
            {
                RunLine(callingEntity, ecs, gameClient, currentLine.ToString());
                history.Add(currentLine.ToString());
                historyIndex = history.Count;
                currentLine.Clear();
            }
        }
        if (Input.IsKeyPressed(GLFW.Keys.Up))
        {
            if (historyIndex > 0)
            {
                historyIndex--;

                currentLine.Clear();
                currentLine.Append(history[historyIndex]);
            }
        }
        if (Input.IsKeyPressed(GLFW.Keys.Down))
        {
            if (historyIndex < history.Count - 1)
            {
                historyIndex++;

                currentLine.Clear();
                currentLine.Append(history[historyIndex]);
            }
            else
            {
                currentLine.Clear();
            }
        }

        while (ConsoleLines.Count > 50)
        {
            ConsoleLines.RemoveAt(0);
        }

        currentCaretTime += GameTime.DeltaTime;
        if (currentCaretTime > caretInterval)
        {
            currentCaretTime = 0f;

            caret = caret == "" ? "_" : "";
        }
    }

    public static RenderTexture Render(Font font)
    {
        Renderer.SetRenderTarget(canvas, null);
        Renderer.Clear(ColorF.Black * 0.2f);

        float margin = 20f;

        float rowHeight = RowHeight;
        float dist = 5;
        Vector2 basePosition = new Vector2(margin, DisplayManager.GetWindowSizeInPixels().Y - rowHeight - margin);

        for (int i = 0; i < ConsoleLines.Count; i++)
        {
            Vector2 offset = new Vector2(0, -(i + 1) * rowHeight - dist);
            Renderer.Text.RenderText(font, GameConsole.ConsoleLines[ConsoleLines.Count - i - 1].ToString(), basePosition + offset, 1f, ColorF.White, Renderer.Camera, true);
        }
        Renderer.Text.RenderText(font, $"> {currentLine.ToString()}{caret}", basePosition, 1f, ColorF.White, Renderer.Camera);

        Renderer.SetRenderTarget(null, null);
        return canvas;
    }
}