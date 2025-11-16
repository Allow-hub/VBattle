using TechC.VBattle.Core.Managers;
using TechC.VBattle.Core.Util;
using TechC.VBattle.Systems;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// ガードのオブジェクトの見た目
    /// ガードの耐久値に合わせて見た目を小さくしたりする
    /// </summary>
    public class GuardView : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private GameObject[] shieldObj;
        [SerializeField] private Vector3 rotateAxis;
        [SerializeField] private float speedFactor;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float minScale = 0.3f;
        [SerializeField] private GameObject breakPrefab;
        [SerializeField] private float apearEffect = 2f;
        private Vector3 point;
        [Tooltip("ガードの最大サイズ")]
        [SerializeField] private Vector3 maxScale;
        [SerializeField, Tooltip("ガードの中心Yオフセット")] private float centerYOffset = 1f;


        private bool[] previousShieldActiveStates;

        private void Start()
        {
            transform.localScale = maxScale;
            point = characterController.transform.position;
            point.y += centerYOffset;

            // 初期化
            previousShieldActiveStates = new bool[shieldObj.Length];
            for (int i = 0; i < shieldObj.Length; i++)
                previousShieldActiveStates[i] = shieldObj[i] != null && shieldObj[i].activeSelf;

            PositionShields();
        }

        private void PositionShields()
        {
            // 配置する中心（プレイヤーの位置）
            Vector3 center = point;

            int count = shieldObj.Length;

            for (int i = 0; i < count; i++)
            {
                if (shieldObj[i] == null) continue;

                // 等間隔で円周上に配置（Y軸周りに並べる例）
                float angle = i * Mathf.PI * 2 / count;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                Vector3 offset = new Vector3(x, 0, z);

                shieldObj[i].transform.position = center + offset;

                // プレイヤーの方向を向かせる
                shieldObj[i].transform.LookAt(center);
            }
        }
        
        private void Update()
        {
            // プレイヤーの移動に追従するように中心点を更新
            point = characterController.transform.position + Vector3.up;

            // シールドを回転させる
            foreach (var obj in shieldObj)
            {
                if (obj != null)
                {
                    obj.transform.RotateAround(
                        point,
                        rotateAxis,
                        360.0f / (1.0f / speedFactor) * Time.deltaTime
                    );
                }
            }

            // シールドの向きを外側に向ける
            foreach (var obj in shieldObj)
            {
                if (obj == null) continue;

                Vector3 dir = obj.transform.position - point;
                if (dir != Vector3.zero)
                    obj.transform.rotation = Quaternion.LookRotation(dir);
            }

            // ガードの数によって表示非表示を制御
            float currentGuard = characterController.CurrentGuardPower;
            float maxGuard = characterController.Data.GuardPower;
            float guardPerShield = maxGuard / shieldObj.Length;
            int shieldCount = Mathf.CeilToInt(currentGuard / guardPerShield);
            for (int i = 0; i < shieldObj.Length; i++)
            {
                if (shieldObj[i] != null)
                {
                    bool isActive = i < shieldCount;

                    // シールドがアクティブから非アクティブになったタイミングを検出
                    if (previousShieldActiveStates[i] && !isActive)
                    {
                        // 破壊エフェクトを生成
                        if (breakPrefab != null)
                        {
                            var obj =  EffectFactory.I.GetEffectObj(breakPrefab, shieldObj[i].transform.position, Quaternion.identity);
                            DelayUtility.StartDelayedActionWithPauseAsync(
                            0.1f,
                            () => { EffectFactory.I.ReturnEffect(obj); },
                            () => GameDataBridge.I.IsPaused);
                        }
                    }

                    // 表示状態を更新
                    shieldObj[i].SetActive(isActive);
                    previousShieldActiveStates[i] = isActive;

                    if (isActive)
                    {
                        float shieldStart = i * guardPerShield;
                        float shieldEnd = (i + 1) * guardPerShield;
                        float scaleFactor = Mathf.Clamp01((currentGuard - shieldStart) / guardPerShield);
                        float size = Mathf.Lerp(minScale, 1f, scaleFactor);
                        shieldObj[i].transform.localScale = maxScale * size;
                    }
                }
            }
        }
    }
}