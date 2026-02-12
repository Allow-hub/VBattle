using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 青攻撃の処理クラス
    /// </summary>
    public class AttackBlue : IAttackBehaviour
    {
        [SerializeField] private Rigidbody blueRb;
        [SerializeField] private float orbitRadius = 2f;
        [SerializeField] private float rotationSpeed = 360f;
        [SerializeField] private float yOffset = 1f;

        private GameObject characterObj;
        private float currentAngle = 0f;

        public void Initialize(GameObject owner)
        {
        }

        public void OnRelease()
        {
        }

        public void OnUpdate(float deltaTime)
        {
            if (characterObj == null || blueRb == null) return;

            // 角度更新
            currentAngle += rotationSpeed * deltaTime;
            if (currentAngle >= 360f)
                currentAngle -= 360f;

            float rad = currentAngle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Cos(rad) * orbitRadius,
                yOffset,
                Mathf.Sin(rad) * orbitRadius
            );

            Vector3 targetPos = characterObj.transform.position + offset;

            // Rigidbodyで移動
            blueRb.MovePosition(targetPos);

            // 回転（必要なら）
            blueRb.MoveRotation(
                blueRb.rotation * Quaternion.Euler(0f, rotationSpeed * deltaTime, 0f)
            );
        }

        public void Activate(GameObject character)
        {
            characterObj = character;
            currentAngle = 0f;

            if (blueRb != null && characterObj != null)
            {
                Vector3 initialOffset = new Vector3(orbitRadius, yOffset, 0f);
                blueRb.position = characterObj.transform.position + initialOffset;
            }
        }
    }
}