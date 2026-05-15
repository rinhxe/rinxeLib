using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public class RinxeTools : EditorWindow
{
    private const string SCRIPT_FOLDER = "Assets/Script";

    private List<Type> _types = new List<Type>();
    private string[] _typeNames = new string[0];
    private int _selectedIndex = 0;

    [MenuItem("Rinxe/Select By Script")]
    public static void ShowWindow() => GetWindow<RinxeTools>("Rinxe Tools");

    private void OnEnable() => RefreshTypes();

    private void RefreshTypes()
    {
        _types.Clear();

        var guids = AssetDatabase.FindAssets("t:MonoScript", new[] { SCRIPT_FOLDER });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (monoScript == null) continue;

            var type = monoScript.GetClass();
            if (type == null) continue;
            if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;

            _types.Add(type);
        }

        _types = _types.OrderBy(t => t.Name).ToList();
        _typeNames = _types.Select(t => t.Name).ToArray();
        _selectedIndex = Mathf.Clamp(_selectedIndex, 0, Mathf.Max(0, _types.Count - 1));
    }

    private void OnGUI()
    {
        GUILayout.Label("Select By Script", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        if (_types.Count == 0)
        {
            EditorGUILayout.HelpBox("No MonoBehaviour scripts found in: " + SCRIPT_FOLDER, MessageType.Info);
            if (GUILayout.Button("Refresh")) RefreshTypes();
            return;
        }

        EditorGUILayout.BeginHorizontal();
        _selectedIndex = EditorGUILayout.Popup("Script", _selectedIndex, _typeNames);
        if (GUILayout.Button("↺", GUILayout.Width(28))) RefreshTypes();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        if (GUILayout.Button("Select All in Scene", GUILayout.Height(30)))
            SelectAllWithType(_types[_selectedIndex]);
    }

    private void SelectAllWithType(Type type)
    {
        var allObjects = Resources.FindObjectsOfTypeAll(type);

        var found = allObjects
            .OfType<Component>()
            .Where(c => c.hideFlags == HideFlags.None && c.gameObject.scene.isLoaded)
            .Select(c => c.gameObject)
            .Distinct()
            .ToArray();

        if (found.Length == 0)
        {
            EditorUtility.DisplayDialog("Rinxe Tools",
                $"Không tìm thấy object nào có script '{type.Name}' trong scene.", "OK");
            return;
        }

        Selection.objects = found;
        Debug.Log($"[RinxeTools] Đã chọn {found.Length} object với '{type.Name}'");
    }
}
