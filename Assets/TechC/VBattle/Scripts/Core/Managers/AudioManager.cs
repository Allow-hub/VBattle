using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.Audio;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core.Util;
using TechC.VBattle.InGame.Character;
using UnityEngine;

namespace TechC.VBattle.Core.Managers
{
    /// <summary>
    /// 音関連のマネージャー
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioData audioData;
        [SerializeField] private List<CharacterAudioData> characterAudioDataList = new List<CharacterAudioData>();

        #region 音量設定

        [Range(0f, 1f)] public float masterVolume = 1.0f;
        [Range(0f, 1f)] public float bgmVolume = 1.0f;
        [Range(0f, 1f)] public float seVolume = 1.0f;
        [Range(0f, 1f)] public float voiceVolume = 1.0f;

        #endregion

        // オーディオソース
        private AudioSource bgmSource;
        private AudioSource bgmCrossSource; // クロスフェード用の予備BGM
        private List<AudioSource> seSources = new List<AudioSource>();
        private List<AudioSource> characterSESources = new List<AudioSource>();
        private List<AudioSource> characterVoiceSources = new List<AudioSource>();

        // SE用のプール設定
        [SerializeField] private int seSourceCount = 10;
        [SerializeField] private int characterSESourceCount = 10;
        [SerializeField] private int characterVoiceSourceCount = 10;

        // 現在再生中のBGM
        private BGMID currentBGM = BGMID.None;
        private bool isBgmFading = false;
        /// <summary>
        /// Singletonの初期化処理をoverride
        /// </summary>
        public override void Init()
        {
            base.Init();
            // BGM用のAudioSourceを2つ作成（クロスフェード用）
            GameObject bgmObject = new GameObject("BGM_Source");
            bgmObject.transform.parent = transform;
            bgmSource = bgmObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;

            GameObject bgmCrossObject = new GameObject("BGM_Cross_Source");
            bgmCrossObject.transform.parent = transform;
            bgmCrossSource = bgmCrossObject.AddComponent<AudioSource>();
            bgmCrossSource.playOnAwake = false;

            // SE用のAudioSourceをプール作成
            CreateAudioSourcePool("SE_Source", seSourceCount, seSources);

            // キャラクターSE用のAudioSourceをプール作成
            CreateAudioSourcePool("Character_SE_Source", characterSESourceCount, characterSESources);

            // キャラクターボイス用のAudioSourceをプール作成
            CreateAudioSourcePool("Character_Voice_Source", characterVoiceSourceCount, characterVoiceSources);
        }

        /// <summary>
        /// AudioSourceプールを作成
        /// </summary>
        private void CreateAudioSourcePool(string name, int count, List<AudioSource> pool)
        {
            GameObject poolParent = new GameObject(name + "_Pool");
            poolParent.transform.parent = transform;

            for (int i = 0; i < count; i++)
            {
                GameObject sourceObj = new GameObject(name + "_" + i);
                sourceObj.transform.parent = poolParent.transform;
                AudioSource source = sourceObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                pool.Add(source);
            }
        }

        /// <summary>
        /// キャラクターデータをロード（必要に応じて呼び出し）
        /// </summary>
        public void LoadCharacterAudioData(CharacterAudioData data)
        {
            if (data != null && !characterAudioDataList.Contains(data))
            {
                characterAudioDataList.Add(data);
            }
        }

        /// <summary>
        /// キャラクターデータをアンロード
        /// </summary>
        public void UnloadCharacterAudioData(CharaName characterType)
        {
            characterAudioDataList.RemoveAll(data => data.characterType == characterType);
        }

        /// <summary>
        /// キャラクタータイプからデータを取得
        /// </summary>
        private CharacterAudioData GetCharacterAudioData(CharaName characterType)
        {
            return characterAudioDataList.Find(data => data.characterType == characterType);
        }

        #region BGM 関連

