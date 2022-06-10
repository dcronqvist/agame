using System.Numerics;

namespace AGame.Engine.Networking;

public class InterpolationQueue<T>
{
    private Queue<T> _queue;
    private int _capacity;

    public InterpolationQueue(int capacity)
    {
        this._capacity = capacity;
        this._queue = new Queue<T>(capacity);
    }

    public void Enqueue(T item)
    {
        if (this._queue.Count == this._capacity)
        {
            this._queue.Dequeue();
        }

        this._queue.Enqueue(item);
    }

    public T Dequeue()
    {
        return this._queue.Dequeue();
    }
}