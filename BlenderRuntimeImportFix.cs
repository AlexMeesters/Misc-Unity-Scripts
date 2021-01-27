// CC0, use as you like.
// If you work with .blend files directly, and save them during runtime. You notice any MeshCollider using the mesh
// stops working. This is a band aid for that issue. Install the 'Editor Coroutines' package to use.
// Make sure to place this in a folder named Editor, else you will experience build issues.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

public class BlenderRuntimeImportFix : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {
        if (assetPath.Contains(".blend") && Application.isPlaying)
        {
            EditorCoroutineUtility.StartCoroutine(RefreshMeshColliders(), this);
        }
    }

    IEnumerator RefreshMeshColliders()
    {
        var colliders = Object.FindObjectsOfType<MeshCollider>();
        int c = colliders.Length;

        List<MeshCollider> updateColliders = new List<MeshCollider>();

        for (int i = 0; i < c; i++)
        {
            if (colliders[i].enabled)
            {
                updateColliders.Add(colliders[i]);
                colliders[i].enabled = false;
            }
        }

        // Wait a frame.
        yield return null;

        c = updateColliders.Count;
        for (int i = 0; i < c; i++)
        {
            updateColliders[i].enabled = true;
        }
    }
}
