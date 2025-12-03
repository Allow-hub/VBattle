using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.CommentSystem
{
    /// <summary>
    /// コメントのマテリアル適用処理を担当
    /// </summary>
    [Serializable]
    public class CommentMaterialApplier
    {
        [Header("コメントのマテリアル")]
        [SerializeField] private Material normalCommentMaterial;
        [SerializeField] private Material speedBuffCommentMaterial;
        [SerializeField] private Material attackBuffCommentMaterial;
        [SerializeField] private Material mapChangeCommentMaterial;
        [SerializeField] private Material freezeCommentMaterial;

        private MaterialPropertyBlock propertyBlock;
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        private Dictionary<Material, Color> materialColorCache = new Dictionary<Material, Color>();

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            CacheMaterialColors();
        }

        /// <summary>
        /// マテリアルの色を事前取得してキャッシュ
        /// </summary>
        private void CacheMaterialColors()
        {
            Material[] materials = {
                normalCommentMaterial,
                speedBuffCommentMaterial,
                attackBuffCommentMaterial,
                mapChangeCommentMaterial,
                freezeCommentMaterial
            };

            foreach (var material in materials)
            {
                if (material != null)
                {
                    if (material.HasProperty(ColorPropertyId))
                    {
                        Color color = material.GetColor(ColorPropertyId);
                        materialColorCache[material] = color;
                    }
                }
            }
        }


        /// <summary>
        /// コメントタイプに応じたMaterialを取得
        /// </summary>
        public Material GetCommentMaterial(CommentType? commentType)
        {
            if (commentType != null)
            {
                switch (commentType)
                {
                    case CommentType.AttackBuff:
                        return attackBuffCommentMaterial;
                    case CommentType.SpeedBuff:
                        return speedBuffCommentMaterial;
                    case CommentType.MapChange:
                        return mapChangeCommentMaterial;
                    case CommentType.Freeze:
                        return freezeCommentMaterial;
                    case CommentType.Normal:
                    default:
                        return normalCommentMaterial;
                }
            }
            return normalCommentMaterial;
        }

        /// <summary>
        /// 文字オブジェクトリストに Material を適用
        /// </summary>
        public void ApplyMaterialToCharacters(List<GameObject> characters, Material material)
        {
            if (characters == null || material == null)
            {
                Debug.LogWarning("characters または material が null です");
                return;
            }

            if (propertyBlock == null)
            {
                Init();
            }

            propertyBlock.Clear();

            if (materialColorCache.TryGetValue(material, out Color cachedColor))
            {
                propertyBlock.SetColor(ColorPropertyId, cachedColor);
            }

            foreach (var charObj in characters)
            {
                if (charObj == null) continue;

                var renderer = charObj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = material;
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }

        /// <summary>
        /// フリーズエフェクト専用の適用メソッド
        /// </summary>
        public void ApplyFreezeEffectToCharacters(List<GameObject> characters, Material originalMaterial)
        {
            if (characters == null)
            {
                Debug.LogWarning("characters が null です");
                return;
            }

            if (propertyBlock == null)
            {
                Init();
            }

            propertyBlock.Clear();

            if (materialColorCache.TryGetValue(freezeCommentMaterial, out Color freezeColor))
            {
                propertyBlock.SetColor(ColorPropertyId, freezeColor);
            }

            foreach (var charObj in characters)
            {
                if (charObj == null) continue;

                var renderer = charObj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = freezeCommentMaterial;
                    renderer.SetPropertyBlock(propertyBlock);
                }
            }
        }
    }
}
