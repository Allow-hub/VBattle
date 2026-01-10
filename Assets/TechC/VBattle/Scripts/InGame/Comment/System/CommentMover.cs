using System;
using System.Collections.Generic;
using TechC.VBattle.Core.Util;
using UnityEngine;

namespace TechC.VBattle.InGame.Comment
{
    /// <summary>
    /// コメントの移動処理を担当
    /// </summary>
    [Serializable]
    public class CommentMover
    {
        // 定数定義
        private const float CURVE_CENTER_POSITION = 0.5f; // 湾曲の中心位置
        private const float CURVE_DISTANCE_MULTIPLIER = 2f; // 中心距離計算の倍率
        private const float Y_INFLUENCE_RATIO = 0.3f; // Y軸への影響比率
        private const float Y_CURVE_SCALE = 0.1f; // Y軸湾曲のスケール
        private const float MAX_ROTATION_INTENSITY = 15f; // 最大回転角度
        
        [Header("コメントを非表示にする場所")]
        [SerializeField] private Transform LeftDespawnPos;
        private float despawnPosX;
        
        [Header("湾曲設定")]
        [SerializeField] private float curveRadius = 30f; // 湾曲の半径
        [SerializeField] private float curveIntensity = 0.8f; // 湾曲の強さ（0=直線、1=強い湾曲）
        [SerializeField] private float screenWidth = 20f; // スクリーンの幅（自動検出も可能）
        
        private Dictionary<Transform, CommentCurveData> commentCurveDataMap = new Dictionary<Transform, CommentCurveData>();
        
        [System.Serializable]
        private class CommentCurveData
        {
            public Vector3 startPosition;
            public float startTime;
            public float totalDistance;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            despawnPosX = LeftDespawnPos.transform.position.x;
        }

        /// <summary>
        /// コメント移動処理を開始（湾曲軌道）
        /// </summary>
        public void StartMoving(Transform trans, List<GameObject> chars, SpecialCommentTrigger specialCommentTrigger, Material originalMaterial)
        {
            InitializeCurveData(trans);
            
            DelayUtility.StartRepeatedActionWhileWithPauseAsync(
                () => trans.gameObject.activeInHierarchy && trans.position.x > despawnPosX,
                Time.fixedDeltaTime,
                async () => MoveCommentFrame(trans, chars, specialCommentTrigger, originalMaterial),
                InGameManager.I.GetPauseStateFunc
            );
        }
        
        /// <summary>
        /// コメントの湾曲データを初期化
        /// </summary>
        private void InitializeCurveData(Transform trans)
        {
            var curveData = new CommentCurveData
            {
                startPosition = trans.position,
                startTime = Time.time,
                totalDistance = 0f
            };
            
            if (commentCurveDataMap.ContainsKey(trans))
                commentCurveDataMap[trans] = curveData;
            else
                commentCurveDataMap.Add(trans, curveData);
        }

        /// <summary>
        /// 1フレーム分の移動処理（湾曲軌道）
        /// </summary>
        private void MoveCommentFrame(Transform trans, List<GameObject> chars, SpecialCommentTrigger specialCommentTrigger, Material originalMaterial)
        {
            if (!trans.gameObject.activeInHierarchy) return;
            if (CommentDisplay.I.IsCommentFrozen) return;

            MoveCurvedComment(trans);

            if (trans.position.x <= despawnPosX)
                ReturnComment(trans.gameObject, chars);
            
        }
        
        /// <summary>
        /// 湾曲モニター風の移動計算
        /// </summary>
        private void MoveCurvedComment(Transform trans)
        {
            if (!commentCurveDataMap.ContainsKey(trans)) return;
            
            var curveData = commentCurveDataMap[trans];
            float speed = CommentDisplay.I.GetCurrentSpeed();
            
            curveData.totalDistance += speed * Time.deltaTime;
            float normalizedProgress = curveData.totalDistance / screenWidth;
            
            float newX = curveData.startPosition.x - curveData.totalDistance;
            float curveOffset = CalculateCurveOffset(normalizedProgress);
            float newZ = curveData.startPosition.z + curveOffset;
            float yOffset = CalculateYCurveOffset(normalizedProgress);
            float newY = curveData.startPosition.y + yOffset;
            
            trans.position = new Vector3(newX, newY, newZ);
            AdjustRotationForCurve(trans, normalizedProgress);
        }
        
        /// <summary>
        /// Z軸方向の湾曲オフセットを計算
        /// </summary>
        private float CalculateCurveOffset(float normalizedProgress)
        {
            float centerDistance = Mathf.Abs(normalizedProgress - CURVE_CENTER_POSITION) * CURVE_DISTANCE_MULTIPLIER;
            float curveAmount = (1f - centerDistance * centerDistance) * curveIntensity;
            return curveAmount * curveRadius;
        }
        
        /// <summary>
        /// Y軸方向の微妙な湾曲オフセット
        /// </summary>
        private float CalculateYCurveOffset(float normalizedProgress)
        {
            float centerDistance = Mathf.Abs(normalizedProgress - CURVE_CENTER_POSITION) * CURVE_DISTANCE_MULTIPLIER;
            float yCurveAmount = (1f - centerDistance * centerDistance) * curveIntensity * Y_INFLUENCE_RATIO;
            return yCurveAmount * (curveRadius * Y_CURVE_SCALE);
        }
        
        /// <summary>
        /// 湾曲に合わせてコメントの回転を調整
        /// </summary>
        private void AdjustRotationForCurve(Transform trans, float normalizedProgress)
        {
            if (curveIntensity <= 0f) return;
            
            float centerBias = (normalizedProgress - CURVE_CENTER_POSITION) * CURVE_DISTANCE_MULTIPLIER;
            float rotationY = centerBias * MAX_ROTATION_INTENSITY * curveIntensity;
            
            trans.rotation = Quaternion.Euler(0, rotationY, 0);
        }

        /// <summary>
        /// コメントをプールに返却
        /// </summary>
        private void ReturnComment(GameObject comment, List<GameObject> chars)
        {
            if (commentCurveDataMap.ContainsKey(comment.transform))
                commentCurveDataMap.Remove(comment.transform);
            
            CommentDisplay.I.OnCommentReturned(comment);
        }
    }
}
