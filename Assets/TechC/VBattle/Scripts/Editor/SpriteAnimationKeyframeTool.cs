using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace TechC.VBattle.Editor
{
    public class SpriteAnimationKeyframeTool : EditorWindow
    {
        // ターゲットタイプの enum を追加
        private enum TargetType
        {
            SpriteRenderer,
            Image
        }

        private List<Sprite> sprites = new List<Sprite>();
        private AnimationClip animationClip;
        private string propertyPath = "m_Sprite";
        private float duration = 10f;
        private bool useFrameRate = true;
        private int frameRate = 30;
        // ターゲットタイプのフィールドを追加
        private TargetType targetType = TargetType.SpriteRenderer;

        [MenuItem("Tools/Sprite Animation Keyframe Tool")]
        public static void ShowWindow()
        {
            GetWindow<SpriteAnimationKeyframeTool>("Sprite Animation Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("画像アニメーション自動配置ツール", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // アニメーションクリップの設定
            animationClip = (AnimationClip)EditorGUILayout.ObjectField(
                "Animation Clip",
                animationClip,
                typeof(AnimationClip),
                false
            );

            EditorGUILayout.Space();

            // ターゲットタイプの選択を追加
            targetType = (TargetType)EditorGUILayout.EnumPopup("ターゲットタイプ", targetType);

            EditorGUILayout.Space();

            // スプライトリストの表示
            GUILayout.Label($"スプライト数: {sprites.Count}", EditorStyles.helpBox);

            if (GUILayout.Button("スプライトを追加"))
            {
                AddSprites();
            }

            if (GUILayout.Button("スプライトをクリア"))
            {
                sprites.Clear();
            }

            EditorGUILayout.Space();

            // 時間設定
            useFrameRate = EditorGUILayout.Toggle("フレームレートで指定", useFrameRate);

            if (useFrameRate)
            {
                frameRate = EditorGUILayout.IntField("フレームレート (fps)", frameRate);
                if (sprites.Count > 0)
                {
                    float calcDuration = (float)sprites.Count / frameRate;
                    EditorGUILayout.LabelField("計算された長さ", $"{calcDuration:F3} 秒");
                }
            }
            else
            {
                duration = EditorGUILayout.FloatField("アニメーション長さ (秒)", duration);
                if (sprites.Count > 0)
                {
                    float calcFrameRate = sprites.Count / duration;
                    EditorGUILayout.LabelField("計算されたフレームレート", $"{calcFrameRate:F2} fps");
                }
            }

            EditorGUILayout.Space();
            propertyPath = EditorGUILayout.TextField("プロパティパス", propertyPath);

            EditorGUILayout.Space();

            // キーフレーム生成ボタン
            EditorGUI.BeginDisabledGroup(animationClip == null || sprites.Count == 0);
            if (GUILayout.Button("キーフレームを生成", GUILayout.Height(40)))
            {
                GenerateKeyframes();
            }
            EditorGUI.EndDisabledGroup();

            // スプライトリストのスクロール表示
            if (sprites.Count > 0)
            {
                EditorGUILayout.Space();
                GUILayout.Label("登録されたスプライト:", EditorStyles.boldLabel);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                for (int i = 0; i < Mathf.Min(sprites.Count, 10); i++)
                {
                    EditorGUILayout.ObjectField($"[{i}]", sprites[i], typeof(Sprite), false);
                }
                if (sprites.Count > 10)
                {
                    EditorGUILayout.LabelField($"... 他 {sprites.Count - 10} 枚");
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void AddSprites()
        {
            // Project ウィンドウで選択されたオブジェクトを取得
            var selectedObjects = Selection.objects;
            var selectedSprites = new List<Sprite>();
            
            foreach (var obj in selectedObjects)
            {
                // Texture2D の場合、サブアセットとして Sprite を取得
                if (obj is Texture2D texture)
                {
                    string assetPath = AssetDatabase.GetAssetPath(texture);
                    var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                    foreach (var asset in allAssets)
                    {
                        if (asset is Sprite sprite)
                        {
                            selectedSprites.Add(sprite);
                        }
                    }
                }
                // 直接 Sprite が選択されている場合も追加（保険）
                else if (obj is Sprite sprite)
                {
                    selectedSprites.Add(sprite);
                }
            }
            
            if (selectedSprites.Count == 0)
            {
                EditorUtility.DisplayDialog("警告", "Project ウィンドウでスプライトまたはテクスチャを選択してください。", "OK");
                return;
            }

            foreach (var sprite in selectedSprites)
            {
                if (!sprites.Contains(sprite))
                {
                    sprites.Add(sprite);
                }
            }

            // 名前でソート
            sprites = sprites.OrderBy(s => s.name).ToList();
        }

        private void GenerateKeyframes()
        {
            if (animationClip == null || sprites.Count == 0)
            {
                EditorUtility.DisplayDialog("エラー", "Animation ClipとSpritesを設定してください", "OK");
                return;
            }

            // アニメーションの長さを計算
            float actualDuration = useFrameRate ? (float)sprites.Count / frameRate : duration;

            // キーフレームを生成
            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                // ターゲットタイプに応じて type を切り替え
                type = targetType == TargetType.SpriteRenderer ? typeof(SpriteRenderer) : typeof(UnityEngine.UI.Image),
                path = "",
                propertyName = propertyPath
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];

            for (int i = 0; i < sprites.Count; i++)
            {
                float time = (actualDuration / sprites.Count) * i;
                // Sprite の有効性を確認（保険）
                if (sprites[i] == null)
                {
                    EditorUtility.DisplayDialog("エラー", $"スプライト {i} が無効です。再度追加してください。", "OK");
                    return;
                }
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = time,
                    value = sprites[i]
                };
            }

            // アニメーションクリップに設定
            AnimationUtility.SetObjectReferenceCurve(animationClip, spriteBinding, keyframes);

            // アニメーションクリップの長さを設定
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(animationClip);
            settings.stopTime = actualDuration;
            AnimationUtility.SetAnimationClipSettings(animationClip, settings);

            EditorUtility.SetDirty(animationClip);
            AssetDatabase.SaveAssets();

            // アセットデータベースを強制更新（参照を維持）
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "完了",
                $"{sprites.Count}枚のスプライトを{actualDuration:F3}秒のアニメーションに配置しました。",
                "OK"
            );
        }
    }
}
