using UnityEngine;

namespace TechC.VBattle.InGame.Gimmick
{
    /// <summary>
    /// ウィンドウギミックの当たり判定用
    /// </summary>
    public class WindowCol : MonoBehaviour
    {
        [SerializeField] private float bounceForce = 5f;
        [SerializeField] private bool useContactNormal = true; // 接触法線を使用するか
        private bool isBounced = false;

        private void OnEnable()
        {
            isBounced = false;
        }

        private void OnCollisionEnter(Collision col)
        {
            if (isBounced) return;
            if (!col.gameObject.CompareTag("Player")) return;
            var rb = col.gameObject.GetComponent<Rigidbody>();
            if (rb == null) return;
            Vector3 bounceDirection;

            if (useContactNormal && col.contactCount > 0)
                bounceDirection = col.contacts[0].normal;// 接触点の法線方向（接触面と垂直な方向）
            else
            {
                // プレイヤーからウィンドウへのベクトルの逆方向
                Vector3 collisionDirection = (transform.position - col.transform.position).normalized;
                bounceDirection = -collisionDirection;
            }

            // 力を加える
            rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
            isBounced = true;
        }
    }
}