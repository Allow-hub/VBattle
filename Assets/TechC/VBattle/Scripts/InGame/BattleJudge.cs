using TechC.VBattle.InGame.Events;
using TechC.VBattle.InGame.Character;
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
        private readonly IDamageable player_1;
        private readonly IDamageable player_2;
        private readonly BattleEventBus eventBus;

        public BattleJudge(IDamageable p1, IDamageable p2, BattleEventBus eventBus)
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
                PublishAttackResult(attackEvent, null, false, false, false, 0);
                return;
            }

            // 当たり判定（ヒット対象のリストに含まれるか）
            bool isHitRange = ContainsTargetInHitList(attackEvent, target);

            if (!isHitRange)
            {
                PublishAttackResult(attackEvent, target, false, false, false, 0);
                return;
            }

            // 攻撃命中時のダメージ/属性判定
            bool isHit = true;
            bool isGuard = target.IsGuarding;
            bool isCounter = false; // 追加入力予定があれば後で判定可能
            int damage = attackEvent.attackData.damage;

            if (target.IsInvincible || target.IsGuarding)
            {
                // 無敵・ガード中はいずれもノーダメージ
                isHit = false;
                damage = 0;
            }

            PublishAttackResult(attackEvent, target, isHit, isCounter, isGuard, damage);
        }

        /// <summary>
        /// 対象が攻撃のヒットリストに含まれているか判定する
        /// </summary>
        private bool ContainsTargetInHitList(AttackRequestEvent attackEvent, IDamageable target)
        {
            if (attackEvent.hitTargets == null) return false;

            return attackEvent.hitTargets.Any(col =>
            {
                var damageable = col.GetComponentInParent<IDamageable>();
                return damageable != null && damageable.GameObject == target.GameObject;
            });
        }

        /// <summary>
        /// 攻撃者の対戦相手を特定する
        /// 飛び道具の場合はOwnerを辿って本来の攻撃者を特定
        /// </summary>
        private IDamageable GetOpponent(IAttacker attacker)
        {
            if (attacker == null) return null;

            // 攻撃者の所有者を取得（飛び道具などの場合はOwnerが設定されている）
            var owner = attacker.Owner;
            if (owner == null) return null;

            // 所有者のGameObjectで対戦相手を判定
            if (owner.gameObject == player_1.GameObject) return player_2;
            if (owner.gameObject == player_2.GameObject) return player_1;

            return null;
        }

        /// <summary>
        /// 攻撃判定結果をイベントで通知する
        /// ダメージ適用やリアクションはイベント購読側が実装
        /// </summary>
        private void PublishAttackResult(
            AttackRequestEvent request,
            IDamageable target,
            bool isHit,
            bool isCounter,
            bool isGuard,
            int damage)
        {
            var resultEvent = new AttackResultEvent
            {
                attacker = request.attacker,
                target = target as CharacterController, // CharacterControllerにキャスト（nullの可能性あり）
                attackData = request.attackData,
                isHit = isHit,
                isCounter = isCounter,
                isGuard = isGuard,
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