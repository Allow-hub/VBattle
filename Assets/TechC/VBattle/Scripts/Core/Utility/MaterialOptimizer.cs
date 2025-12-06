using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.Core.Util
{
    /// <summary>
    /// マテリアル最適化ツール
    /// </summary>
    public class MaterialOptimizer : MonoBehaviour
    {
        [Header("テクスチャアトラス設定")]
        [Tooltip("生成するアトラスの最大サイズ")]
        [SerializeField] private int atlasSize = 4096;

        [Tooltip("アトラスのパディング")]
        [SerializeField] private int padding = 2;

        [Header("マテリアル設定")]
        [Tooltip("使用するシェーダー")]
        [SerializeField] private Shader targetShader;

        [Tooltip("GPU Instancingを有効化")]
        [SerializeField] private bool enableGPUInstancing = true;

        [Header("最適化対象")]
        [Tooltip("最適化するレンダラー")]
        [SerializeField] private Renderer[] targetRenderers;

        [Tooltip("子オブジェクトも含める")]
        [SerializeField] private bool includeChildren = true;

        [Header("簡易モード")]
        [Tooltip("テクスチャアトラスを作らず、GPU Instancingのみ有効化")]
        [SerializeField] private bool simpleMode = false;

        private Dictionary<Texture2D, Rect> textureToRect = new Dictionary<Texture2D, Rect>();
        private Texture2D atlasTexture;
        private Material combinedMaterial;

        [ContextMenu("マテリアルを最適化")]
        public void OptimizeMaterials()
        {
            if (simpleMode)
            {
                EnableGPUInstancing();
                return;
            }

            List<Renderer> renderers = new List<Renderer>();

            if (targetRenderers != null && targetRenderers.Length > 0)
            {
                renderers.AddRange(targetRenderers);
            }

            if (includeChildren)
            {
                renderers.AddRange(GetComponentsInChildren<Renderer>());
            }

            if (renderers.Count == 0)
            {
                Debug.LogWarning("最適化対象のレンダラーが見つかりません");
                return;
            }

            Debug.Log($"マテリアル最適化開始: {renderers.Count}個のレンダラーを処理");

            // 1. テクスチャを収集
            List<Texture2D> textures = CollectTextures(renderers);
            Debug.Log($"収集したテクスチャ: {textures.Count}個");

            if (textures.Count == 0)
            {
                Debug.LogWarning("テクスチャが見つかりません。GPU Instancingのみ有効化します。");
                EnableGPUInstancing();
                return;
            }

            // 2. テクスチャの読み込み可能化
            List<Texture2D> readableTextures = MakeTexturesReadable(textures);

            // 3. テクスチャアトラスを作成
            if (!CreateTextureAtlas(readableTextures))
            {
                Debug.LogError("テクスチャアトラス作成に失敗しました");
                return;
            }

            // 4. 統合マテリアルを作成
            CreateCombinedMaterial();

            // 5. UVを再計算してマテリアルを適用
            ApplyOptimizedMaterials(renderers);

            Debug.Log("マテリアル最適化完了！");
        }

        /// <summary>
        /// テクスチャを収集
        /// </summary>
        List<Texture2D> CollectTextures(List<Renderer> renderers)
        {
            HashSet<Texture2D> textureSet = new HashSet<Texture2D>();

            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;

                    // メインテクスチャを取得
                    if (material.mainTexture is Texture2D mainTex)
                    {
                        textureSet.Add(mainTex);
                    }
                }
            }

            return new List<Texture2D>(textureSet);
        }

        /// <summary>
        /// テクスチャを読み込み可能にする
        /// </summary>
        List<Texture2D> MakeTexturesReadable(List<Texture2D> textures)
        {
            List<Texture2D> readableTextures = new List<Texture2D>();

            foreach (var tex in textures)
            {
                try
                {
                    // 既に読み込み可能な場合
                    if (tex.isReadable)
                    {
                        readableTextures.Add(tex);
                        continue;
                    }

                    // RenderTextureを使って読み込み可能なコピーを作成
                    RenderTexture tmp = RenderTexture.GetTemporary(
                        tex.width,
                        tex.height,
                        0,
                        RenderTextureFormat.ARGB32,
                        RenderTextureReadWrite.Linear
                    );

                    Graphics.Blit(tex, tmp);
                    RenderTexture previous = RenderTexture.active;
                    RenderTexture.active = tmp;

                    Texture2D readableTex = new Texture2D(tex.width, tex.height);
                    readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                    readableTex.Apply();

                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(tmp);

                    readableTextures.Add(readableTex);
                    Debug.Log($"テクスチャを読み込み可能に変換: {tex.name}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"テクスチャ変換エラー ({tex.name}): {e.Message}");
                }
            }

            return readableTextures;
        }

        /// <summary>
        /// テクスチャアトラスを作成
        /// </summary>
        bool CreateTextureAtlas(List<Texture2D> textures)
        {
            if (textures.Count == 0)
            {
                Debug.LogWarning("アトラスに含めるテクスチャがありません");
                return false;
            }

            try
            {
                // Texture2D配列を作成
                Texture2D[] textureArray = textures.ToArray();

                // アトラスを作成
                atlasTexture = new Texture2D(atlasSize, atlasSize);
                Rect[] uvRects = atlasTexture.PackTextures(textureArray, padding, atlasSize);

                if (uvRects == null || uvRects.Length == 0)
                {
                    Debug.LogError("PackTexturesが失敗しました");
                    return false;
                }

                // テクスチャとUV座標のマッピングを保存
                textureToRect.Clear();
                for (int i = 0; i < textures.Count; i++)
                {
                    textureToRect[textures[i]] = uvRects[i];
                    Debug.Log($"テクスチャ {textures[i].name}: UV Rect = {uvRects[i]}");
                }

                Debug.Log($"テクスチャアトラス作成完了: {atlasSize}x{atlasSize}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"テクスチャアトラス作成エラー: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 統合マテリアルを作成
        /// </summary>
        void CreateCombinedMaterial()
        {
            if (targetShader == null)
            {
                targetShader = Shader.Find("Standard");
            }

            combinedMaterial = new Material(targetShader);
            combinedMaterial.name = "CombinedMaterial";
            combinedMaterial.mainTexture = atlasTexture;

            // GPU Instancingを有効化
            if (enableGPUInstancing)
            {
                combinedMaterial.enableInstancing = true;
            }

            Debug.Log("統合マテリアル作成完了");
        }

        /// <summary>
        /// 最適化されたマテリアルを適用（UV変換なし版）
        /// </summary>
        void ApplyOptimizedMaterials(List<Renderer> renderers)
        {
            int appliedCount = 0;

            foreach (var renderer in renderers)
            {
                // 統合マテリアルを適用
                Material[] materials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = combinedMaterial;
                }
                renderer.sharedMaterials = materials;
                appliedCount++;
            }

            Debug.Log($"マテリアル適用完了: {appliedCount}個のレンダラー");
            Debug.LogWarning("注意: UV座標は変換されていません。テクスチャの配置が正しくない場合があります。");
        }

        [ContextMenu("マテリアル数を確認")]
        public void CheckMaterialCount()
        {
            List<Renderer> renderers = new List<Renderer>();
            
            if (includeChildren)
            {
                renderers.AddRange(GetComponentsInChildren<Renderer>());
            }

            HashSet<Material> materials = new HashSet<Material>();
            HashSet<Texture2D> textures = new HashSet<Texture2D>();
            HashSet<Shader> shaders = new HashSet<Shader>();

            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null)
                    {
                        materials.Add(mat);
                        shaders.Add(mat.shader);
                        if (mat.mainTexture is Texture2D tex)
                        {
                            textures.Add(tex);
                        }
                    }
                }
            }

            Debug.Log($"=== マテリアル統計 ===");
            Debug.Log($"レンダラー数: {renderers.Count}");
            Debug.Log($"ユニークなマテリアル数: {materials.Count}");
            Debug.Log($"ユニークなテクスチャ数: {textures.Count}");
            Debug.Log($"ユニークなシェーダー数: {shaders.Count}");
            
            // シェーダー別にカウント
            Dictionary<Shader, int> shaderCount = new Dictionary<Shader, int>();
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null)
                    {
                        if (!shaderCount.ContainsKey(mat.shader))
                            shaderCount[mat.shader] = 0;
                        shaderCount[mat.shader]++;
                    }
                }
            }

            Debug.Log("=== シェーダー別使用数 ===");
            foreach (var kvp in shaderCount)
            {
                Debug.Log($"{kvp.Key.name}: {kvp.Value}回");
            }
        }

        [ContextMenu("GPU Instancingを有効化")]
        public void EnableGPUInstancing()
        {
            List<Renderer> renderers = new List<Renderer>();
            
            if (includeChildren)
            {
                renderers.AddRange(GetComponentsInChildren<Renderer>());
            }

            int count = 0;
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null && !mat.enableInstancing)
                    {
                        mat.enableInstancing = true;
                        count++;
                    }
                }
            }

            Debug.Log($"GPU Instancingを有効化: {count}個のマテリアル");
        }

        [ContextMenu("同じマテリアルに統一")]
        public void UnifyMaterials()
        {
            List<Renderer> renderers = new List<Renderer>();
            
            if (includeChildren)
            {
                renderers.AddRange(GetComponentsInChildren<Renderer>());
            }

            // 最も使われているマテリアルを見つける
            Dictionary<Material, int> materialCount = new Dictionary<Material, int>();
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null)
                    {
                        if (!materialCount.ContainsKey(mat))
                            materialCount[mat] = 0;
                        materialCount[mat]++;
                    }
                }
            }

            Material mostUsedMaterial = null;
            int maxCount = 0;
            foreach (var kvp in materialCount)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    mostUsedMaterial = kvp.Key;
                }
            }

            if (mostUsedMaterial == null)
            {
                Debug.LogWarning("統一するマテリアルが見つかりません");
                return;
            }

            // GPU Instancingを有効化
            if (!mostUsedMaterial.enableInstancing)
            {
                mostUsedMaterial.enableInstancing = true;
            }

            // 全てのレンダラーに同じマテリアルを適用
            int appliedCount = 0;
            foreach (var renderer in renderers)
            {
                Material[] materials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = mostUsedMaterial;
                }
                renderer.sharedMaterials = materials;
                appliedCount++;
            }

            Debug.Log($"マテリアル統一完了: {appliedCount}個のレンダラーに '{mostUsedMaterial.name}' を適用");
        }

#if UNITY_EDITOR
        [ContextMenu("アトラスを保存")]
        public void SaveAtlas()
        {
            if (atlasTexture == null)
            {
                Debug.LogWarning("アトラステクスチャがありません。先に最適化を実行してください。");
                return;
            }

            string path = "Assets/CombinedTextures/";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            // テクスチャを保存
            byte[] bytes = atlasTexture.EncodeToPNG();
            string texturePath = path + "Atlas_" + System.DateTime.Now.Ticks + ".png";
            System.IO.File.WriteAllBytes(texturePath, bytes);

            // マテリアルを保存
            if (combinedMaterial != null)
            {
                string materialPath = path + "CombinedMaterial_" + System.DateTime.Now.Ticks + ".mat";
                UnityEditor.AssetDatabase.CreateAsset(combinedMaterial, materialPath);
            }

            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"アトラスを保存しました: {texturePath}");
        }
#endif
    }
}