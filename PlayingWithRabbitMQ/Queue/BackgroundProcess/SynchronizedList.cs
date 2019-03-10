using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PlayingWithRabbitMQ.Queue.BackgroundProcess
{
  public class SynchronizedList<T> : IList<T>, ICollection
  {
    private readonly IList<T> _list;
    private readonly object _lockObject;

    public SynchronizedList() : this(Enumerable.Empty<T>())
    {

    }

    public SynchronizedList(IEnumerable<T> collection)
    {
      _list = new List<T>(collection);

      _lockObject = ((ICollection)_list).SyncRoot;
    }


    public int IndexOf(T item)
    {
      lock (_lockObject) return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      lock (_lockObject) _list.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
      lock (_lockObject) _list.RemoveAt(index);
    }

    public T this[int index]
    {
      get
      {
        lock (_lockObject) return _list[index];
      }
      set
      {
        lock (_lockObject) _list[index] = value;
      }
    }


    public void Add(T item)
    {
      lock (_lockObject) _list.Add(item);
    }

    public void Clear()
    {
      lock (_lockObject) _list.Clear();
    }

    public bool Contains(T item)
    {
      lock (_lockObject) return _list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      lock (_lockObject) _list.CopyTo(array, arrayIndex);
    }

    public int Count { get { lock (_lockObject) return _list.Count; } }

    public bool IsReadOnly => false;

    public bool Remove(T item)
    {
      lock (_lockObject) return _list.Remove(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
      lock (_lockObject) return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      lock (_lockObject) return ((IEnumerable)_list).GetEnumerator();
    }

    public void CopyTo(Array array, int index)
    {
      lock (_lockObject) ((ICollection)_list).CopyTo(array, index);
    }

    public bool IsSynchronized => true;

    public object SyncRoot => _lockObject;
  }
}
