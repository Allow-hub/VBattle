using UnityEngine;

namespace TechC
{
    public class SelectPickAnim : MonoBehaviour
    {
        [SerializeField] private float animDelay = 1f;
        [SerializeField] private float appearDelay = 1.2f;
        [SerializeField] private GameObject ameObj;
        [SerializeField] private GameObject teramiObj;
        private GameObject lastObj = null;
        private int animName = Animator.StringToHash("IsShowingPannel");
        public void PlayAnim(GameObject prefab)
        {
            GameObject obj = NameToObj(prefab.name);
            lastObj = obj;
            var anim = obj?.GetComponentInChildren<Animator>();
            // DelayUtility.StartDelayedAction(this, appearDelay, () => obj?.SetActive(true));

            // DelayUtility.StartDelayedAction(this, appearDelay + animDelay, () => anim?.SetBool(animName, true));
        }

        private GameObject NameToObj(string name)
        {
            if (name.Contains("Ame"))
                return ameObj;
            else if (name.Contains("Terami"))
                return teramiObj;
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
