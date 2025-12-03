using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TechC.VBattle.InGame.Character;
using Allow.EditorTools;

namespace TechC.VBattle.Editor
{
   [CustomEditor(typeof(AttackObjectController))]
    public class AttackObjectControllerEditor : PolymorphicListEditor<AttackObjectController, IAttackBehaviour>
    {
        protected override string PropertyName => "behaviours";
    }
}
