using System.Collections.Generic;
using System.Numerics;
using AGame.Engine.Configuration;

namespace AGame.Engine.Graphics;

// Should function as a state machine for animations and animation transitions.
public class Animator
{
    public Dictionary<string, AnimationState> States { get; set; }
    public string CurrentState { get; set; }

    private string _nextState;
    private int _currentFramesPerSecond;
    private int _nextFramesPerSecond;
    private bool _transitioned;

    public Animator(IEnumerable<AnimationState> states, string initialState, int initialFps)
    {
        this.States = new Dictionary<string, AnimationState>();
        foreach (var state in states)
        {
            this.States.Add(state.Name, state);
        }
        this.CurrentState = initialState;
        this._currentFramesPerSecond = initialFps;
    }

    public bool Update(float deltaTime)
    {
        AnimationState state = this.States[this.CurrentState];

        if (state.Animation.Update(this._currentFramesPerSecond, deltaTime))
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

            this._currentFramesPerSecond = this._nextFramesPerSecond;
            return true;
        }

        if (this._transitioned)
        {
            this._transitioned = false;
            return true;
        }

        return false;
    }

    public Animation GetCurrentAnimation()
    {
        return this.States[this.CurrentState].Animation;
    }

    public void SetNextAnimation(string state, int framesPerSecond)
    {
        this._nextState = state;
        this._nextFramesPerSecond = framesPerSecond;

        if (!this.States[this.CurrentState].MustFinish)
        {
            if (this._nextState != this.CurrentState)
            {
                this.States[this.CurrentState].Animation.Reset();
                this.CurrentState = state;
                this._transitioned = true;
                this._currentFramesPerSecond = this._nextFramesPerSecond;
            }
        }
    }

    public void Render(Vector2 position)
    {
        this.States[this.CurrentState].Animation.Render(position);
    }

    public Animator Clone()
    {
        return new Animator(this.States.Values, this.CurrentState, this._currentFramesPerSecond);
    }
}

public class AnimationState
{
    public string Name { get; set; }
    public Animation Animation { get; set; }
    public bool MustFinish { get; set; }
    public string DefaultNextState { get; set; }

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

    public AnimationState Clone()
    {
        return new AnimationState(this.Name, this.Animation.Clone())
        {
            MustFinish = this.MustFinish,
            DefaultNextState = this.DefaultNextState,
        };
    }
}