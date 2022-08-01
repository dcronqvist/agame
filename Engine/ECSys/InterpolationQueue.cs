using System;
using System.Collections.Generic;
using AGame.Engine.Configuration;

namespace AGame.Engine.ECSys;

public class InterpolationQueue<T>
{
    private Queue<T> _queue;
    private Func<T, T, float, T> _interpolator;
    private float _currentInterpolationTime;
    private T _currentValue;

    public InterpolationQueue(T initialValue, Func<T, T, float, T> interpolationFunction)
    {
        this._queue = new Queue<T>();
        this._interpolator = interpolationFunction;
        this._currentInterpolationTime = 0f;
        this._currentValue = initialValue;
        this._queue.Enqueue(initialValue);
    }

    public void Enqueue(T item)
    {
        this._queue.Enqueue(item);
    }

    public T GetCurrentValue()
    {
        return this._currentValue;
    }

    public void Update(float deltaTime)
    {
        while (this._queue.Count > 1)
        {
            this._queue.Dequeue();
        }

        if (this._queue.Count > 0)
        {
            this._currentValue = this._interpolator(this._currentValue, this._queue.Peek(), deltaTime);
        }
    }
}