using AGame.Engine.Graphics;

namespace AGame.Engine.Assets;

public class AnimationStateDescription
{
    public string Name { get; set; }
    public string Animation { get; set; }
    public bool MustFinish { get; set; }
    public string DefaultNextState { get; set; }

    public AnimationState GetAnimationState()
    {
        return new AnimationState(this.Name, ModManager.GetAsset<AnimationDescription>(this.Animation).GetAnimation()).SetDefaultNextState(this.DefaultNextState).SetMustFinish(this.MustFinish);
    }
}

public class AnimatorDescription : Asset
{
    public List<AnimationStateDescription> States { get; set; }
    public string InitialState { get; set; }

    public override bool InitOpenGL()
    {
        // Nothing needs to be done.
        return true;
    }

    public Animator GetAnimator()
    {
        return new Animator(States.Select(s => s.GetAnimationState()), InitialState);
    }
}