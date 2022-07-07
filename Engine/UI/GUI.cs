using System.Drawing;
using System.Numerics;
using AGame.Engine.Assets;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.UI;

public static class GUI
{
    public static int _hotID;
    public static int _activeID;
    public static int _kbdFocusID;

    private static int _idCounter;

    private static Queue<char> _charQueue;
    private static bool _caretVisible;
    private static float _caretInterval;
    private static float _currentCaretTime;

    private static Font _font;

    private static bool _transitionedScreen;
    private static bool _playedHotSound;
    private static bool _playedActiveSound;

    public static void Init()
    {
        _font = ModManager.GetAsset<Font>("default.font.rainyhearts");
        _charQueue = new Queue<char>();

        Input.OnChar += (sender, c) =>
        {
            if (_kbdFocusID != -1)
            {
                _charQueue.Enqueue(c);
            }
        };

        Input.OnBackspace += (sender, e) =>
        {
            if (_kbdFocusID != -1)
            {
                _charQueue.Enqueue('\b');
            }
        };

        Input.OnEnterPressed += (sender, e) =>
        {
            if (_kbdFocusID != -1)
            {
                _charQueue.Enqueue('\n');
            }
        };

        _hotID = -1;
        _activeID = -1;
        _kbdFocusID = -1;
        _caretVisible = false;
        _caretInterval = 0.6f;
        _currentCaretTime = 0f;
        _playedHotSound = false;
        _playedActiveSound = false;
    }

    public static void Begin()
    {
        _idCounter = 0;

        if (_transitionedScreen && !Input.IsMouseButtonDown(MouseButton.Left))
        {
            _transitionedScreen = false;
        }
    }

    public static void NotifyScreenTransition()
    {
        _transitionedScreen = true;
        _hotID = -1;
        _activeID = -1;
        _kbdFocusID = -1;
        _caretVisible = false;
        _caretInterval = 0.6f;
        _currentCaretTime = 0f;
    }

    public static void End()
    {
        if (_hotID == -1 && _playedHotSound)
        {
            _playedHotSound = false;
        }

        if (_activeID == -1 && _playedActiveSound)
        {
            _playedActiveSound = false;
        }

        if (!Input.IsMouseButtonDown(MouseButton.Left))
        {
            _hotID = -1;
            _activeID = -1;
            _dropdownHotID = -1;
        }
        else
        {
            // Left mouse is down

            if (_activeID == -1)
            {
                // Nothing is active, so we pressed nothing
                _kbdFocusID = -1;
                _currentCaretTime = 0f;
                _caretVisible = false;
                _showingDropdownID = -1;
            }
        }
    }

    private static bool TryBecomeHot(int id)
    {
        if (_hotID == -1 && !_transitionedScreen && DisplayManager.IsWindowFocused())
        {
            if (!_playedHotSound)
            {
                //Audio.Play("default.audio.click");
                _playedHotSound = true;
            }

            _hotID = id;
            return true;
        }
        return false;
    }

    private static bool TryBecomeActive(int id)
    {
        if (_activeID == -1 && Input.IsMouseButtonDown(MouseButton.Left))
        {
            if (!_playedActiveSound)
            {
                //Audio.Play("default.audio.click", 1.2f);
                _playedActiveSound = true;
            }

            _kbdFocusID = -1;
            _activeID = id;
            _showingDropdownID = -1;
            _caretVisible = false;
            return true;
        }
        return false;
    }

    private static int GetNextID()
    {
        return _idCounter++;
    }

    public static bool Button(string text, Vector2 position, Vector2 size)
    {
        ColorF defaultColor = ColorF.DarkGray;
        ColorF hotColor = ColorF.Gray;
        ColorF activeColor = ColorF.LightGray;

        int id = GetNextID();

        RectangleF rect = new RectangleF(position.X, position.Y, size.X, size.Y);
        Vector2 mousePos = Input.GetMousePositionInWindow();

        float scale = 2f;
        Vector2 textPos = new Vector2(position.X + size.X / 2, position.Y + size.Y / 2) - _font.MeasureString(text, scale) / 2f;
        float yOffset = -4f;
        float xOffset = 0;
        Vector2 offset = new Vector2(xOffset, yOffset);

        if (rect.Contains(mousePos.X, mousePos.Y) && TryBecomeHot(id) && TryBecomeActive(id))
        {
            // Hot and active!
        }

        if (_hotID == id)
        {
            if (_activeID == id)
            {
                // Both active and hot, so hovered and clicked!
                Renderer.Primitive.RenderRectangle(rect, activeColor);
            }
            else
            {
                // Just hot, so hovered!
                Renderer.Primitive.RenderRectangle(rect, hotColor);
            }
        }
        else
        {
            if (_activeID == id)
            {
                // Just active, so clicked!
                Renderer.Primitive.RenderRectangle(rect, activeColor);
            }
            else
            {
                // Neither active nor hot, so not hovered!
                Renderer.Primitive.RenderRectangle(rect, defaultColor);
            }
        }

        Renderer.Text.RenderText(_font, text, textPos + Vector2.One * 2f + offset, scale, ColorF.Black, Renderer.Camera);
        Renderer.Text.RenderText(_font, text, textPos + offset, scale, ColorF.White, Renderer.Camera);

        return _activeID == id && Input.IsMouseButtonPressed(MouseButton.Left);
    }

