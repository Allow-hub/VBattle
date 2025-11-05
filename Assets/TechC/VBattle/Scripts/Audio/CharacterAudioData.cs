using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.Audio
{
    /// <summary>
    /// キャラクター別音声データを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterAudioData", menuName = "Audio/CharacterAudioData")]
    public class CharacterAudioData : ScriptableObject
    {
        [Header("キャラクタータイプ")]
        public CharacterType characterType;

        [System.Serializable]
        public class CharacterSEInfo
        {
            public CharacterSEType seType;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1.0f;
            [Range(0f, 2f)] public float pitch = 1.0f;
            public bool loop = false;
        }

        [System.Serializable]
        public class CharacterVoiceInfo
        {
            public CharacterVoiceType voiceType;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1.0f;
            [Range(0f, 2f)] public float pitch = 1.0f;
        }

        [Header("キャラクターSE設定")]
        public List<CharacterSEInfo> seList = new List<CharacterSEInfo>();

        [Header("キャラクターボイス設定")]
        public List<CharacterVoiceInfo> voiceList = new List<CharacterVoiceInfo>();

        /// <summary>
        /// SEタイプからキャラクターSEデータを取得
        /// </summary>
        public CharacterSEInfo GetCharacterSE(CharacterSEType seType)
        {
            return seList.Find(se => se.seType == seType);
        }

        /// <summary>
        /// ボイスタイプからキャラクターボイスデータを取得
        /// </summary>
        public CharacterVoiceInfo GetCharacterVoice(CharacterVoiceType voiceType)
        {
            return voiceList.Find(voice => voice.voiceType == voiceType);
        }
    }
}
