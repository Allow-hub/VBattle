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
        if(!isActive)return ;
        // Hierarchy / Project の行描画にフック
        EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;
        EditorApplication.projectWindowItemOnGUI += DrawProjectItem;

        // Inspector の描画
        Editor.finishedDefaultHeaderGUI += DrawRainbowInspector;

        // アニメーション用 Repaint
        EditorApplication.update += () =>
        {
            baseHue += 0.002f; // アニメ速度
            if (baseHue > 1f) baseHue = 0f;

            // Hierarchy / Project の Repaint
            foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                string typeName = w.GetType().Name;
                if (typeName == "SceneHierarchyWindow" || typeName == "ProjectBrowser")
                    w.Repaint();
            }

            // Inspector の Repaint
            foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                if (w.GetType().Name == "InspectorWindow")
                    w.Repaint();
            }
        };
    }

    // -----------------------------
    // Hierarchy 要素単位
    // -----------------------------
    private static void DrawHierarchyItem(int instanceID, Rect rect)
    {
        DrawRainbowRow(rect);
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj != null)
            EditorGUI.LabelField(rect, obj.name, EditorStyles.label);
    }

    // -----------------------------
    // Project 要素単位
    // -----------------------------
    private static void DrawProjectItem(string guid, Rect rect)
    {
        DrawRainbowRow(rect);
    }

    // -----------------------------
    // Inspector ウィンドウ（全体）
    // -----------------------------
    private static void DrawRainbowInspector(Editor editor)
    {
        Rect r = GUILayoutUtility.GetRect(0, 1000, 0, 1000, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        float hue = Mathf.Repeat(baseHue, 1f);
        Color c = Color.HSVToRGB(hue, 1f, 1f);
        EditorGUI.DrawRect(r, c);
    }

    // -----------------------------
    // 共通：行を虹色に描く
    // -----------------------------
    private static void DrawRainbowRow(Rect rect)
    {
        // 行ごとに hue をオフセット
        float hue = Mathf.Repeat(baseHue + rect.y * 0.001f, 1f);
        Color c = Color.HSVToRGB(hue, 1f, 1f);
        Rect fullRect = new Rect(0, rect.y, rect.width + 2000, rect.height); // 横幅大きめ
        EditorGUI.DrawRect(fullRect, c);
    }
}
#endif
