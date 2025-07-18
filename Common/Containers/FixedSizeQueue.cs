using System.Collections.Concurrent;

namespace FlAdmin.Common.Containers;


// reffed from https://stackoverflow.com/questions/5852863/fixed-size-queue-which-automatically-dequeues-old-values-upon-new-enqueues

public class FixedSizeQueue<T>(int size)
{
    public readonly Queue<T> queue = new Queue<T>();

    private int Size { get; set; } = size;

    public void Enqueue(T obj)
    {
        queue.Enqueue(obj);

        while (queue.Count > Size)
        {
            T outObj;
            queue.TryDequeue(out outObj);
        }
    }
}