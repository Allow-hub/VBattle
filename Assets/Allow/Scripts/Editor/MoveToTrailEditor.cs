using System;
using UnityEngine;
using UnityEditor;

namespace TechC
{
    [CustomEditor(typeof(MoveToTrail))]
    [CanEditMultipleObjects]
    public class MoveToTrailEditor : Editor
    {
        // 元々は初期状態で自動的に同期方向を決める機能がありましたが、
        // マテリアルのタイルリング値や Material Data の値を変更しても問題なく動作するため、
        // Revert や Renderer の変更などの例外処理を全てコードで書くと複雑になりすぎる
        // そのため、作業者が手動で同期方向を決める方式に変更
        // 機能封印 // SerializedProperty m_overrideMaterial_sp; // デフォルトではマテリアル値を優先しますが、チェックをオンにすると Material Data の値が優先され、マテリアルのタイルリングを上書き

        SerializedProperty m_moveObject_sp;
        SerializedProperty m_shaderPropertyName_sp;
        SerializedProperty m_shaderPropertyID_sp;
        SerializedProperty m_materialData_sp;

        private MoveToTrail m_mttuv;

        private void OnEnable()
        {
            // 機能封印 // m_overrideMaterial_sp = serializedObject.FindProperty("m_overrideMaterial");
            m_moveObject_sp = serializedObject.FindProperty("m_moveObject");
            m_shaderPropertyName_sp = serializedObject.FindProperty("m_shaderPropertyName");
            m_shaderPropertyID_sp = serializedObject.FindProperty("m_shaderPropertyID");
            m_materialData_sp = serializedObject.FindProperty("m_materialData");

            m_mttuv = target as MoveToTrail;

            serializedObject.Update();
            InitializeEditor();
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable()
        {
            serializedObject.Update(); // 最新状態を反映
                                       // 全てのマテリアルの _MoveToMaterialUV を 0 にリセット
                                       // なぜこれをするか？ シェーダープロパティが存在しなくても、場合によってマテリアルの Saved Property に存在することがあり、
                                       // それによって常に Dirty 状態になるのを防ぐため。シェーダーや ShaderGraph で一度でもインスペクターに Expose されると、マテリアルにプロパティが保存される。
                                       // Saved Property が存在しても、ユーザー操作による Dirty を完全に防ぐ方法は Unity API では未確認。
            for (int i = 0; i < m_materialData_sp.arraySize; i++)
            {
                SerializedProperty materialDataElement_sp = m_materialData_sp.GetArrayElementAtIndex(i);
                TrailRenderer trailRenderer = (TrailRenderer)materialDataElement_sp.FindPropertyRelative("m_trailRenderer").objectReferenceValue;
                if (trailRenderer != null)
                {
                    Material mat = trailRenderer.sharedMaterial;
                    if (mat != null)
                    {
                        mat.SetFloat(m_shaderPropertyID_sp.intValue, 0f);
                    }
                }
            }
            // serializedObject.ApplyModifiedProperties(); // マテリアル変更のみなので ApplyModifiedProperties は不要
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int checkTrailRenderertile = CheckTrailRendererTile();
            if (checkTrailRenderertile != -1)
            {
                string message = String.Format("Element {0} の Trail Renderer の Texture Mode が Tile ではありません。", checkTrailRenderertile);
                EditorGUILayout.HelpBox(message, MessageType.Warning);
            }

            string checkTrailRendererShader = CheckTrailRendererShader();
            if (checkTrailRendererShader != "")
            {
                EditorGUILayout.HelpBox(checkTrailRendererShader, MessageType.Warning);
            }

            EditorGUI.BeginChangeCheck();
            {
                // 機能封印 // m_overrideMaterial_sp.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Override Material", "UV Tiling 値の変更方向を決定します。チェックをオンにすると MoveToTrail コンポーネントの値が優先、オフにするとマテリアルの値が優先"), m_overrideMaterial_sp.boolValue);
                EditorGUILayout.PropertyField(m_moveObject_sp);
                EditorGUILayout.PropertyField(m_shaderPropertyName_sp);
                EditorGUILayout.PropertyField(m_materialData_sp);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "MoveToTrail changed");
                InitializeEditor(); // 変更があれば初期化
            }

            for (int i = 0; i < m_materialData_sp.arraySize; i++)
            {
                SerializedProperty materialDataElement_sp = m_materialData_sp.GetArrayElementAtIndex(i);
                SyncElementTiling(materialDataElement_sp); // 変更がなくても定期的にマテリアルと同期
            }
            serializedObject.ApplyModifiedProperties();
            // base.OnInspectorGUI();
        }

