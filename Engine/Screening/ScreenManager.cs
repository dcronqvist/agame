using System.Collections.Generic;
using AGame.Engine;
using AGame.Engine.UI;

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

        private static string _nextScreenName;
        private static bool _requestedTransition;
        private static string[] _nextScreenArgs;

        static ScreenManager()
        {
            Screens = new Dictionary<string, Screen>();
            CurrentScreenName = "";

            _nextScreenName = "";
            _requestedTransition = false;
        }

        public static void AddScreen(string name, Screen screen)
        {
            Screens.Add(name, screen);
        }

        public static T GetScreen<T>(string name) where T : Screen
        {
            if (!Screens.ContainsKey(name))
                return null;

            return Screens[name] as T;
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

        public static void GoToScreen(string name, params string[] args)
        {
            _nextScreenName = name;
            _requestedTransition = true;
            _nextScreenArgs = args;
        }

        public static void Update()
        {
            if (_requestedTransition)
            {
                CurrentScreen?.OnLeave();
                CurrentScreenName = _nextScreenName;
                GUI.NotifyScreenTransition();
                CurrentScreen.OnEnter(_nextScreenArgs);
                _requestedTransition = false;
            }

            CurrentScreen?.Update();
        }

        public static void Render()
        {
            CurrentScreen?.Render();
        }
    }
}