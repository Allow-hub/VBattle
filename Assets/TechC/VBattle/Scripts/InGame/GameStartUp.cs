using TechC.VBattle.InGame.Character;
using TechC.VBattle.InGame.UI;
using TechC.VBattle.Systems;
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
        [SerializeField] private InGameUIController inGameUIController;

        private void Awake()
        {
            inGameManager.Init();
            charaAttackFactory.Init();
            effectFactory.Init();
            attackVisualizer.Init();
            inGameUIController.Init();
        }
    }
}