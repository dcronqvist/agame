using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using System.Numerics;

namespace AGame.Engine
{
    static class Input
    {
        public static Dictionary<Keys, bool> currentKeyboardState;
        public static Dictionary<Keys, bool> previousKeyboardState;

        public static Dictionary<MouseButton, bool> currentMouseState;
        public static Dictionary<MouseButton, bool> previousMouseState;

        public static EventHandler<char> OnChar;
        public static EventHandler OnBackspace;

        public static void Init()
        {
            currentKeyboardState = GetKeyboardState();
            previousKeyboardState = currentKeyboardState;

            currentMouseState = GetMouseState();
            previousMouseState = currentMouseState;

            Glfw.SetCharCallback(DisplayManager.WindowHandle, (Window, codePoint) =>
            {
                OnChar?.Invoke(null, (char)codePoint);
            });

            Glfw.SetKeyCallback(DisplayManager.WindowHandle, (Window, key, scanCode, state, mods) =>
            {
                if (key == Keys.Backspace)
                {
                    if (state != InputState.Release)
                    {
                        OnBackspace?.Invoke(null, EventArgs.Empty);
                    }
                }
            });
        }

        public static Dictionary<Keys, bool> GetKeyboardState()
        {
            Keys[] keys = Enum.GetValues<Keys>();
            Dictionary<Keys, bool> dic = new Dictionary<Keys, bool>();
            foreach (Keys key in keys)
            {
                if (key != Keys.Unknown)
                {
                    dic.Add(key, Glfw.GetKey(DisplayManager.WindowHandle, key) == InputState.Press);
                }
            }
            return dic;
        }

        public static Dictionary<MouseButton, bool> GetMouseState()
        {
            MouseButton[] mouseButtons = Enum.GetValues<MouseButton>();
            Dictionary<MouseButton, bool> dic = new Dictionary<MouseButton, bool>();

            foreach (MouseButton button in mouseButtons)
            {
                if (!dic.ContainsKey(button))
                {
                    dic.Add(button, Glfw.GetMouseButton(DisplayManager.WindowHandle, button) == InputState.Press);
                }
            }

            return dic;
        }

        public static void Begin()
        {
            currentKeyboardState = GetKeyboardState();
            currentMouseState = GetMouseState();
        }

        public static void End()
        {
            previousKeyboardState = currentKeyboardState;
            previousMouseState = currentMouseState;
        }

        public static bool IsKeyDown(Keys key)
        {
            return currentKeyboardState[key];
        }

        public static bool IsKeyPressed(Keys key)
        {
            return currentKeyboardState[key] && !previousKeyboardState[key];
        }

        public static bool IsKeyReleased(Keys key)
        {
            return !currentKeyboardState[key] && previousKeyboardState[key];
        }

        public static bool IsMouseButtonDown(MouseButton button)
        {
            return currentMouseState[button];
        }

        public static bool IsMouseButtonPressed(MouseButton button)
        {
            return currentMouseState[button] && !previousMouseState[button];
        }

        public static bool IsMouseButtonReleased(MouseButton button)
        {
            return !currentMouseState[button] && previousMouseState[button];
        }

        public static Vector2 GetMousePosition()
        {
            Glfw.GetCursorPosition(DisplayManager.WindowHandle, out double x, out double y);

            return new Vector2((float)x, (float)y);
        }
    }
}