using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    Dictionary<string, int> _totalCounts = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Prewarm(string key, GameObject prefab, int count)
    {
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new Queue<GameObject>();
            _totalCounts[key] = 0;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(prefab, transform);
            go.SetActive(false);
            _pools[key].Enqueue(go);
            _totalCounts[key]++;
        }
    }

    public GameObject Get(string key, Vector3 position)
    {
        if (_pools.ContainsKey(key) && _pools[key].Count > 0)
        {
            GameObject go = _pools[key].Dequeue();
            go.transform.position = position;
            go.SetActive(true);
            return go;
        }
        return null;
    }

    public void Return(string key, GameObject go)
    {
        go.SetActive(false);
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new Queue<GameObject>();
            _totalCounts[key] = 0;
        }
        _pools[key].Enqueue(go);
    }

    public int GetAvailableCount(string key)
    {
        return _pools.ContainsKey(key) ? _pools[key].Count : 0;
    }

    public int GetTotalCount(string key)
    {
        return _totalCounts.ContainsKey(key) ? _totalCounts[key] : 0;
    }
}
