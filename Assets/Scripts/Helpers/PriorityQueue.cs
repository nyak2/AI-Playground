using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    // Priority Queue does not matter who comes in first
    // The one with highest priority goes out first

    private List<Tuple<T, float>> elements = new List<Tuple<T, float>>();
    // A tuple is a pair/group of objects

    public int Count { get => elements.Count; }

    // Enqueue
    public void Enqueue(T item, float priority)
    {
        elements.Add(Tuple.Create(item, priority));
    }

    // Dequeue
    public T Dequeue()
    {
        // Priority Queue does not matter who comes in first
        // The one with highest priority goes out first

        // In this implementation, highest priority means:
        // lower number = higher priority

        int bestIndex = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            // If the currently iterating item (elements[i]) has higher priority (lower number)
            // than the one we thought to be the best so far (elements[bestIndex])
            // then change bestIndex to this element's index
            if (elements[i].Item2 < elements[bestIndex].Item2)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Item1;
        elements.RemoveAt(bestIndex); // Exit the queue

        return bestItem;
    }
}