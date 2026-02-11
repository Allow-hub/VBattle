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
        [SerializeField] private float followDelay = 0.1f; // 追従のディレイ（秒）
        [SerializeField] private float followSmoothness = 5f; // 追従の滑らかさ（大きいほど素早く追従）

        private GameObject owner;
        private Vector3 currentMoveDir;
        private GameObject character;
        private Rigidbody characterRb;
        private Transform characterTransform;

        // ディレイ用の変数
        private float delayTimer = 0f;
        private Vector3 targetCharacterPos;
        private bool isDelayActive = false;

        public void Initialize(GameObject owner)
        {
            this.owner = owner;
        }

        public void OnRelease()
        {
            rb.velocity = Vector3.zero;
            currentMoveDir = Vector3.zero;
            delayTimer = 0f;
            isDelayActive = false;

            if (characterTransform == null || character == null) return;

            if (characterRb != null)
            {
                characterRb.MovePosition(characterTransform.position);
            }
            else
            {
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
                    if (characterTransform != null)
                        rb.MovePosition(characterTransform.position);
                    break;

                case AttackMoverType.CharacterFollowsObject:
                    {
                        // 攻撃オブジェクトを移動
                        Vector3 delta = currentMoveDir.normalized * moveSpeed * deltaTime;
                        rb.MovePosition(rb.position + delta);

                        // キャラクターを攻撃オブジェクトに追従させる
                        if (character != null && characterTransform != null)
                        {
                            // 目標位置を計算
                            Vector3 newTargetPos = new Vector3(
                                rb.position.x,
                                rb.position.y + characterYOffset,
                                characterTransform.position.z
                            );

                            // ディレイ処理
                            if (!isDelayActive)
                            {
                                // ディレイ開始
                                delayTimer += deltaTime;

                                if (delayTimer >= followDelay)
                                {
                                    // ディレイ時間経過後、追従開始
                                    isDelayActive = true;
                                    targetCharacterPos = newTargetPos;
                                }
                            }
                            else
                            {
                                // ディレイ後、滑らかに追従
                                targetCharacterPos = Vector3.Lerp(
                                    characterTransform.position,
                                    newTargetPos,
                                    followSmoothness * deltaTime
                                );
                            }

                            // キャラクターを移動
                            if (isDelayActive)
                            {
                                if (characterRb != null)
                                    characterRb.MovePosition(targetCharacterPos);
                                // else
                                //     character.transform.position = targetCharacterPos;
                            }
                        }
                    }
                    break;
            }
        }

        public void Activate(GameObject character)
        {
            if (rb == null)
                return;

            if (character == null)
                return;

            if (owner == null)
                return;

            this.character = character;
            characterRb = character.GetComponent<Rigidbody>();

            // ディレイをリセット
            delayTimer = 0f;
            isDelayActive = false;

            if (moverType == AttackMoverType.CharacterFollowsObject || moverType == AttackMoverType.FollowCharacter)
            {
                characterTransform = character.transform;
                targetCharacterPos = character.transform.position;
            }

            // 追加：characterTransform が設定されたか確認
            if (characterTransform != null && owner != null)
            {
                owner.transform.position = characterTransform.position;
            }

            // キャラクターの前方向に移動方向を設定
            if (character != null)
            {
                currentMoveDir = new Vector3(
                    moveDir.x * character.transform.forward.x,
                    moveDir.y,
                    moveDir.z
                );
            }
        }
    }

    public enum AttackMoverType
    {
        None,
        FollowCharacter,
        CharacterFollowsObject,
    }
}