    public static bool Slider(string text, Vector2 position, Vector2 size, ref float value)
    {
        ColorF defaultColor = ColorF.DarkGray;
        ColorF hotColor = ColorF.Gray;
        ColorF activeColor = ColorF.LightGray;

        // Should return true if the slider was changed.
        // The value should be updated to the ref parameter.
        // value is always going to be a value between 0 and 1.
        int id = GetNextID();

        float oldValue = value;

        float sliderWidth = size.Y;
        float xPos = position.X + ((size.X - sliderWidth) * value);

        RectangleF rect = new RectangleF(position.X, position.Y, size.X, size.Y).Inflate(-2);
        Vector2 mousePos = Input.GetMousePositionInWindow();

        if (rect.Contains(mousePos.X, mousePos.Y) && TryBecomeHot(id) && TryBecomeActive(id))
        {
            // Hot and active!
        }

        if (_activeID == id)
        {
            if (Input.IsMouseButtonDown(MouseButton.Left))
            {
                xPos = Utilities.Clamp(position.X, position.X + size.X - sliderWidth, (mousePos.X - sliderWidth / 2f));
                value = (xPos - position.X) / (size.X - sliderWidth);
            }
        }

        Renderer.Primitive.RenderRectangle(rect.Inflate(2), ColorF.Darken(ColorF.DarkGray, 0.5f));

        ColorF color = _activeID == id ? activeColor : (_hotID == id ? hotColor : defaultColor);

        Renderer.Primitive.RenderRectangle(new RectangleF(xPos, position.Y, sliderWidth, size.Y).Inflate(-4), color);

        float scale = 2f;
        Vector2 textPos = new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f) - _font.MeasureString(text, scale) / 2f;

        Renderer.Text.RenderText(_font, text, textPos + Vector2.One * 2f, scale, ColorF.Black, Renderer.Camera);
        Renderer.Text.RenderText(_font, text, textPos, scale, ColorF.White, Renderer.Camera);

