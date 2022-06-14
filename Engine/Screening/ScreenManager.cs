using System.Collections.Generic;
using AGame.Engine;

namespace AGame.Engine.Screening
{
    public static class ScreenManager
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
        public static string[] Args { get; set; }

        static ScreenManager()
        {
            Screens = new Dictionary<string, Screen>();
            CurrentScreenName = "";
        }

        public static void AddScreen(string name, Screen screen)
        {
            Screens.Add(name, screen);
        }

        public static void Init(string[] args)
        {
            Args = args;

            Type[] screenTypes = Utilities.FindDerivedTypes(typeof(Screen)).Where(x => x != typeof(Screen)).ToArray();

            foreach (Type screenType in screenTypes)
            {
                Screen screen = (Screen)Activator.CreateInstance(screenType);
                screen.Initialize();
                AddScreen(screen.Name, screen);
            }
        }

        public static void GoToScreen(string name)
        {
            CurrentScreen?.OnLeave();
            CurrentScreenName = name;
            CurrentScreen.OnEnter(Args);
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