using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 攻撃オブジェクトの移動処理
    /// AttackObjectControllerが実行を管理
    /// </summary>
    [System.Serializable]
    public class AttackMover : IAttackBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private Vector3 moveDir = Vector3.forward;
        private Vector3 currentMoveDir;
        // 追従関連
        [SerializeField] private bool followCharacter = false;
        private Transform characterTransform;
        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
            rb.velocity = Vector3.zero; // リリース時に速度をリセット
            currentMoveDir = Vector3.zero; // 移動方向もリセット
            characterTransform = null;
        }

        public void OnUpdate(float deltaTime)
        {
            if (rb == null) return;
            if (followCharacter && characterTransform != null)
                rb.MovePosition(characterTransform.position);// キャラの位置に追従
            else
            {
                Vector3 delta = currentMoveDir.normalized * moveSpeed * deltaTime;
                rb.MovePosition(rb.position + delta);
            }
        }

        public void Activate(GameObject character)
        {
            if (rb == null) return;
            if (followCharacter)
                characterTransform = character.transform;
            currentMoveDir = new Vector3(moveDir.x * character.transform.forward.x, moveDir.y, moveDir.z); // キャラクターの前方向に移動
        }
    }
}