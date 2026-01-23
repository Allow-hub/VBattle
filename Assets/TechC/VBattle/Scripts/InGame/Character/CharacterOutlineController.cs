using UnityEngine;
using TechC.VBattle.Core.Extensions;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// キャラクターのアウトライン表示を管理する内部コントローラー
    /// </summary>
    [System.Serializable]
    public class CharacterOutlineController
    {
        private const int PLAYER_1_INDEX = 1;
        private const float ALPHA_OPAQUE = 1f;
        private const int OUTLINE_MATERIAL_COUNT = 1;
        
        [Header("アウトライン設定")]
        [SerializeField] private Material outlineMaterialBase;
        [SerializeField] private Color player1OutlineColor = new Color(0.26f, 1f, 0.99f, 1f);
        [SerializeField] private Color player2OutlineColor = new Color(1f, 0.68f, 0.25f, 1f);
        [SerializeField] private SkinnedMeshRenderer[] targetRenderers;

        private MaterialPropertyBlock propertyBlock;
        private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
        private bool isApplied = false;

        /// <summary>
        /// プレイヤーIDに応じたアウトラインを適用
        /// </summary>
        public void ApplyOutline(int playerIndex)
        {
            if (isApplied)
            {
                CustomLogger.Warning("[CharacterOutlineController] Outline already applied");
                return;
            }

            if (outlineMaterialBase == null || targetRenderers == null || targetRenderers.Length == 0)
            {
                CustomLogger.Error("[CharacterOutlineController] Invalid configuration");
                return;
            }

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            Color targetColor = playerIndex == PLAYER_1_INDEX ? player1OutlineColor : player2OutlineColor;
            targetColor.a = ALPHA_OPAQUE;
            propertyBlock.SetColor(OutlineColorID, targetColor);

            foreach (var smr in targetRenderers)
            {
                if (smr == null) continue;

                Material[] materials = smr.sharedMaterials;

                if (materials.Length == 0 || materials[materials.Length - 1] != outlineMaterialBase)
                {
                    int currentLength = materials.Length;
                    System.Array.Resize(ref materials, materials.Length + OUTLINE_MATERIAL_COUNT);
                    materials[currentLength] = outlineMaterialBase;
                    smr.materials = materials;
                }

                int outlineIndex = materials.Length - 1;
                smr.SetPropertyBlock(propertyBlock, outlineIndex);
            }

            isApplied = true;
        }

        /// <summary>リソースのクリーンアップ</summary>
        public void Cleanup()
        {
            propertyBlock = null;
            isApplied = false;
        }
    }
}
