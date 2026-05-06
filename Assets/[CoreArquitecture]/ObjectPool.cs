using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly GameObject _prefab;
    private readonly Transform _parent;
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();

    public ObjectPool(GameObject prefab, Transform parent, int initialSize)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < initialSize; i++)
            CreateNew();
    }

    public GameObject Get(Vector3 position)
    {
        GameObject obj = _pool.Count > 0 ? _pool.Dequeue() : CreateNew();
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }

    private GameObject CreateNew()
    {
        GameObject obj = Object.Instantiate(_prefab, _parent);
        obj.SetActive(false);
        return obj;
    }
}

