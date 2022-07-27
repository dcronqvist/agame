using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AGame.Engine;
using AGame.Engine.Assets;
using AGame.Engine.Configuration;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;
using AGame.Engine.UI;
using AGame.Engine.World;

namespace AGame.Engine.Screening
{
    public static class ScreenManager
    {
        public static List<BaseScreen> Screens { get; set; }
        public static BaseScreen CurrentScreen { get; set; }
        public static string[] Args { get; set; }

        private static Type _nextScreen;
        private static bool _requestedTransition;
        private static float _transitionTime;
        private static float _currentTransitionTime;
        private static bool _transitioning;
        private static ScreenEnterArgs _nextScreenArgs;

        static ScreenManager()
        {
            Screens = new List<BaseScreen>();
            _nextScreen = null;
            _requestedTransition = false;
            _currentTransitionTime = 0f;
            _transitioning = false;
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

        public static void GoToScreen<TScreen, TScreenOnEnter>(TScreenOnEnter args, float transitionTime = 0.3f) where TScreen : BaseScreen where TScreenOnEnter : ScreenEnterArgs
        {
            _nextScreen = typeof(TScreen);
            _requestedTransition = true;
            _nextScreenArgs = args;
            _transitionTime = transitionTime;

            Logging.Log(LogLevel.Info, $"Requested screen transition to {_nextScreen.Name}");
        }

        public static void Update()
        {
            if (_requestedTransition)
            {
                _transitioning = true;
                _requestedTransition = false;
            }

            if (_transitioning)
            {
                _currentTransitionTime += GameTime.DeltaTime;

                if (_currentTransitionTime >= _transitionTime / 2f && _nextScreen != null)
                {
                    // Perform transition

                    CurrentScreen?.OnLeave();
                    GUI.NotifyScreenTransition();
                    CurrentScreen = GetScreen(_nextScreen);
                    CurrentScreen?.OnEnter(_nextScreenArgs);
                    _requestedTransition = false;
                    Logging.Log(LogLevel.Info, $"Transitioned to screen {_nextScreen.Name}");
                    Audio.SetListenerPosition(CoordinateVector.Zero);
                    _nextScreen = null;
                }

                if (_currentTransitionTime >= _transitionTime)
                {
                    _transitioning = false;
                    _currentTransitionTime = 0f;
                }
            }
            else
            {
                CurrentScreen?.Update();
            }
        }

        public static void Render()
        {
            CurrentScreen?.Render();

            if (_transitioning)
            {
                var fadeColor = ColorF.Black;

                var donePerc = _currentTransitionTime / _transitionTime;
                var x = Utilities.GetNegAbsCurve(donePerc);

                var alpha = Utilities.EaseInOutQuint(x);

                var rect = new RectangleF(0f, 0f, DisplayManager.GetWindowSizeInPixels().X, DisplayManager.GetWindowSizeInPixels().Y);

                Renderer.SetRenderTarget(null, null);
                Renderer.Primitive.RenderRectangle(rect, fadeColor * alpha);
            }
        }
    }
}