using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GLFW;
using AGame.Graphics;

namespace AGame
{
    static class Input
    {
        public static Dictionary<Keys, bool> currentKeyboardState;
        public static Dictionary<Keys, bool> previousKeyboardState;

        public static Dictionary<MouseButton, bool> currentMouseState;
        public static Dictionary<MouseButton, bool> previousMouseState;

        public static void Init()
        {
            currentKeyboardState = GetKeyboardState();
            previousKeyboardState = currentKeyboardState;

            currentMouseState = GetMouseState();
            previousMouseState = currentMouseState;
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
    }
}