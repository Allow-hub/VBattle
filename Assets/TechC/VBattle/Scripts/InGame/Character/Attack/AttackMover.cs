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
        
        private GameObject owner;
        private Vector3 currentMoveDir;
        private GameObject character;
        private Rigidbody characterRb; // ★★★ キャラクターのRigidbody ★★★
        // 追従関連
        private Transform characterTransform;

        public void Initialize(GameObject owner)
        {
            this.owner = owner;
        }

        public void OnRelease()
        {
            rb.velocity = Vector3.zero; // リリース時に速度をリセット
            currentMoveDir = Vector3.zero; // 移動方向もリセット
            
            if(characterTransform == null || character == null) return;
            
            // ★★★ Rigidbodyで位置を戻す ★★★
            if (characterRb != null)
            {
                characterRb.MovePosition(characterTransform.position);
            }
            else
            {
                // Rigidbodyがない場合は直接設定（フォールバック）
                character.transform.position = characterTransform.position;
            }
            
            character = null;
            characterTransform = null;
            characterRb = null;
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
                    // 攻撃オブジェクトがキャラクターに追従
                    if (characterTransform != null)
                        rb.MovePosition(characterTransform.position);
                    break;
                case AttackMoverType.CharacterFollowsObject:
                    {
                        // 攻撃オブジェクトを移動
                        Vector3 delta = currentMoveDir.normalized * moveSpeed * deltaTime;
                        rb.MovePosition(rb.position + delta);
                        
                        // キャラクターを攻撃オブジェクトに追従させる
                        if(character != null)
                        {
                            Vector3 targetPos = new Vector3(
                                rb.position.x, 
                                rb.position.y + characterYOffset, 
                                characterTransform.position.z
                            );
                            
                            // ★★★ Rigidbodyで移動（物理演算を尊重） ★★★
                            if (characterRb != null)
                            {
                                Debug.Log("MO");
                                characterRb.MovePosition(targetPos);
                            }
                            else
                            {
                                // Rigidbodyがない場合のフォールバック
                                character.transform.position = targetPos;
                            }
                        }
                    }
                    break;
            }
        }

        public void Activate(GameObject character)
        {
            if (rb == null) return;
            
            this.character = character;
            
            characterRb = character.GetComponent<Rigidbody>();
            
            if (characterRb == null)
            {
                Debug.LogWarning($"Character {character.name} does not have a Rigidbody component. " +
                               "AttackMover will use direct transform manipulation as fallback.");
            }
            
            if (moverType == AttackMoverType.CharacterFollowsObject || moverType == AttackMoverType.FollowCharacter)
                characterTransform = character.transform;
            
            if(characterTransform != null)
            {
                // 攻撃オブジェクトをキャラクター位置にセット
                owner.transform.position = characterTransform.position;
            }
            
            // キャラクターの前方向に移動方向を設定
            currentMoveDir = new Vector3(
                moveDir.x * character.transform.forward.x, 
                moveDir.y, 
                moveDir.z
            );
        }
    }

    public enum AttackMoverType
    {
        None,
        FollowCharacter,
        CharacterFollowsObject,
    }
}