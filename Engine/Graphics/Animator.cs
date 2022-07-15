using System.Collections.Generic;
using System.Numerics;
using AGame.Engine.Configuration;
using AGame.Engine.Graphics.Rendering;

namespace AGame.Engine.Graphics;

// Should function as a state machine for animations and animation transitions.
public class Animator
{
    public Dictionary<string, AnimationState> States { get; set; }
    public string CurrentState { get; set; }

    private string _nextState;
    private float _currentSecondsPerFrame;
    private float _nextSecondsPerFrame;
    private TextureRenderEffects _currentEffects;
    private TextureRenderEffects _nextEffects;

    private bool _transitioned;

    public Animator(IEnumerable<AnimationState> states, string initialState)
    {
        this.States = new Dictionary<string, AnimationState>();
        foreach (var state in states)
        {
            this.States.Add(state.Name, state);
        }
        this.CurrentState = initialState;
        this._currentSecondsPerFrame = this.GetAnimationState(this.CurrentState).MillisPerFrame / 1000f;
    }

    public bool Update(float deltaTime)
    {
        AnimationState state = this.States[this.CurrentState];

        if (state.Animation.Update(this._currentSecondsPerFrame, deltaTime))
        {
            // The animation has finished, go to the next state.
            if (this._nextState == null)
            {
                // No new state has been requested, go to the default next state.
                this.CurrentState = state.DefaultNextState;
            }
            else
            {
                this.CurrentState = this._nextState;
                this._nextState = null;
            }

            this._currentSecondsPerFrame = this._nextSecondsPerFrame;
            this._currentEffects = this._nextEffects;
            return true;
        }

        if (this._transitioned)
        {
            this._transitioned = false;
            return true;
        }

        return false;
    }

    public AnimationState GetAnimationState(string stateName)
    {
        return this.States[stateName];
    }

    public Animation GetCurrentAnimation()
    {
        return this.States[this.CurrentState].Animation;
    }

    public void SetNextAnimation(string state)
    {
        this._nextState = state;
        this._nextSecondsPerFrame = this.States[this._nextState].MillisPerFrame / 1000f;
        this._nextEffects = this.States[this._nextState].Effects;

        if (!this.States[this.CurrentState].MustFinish)
        {
            if (this._nextState != this.CurrentState)
            {
                this.States[this.CurrentState].Animation.Reset();
                this.CurrentState = state;
                this._transitioned = true;
                this._currentSecondsPerFrame = this._nextSecondsPerFrame;
                this._currentEffects = this._nextEffects;
            }
        }
    }

    public void Render(Vector2 position, ColorF tint)
    {
        this.States[this.CurrentState].Animation.Render(position, tint, this._currentEffects);
    }

    public Animator Clone()
    {
        return new Animator(this.States.Values, this.CurrentState);
    }
}

public class AnimationState
{
    public string Name { get; set; }
    public Animation Animation { get; set; }
    public bool MustFinish { get; set; }
    public string DefaultNextState { get; set; }
    public int MillisPerFrame { get; set; }
    public TextureRenderEffects Effects { get; set; }

    public AnimationState(string name, Animation animation)
    {
        this.Animation = animation;
        this.Name = name;
        this.MustFinish = false;
        this.DefaultNextState = name;
    }

    public AnimationState SetMustFinish(bool mustFinish)
    {
        this.MustFinish = mustFinish;
        return this;
    }

    public AnimationState SetDefaultNextState(string defaultNextState)
    {
        this.DefaultNextState = defaultNextState;
        return this;
    }

    public AnimationState SetMillisPerFrame(int millisPerFrame)
    {
        this.MillisPerFrame = millisPerFrame;
        return this;
    }

    public AnimationState SetEffects(TextureRenderEffects effects)
    {
        this.Effects = effects;
        return this;
    }

    public AnimationState Clone()
    {
        return new AnimationState(this.Name, this.Animation.Clone())
        {
            MustFinish = this.MustFinish,
            DefaultNextState = this.DefaultNextState,
            MillisPerFrame = this.MillisPerFrame,
            Effects = this.Effects,
        };
    }
}