        /// <summary>
        /// BGMを再生
        /// </summary>
        public void PlayBGM(BGMID id, bool isCrossFade = true)
        {
            if (currentBGM == id)
                return;

            if (isBgmFading)
            {
                StopAllCoroutines();
                isBgmFading = false;
            }

            AudioData.BGMInfo bgmInfo = audioData.GetBGM(id);
            if (bgmInfo == null || bgmInfo.clip == null)
            {
                CustomLogger.Warning($"BGM の ID {id} が見つかりません", LogTagUtil.TagAudio);
                return;
            }

            currentBGM = id;

            if (isCrossFade && bgmSource.isPlaying)
            {
                StartCoroutine(CrossFadeBGM(bgmInfo));
            }
            else
            {
                bgmSource.clip = bgmInfo.clip;
                bgmSource.volume = bgmInfo.volume * bgmVolume * masterVolume;
                bgmSource.pitch = bgmInfo.pitch;
                bgmSource.loop = bgmInfo.loop;

                if (bgmInfo.fadeInTime > 0)
                {
                    bgmSource.volume = 0;
                    bgmSource.Play();
                    StartCoroutine(FadeBGM(bgmSource, 0, bgmInfo.volume * bgmVolume * masterVolume, bgmInfo.fadeInTime));
                }
                else
                {
                    bgmSource.Play();
                }
            }
        }

        /// <summary>
        /// BGMを停止
        /// </summary>
        public void StopBGM(float fadeOutTime = 0.5f)
        {
            if (!bgmSource.isPlaying)
                return;

            AudioData.BGMInfo bgmInfo = audioData.GetBGM(currentBGM);
            float actualFadeTime = bgmInfo != null ? bgmInfo.fadeOutTime : fadeOutTime;

            if (actualFadeTime > 0)
            {
                StartCoroutine(FadeBGM(bgmSource, bgmSource.volume, 0, actualFadeTime, true));
            }
            else
            {
                bgmSource.Stop();
            }

            currentBGM = BGMID.None;
        }

        /// <summary>
        /// BGMのクロスフェード処理
        /// </summary>
        private IEnumerator CrossFadeBGM(AudioData.BGMInfo newBgmInfo)
        {
            isBgmFading = true;

            // 現在のBGMを別のソースにコピー
            AudioSource currentSource = bgmSource;
            AudioSource nextSource = bgmCrossSource;

            // 次のBGMを設定
            nextSource.clip = newBgmInfo.clip;
            nextSource.volume = 0;
            nextSource.pitch = newBgmInfo.pitch;
            nextSource.loop = newBgmInfo.loop;
            nextSource.Play();

            float startVolume = currentSource.volume;
            float endVolume = newBgmInfo.volume * bgmVolume * masterVolume;
            float fadeTime = Mathf.Max(newBgmInfo.fadeInTime, 0.5f);

            float timer = 0;
            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float t = timer / fadeTime;
                currentSource.volume = Mathf.Lerp(startVolume, 0, t);
                nextSource.volume = Mathf.Lerp(0, endVolume, t);
                yield return null;
            }

