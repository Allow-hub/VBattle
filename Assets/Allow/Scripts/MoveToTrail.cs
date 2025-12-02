using UnityEngine;

namespace TechC
{
    // Trail Renderer の先端が動いた距離をマテリアルの UV スクロールに反映するスクリプト
    // Trail Renderer の Texture Mode が Tile に設定されている必要があります
    // マテリアルに渡される値は 0～1 の範囲になります
    [ExecuteAlways]
    public class MoveToTrail : MonoBehaviour
    {
        [System.Serializable]
        public struct MaterialData
        {
            public MaterialData(TrailRenderer trailRenderer, Material material, Vector2 uvScale, float move)
            {
                m_trailRenderer = trailRenderer;
                m_uvTiling = uvScale;
                m_move = move;
            }

            public TrailRenderer m_trailRenderer;
            [HideInInspector] public Vector2 m_uvTiling; // UV タイルのスケール
            [HideInInspector] public float m_move;       // 移動量（0～1）
        }

#if UNITY_EDITOR
        //public bool m_overrideMaterial = true;
#endif

        public Transform m_moveObject;                  // 移動対象のオブジェクト
        public string m_shaderPropertyName = "_MoveToMaterialUV"; // シェーダーで受け取る UV プロパティ名
        public int m_shaderPropertyID;                 // プロパティ名を使わずに ID でアクセスするための変数
        public MaterialData[] m_materialData = new MaterialData[1] { new MaterialData(null, null, new Vector2(1, 1), 0f) };

        private Vector3 m_beforePosW = Vector3.zero;   // 前フレームのワールド座標

        void Start()
        {
            Initialize();
        }

        void LateUpdate()
        {
            if (m_moveObject == null)
                return;
            if (m_materialData == null || m_materialData.Length == 0)
                return;

            Vector3 nowPosW = m_moveObject.transform.position;
            if (nowPosW == m_beforePosW)
                return; // 位置に変化がなければ処理しない

            float distance = Vector3.Distance(nowPosW, m_beforePosW);
            m_beforePosW = nowPosW;

            for (int i = 0; i < m_materialData.Length; i++)
            {
                if (m_materialData[i].m_trailRenderer == null)
                    continue;

                m_materialData[i].m_move += distance * m_materialData[i].m_uvTiling.x;
                // m_move が大きくなりすぎないように 1 以上は余りを使う
                if (m_materialData[i].m_move > 1f)
                {
                    m_materialData[i].m_move = m_materialData[i].m_move % 1f;
                }

                // プロパティ存在チェックをせずに設定
                // 存在チェックするとマテリアルが毎フレーム変更された扱いになってしまう問題がある
                TrailRenderer trailRenderer = m_materialData[i].m_trailRenderer;
                if (trailRenderer != null)
                {
                    Material mat = trailRenderer.sharedMaterial;
                    if (mat != null)
                    {
                        mat.SetFloat(m_shaderPropertyID, m_materialData[i].m_move);
                    }
                }
            }
        }

        public void Initialize()
        {
            if (m_materialData == null || m_materialData.Length == 0)
                return;

            m_shaderPropertyID = Shader.PropertyToID(m_shaderPropertyName);

            for (int i = 0; i < m_materialData.Length; i++)
            {
                m_materialData[i].m_move = 0f;
                TrailRenderer trailRenderer = m_materialData[i].m_trailRenderer;
                if (trailRenderer != null)
                {
                    Material mat = trailRenderer.sharedMaterial;
                    if (mat != null)
                    {
                        m_materialData[i].m_uvTiling = mat.mainTextureScale; // マテリアルのテクスチャスケールを取得
                    }
                }
            }
        }
    }
}