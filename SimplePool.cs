// Simple pooling script that uses UniRX for callbacks.
// Can be useful for particle effects that disable after play. Those kind of things.
// It lacks a lot of functionality tough, like automatic resizing.

using UnityEngine;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;

public class SimplePool : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        if (instance == null)
        {
            instance = new GameObject("Quickpool").AddComponent<QuickPool>();
            GameObject.DontDestroyOnLoad(instance.gameObject);
        }
    }

    private static QuickPool instance;

    private Dictionary<GameObject, List<Transform>> inactiveObjects = new Dictionary<GameObject, List<Transform>>();

    public static Transform Spawn(GameObject resource)
    {
        return instance.SpawnResource(resource);
    }

    private Transform SpawnResource(GameObject resource)
    {
        if (!inactiveObjects.TryGetValue(resource, out List<Transform> inactiveObjectPool))
        {
            inactiveObjectPool = new List<Transform>();
            inactiveObjects.Add(resource, inactiveObjectPool);
        }

        if (inactiveObjectPool.Count == 0)
        {
            Transform instance = GameObject.Instantiate(resource).transform;

            instance.OnDisableAsObservable()
                .Subscribe(_ => ReturnToPool(resource, instance));

            instance.OnEnableAsObservable()
                .Subscribe(_ => RemoveFromPool(resource, instance));

            instance.OnDestroyAsObservable()
                .Subscribe(_ => RemoveFromPool(resource, instance));

            return instance;
        }
        else
        {
            if (inactiveObjectPool.Count > 0)
            {
                var index = inactiveObjectPool.Count - 1;
                Transform t = inactiveObjectPool[index];
                inactiveObjectPool.RemoveAt(index);
                t.gameObject.SetActive(true);

                return t;
            }
        }

        return null;
    }

    private void ReturnToPool(GameObject resource, Transform instance)
    {
        if (inactiveObjects.TryGetValue(resource, out List<Transform> prefabList))
            prefabList.Add(instance);
    }

    private void RemoveFromPool(GameObject resource, Transform instance)
    {
        if (inactiveObjects.TryGetValue(resource, out List<Transform> prefabList))
            prefabList.Remove(instance);
    }
}
