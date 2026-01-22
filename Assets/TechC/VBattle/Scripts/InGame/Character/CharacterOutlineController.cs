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
        [SerializeField] private Color player1OutlineColor = new Color(0.26f, 1f, 0.99f, 1f); // 水色の初期の色
        [SerializeField] private Color player2OutlineColor = new Color(1f, 0.68f, 0.25f, 1f); //オレンジの初期の色
        [SerializeField] private SkinnedMeshRenderer[] targetRenderers;

        private Material outlineMaterialInstance;

        /// <summary>
        /// プレイヤーIDに応じたアウトラインを適用
        /// </summary>
        public void ApplyOutline(int playerIndex)
        {
            if (outlineMaterialBase == null)
            {
                CustomLogger.Error("[CharacterOutlineController] outlineMaterialBase is null");
                return;
            }

            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                CustomLogger.Error("[CharacterOutlineController] targetRenderers is null or empty");
                return;
            }

            outlineMaterialInstance = Object.Instantiate(outlineMaterialBase);

            Color targetColor = playerIndex == PLAYER_1_INDEX ? player1OutlineColor : player2OutlineColor;
            targetColor.a = ALPHA_OPAQUE;

            if (!outlineMaterialInstance.HasProperty("_OutlineColor"))
            {
                CustomLogger.Error("[CharacterOutlineController] Material does not have _OutlineColor property");
                return;
            }

            outlineMaterialInstance.SetColor("_OutlineColor", targetColor);

            // 各レンダラーのマテリアル配列を拡張してアウトラインを追加
            foreach (var smr in targetRenderers)
            {
                if (smr == null) continue;

                Material[] originalMats = smr.materials;
                Material[] newMats = new Material[originalMats.Length + OUTLINE_MATERIAL_COUNT];

                for (int i = 0; i < originalMats.Length; i++)
                    newMats[i] = originalMats[i];

                newMats[originalMats.Length] = outlineMaterialInstance;
                smr.materials = newMats;
            }
        }

        /// <summary>リソースのクリーンアップ</summary>
        public void Cleanup()
        {
            if (outlineMaterialInstance != null)
                Object.Destroy(outlineMaterialInstance);
        }
    }
}
