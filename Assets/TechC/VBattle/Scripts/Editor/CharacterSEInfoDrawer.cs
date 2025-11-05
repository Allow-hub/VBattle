using TechC.VBattle.Audio;
using UnityEditor;
using UnityEngine;

namespace TechC.VBattle.Editor
{
    /// <summary>
    /// CharacterAudioDataのSEElementをvoiceTypeの表記に
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterAudioData.CharacterSEInfo))]
    public class CharacterSEInfoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var seTypeProp = property.FindPropertyRelative("seType");
            string name = seTypeProp.enumDisplayNames[seTypeProp.enumValueIndex];
            EditorGUI.PropertyField(position, property, new GUIContent(name), true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }
}
