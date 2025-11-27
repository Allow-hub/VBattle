using TechC.VBattle.InGame.Events;
using System.Linq;

namespace TechC.VBattle.InGame.Systems
{
    /// <summary>
    /// 対戦中の攻撃判定を調停するクラス。
    /// AttackRequestEvent を受け取り、ヒット判定・無敵/ガード処理を行い、
    /// AttackResultEvent を発行して最終的な攻撃結果を通知する。
    /// 戦闘演出やダメージ適用は受け取る側が行う。
    /// </summary>
    public class BattleJudge
    {
        private readonly Character.CharacterController player_1;
        private readonly Character.CharacterController player_2;
        private readonly BattleEventBus eventBus;

        public BattleJudge(Character.CharacterController p1, Character.CharacterController p2, BattleEventBus eventBus)
        {
            player_1 = p1;
            player_2 = p2;
            this.eventBus = eventBus;

            // 攻撃リクエストイベントを購読
            eventBus.Subscribe<AttackRequestEvent>(OnAttackRequest);
        }

        /// <summary>
        /// 攻撃リクエストを受け、ヒット判定およびダメージ算出を行う
        /// </summary>
        private void OnAttackRequest(AttackRequestEvent attackEvent)
        {
            // 対戦相手の特定
            var target = GetOpponent(attackEvent.attacker);

            if (target == null)
            {
                PublishAttackResult(attackEvent, null, false, false, 0);
                return;
            }

            // 当たり判定（ヒット対象のリストに含まれるか）
            bool isHitRange = ContainsTargetInHitList(attackEvent, target);

            if (!isHitRange)
            {
                PublishAttackResult(attackEvent, target, false, false, 0);
                return;
            }

            // 攻撃命中時のダメージ/属性判定
            bool isHit = true;
            bool isCounter = false; // 追加入力予定があれば後で判定可能
            int damage = attackEvent.attackData.damage;

            if (target.IsInvincible || target.IsGuarding)
            {
                // 無敵・ガード中はいずれもノーダメージ
                isHit = false;
                damage = 0;
            }

            PublishAttackResult(attackEvent, target, isHit, isCounter, damage);
        }

        /// <summary>
        /// 対象が攻撃のヒットリストに含まれているか判定する
        /// </summary>
        private bool ContainsTargetInHitList(AttackRequestEvent attackEvent, Character.CharacterController target)
        {
            if (attackEvent.hitTargets == null) return false;

            return attackEvent.hitTargets.Any(col =>
                col.GetComponentInParent<Character.CharacterController>() == target
            );
        }

        /// <summary>
        /// 自分と異なる方のプレイヤーを返す
        /// </summary>
        private Character.CharacterController GetOpponent(Character.CharacterController attacker)
            => attacker == player_1 ? player_2 : player_1;

        /// <summary>
        /// 攻撃判定結果をイベントで通知する
        /// ダメージ適用やリアクションはイベント購読側が実装
        /// </summary>
        private void PublishAttackResult(
            AttackRequestEvent request,
            Character.CharacterController target,
            bool isHit,
            bool isCounter,
            int damage)
        {
            var resultEvent = new AttackResultEvent
            {
                attacker = request.attacker,
                target = target,
                attackData = request.attackData,
                isHit = isHit,
                isCounter = isCounter,
                damage = damage
            };
            eventBus.Publish(resultEvent);
        }

        public void Dispose()
        {
            eventBus.Unsubscribe<AttackRequestEvent>(OnAttackRequest);
        }
    }
}
