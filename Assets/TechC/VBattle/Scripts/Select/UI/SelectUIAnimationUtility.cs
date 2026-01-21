using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TechC.VBattle.Select.UI
{
    /// <summary>
    /// キャラクター選択シーン専用のUIアニメーション処理を提供するユーティリティクラス
    /// </summary>
    public static class SelectUIAnimationUtility
    {
        // フェードイン＆スケールアニメーションのデフォルト値
        private const float DEFAULT_FADE_IN_DURATION = 0.3f;
        private const float DEFAULT_FADE_IN_SCALE_FROM = 0.5f;
        private const float DEFAULT_FADE_IN_SCALE_TO = 1.0f;
        
        // イージング関数の定数
        private const float EASE_OUT_BACK_C1 = 1.70158f;
        private const float EASE_OUT_BACK_C3 = EASE_OUT_BACK_C1 + 1f;
        
        // アルファ値の定数
        private const float ALPHA_TRANSPARENT = 0f;
        private const float SCALE_NORMAL = 1f;

        /// <summary>
        /// 画像をフェードイン＆スケールアップアニメーションで表示
        /// </summary>
        /// <param name="image">アニメーション対象の画像</param>
        /// <param name="newSprite">設定する新しいスプライト</param>
        /// <param name="duration">アニメーション時間（秒）</param>
        /// <param name="scaleFrom">開始時のスケール倍率</param>
        /// <param name="scaleTo">終了時のスケール倍率</param>
        public static IEnumerator FadeInWithScale(
            Image image, 
            Sprite newSprite, 
            float duration = DEFAULT_FADE_IN_DURATION, 
            float scaleFrom = DEFAULT_FADE_IN_SCALE_FROM, 
            float scaleTo = DEFAULT_FADE_IN_SCALE_TO)
        {
            if (image == null) yield break;

            // 初期状態：小さく＆透明
            Vector3 originalScale = image.transform.localScale;
            Color originalColor = image.color;
            
            image.sprite = newSprite;
            image.transform.localScale = originalScale * scaleFrom;
            image.color = new Color(originalColor.r, originalColor.g, originalColor.b, ALPHA_TRANSPARENT);

            // アニメーション：拡大＆フェードイン
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float eased = EaseOutBack(t);

                image.transform.localScale = originalScale * Mathf.Lerp(scaleFrom, scaleTo, eased);
                image.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(ALPHA_TRANSPARENT, originalColor.a, t));

                yield return null;
            }

            // 最終状態に確実に設定
            image.transform.localScale = originalScale;
            image.color = originalColor;
        }

        /// <summary>
        /// イージング関数：EaseOutBack（少しオーバーシュートして戻る）
        /// </summary>
        private static float EaseOutBack(float t)
        {
            return SCALE_NORMAL + EASE_OUT_BACK_C3 * Mathf.Pow(t - SCALE_NORMAL, 3f) + EASE_OUT_BACK_C1 * Mathf.Pow(t - SCALE_NORMAL, 2f);
        }
    }
}
