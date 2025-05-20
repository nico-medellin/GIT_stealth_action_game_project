using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MissingReferenceTreeView : EditorWindow
{
    private Vector2 scroll;
    private Dictionary<GameObject, List<string>> missingMap = new();

    [MenuItem("Tools/Missing Reference Tree View")]
    public static void ShowWindow()
    {
        GetWindow<MissingReferenceTreeView>("Missing References");
    }

    private void OnFocus()
    {
        ScanScene();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Rescan Scene"))
        {
            ScanScene();
        }

        EditorGUILayout.Space();

        if (missingMap.Count == 0)
        {
            EditorGUILayout.LabelField("No missing references found.");
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var entry in missingMap)
        {
            var go = entry.Key;
            if (GUILayout.Button(GetFullPath(go), EditorStyles.foldoutHeader))
            {
                Selection.activeGameObject = go;
                EditorGUIUtility.PingObject(go);
            }

            foreach (var issue in entry.Value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("• " + issue, EditorStyles.helpBox);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
    }

    private void ScanScene()
    {
        missingMap.Clear();
        var allGameObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (var go in allGameObjects)
        {
            var issues = new List<string>();
            var components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                var c = components[i];

                if (c == null)
                {
                    issues.Add($"Missing Component at index {i}");
                    continue;
                }

                SerializedObject so = new SerializedObject(c);
                var sp = so.GetIterator();

                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
                        {
                            issues.Add($"Missing reference: {sp.displayName} in {c.GetType().Name}");
                        }
                    }
                }
            }

            if (issues.Count > 0)
            {
                missingMap.Add(go, issues);
            }
        }
    }

    private string GetFullPath(GameObject go)
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
