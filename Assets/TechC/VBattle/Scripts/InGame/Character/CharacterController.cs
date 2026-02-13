using UnityEngine;
using System.Collections.Generic;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core;
using TechC.VBattle.Core.Util;
using UnityEngine.InputSystem;
using TechC.VBattle.InGame.Events;
using TechC.VBattle.InGame.Comment;
using TechC.VBattle.Core.Managers;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// キャラクターのコントローラーの本体
    /// IAttacker: 攻撃者として振る舞う
    /// IDamageable: ダメージを受ける対象として振る舞う
    /// </summary>
    public partial class CharacterController : MonoBehaviour, IAttacker, IDamageable
    {
        [SerializeField] private CharacterData characterData;
        [SerializeField] private Animator anim;
        [SerializeField] private AttackSet attackSet;
        [SerializeField] private float groundCheckDistance;
        [SerializeField] private GameObject guardObj;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private Transform handPos;
        [SerializeField, ReadOnly] private int playerIndex;
        [SerializeField, ReadOnly] private string playerTag = "Player";
        [SerializeField] private float wallCheckDistance = 0.6f;

        // ===== 公開プロパティ =====
        public int PlayerIndex => playerIndex;
        public InputDevice DeviceName { get; private set; }
        public bool IsNPC { get; private set; }
        public int CurrentHP { get; private set; }
        [SerializeField, ReadOnly] private float idleAnimSpeed = 1.1f;
        public float IdleAnimSpeed => idleAnimSpeed;
        public string PlayerTag => playerTag;

        // 攻撃情報
        public AttackType CurrentAttackType { get; private set; }
        public AttackDirection CurrentAttackDirection { get; private set; }

        // ===== コンポーネント =====
        public Animator Anim => anim;
        public Rigidbody Rb => rb;
        private Rigidbody rb;

        // ===== 状態管理 =====
        public StateMachine StateMachine => stateMachine;
        private StateMachine stateMachine;
        public CommandInvoker CommandInvoker => commandInvoker;
        private CommandInvoker commandInvoker;
        private Dictionary<System.Type, CharacterState> stateCache = new();

        // ===== データ値 =====
        public CharacterData Data => characterData;
        public AttackSet AttackSet => attackSet;
        private float currentGuardPower;
        public float CurrentGuardPower => currentGuardPower;

        private float currentSpecialGauge = 0;
        public float CurrentSpecialGauge => currentSpecialGauge;
        private bool isInvincible = false;
        public bool IsInvincible => isInvincible;
        private bool isGuarding = false;
        public bool IsGuarding => isGuarding;

        // ===== コメントアイテム関連 =====
        public GameObject HoldItem { get; private set; }
        public CommentAbilityHandler CommentAbilityHandler { get; private set; }

        public Transform HandPos => handPos;

        // ===== ジャンプ関連 =====
        private int currentJumpCount = 0;
        private int maxJumpCount = 2;

        // ===== カウンター関連 =====
        private bool canCounter = false;
        private AttackData counterAttackData = null;
        private bool isExecutingCounterAttack = false;

        public bool CanCounter => canCounter;
        public bool IsExecutingCounterAttack => isExecutingCounterAttack;

        // ===== IAttacker実装 =====
        GameObject IAttacker.AttackerObj => gameObject;
        Transform IAttacker.Transform => transform;
        CharacterController IAttacker.Owner => this; // 自分自身が所有者

        // ===== IDamageable実装 =====
        GameObject IDamageable.GameObject => gameObject;
        bool IDamageable.IsInvincible => isInvincible;
        bool IDamageable.IsGuarding => isGuarding;

        // ===== アウトライン管理 =====
        [SerializeField] private CharacterOutlineController outlineController;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            CommentAbilityHandler = new CommentAbilityHandler();

            // すべての状態を登録してキャッシュ
            RegisterState(new NeutralState(this));
            RegisterState(new AirState(this));
            RegisterState(new AttackState(this));
            RegisterState(new DamageState(this));
            RegisterState(new GuardState(this));
            RegisterState(new CrouchState(this));

            stateMachine = new StateMachine();
            commandInvoker = new CommandInvoker(this);
            currentGuardPower = Data.GuardPower;
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="playerIndex">PlayerID: 1or2</param>
        /// <param name="deviceName">デバイス名</param>
        /// <param name="isNPC">NPCかどうか</param>
        public void Init(int playerIndex, InputDevice deviceName, bool isNPC)
        {
            this.playerIndex = playerIndex;
            DeviceName = deviceName;
            IsNPC = isNPC;
            CurrentHP = characterData.MaxHP;
            currentGuardPower = characterData.GuardPower;

            outlineController?.ApplyOutline(playerIndex);

            InGameManager.I.BattleBus.Subscribe<AttackResultEvent>(HandleAttackResult);
            InGameManager.I.BattleBus.Subscribe<AttackResultEvent>(OnAttackResult);
        }

        private void Start()
        {
            // 初期状態はNeutral
            stateMachine.Start(GetState<NeutralState>());
        }

        private void Update()
        {
            commandInvoker.Update();
            if (PlayerIndex == 1)
                CustomLogger.Info($"{stateMachine.CurrentState}", LogTagUtil.TagState);
        }

        private void FixedUpdate()
        {
            commandInvoker.FixedUpdate();
            if (stateMachine.CurrentState == GetState<AttackState>()) return;
            if (!IsGrounded() && stateMachine.CurrentState != GetState<AirState>())
                stateMachine.ChangeState(GetState<AirState>());
        }

        /// <summary>
        /// ステートの登録
        /// </summary>
        /// <param name="state">登録したいステート</param>
        private void RegisterState(CharacterState state) => stateCache[state.GetType()] = state;

        /// <summary>
        /// 状態取得用のヘルパーメソッド
        /// </summary>
        public T GetState<T>() where T : CharacterState
        {
            if (stateCache.TryGetValue(typeof(T), out var state))
                return state as T;

            Debug.LogError($"State {typeof(T).Name} not found!");
            return null;
        }

        /// <summary>
        /// コマンド実行、ステートが実行の可否を決める 
        /// ジェネリクスで値型かつICommandの継承をしていることを保証
        /// ICommandを引数として受け取るとinterface(参照型)として展開されるのでボクシングの発生がある
        /// </summary>
        /// <param name="command"></param>
        public void ExecuteCommand<T>(T command) where T : struct, ICommand
        {
            var currentState = stateMachine.CurrentState;

            if (currentState == null || !currentState.CanExecuteCommand(command))
            {
                CustomLogger.Info($"Command {command.Type} rejected in state {currentState?.GetType().Name}", LogTagUtil.TagCommand);
                return;
            }

            if (command is MoveCommand moveCmd)
                Move(moveCmd.Dir, moveCmd.IsDashing);
            else if (command is JumpCommand)
                Jump();
            else if (command is AttackCommand attackCmd)
                Attack(attackCmd.AttackType, attackCmd.AttackDirection);
            else if (command is GuardCommand guardCmd)
            {
                if (guardCmd.IsPress)
                    StartGuard();
                else
                    EndGuard();
            }
            else if (command is CrouchCommand crouchCommand)
            {
                if (crouchCommand.IsPress)
                    StartCrouch();
                else
                    EndCrouch();
            }
            else if (command is SpecialCommand)
            {
                // 必殺技は100%かつ適切な状態でのみ実行可能
                if (CurrentSpecialGauge >= Data.maxSpecialGauge)
                    PerformSpecial();
            }
            currentState.OnCommandExecuted(command);
        }

        /// <summary>
        /// 攻撃結果を受け取る
        /// </summary>
        private void OnAttackResult(AttackResultEvent e)
        {
            // 攻撃者の所有者を取得（飛び道具の場合はその所有者）
            CharacterController attackerOwner = e.attacker.Owner;

            // 自分が攻撃者（の所有者）の場合
            if (attackerOwner == this && e.isHit)
            {
                // ヒット時にゲージ増加
                float gaugeGain = e.attackData.specialGaugeGain * characterData.specialGaugeChargeRate;
                AddSpecialGauge(gaugeGain);
            }

            // 自分が攻撃を受けた場合
            if (e.target == this && e.isHit)
            {
                // 被弾時にもゲージ増加
                float gaugeGain = e.attackData.specialGaugeGainOnHit * characterData.specialGaugeChargeRate;
                AddSpecialGauge(gaugeGain);
            }
        }

        public bool IsGrounded()
        {
            Vector3 origin = transform.position;
            Vector3 dir = Vector3.down;
            // Raycast 判定
            bool grounded = Physics.Raycast(origin, dir, out RaycastHit hit, groundCheckDistance, groundMask);

            // デバッグ描画
            // Color rayColor = grounded ? Color.green : Color.red;
            // Debug.DrawRay(origin, dir * groundCheckDistance, rayColor);
            return grounded;
        }

        public void SetGuardPower(float amount) => currentGuardPower = amount;
        public void DecreaseGuardPower(float amount) => currentGuardPower -= amount;

        /// <summary>
        /// 必殺技ゲージを増加させる
        /// </summary>
        /// <param name="amount"></param>
        public void AddSpecialGauge(float amount)
        {
            currentSpecialGauge = Mathf.Clamp(currentSpecialGauge + amount, 0f, characterData.maxSpecialGauge);

            // ゲージ変更イベントを発行
            InGameManager.I.BattleBus.Publish(new SpecialGaugeChangedEvent
            {
                PlayerIndex = playerIndex,
                CurrentGauge = currentSpecialGauge,
                MaxGauge = characterData.maxSpecialGauge,
                Percentage = currentSpecialGauge / characterData.maxSpecialGauge
            });
        }

        /// <summary>
        /// 必殺技ゲージを消費する
        /// </summary>
        /// <param name="amount"></param>
        public void UseSpecialGauge(float amount)
        {
            currentSpecialGauge -= amount;
            if (currentSpecialGauge < 0)
                currentSpecialGauge = 0;
            InGameManager.I.BattleBus.Publish(new SpecialGaugeChangedEvent
            {
                PlayerIndex = playerIndex,
                CurrentGauge = currentSpecialGauge,
                MaxGauge = characterData.maxSpecialGauge,
                Percentage = currentSpecialGauge / characterData.maxSpecialGauge
            });
            // 必殺技発動、攻撃状態へ遷移
            CurrentAttackType = AttackType.Special;
            CurrentAttackDirection = AttackDirection.Neutral;
            stateMachine.ChangeState(GetState<AttackState>());
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Ground")) return;
            var charaName = GameDataBridge.I.GetPlayerSetup(PlayerIndex).SelectedCharacter.CharacterName;

            AudioManager.I?.PlayCharacterSE(charaName, Audio.CharacterSEType.Land);
            currentJumpCount = 0;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Ground")) return;

            // 移動入力がない場合のみ速度を減速（移動中は除外）
            // if (!commandInvoker.HasMoveInput)
            // {
            Vector3 velocity = rb.velocity;
            velocity.x *= 0.8f;
            velocity.z *= 0.8f;
            rb.velocity = velocity;
            // }
        }

        /// <summary>
        /// クローン用にプレイヤーIDを設定
        /// </summary>
        /// <param name="id"></param>
        public void SetClonePlayerID(int index)
        {
            playerIndex = index;
            outlineController?.ApplyOutline(playerIndex);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (stateMachine.CurrentState == GetState<AirState>()) return;
            stateMachine.ChangeState(GetState<AirState>());
        }

        // ===== コメントアイテム関連メソッド =====
        /// <summary>
        /// 持っているアイテムを設定する
        /// </summary>
        /// <param name="item">持つアイテム（nullで解除）</param>
        public void SetHoldItem(GameObject item) =>  HoldItem = item;

        // ===== カウンター関連メソッド =====
        public void SetCanCounter(bool val) => canCounter = val;
        public void SetCounterAttackData(AttackData data) => counterAttackData = data;
        public AttackData GetCounterAttackData() => counterAttackData;
        public void ClearCounterAttackData() => counterAttackData = null;

        public void SetExecutingCounterAttack(bool val) => isExecutingCounterAttack = val;

        private void OnDestroy()
        {
            if (InGameManager.I != null && InGameManager.I.BattleBus != null)
            {
                InGameManager.I.BattleBus.Unsubscribe<AttackResultEvent>(HandleAttackResult);
                InGameManager.I.BattleBus.Unsubscribe<AttackResultEvent>(OnAttackResult);
            }

            stateMachine?.Cancel();
            CommentAbilityHandler?.Dispose(); // クリーンアップ
            outlineController?.Cleanup();
        }
    }
}