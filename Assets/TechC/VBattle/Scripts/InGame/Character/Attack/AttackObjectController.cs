using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// 各攻撃のオブジェクトの管理クラス
    /// それぞれの機能を組み立てて実行する
    /// </summary>
    public class AttackObjectController : MonoBehaviour
    {
        [SerializeReference] private List<IAttackBehaviour> behaviours;
        public List<IAttackBehaviour> Behaviours => behaviours;
        public int PlayerID => playerID;
        private int playerID;
        public string PlayerTag => playerTag;
        private string playerTag = "Player";
        private GameObject character;

        private void Start()
        {
            if (behaviours == null) return;

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.Initialize(gameObject);
            }
        }

        private void OnDisable()
        {
            if (behaviours == null) return;

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.OnRelease();
            }
        }

        private void Update()
        {
            if (behaviours == null) return;

            float delta = Time.deltaTime;
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.OnUpdate(delta);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (behaviours == null) return;
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.OnTriggerEnter(other);
            }
        }

        /// <summary>
        /// プレイヤーを登録
        /// </summary>
        /// <param name="id">プレイヤーのID</param>
        /// <param name="characterObj">キャラクターのオブジェクト</param>
        public void SetPlayer(int id, GameObject characterObj)
        {
            if (id < 0) return; // 無効なIDは無視
            playerID = id;
            character = characterObj;
            if (behaviours == null) return;
            foreach (var behaviour in behaviours)
            {
                if (behaviour == null) continue;
                behaviour?.Activate(character);
            }
        }
    }
}