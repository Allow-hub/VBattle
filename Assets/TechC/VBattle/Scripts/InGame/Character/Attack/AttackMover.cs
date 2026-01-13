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
        [SerializeField] private AttackMoverType moverType = AttackMoverType.None;
        [SerializeField] private float characterYOffset = 0f; // Y軸のオフセット
        private Vector3 currentMoveDir;
        private GameObject character;
        // 追従関連
        private Transform characterTransform;

        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
            rb.velocity = Vector3.zero; // リリース時に速度をリセット
            currentMoveDir = Vector3.zero; // 移動方向もリセット
            if(characterTransform != null)
                character.transform.position = characterTransform.position;
            character = null;
            characterTransform = null;
        }

        public void OnUpdate(float deltaTime)
        {
            if (rb == null) return;
            switch (moverType)
            {
                case AttackMoverType.None:
                    {
                        Vector3 delta = currentMoveDir.normalized * moveSpeed * deltaTime;
                        rb.MovePosition(rb.position + delta);
                    }
                    return;
                case AttackMoverType.FollowCharacter:
                    rb.MovePosition(characterTransform.position);// キャラの位置に追従
                    break;
                case AttackMoverType.CharacterFollowsObject:
                    {
                        Vector3 delta = currentMoveDir.normalized * moveSpeed * deltaTime;
                        rb.MovePosition(rb.position + delta);
                        if(character != null)
                        {
                            Vector3 targetPos = new Vector3(rb.position.x, rb.position.y, characterTransform.position.z);
                            targetPos.y += characterYOffset; // Y軸オフセット
                            character.transform.position = targetPos;
                        }
                    }
                    break;
            }
        }

        public void Activate(GameObject character)
        {
            if (rb == null) return;
            if (moverType == AttackMoverType.CharacterFollowsObject)
                characterTransform = character.transform;
            this.character = character;
            currentMoveDir = new Vector3(moveDir.x * character.transform.forward.x, moveDir.y, moveDir.z); // キャラクターの前方向に移動
        }
    }

    public enum AttackMoverType
    {
        None,
        FollowCharacter,
        CharacterFollowsObject,
    }
}