namespace AGame.Engine.Screening;

public abstract class ScreenEnterArgs { }

public abstract class BaseScreen
{
    public abstract void Initialize();
    public abstract void Update();
    public abstract void Render();

    public abstract void OnEnter(ScreenEnterArgs args);
    public abstract void OnLeave();
}

public abstract class Screen<TEnterArgs> : BaseScreen where TEnterArgs : ScreenEnterArgs
{
    public override void OnEnter(ScreenEnterArgs args)
    {
        this.OnEnter((TEnterArgs)args);
    }

    public abstract void OnEnter(TEnterArgs args);
}
