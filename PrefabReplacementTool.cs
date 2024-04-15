using UnityEngine;
using UnityEditor;

public class PrefabReplacementTool : EditorWindow
{
    private GameObject prefabToInstantiate;

    [MenuItem("LS/Tools/Replace Selected Objects with Prefab")]
    static void Init()
    {
        PrefabReplacementTool window = (PrefabReplacementTool)EditorWindow.GetWindow(typeof(PrefabReplacementTool));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Select Prefab to Replace With:", EditorStyles.boldLabel);
        prefabToInstantiate = EditorGUILayout.ObjectField("Prefab", prefabToInstantiate, typeof(GameObject), false) as GameObject;

        if (GUILayout.Button("Replace Selected with Prefab"))
        {
            ReplaceSelectedWithPrefab();
        }
    }

    void ReplaceSelectedWithPrefab()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected.");
            return;
        }

        if (prefabToInstantiate == null)
        {
            Debug.LogWarning("Prefab not assigned.");
            return;
        }

        foreach (GameObject selectedObject in Selection.gameObjects)
        {
            if (MatchHierarchy(selectedObject.transform, prefabToInstantiate.transform))
            {
                ReplaceObjectWithPrefab(selectedObject, prefabToInstantiate);
            }
            else
            {
                Debug.LogError("Prefab structure does not match the selected object's hierarchy and names: " + selectedObject.name);
            }
        }
    }

    void ReplaceObjectWithPrefab(GameObject obj, GameObject prefab)
    {
        Transform parent = obj.transform.parent;
        int siblingIndex = obj.transform.GetSiblingIndex();
        Vector3 position = obj.transform.position;
        Quaternion rotation = obj.transform.rotation;
        Vector3 scale = obj.transform.localScale; // Added line to get the scale of the original object

        GameObject newObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

        newObject.transform.SetParent(parent);
        newObject.transform.SetSiblingIndex(siblingIndex);
        newObject.transform.position = position;
        newObject.transform.rotation = rotation;
        newObject.transform.localScale = scale; // Added line to set the scale of the new object

        // Match the transformations of children objects
        MatchChildTransformations(obj.transform, newObject.transform);

        Undo.DestroyObjectImmediate(obj);
    }


    bool MatchHierarchy(Transform objTransform, Transform prefabTransform)
    {
        if (objTransform.childCount != prefabTransform.childCount)
            return false;

        for (int i = 0; i < objTransform.childCount; i++)
        {
            if (!MatchHierarchy(objTransform.GetChild(i), prefabTransform.GetChild(i)))
                return false;
        }

        return true;
    }

    void MatchChildTransformations(Transform originalTransform, Transform newTransform)
    {
        for (int i = 0; i < originalTransform.childCount; i++)
        {
            Transform originalChild = originalTransform.GetChild(i);
            Transform newChild = newTransform.Find(originalChild.name);

            if (newChild != null)
            {
                newChild.position = originalChild.position;
                newChild.rotation = originalChild.rotation;
                newChild.localScale = originalChild.localScale;
                MatchChildTransformations(originalChild, newChild);
            }
        }
    }
}
