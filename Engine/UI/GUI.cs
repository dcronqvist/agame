using System.Drawing;
using System.Numerics;
using AGame.Engine.GLFW;
using AGame.Engine.Graphics;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.UI;

public static class GUI
{
    private static int _hotID;
    private static int _activeID;

    private static int _idCounter;

    public static void Begin()
    {
        _idCounter = 0;
    }

    public static void End()
    {
        if (!Input.IsMouseButtonDown(MouseButton.Left))
        {
            _activeID = -1;
            _hotID = -1;
        }
    }

    private static bool CanBecomeHot()
    {
        if (_hotID == -1)
        {
            return true;
        }
        return false;
    }

    private static int GetNextID()
    {
        return _idCounter++;
    }

    public static bool Button(Vector2 position, Vector2 size)
    {
        int id = GetNextID();

        RectangleF rect = new RectangleF(position.X, position.Y, size.X, size.Y);
        Vector2 mousePos = Input.GetMousePositionInWindow();

        if (rect.Contains(mousePos.X, mousePos.Y) && CanBecomeHot())
        {
            _hotID = id;

            if (_activeID == -1 && Input.IsMouseButtonDown(MouseButton.Left))
            {
                _activeID = id;
            }
        }

        Renderer.Primitive.RenderRectangle(rect, ColorF.Black);

        if (_hotID == id)
        {
            if (_activeID == id)
            {
                // Both active and hot, so hovered and clicked!
                Renderer.Primitive.RenderRectangle(rect, ColorF.BlueGray);
            }
            else
            {
                // Just hot, so hovered!
                Renderer.Primitive.RenderRectangle(rect, ColorF.DarkGoldenRod);
            }
        }
        else
        {
            if (_activeID == id)
            {
                // Just active, so clicked!
                Renderer.Primitive.RenderRectangle(rect, ColorF.BlueGray);
            }
            else
            {
                // Neither active nor hot, so not hovered!
                Renderer.Primitive.RenderRectangle(rect, ColorF.White);
            }
        }

        return _activeID == id && Input.IsMouseButtonPressed(MouseButton.Left);
    }

    public static bool Slider(Vector2 position, Vector2 size, ref float value)
    {
        // Should return true if the slider was changed.
        // The value should be updated to the ref parameter.
        // value is always going to be a value between 0 and 1.
        int id = GetNextID();

        float oldValue = value;

        float sliderWidth = size.Y;
        float xPos = position.X + ((size.X - sliderWidth) * value);

        RectangleF rect = new RectangleF(position.X, position.Y, size.X, size.Y).Inflate(-2);
        Vector2 mousePos = Input.GetMousePositionInWindow();

        if (rect.Contains(mousePos.X, mousePos.Y) && CanBecomeHot())
        {
            _hotID = id;

            if (_activeID == -1 && Input.IsMouseButtonDown(MouseButton.Left))
            {
                _activeID = id;
            }
        }

        if (_activeID == id)
        {
            if (Input.IsMouseButtonDown(MouseButton.Left))
            {
                xPos = Utilities.Clamp(position.X, position.X + size.X - sliderWidth, (mousePos.X - sliderWidth / 2f));
                value = (xPos - position.X) / (size.X - sliderWidth);
            }
        }

        Renderer.Primitive.RenderRectangle(rect.Inflate(2), ColorF.Gray);

        Renderer.Primitive.RenderRectangle(new RectangleF(xPos, position.Y, sliderWidth, size.Y).Inflate(-4), _activeID == id ? ColorF.White : ColorF.LightGray);

        return oldValue != value;
    }
}