        return oldValue != value;
    }

    public static bool TextField(string placeHolder, Vector2 position, Vector2 size, ref string value)
    {
        ColorF defaultColor = ColorF.Darken(ColorF.DarkGray, 0.5f);
        ColorF hotColor = ColorF.DarkGray;
        ColorF activeColor = ColorF.DarkGray;

        int id = GetNextID();

        string startText = value;

        RectangleF rect = new RectangleF(position.X, position.Y, size.X, size.Y);
        Vector2 mousePos = Input.GetMousePositionInWindow();

        string text = value == "" ? placeHolder : value;

        float scale = 2f;
        Vector2 textPos = new Vector2(position.X, position.Y + size.Y / 2) - new Vector2(0f, (_font.MeasureString(text, scale).Y / 2f));

        if (rect.Contains(mousePos.X, mousePos.Y) && TryBecomeHot(id) && TryBecomeActive(id))
        {
            // Hot and active!
            _kbdFocusID = id;
            _caretVisible = true;
        }

        if (_kbdFocusID == id)
        {
            if (_charQueue.Count > 0)
            {
                char c = _charQueue.Dequeue();

                if (c == '\b')
                {
                    if (value.Length > 0)
                    {
                        value = value.Substring(0, value.Length - 1);
                    }
                }
                else if (c == '\n')
                {
                    _kbdFocusID = -1;
                    _caretVisible = false;
                    _currentCaretTime = 0f;
                    return true;
                }
                else
                {
                    float maxSize = size.X - 20f;

                    if (_font.MeasureString(value + c, scale).X < maxSize)
                    {
                        value += c;
                    }
                }
            }

            _currentCaretTime += GameTime.DeltaTime;

            if (_currentCaretTime > _caretInterval)
            {
                _currentCaretTime = 0f;
                _caretVisible = !_caretVisible;
            }

            Renderer.Primitive.RenderRectangle(rect.Inflate(2), ColorF.White);
        }

        float yOffset = -4f;
        float xOffset = 10f;
        Vector2 offset = new Vector2(xOffset, yOffset);

        Renderer.Primitive.RenderRectangle(rect, _hotID == id ? (_activeID == id ? activeColor : hotColor) : defaultColor);
        Renderer.Text.RenderText(_font, text, textPos + Vector2.One * 2f + offset, scale, ColorF.Black, Renderer.Camera);
        Renderer.Text.RenderText(_font, text, textPos + offset, scale, text == placeHolder ? ColorF.Gray : ColorF.White, Renderer.Camera);

        if (_caretVisible && _kbdFocusID == id)
        {
            Vector2 endOfString = textPos + new Vector2(_font.MeasureString(text, scale).X, 0f) + offset;
            Vector2 caretOffset = new Vector2(0f, -2f);

            Vector2 caretPos = text == placeHolder ? textPos + offset + caretOffset : endOfString + caretOffset;

            Renderer.Text.RenderText(_font, "|", caretPos + Vector2.One * 2f, scale, ColorF.Black, Renderer.Camera);
            Renderer.Text.RenderText(_font, "|", caretPos, scale, ColorF.White, Renderer.Camera);
        }

        return value != startText;
    }

    private static int _showingDropdownID = -1;
    private static int _dropdownHotID = -1;

    public static bool Dropdown(string[] options, Vector2 position, Vector2 size, ref int selectedOption)
    {
        ColorF defaultColor = ColorF.Darken(ColorF.DarkGray, 0.5f);
        ColorF hotColor = ColorF.DarkGray;
        ColorF activeColor = ColorF.Gray;

        int oldSelect = selectedOption;

        int id = GetNextID();

        RectangleF rect = new RectangleF(position.X, position.Y, size.X, size.Y);
        Vector2 mousePos = Input.GetMousePositionInWindow();

        if (rect.Contains(mousePos.X, mousePos.Y) && TryBecomeHot(id) && TryBecomeActive(id))
        {
            // Hot and active!
            if (_showingDropdownID == id)
            {
                _showingDropdownID = -1;
            }
            else
            {
                _showingDropdownID = id;
            }
        }

        ColorF color = _activeID == id ? activeColor : (_hotID == id ? hotColor : defaultColor);
        Renderer.Primitive.RenderRectangle(rect, color);

        string currentSelected = options[selectedOption];
        float scale = 2f;
        Vector2 textPos = new Vector2(position.X, position.Y + size.Y / 2) - new Vector2(0f, (_font.MeasureString(currentSelected, scale).Y / 2f));
        float yOffset = -4f;
        float xOffset = 10f;
        Vector2 offset = new Vector2(xOffset, yOffset);

        textPos = textPos + offset;

        Renderer.Text.RenderText(_font, currentSelected, textPos + Vector2.One * 2f, scale, ColorF.Black, Renderer.Camera);
        Renderer.Text.RenderText(_font, currentSelected, textPos, scale, ColorF.White, Renderer.Camera);

        if (_showingDropdownID == id)
        {
            for (int i = 0; i < options.Length; i++)
            {
                string opt = options[i];
                Vector2 optionPos = position + new Vector2(0f, (i + 1) * size.Y);

                RectangleF optRect = new RectangleF(optionPos.X, optionPos.Y, size.X, size.Y);
                if (optRect.Contains(mousePos.X, mousePos.Y))
                {
                    _dropdownHotID = i;
                    if (Input.IsMouseButtonDown(MouseButton.Left))
                    {
                        selectedOption = i;
                        _showingDropdownID = -1;
                    }
                }

                Renderer.Primitive.RenderRectangle(optRect, _dropdownHotID == i ? hotColor : ColorF.Darken(defaultColor, 1.5f));
                Vector2 optTextPos = new Vector2(optionPos.X, optionPos.Y + size.Y / 2) - new Vector2(0f, (_font.MeasureString(opt, scale).Y / 2f)) + offset;

                Renderer.Text.RenderText(_font, opt, optTextPos + Vector2.One * 2f, scale, ColorF.Black, Renderer.Camera);
                Renderer.Text.RenderText(_font, opt, optTextPos, scale, ColorF.White, Renderer.Camera);
            }
        }

        return oldSelect != selectedOption;
    }

    public static bool Checkbox(string text, Vector2 position, float size, ref bool value)
    {
        bool oldValue = value;

        ColorF defaultColor = ColorF.Darken(ColorF.DarkGray, 0.5f);
        ColorF hotColor = ColorF.DarkGray;
        ColorF activeColor = ColorF.Gray;

        int id = GetNextID();

        RectangleF rect = new RectangleF(position.X, position.Y, size, size);
        Vector2 mousePos = Input.GetMousePositionInWindow();

        if (rect.Contains(mousePos.X, mousePos.Y) && TryBecomeHot(id) && TryBecomeActive(id))
        {
            // Hot and active!
            value = !value;
        }

        ColorF color = _activeID == id ? hotColor : (_hotID == id ? hotColor : defaultColor);
        Renderer.Primitive.RenderRectangle(rect, color);

        if (value)
        {
            Renderer.Primitive.RenderRectangle(rect.Inflate((-size * 0.125f)), ColorF.White);
        }

        float scale = 2f;
        Vector2 textPos = new Vector2(position.X + size, position.Y + size / 2) - new Vector2(0f, (_font.MeasureString(text, scale).Y / 2f));
        float yOffset = -4f;
        float xOffset = 10f;
        Vector2 offset = new Vector2(xOffset, yOffset);

        Renderer.Text.RenderText(_font, text, textPos + Vector2.One * 2f + offset, scale, ColorF.Black, Renderer.Camera);
        Renderer.Text.RenderText(_font, text, textPos + offset, scale, ColorF.White, Renderer.Camera);

        return oldValue != value;
    }
}