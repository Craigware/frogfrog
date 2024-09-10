using System;
using Godot;


public class PathNode {
    public Vector2 position;
    public int g;
    public int h;
    public int a => g + h;
    public bool traversable;

    public PathNode(Vector2 position) {
    }

    public override string ToString() {
        return a.ToString();
    }
}

public class MinHeap {
    int capacity;
    int size;
    PathNode[] heap;

    public MinHeap() {
        capacity = 3;
        size = 0;
        heap = new PathNode[capacity];
    }

    public int GetLeftIndex(int index) { return 2*index + 1; }
    public int GetRightIndex(int index) { return 2*index + 2; }
    public int GetParentIndex(int index) { return (index-1)/2; }
    
    public bool HasParent(int index) { return (index-1)/2 > 0; }
    public bool HasLeft(int index) { return GetLeftIndex(index) < size; }
    public bool HasRight(int index) { return GetRightIndex(index) < size; }

    public PathNode GetLeft(int index) { return heap[GetLeftIndex(index)]; }
    public PathNode GetRight(int index)  { return heap[GetRightIndex(index)]; }
    public PathNode GetParent(int index) { return heap[GetParentIndex(index)]; }

    public void Swap(int indexOne, int indexTwo) { (heap[indexOne], heap[indexTwo]) =  (heap[indexTwo], heap[indexOne]); }

    public void EnsureCapacity() {
        if (size == capacity) {
            capacity *= 2;
            Array.Resize(ref heap, capacity);
        }
    }

    public void SiftUp() {
        int index = size - 1;
        while (HasParent(index)) {
            if (GetParent(index).a > heap[index].a) {
                Swap(GetParentIndex(index), index);
                index = GetParentIndex(index);
            } else break;
        }
    }

    public void SiftDown() {
        int index = 0;
        while (HasLeft(index)) {
            var smallestIndex = GetLeftIndex(index);
            if (HasRight(index) && GetRight(index).a < GetLeft(index).a) {
                smallestIndex = GetRightIndex(index);
            }
            if (heap[smallestIndex].a < heap[index].a) {
                Swap(smallestIndex, index);
            } else break;
            index = smallestIndex;
        }
    }

    public PathNode ExtractMin() {
        Swap(0, size-1);
        var min = heap[size-1];
        heap[size-1] = null;
        size--;
        SiftDown();
        return min;
    }

    public void Insert(PathNode item) {
        EnsureCapacity();
        heap[size] = item;
        size++;
        SiftUp();
    }
}