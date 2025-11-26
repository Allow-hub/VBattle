using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC.VBattle.Audio
{
    #region 列挙型

    /// <summary>
    /// BGM用の列挙型
    /// </summary>
    public enum BGMID
    {
        None = -1,
        Title,
        Home,
        Battle,
        Win,
        Lose,
    }

    /// <summary>
    /// SE用（共通のSE）
    /// </summary>
    public enum SEID
    {
        None = -1,
        ButtonClick,
        MenuOpen,
        MenuClose,
        CommentGet,
        Guard,
        Hit,
        WallHit,
        TabNotification,
        Grass
    }
    
    /// <summary>
    /// キャラクターSE用
    /// </summary>
    public enum CharacterSEType
    {
        None = -1,

        //通常行動
        Walk,
        Run,
        Guard,
        Jump,
        Land,

        //弱攻撃
        WeakNormalAttack_1,
        WeakNormalAttack_2,
        WeakNormalAttack_3,
        WeakLeftAttack,
        WeakRightAttack,
        WeakUpAttack,
        WeakDownAttack,

        //強攻撃
        StrongNormalAttack,
        StrongLeftAttack,
        StrongRightAttack,
        StrongUpAttack,
        StrongDownAttack,

        //アピール
        AppealNormalAttack,
        AppealLeftAttack,
        AppealRightAttack,
        AppealUpAttack,
        AppealDownAttack,

        //必殺技
        Ult,

        //コンボ
        Combo,
        GuardBreak
    }

    /// <summary>
    /// キャラクターボイス用
    /// </summary>
    public enum CharacterVoiceType
    {
        None = -1,
        Damage,
        Death,
        Victory,

        //弱攻撃
        WeakNormalAttack_1,
        WeakNormalAttack_2,
        WeakNormalAttack_3,
        WeakLeftAttack,
        WeakRightAttack,
        WeakUpAttack,
        WeakDownAttack,

        //強攻撃
        StrongNormalAttack,
        StrongLeftAttack,
        StrongRightAttack,
        StrongUpAttack,
        StrongDownAttack,

        //アピール
        AppealNormalAttack,
        AppealLeftAttack,
        AppealRightAttack,
        AppealUpAttack,
        AppealDownAttack,
    }

    #endregion


    /// <summary>
    /// 共通音声データを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "AudioData", menuName = "TechC/Audio/AudioData")]
    public class AudioData : ScriptableObject
    {
        [System.Serializable]
        public class BGMInfo
        {
            public BGMID id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1.0f;
            [Range(0f, 2f)] public float pitch = 1.0f;
            public bool loop = true;
            [Range(0f, 5f)] public float fadeInTime = 0.5f;
            [Range(0f, 5f)] public float fadeOutTime = 0.5f;
        }

        [System.Serializable]
        public class SEInfo
        {
            public SEID id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1.0f;
            [Range(0f, 2f)] public float pitch = 1.0f;
            public bool loop = false;
        }

        [Header("BGM設定")]
        public List<BGMInfo> bgmList = new List<BGMInfo>();

        [Header("共通SE設定")]
        public List<SEInfo> seList = new List<SEInfo>();

        /// <summary>
        /// IDからBGMデータを取得
        /// </summary>
        public BGMInfo GetBGM(BGMID id)
        {
            return bgmList.Find(bgm => bgm.id == id);
        }

        /// <summary>
        /// IDからSEデータを取得
        /// </summary>
        public SEInfo GetSE(SEID id)
        {
            return seList.Find(se => se.id == id);
        }
    }
}
