using System.Collections.Generic;
using AGame.Engine;
using AGame.Engine.UI;

namespace AGame.Engine.Screening
{
    public static class ScreenManager
    {
        public static List<BaseScreen> Screens { get; set; }
        public static BaseScreen CurrentScreen { get; set; }
        public static string[] Args { get; set; }

        private static Type _nextScreen;
        private static bool _requestedTransition;
        private static ScreenEnterArgs _nextScreenArgs;

        static ScreenManager()
        {
            Screens = new List<BaseScreen>();
            _nextScreen = null;
            _requestedTransition = false;
            CurrentScreen = null;
        }

        public static void Init(string[] args)
        {
            Args = args;

            Type[] screenTypes = Utilities.FindDerivedTypes(typeof(BaseScreen)).Where(x => !x.IsAbstract).ToArray();

            foreach (Type screenType in screenTypes)
            {
                BaseScreen screen = (BaseScreen)Activator.CreateInstance(screenType);
                screen.Initialize();
                Screens.Add(screen);
            }
        }

        public static T GetScreen<T>() where T : BaseScreen
        {
            return (T)Screens.Find(x => x.GetType() == typeof(T));
        }

        public static BaseScreen GetScreen(Type screenType)
        {
            return Screens.Find(x => x.GetType() == screenType);
        }

        public static void GoToScreen<TScreen, TScreenOnEnter>(TScreenOnEnter args) where TScreen : BaseScreen where TScreenOnEnter : ScreenEnterArgs
        {
            _nextScreen = typeof(TScreen);
            _requestedTransition = true;
            _nextScreenArgs = args;
        }

        public static void Update()
        {
            if (_requestedTransition)
            {
                CurrentScreen?.OnLeave();
                GUI.NotifyScreenTransition();
                CurrentScreen = GetScreen(_nextScreen);
                CurrentScreen?.OnEnter(_nextScreenArgs);
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