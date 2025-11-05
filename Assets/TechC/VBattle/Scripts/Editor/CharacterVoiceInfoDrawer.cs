using TechC.VBattle.Audio;
using UnityEditor;
using UnityEngine;

namespace TechC.VBattle.Editor
{
    /// <summary>
    /// CharacterAudioDataのVoiceElementをvoiceTypeの表記に
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterAudioData.CharacterVoiceInfo))]
    public class CharacterVoiceInfoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var voiceTypeProp = property.FindPropertyRelative("voiceType");
            string name = voiceTypeProp.enumDisplayNames[voiceTypeProp.enumValueIndex];
            EditorGUI.PropertyField(position, property, new GUIContent(name), true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }
    }
}