            // フェード完了後、ソースを入れ替え
            currentSource.Stop();
            SwapBGMSources();
            isBgmFading = false;
        }

        /// <summary>
        /// BGMのフェード処理
        /// </summary>
        private IEnumerator FadeBGM(AudioSource source, float startVolume, float endVolume, float fadeTime, bool stopAfterFade = false)
        {
            isBgmFading = true;

            float timer = 0;
            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float t = timer / fadeTime;
                source.volume = Mathf.Lerp(startVolume, endVolume, t);
                yield return null;
            }

            if (stopAfterFade)
            {
                source.Stop();
            }

            isBgmFading = false;
        }

        /// <summary>
        /// BGMソースの入れ替え
        /// </summary>
        private void SwapBGMSources()
        {
            AudioSource temp = bgmSource;
            bgmSource = bgmCrossSource;
            bgmCrossSource = temp;
        }

        /// <summary>
        /// BGMの音量設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            if (bgmSource.isPlaying && !isBgmFading)
            {
                AudioData.BGMInfo bgmInfo = audioData.GetBGM(currentBGM);
                if (bgmInfo != null)
                {
                    bgmSource.volume = bgmInfo.volume * bgmVolume * masterVolume;
                }
            }
        }

        #endregion

        #region SE 関連

        /// <summary>
        /// 共通SEを再生
        /// </summary>
        public AudioSource PlaySE(SEID id)
        {
            AudioData.SEInfo seInfo = audioData.GetSE(id);
            if (seInfo == null || seInfo.clip == null)
            {
                CustomLogger.Warning($"SE の ID {id} が見つかりません", LogTagUtil.TagAudio);
                return null;
            }

            // 未使用のAudioSourceを探す
            AudioSource source = GetAvailableAudioSource(seSources);
            if (source == null)
            {
                CustomLogger.Warning("利用可能な SE AudioSource が見つかりません。プールのサイズを増やすことを検討してください。", LogTagUtil.TagAudio);
                return null;
            }

            source.clip = seInfo.clip;
            source.volume = seInfo.volume * seVolume * masterVolume;
            source.pitch = seInfo.pitch;
            source.loop = seInfo.loop;
            source.Play();

            return source;
        }

        /// <summary>
        /// 指定したSEが再生中でなければ再生する
        /// </summary>
        public AudioSource PlaySE(SEID id, bool preventDuplicate)
        {
            if (!preventDuplicate)
            {
                return PlaySE(id);
            }

            AudioData.SEInfo seInfo = audioData.GetSE(id);
            if (seInfo == null || seInfo.clip == null)
            {
                CustomLogger.Warning($"SE の ID {id} が見つかりません", LogTagUtil.TagAudio);
                return null;
            }

            // すでに同じクリップが再生中かチェック
            foreach (AudioSource source in seSources)
            {
                if (source.isPlaying && source.clip == seInfo.clip)
                {
                    // すでに再生中
                    return null;
                }
            }

            // 未使用のAudioSourceを探す
            AudioSource availableSource = GetAvailableAudioSource(seSources);
            if (availableSource == null)
            {
                CustomLogger.Warning("利用可能な SE AudioSource が見つかりません。プールのサイズを増やすことを検討してください。", LogTagUtil.TagAudio);
                return null;
            }

            availableSource.clip = seInfo.clip;
            availableSource.volume = seInfo.volume * seVolume * masterVolume;
            availableSource.pitch = seInfo.pitch;
            availableSource.loop = seInfo.loop;
            availableSource.Play();

            return availableSource;
        }

        /// <summary>
        /// 指定したSEが再生中かつ、再生開始からminSeconds経過していなければ再生しない
        /// minSeconds以上経過していれば再生する
        /// </summary>
        public AudioSource PlaySE(SEID id, float minSeconds)
        {
            AudioData.SEInfo seInfo = audioData.GetSE(id);
            if (seInfo == null || seInfo.clip == null)
            {
                CustomLogger.Warning($"SE の ID {id} が見つかりません", LogTagUtil.TagAudio);
                return null;
            }

            foreach (AudioSource source in seSources)
            {
                if (source.isPlaying && source.clip == seInfo.clip)
                {
                    if (source.time < minSeconds)
                    {
                        // 指定秒数未満なら再生しない
                        return null;
                    }
                    // 指定秒数以上経過していれば再生許可
                }
            }

            AudioSource availableSource = GetAvailableAudioSource(seSources);
            if (availableSource == null)
            {
                CustomLogger.Warning("利用可能な SE AudioSource が見つかりません。プールのサイズを増やすことを検討してください。", LogTagUtil.TagAudio);
                return null;
            }

            availableSource.clip = seInfo.clip;
            availableSource.volume = seInfo.volume * seVolume * masterVolume;
            availableSource.pitch = seInfo.pitch;
            availableSource.loop = seInfo.loop;
            availableSource.Play();

            return availableSource;
        }

        /// <summary>
        /// キャラクター固有のSEを再生
        /// </summary>
        public AudioSource PlayCharacterSE(CharaName characterType, CharacterSEType seType)
        {
            CharacterAudioData characterData = GetCharacterAudioData(characterType);
            if (characterData == null)
            {
                CustomLogger.Warning($"キャラクター {characterType} の音声データが見つかりません", LogTagUtil.TagAudio);
                return null;
            }

            CharacterAudioData.CharacterSEInfo seInfo = characterData.GetCharacterSE(seType);
            if (seInfo == null || seInfo.clip == null)
            {
                CustomLogger.Warning($"キャラクター {characterType} の SE タイプ {seType} が見つかりません", LogTagUtil.TagAudio);
                return null;
            }

            // 未使用のAudioSourceを探す
            AudioSource source = GetAvailableAudioSource(characterSESources);
            if (source == null)
            {
                CustomLogger.Warning("利用可能なキャラクター SE AudioSource が見つかりません。プールのサイズを増やすことを検討してください。", LogTagUtil.TagAudio);
                return null;
            }

            source.clip = seInfo.clip;
            source.volume = seInfo.volume * seVolume * masterVolume;
            source.pitch = seInfo.pitch;
            source.loop = seInfo.loop;
            source.PlayOneShot(seInfo.clip);
            CustomLogger.Info($"SEを再生しました{characterType},{seInfo.clip},{source.name + source.clip}", LogTagUtil.TagAudio);
            return source;
        }

        /// <summary>
        /// SEを停止
        /// </summary>
        public void StopSE(AudioSource source)
        {
            if (source != null && (seSources.Contains(source) || characterSESources.Contains(source)))
            {
                source.Stop();
            }
        }

        /// <summary>
        /// すべてのSEを停止
        /// </summary>
        public void StopAllSE()
        {
            // 共通SE停止
            foreach (AudioSource source in seSources)
            {
                if (source.isPlaying)
                {
                    source.Stop();
                }
            }

            // キャラクターSE停止
            foreach (AudioSource source in characterSESources)
            {
                if (source.isPlaying)
                {
                    source.Stop();
                }
            }
        }

        /// <summary>
        /// SE音量設定
        /// </summary>
        public void SetSEVolume(float volume)
        {
            seVolume = Mathf.Clamp01(volume);

            // 共通SEの音量調整
            foreach (AudioSource source in seSources)
            {
                if (source.isPlaying)
                {
                    // 元の音量に対する比率を維持
                    float ratio = source.volume / (seVolume * masterVolume);
                    source.volume = ratio * seVolume * masterVolume;
                }
            }

            // キャラクターSEの音量調整
            foreach (AudioSource source in characterSESources)
            {
                if (source.isPlaying)
                {
                    float ratio = source.volume / (seVolume * masterVolume);
                    source.volume = ratio * seVolume * masterVolume;
                }
            }
        }

        #endregion

        #region キャラクターボイス関連

        /// <summary>
        /// キャラクターボイスを再生
        /// </summary>
        public AudioSource PlayCharacterVoice(CharaName characterType, CharacterVoiceType voiceType)
        {
            CharacterAudioData characterData = GetCharacterAudioData(characterType);
            if (characterData == null)
            {
                CustomLogger.Warning($"キャラクター {characterType} の音声データが見つかりません", LogTagUtil.TagAudio);
                return null;
            }

            CharacterAudioData.CharacterVoiceInfo voiceInfo = characterData.GetCharacterVoice(voiceType);
            if (voiceInfo == null || voiceInfo.clip == null)
            {
                CustomLogger.Warning($"キャラクター {characterType} のボイス タイプ {voiceType} が見つかりません", LogTagUtil.TagAudio);
                return null;
            }

            // 未使用のAudioSourceを探す
            AudioSource source = GetAvailableAudioSource(characterVoiceSources);
            if (source == null)
            {
                CustomLogger.Warning("利用可能なキャラクターボイス AudioSource が見つかりません。プールのサイズを増やすことを検討してください。", LogTagUtil.TagAudio);
                return null;
            }

            source.clip = voiceInfo.clip;
            source.volume = voiceInfo.volume * voiceVolume * masterVolume;
            source.pitch = voiceInfo.pitch;
            source.loop = false;
            source.Play();

            return source;
        }

        /// <summary>
        /// ボイスを停止
        /// </summary>
        public void StopVoice(AudioSource source)
        {
            if (source != null && characterVoiceSources.Contains(source))
            {
                source.Stop();
            }
        }

        /// <summary>
        /// すべてのボイスを停止
        /// </summary>
        public void StopAllVoice()
        {
            foreach (AudioSource source in characterVoiceSources)
            {
                if (source.isPlaying)
                {
                    source.Stop();
                }
            }
        }
        /// <summary>
        /// ボイス音量設定
        /// </summary>
        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            foreach (AudioSource source in characterVoiceSources)
            {
                if (source.isPlaying)
                {
                    // 元の音量に対する比率を維持
                    float ratio = source.volume / (voiceVolume * masterVolume);
                    source.volume = ratio * voiceVolume * masterVolume;
                }
            }
        }

        #endregion

        #region 共通処理

        /// <summary>
        /// マスター音量設定
        /// </summary>
        /// <summary>
        /// マスター音量設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            float prevMasterVolume = masterVolume;
            masterVolume = Mathf.Clamp01(volume);

            if (Mathf.Approximately(prevMasterVolume, 0f))
            {
                // 再計算方式：現在のBGMがあれば情報から設定
                if (bgmSource.isPlaying && !isBgmFading)
                {
                    AudioData.BGMInfo bgmInfo = audioData.GetBGM(currentBGM);
                    if (bgmInfo != null)
                    {
                        bgmSource.volume = bgmInfo.volume * bgmVolume * masterVolume;
                    }
                }

                ResetAudioSourceVolumes(seSources, seVolume);
                ResetAudioSourceVolumes(characterSESources, seVolume);
                ResetAudioSourceVolumes(characterVoiceSources, voiceVolume);
            }
            else
            {
                float volumeRatio = masterVolume / prevMasterVolume;

                if (bgmSource.isPlaying && !isBgmFading)
                {
                    bgmSource.volume *= volumeRatio;
                }

                foreach (AudioSource source in seSources)
                {
                    if (source.isPlaying) source.volume *= volumeRatio;
                }
                foreach (AudioSource source in characterSESources)
                {
                    if (source.isPlaying) source.volume *= volumeRatio;
                }
                foreach (AudioSource source in characterVoiceSources)
                {
                    if (source.isPlaying) source.volume *= volumeRatio;
                }
            }
        }

        /// <summary>
        /// 再生中のAudioSourceの音量を再計算する補助メソッド
        /// </summary>
        private void ResetAudioSourceVolumes(List<AudioSource> sources, float categoryVolume)
        {
            foreach (var source in sources)
            {
                if (source.isPlaying && source.clip != null)
                {
                    source.volume = categoryVolume * masterVolume;
                }
            }
        }
        /// <summary>
        /// 未使用のAudioSourceを取得
        /// </summary>
        private AudioSource GetAvailableAudioSource(List<AudioSource> pool)
        {
            // 停止中のものを探す
            foreach (AudioSource source in pool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // すべて使用中の場合は、一番長く再生されているものを選ぶ
            AudioSource oldestSource = null;
            float longestTime = 0;

            foreach (AudioSource source in pool)
            {
                float playTime = source.time;
                if (playTime > longestTime)
                {
                    longestTime = playTime;
                    oldestSource = source;
                }
            }

            return oldestSource;
        }

        #endregion
    }
}