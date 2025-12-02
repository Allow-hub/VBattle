using UnityEngine;
using VRM;

namespace VRM
{
    public class SpringBoneScaler : MonoBehaviour
    {
        void Awake()
        {
            // 子階層も含めて全部拾う
            var springBones = GetComponentsInChildren<VRMSpringBone>();

            foreach (var bone in springBones)
            {
                if (bone != null)
                {
                    bone.UseRuntimeScalingSupport = true;
                }
            }
        }
    }
}
