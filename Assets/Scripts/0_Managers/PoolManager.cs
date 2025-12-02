using UnityEngine;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    Dictionary<int, Queue<GameObject>> poolDict = new Dictionary<int, Queue<GameObject>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int id = prefab.GetInstanceID();

        if (!poolDict.ContainsKey(id))
        {
            poolDict.Add(id, new Queue<GameObject>());
        }

        GameObject obj;

        if (poolDict[id].Count > 0)
        {
            obj = poolDict[id].Dequeue();
        }
        else
        {
            obj = Instantiate(prefab);
            var poolable = obj.AddComponent<Poolable>();
            poolable.prefabID = id;
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        obj.transform.SetParent(null);

        return obj;
    }

    public void Release(GameObject obj)
    {
        if (obj.TryGetComponent<Poolable>(out Poolable poolable))
        {
            int id = poolable.prefabID;
            obj.SetActive(false);

            obj.transform.SetParent(transform);

            if (poolDict.ContainsKey(id))
            {
                poolDict[id].Enqueue(obj);
            }
        }
        else
        {
            Destroy(obj);
        }
    }
}
