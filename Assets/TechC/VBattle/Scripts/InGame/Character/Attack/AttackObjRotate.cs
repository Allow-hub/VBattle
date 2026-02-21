using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    [System.Serializable]
    public class AttackObjRotate : IAttackBehaviour
    {
        [SerializeField] private Vector3 rotateAxis = Vector3.up; // 回転軸
        [SerializeField] private float rotateSpeed = 180f;        // 1秒あたりの回転角度
        private GameObject owner; // 回転させる対象

        public void Initialize(GameObject owner)
        {
            this.owner = owner;
        }

        public void OnRelease()
        {
        }

        public void OnUpdate(float deltaTime)
        {
            if (owner == null) return;

            // deltaTimeを使ってフレーム依存しない回転
            owner.transform.Rotate(rotateAxis * rotateSpeed * deltaTime, Space.Self);
        }
    }
}