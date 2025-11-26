#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class RainbowEditorWindows
{
    private static float baseHue = 0f;
    private static bool isActive = false;

    static RainbowEditorWindows()
    {
        if (!isActive) return;

        // Hierarchy / Project
        EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;
        EditorApplication.projectWindowItemOnGUI += DrawProjectItem;

        // Inspector
        Editor.finishedDefaultHeaderGUI += DrawRainbowInspector;

        // SceneView 追加！！
        SceneView.duringSceneGui += OnSceneGUI;

        // アニメーション更新
        EditorApplication.update += () =>
        {
            baseHue += 0.002f;
            if (baseHue > 1f) baseHue = 0f;

            // Repaint
            foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                string typeName = w.GetType().Name;
                if (typeName == "SceneHierarchyWindow" ||
                    typeName == "ProjectBrowser" ||
                    typeName == "InspectorWindow" ||
                    typeName == "SceneView")     // ← 追加
                {
                    w.Repaint();
                }
            }
        };
    }


    // ==================================================
    // SceneView 全体を虹色アニメーション
    // ==================================================
    private static void OnSceneGUI(SceneView sceneView)
    {
        Rect rect = sceneView.position;

        if (rect.width <= 0 || rect.height <= 0) return;

        // GUI座標に変換
        Rect guiRect = new Rect(0, 0, rect.width, rect.height);

        // 上から下へグラデーション
        Handles.BeginGUI();
        for (float y = 0; y < guiRect.height; y += 2f)
        {
            float t = y / guiRect.height;
            float hue = Mathf.Repeat(baseHue + t, 1f);
            Color c = Color.HSVToRGB(hue, 1f, 1f);
            EditorGUI.DrawRect(new Rect(0, y, guiRect.width, 2f), c);
        }
        Handles.EndGUI();
    }


    // ==================================================
    // 既存機能
    // ==================================================

    private static void DrawHierarchyItem(int instanceID, Rect rect)
    {
        DrawRainbowRow(rect);
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj != null)
            EditorGUI.LabelField(rect, obj.name, EditorStyles.label);
    }

    private static void DrawProjectItem(string guid, Rect rect)
    {
        DrawRainbowRow(rect);
    }

    private static void DrawRainbowInspector(Editor editor)
    {
        Rect r = GUILayoutUtility.GetRect(0, 1000, 0, 1000,
            GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        float hue = Mathf.Repeat(baseHue, 1f);
        Color c = Color.HSVToRGB(hue, 1f, 1f);
        EditorGUI.DrawRect(r, c);
    }

    private static void DrawRainbowRow(Rect rect)
    {
        float hue = Mathf.Repeat(baseHue + rect.y * 0.001f, 1f);
        Color c = Color.HSVToRGB(hue, 1f, 1f);
        Rect fullRect = new Rect(0, rect.y, rect.width + 2000, rect.height);
        EditorGUI.DrawRect(fullRect, c);
    }
}
#endif
