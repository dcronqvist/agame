using System;
using System.Collections;
using System.Collections.Generic;
using GLFW;
using AGame.Graphics;

namespace AGame
{
    static class Input
    {
        public static Dictionary<Keys, bool> currentKeyboardState;
        public static Dictionary<Keys, bool> previousKeyboardState;

        public static void Init()
        {
            currentKeyboardState = GetKeyboardState();
            previousKeyboardState = currentKeyboardState;
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

        public static void Begin()
        {
            currentKeyboardState = GetKeyboardState();
        }

        public static void End()
        {
            previousKeyboardState = currentKeyboardState;
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
    }
}