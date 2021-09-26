using System.Collections.Generic;
using AGame.MyGame;

namespace AGame.Engine.Screening
{
    static class ScreenManager
    {
        public static Dictionary<string, Screen> Screens { get; set; }
        public static string CurrentScreenName { get; set; }
        public static Screen CurrentScreen
        {
            get
            {
                if (!Screens.ContainsKey(CurrentScreenName))
                    return null;

                return Screens[CurrentScreenName];
            }
        }

        static ScreenManager()
        {
            Screens = new Dictionary<string, Screen>();
            CurrentScreenName = "";
        }

        public static void AddScreen(string name, Screen screen)
        {
            Screens.Add(name, screen);
        }

        public static void Init()
        {
            // Add screens
            AddScreen("testscreen", new TestScreen().Initialize());
        }

        public static void GoToScreen(string name)
        {
            CurrentScreen?.OnLeave();
            CurrentScreenName = name;
            CurrentScreen.OnEnter();
        }

        public static void Update()
        {
            CurrentScreen?.Update();
        }

        public static void Render()
        {
            CurrentScreen?.Render();
        }
    }
}