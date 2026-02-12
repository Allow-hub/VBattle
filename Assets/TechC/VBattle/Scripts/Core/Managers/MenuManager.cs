using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TechC.VBattle.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace TechC.VBattle.Core.Managers
{
    /// <summary>
    /// メニュー関連の管理クラス
    /// </summary>
    public class MenuManager : Singleton<MenuManager>
    {
        [SerializeField] private CanvasGroup menuCanvasGroup;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minusButton;
        [SerializeField] private Slider audioSlider;
        [SerializeField] private float volumeStep = 0.1f; // 音量の増減幅
        
        private float volumeRatio = 1.0f;
        private bool isMenu = false;
        
        public bool IsMenu => isMenu;
        protected override bool UseDontDestroyOnLoad => true;

        public override void Init()
        {
            isMenu = false;
        }

        private void Start()
        {
            volumeRatio = AudioManager.I.masterVolume;
            audioSlider.value = volumeRatio;
            
            homeButton.onClick.AddListener(OnHomeButtonClicked);
            plusButton.onClick.AddListener(OnPlusButtonClicked);
            minusButton.onClick.AddListener(OnMinusButtonClicked);
            audioSlider.onValueChanged.AddListener(OnAudioSliderValueChanged);
        }

        private void OnDestroy()
        {
            // リスナーの解除
            homeButton.onClick.RemoveListener(OnHomeButtonClicked);
            plusButton.onClick.RemoveListener(OnPlusButtonClicked);
            minusButton.onClick.RemoveListener(OnMinusButtonClicked);
            audioSlider.onValueChanged.RemoveListener(OnAudioSliderValueChanged);
        }

        private void OnHomeButtonClicked()
        {
            CloseMenu();
            SceneLoader.I?.LoadTitleSceneAsync().Forget();
        }

        private void OnMinusButtonClicked()
        {
            AudioManager.I?.PlaySE(SEID.ButtonClick);
            volumeRatio = Mathf.Clamp01(volumeRatio - volumeStep);
            AudioManager.I?.SetMasterVolume(volumeRatio);
            audioSlider.value = volumeRatio;
        }

        private void OnPlusButtonClicked()
        {
            AudioManager.I?.PlaySE(SEID.ButtonClick);
            volumeRatio = Mathf.Clamp01(volumeRatio + volumeStep);
            AudioManager.I?.SetMasterVolume(volumeRatio);
            audioSlider.value = volumeRatio;
        }

        private void OnAudioSliderValueChanged(float value)
        {
            volumeRatio = value;
            AudioManager.I?.SetMasterVolume(volumeRatio);
        }

        /// <summary>
        /// メニューの開閉を切り替える
        /// </summary>
        /// <param name="isMenu"></param>
        public void PressMenu(bool isMenu)
        {
            if (isMenu)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        private void OpenMenu()
        {
            AudioManager.I?.PlaySE(SEID.MenuOpen);
            menuCanvasGroup.alpha = 1f;
            menuCanvasGroup.interactable = true;
            menuCanvasGroup.blocksRaycasts = true;
            isMenu = true;
        }

        private void CloseMenu()
        {
            AudioManager.I?.PlaySE(SEID.MenuClose);
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
            isMenu = false;
        }
    }
}