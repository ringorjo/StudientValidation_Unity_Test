using NUnit.Framework;
using UnityEngine;

public class ObjectPoolTests
{
    private GameObject _prefabGameObject;
    private Transform _parent;

    [SetUp]
    public void SetUp()
    {
        _prefabGameObject = new GameObject("PoolItemPrefab", typeof(RectTransform));
        _prefabGameObject.SetActive(false);
        _parent = new GameObject("PoolParent").transform;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_prefabGameObject);
        Object.DestroyImmediate(_parent.gameObject);
    }

    [Test]
    public void Get_NoFreeInstances_CreatesNew()
    {
        var pool = new ObjectPool<RectTransform>(_prefabGameObject.GetComponent<RectTransform>(), _parent);

        RectTransform instance = pool.Get();

        Assert.IsNotNull(instance);
        Assert.IsTrue(instance.gameObject.activeSelf);
    }

    [Test]
    public void Release_ThenGet_ReusesSameInstance()
    {
        var pool = new ObjectPool<RectTransform>(_prefabGameObject.GetComponent<RectTransform>(), _parent);
        RectTransform first = pool.Get();

        pool.Release(first);
        RectTransform second = pool.Get();

        Assert.AreSame(first, second);
    }

    [Test]
    public void Release_DeactivatesInstance()
    {
        var pool = new ObjectPool<RectTransform>(_prefabGameObject.GetComponent<RectTransform>(), _parent);
        RectTransform instance = pool.Get();

        pool.Release(instance);

        Assert.IsFalse(instance.gameObject.activeSelf);
    }
}
