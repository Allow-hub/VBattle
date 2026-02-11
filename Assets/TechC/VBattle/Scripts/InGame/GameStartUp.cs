using TechC.VBattle.Core.Window;
using TechC.VBattle.InGame.Character;
using TechC.VBattle.InGame.Gimmick;
using TechC.VBattle.InGame.UI;
using TechC.VBattle.Systems;
using TechC.VBattle.InGame.Comment;
using UnityEngine;

namespace TechC.VBattle.InGame
{
    /// <summary>
    /// Singletonの初期化順序を決める
    /// </summary>
    [DefaultExecutionOrder(-9999)]
    public class GameStartUp : MonoBehaviour
    {
        [SerializeField] private InGameManager inGameManager;
        [SerializeField] private CharaAttackFactory charaAttackFactory;
        [SerializeField] private EffectFactory effectFactory;
        [SerializeField] private AttackVisualizer attackVisualizer;
        [SerializeField] private BattleGimmickManager battleGimmickManager;
        [SerializeField] private WindowColliderFactory windowColliderFactory;
        [SerializeField] private InGameUIController inGameUIController;
        
        // InGameのPrefabにComment関連のオブジェクトをオーバーライドしていないため、
        // 同様にコメントアウトしておく→将来的にコメントを含めてビルドするようになったら再度コメントアウト解除する
        // [SerializeField] private CommentFactory commentFactory;
        // [SerializeField] private BuffFactory buffFactory;
        // [SerializeField] private CommentDisplay commentDisplay;

        private void Awake()
        {
            inGameManager.Init();
            charaAttackFactory.Init();
            effectFactory.Init();
            attackVisualizer.Init();
            windowColliderFactory.Init();
            battleGimmickManager.Init();
            inGameUIController.Init();
            // commentFactory.Init();
            // buffFactory.Init();
            // commentDisplay.Init();
        }
    }
}