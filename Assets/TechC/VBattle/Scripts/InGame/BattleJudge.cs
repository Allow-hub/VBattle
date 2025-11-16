
using TechC.VBattle.InGame.Character;

namespace TechC.VBattle.InGame.Systems
{
    /// <summary>
    /// 戦闘の調停者
    /// BattleEventBusからイベントを受け取り、攻撃の成功かどうかを判定して返す
    /// </summary>
    public class BattleJudge
    {
        private readonly CharacterController player_1;
        private readonly CharacterController player_2;

        public BattleJudge(CharacterController p1, CharacterController p2)
        {
            player_1 = p1;
            player_2 = p2;
        }

        private void Update()
        {

        }
    }
}
