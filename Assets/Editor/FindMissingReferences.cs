using UnityEngine;
using UnityEditor;
 
public class MissingReferenceFinder : EditorWindow
{
    [MenuItem("Tools/Find Missing References")]
    public static void ShowWindow()
    {
        var objects = GameObject.FindObjectsOfType<GameObject>();
        foreach (var go in objects)
        {
            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogError($"Missing Component in GameObject: {GetFullPath(go)}", go);
                    continue;
                }

                SerializedObject so = new SerializedObject(components[i]);
                var sp = so.GetIterator();
                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
                        {
                            Debug.LogError($"Missing reference in {components[i].GetType()} on GameObject: {GetFullPath(go)}", go);
                        }
                    }
                }
            }
        }
    }

    static string GetFullPath(GameObject go)
    {
        string path = go.name;
        while (go.transform.parent != null)
        {
            go = go.transform.parent.gameObject;
            path = go.name + "/" + path;
        }
        return path;
    }
}
