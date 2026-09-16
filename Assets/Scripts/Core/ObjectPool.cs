using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Stack<T> _inactive = new Stack<T>();
    private readonly HashSet<T> _active = new HashSet<T>();

    public ObjectPool(T prefab, Transform parent, int prewarmCount = 0)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < prewarmCount; i++)
        {
            T instance = CreateInstance();
            instance.gameObject.SetActive(false);
            _inactive.Push(instance);
        }
    }

    public T Get()
    {
        T instance = _inactive.Count > 0 ? _inactive.Pop() : CreateInstance();
        instance.gameObject.SetActive(true);
        _active.Add(instance);
        return instance;
    }

    public void Release(T instance)
    {
        if (!_active.Remove(instance)) return;

        instance.gameObject.SetActive(false);
        _inactive.Push(instance);
    }

    public void ReleaseAll()
    {
        foreach (T instance in new List<T>(_active))
        {
            Release(instance);
        }
    }

    private T CreateInstance()
    {
        return Object.Instantiate(_prefab, _parent);
    }
}
