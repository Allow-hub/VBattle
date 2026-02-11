using TechC.VBattle.Core.Util;
using UnityEngine;

namespace TechC.VBattle.Select.UI
{
    /// <summary>
    /// キャラクター選択時のアニメーション制御
    /// </summary>
    public class SelectPickAnim : MonoBehaviour
    {
        private const string CHARACTER_NAME_AME = "Ame";
        private const string CHARACTER_NAME_TERAMI = "Terami";
        private const string ANIMATOR_PARAM_IS_SHOWING_PANEL = "IsShowingPannel";

        [SerializeField] private float animDelay = 1f;
        [SerializeField] private float appearDelay = 1.2f;
        [SerializeField] private GameObject ameObj;
        [SerializeField] private GameObject teramiObj;

        private GameObject lastObj = null;
        private int animName = Animator.StringToHash(ANIMATOR_PARAM_IS_SHOWING_PANEL);

        public void PlayAnim(GameObject prefab)
        {
            GameObject obj = NameToObj(prefab.name);
            lastObj = obj;
            var anim = obj?.GetComponentInChildren<Animator>();
            
            _ = DelayUtility.StartDelayedActionAsync(appearDelay, () => obj?.SetActive(true));
            _ = DelayUtility.StartDelayedActionAsync(appearDelay + animDelay, () => anim?.SetBool(animName, true));
        }

        private GameObject NameToObj(string name)
        {
            if (name.Contains(CHARACTER_NAME_AME)) return ameObj;
            if (name.Contains(CHARACTER_NAME_TERAMI)) return teramiObj;
            return ameObj;
        }

        public void ResetAnim()
        {
            if (lastObj == null) return;
            var anim = lastObj?.GetComponentInChildren<Animator>();
            anim?.SetBool(animName, false);
            lastObj?.SetActive(false);
            lastObj = null;
        }
    }
}