        // マテリアルのタイルリングのみ同期
        private void SyncElementTiling(SerializedProperty materialDataElement_sp)
        {
            TrailRenderer trailRenderer = (TrailRenderer)materialDataElement_sp.FindPropertyRelative("m_trailRenderer").objectReferenceValue;
            if (trailRenderer != null)
            {
                Material mat = trailRenderer.sharedMaterial;
                if (mat != null)
                {
                    if (materialDataElement_sp.FindPropertyRelative("m_uvTiling").vector2Value != mat.mainTextureScale)
                    {
                        // 機能封印 //
                        // if (m_overrideMaterial_sp.boolValue)
                        if (false)
                        {
                            // MoveToTrail の値を優先してマテリアルを変更
                            // mat.mainTextureScale = materialDataElement_sp.FindPropertyRelative("m_uvTiling").vector2Value;
                        }
                        else
                        {
                            // マテリアルを基準に MoveToTrail データを変更
                            materialDataElement_sp.FindPropertyRelative("m_uvTiling").vector2Value = mat.mainTextureScale;
                        }
                    }
                }
            }
        }

        // エディター用初期化。Undo などに対応
        private void InitializeEditor()
        {
            if (m_materialData_sp.serializedObject.targetObject == null || m_materialData_sp.arraySize == 0)
                return;

            m_shaderPropertyID_sp.intValue = Shader.PropertyToID(m_shaderPropertyName_sp.stringValue);

            // m_materialData 初期化
            for (int i = 0; i < m_materialData_sp.arraySize; i++)
            {
                SerializedProperty materialDataElement_sp = m_materialData_sp.GetArrayElementAtIndex(i);
                SyncElementTiling(materialDataElement_sp); // Tiling を同期

                // m_move を初期化
                materialDataElement_sp.FindPropertyRelative("m_move").floatValue = 0f;
            }
        }

        // Trail Renderer の Texture Mode が Tile になっているかチェック
        // 問題なければ -1、問題あれば該当番号を返す
        private int CheckTrailRendererTile()
        {
            if (m_mttuv.m_materialData.Length == 0)
                return -1;

            for (int i = 0; i < m_mttuv.m_materialData.Length; i++)
            {
                if (m_mttuv.m_materialData[i].m_trailRenderer == null)
                    continue;
                TrailRenderer trailRenderer = (TrailRenderer)m_mttuv.m_materialData[i].m_trailRenderer;
                if (trailRenderer.textureMode != LineTextureMode.Tile)
                {
                    return i;
                }
            }
            return -1;
        }

        // Trail Renderer のシェーダーやマテリアル状態をチェック
        private string CheckTrailRendererShader()
        {
            if (m_mttuv.m_materialData.Length == 0)
                return "";

            for (int i = 0; i < m_mttuv.m_materialData.Length; i++)
            {
                TrailRenderer trailRenderer = m_mttuv.m_materialData[i].m_trailRenderer;
                if (trailRenderer == null)
                    continue;
                Material mat = trailRenderer.sharedMaterial;
                // マテリアルが null かチェック
                if (mat == null)
                {
                    string message = String.Format("Element {0} のレンダラーにマテリアルが設定されていません。", i);
                    return message;
                }

                // 必要に応じて追加チェックも可能
            }

            return "";
        }
    }
}