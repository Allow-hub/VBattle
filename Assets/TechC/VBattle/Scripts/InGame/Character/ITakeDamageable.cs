using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public interface ITakeDamageable
    {
        void TakeDamage(float damage, float stunDuration = 0.3f);
    }
}
