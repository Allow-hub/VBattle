using UnityEngine;
using System.Collections.Generic;
using TechC.VBattle.Core.Extensions;
using TechC.VBattle.Core;
using TechC.VBattle.Core.Util;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// キャラクターのコントローラーの本体
    /// </summary>
    public partial class CharacterController : MonoBehaviour, ITakeDamageable
    {
        [SerializeField] private CharacterData characterData;
        [SerializeField] private Animator anim;
        [SerializeField] private AttackSet attackSet;
        public AttackSet AttackSet=>attackSet;
        [SerializeField, ReadOnly] private float idleAnimSpeed = 1.1f;
        public float IdleAnimSpeed => idleAnimSpeed;
        public AttackType CurrentAttackType {private set; get; }
        public AttackDirection CurrentAttackDirection { private set; get; }
        public Animator Anim => anim;
        [SerializeField] private float groundCheckDistance;
        [SerializeField] private GameObject guardObj;
        [SerializeField] private LayerMask groundMask;
        private int currentJumpCount = 0;
        private int maxJumpCount = 2;
        public Rigidbody Rb=>rb;
        private Rigidbody rb;
        public StateMachine StateMachine => stateMachine;
        private StateMachine stateMachine;
        public CommandInvoker CommandInvoker => commandInvoker;
        private CommandInvoker commandInvoker;
        private Dictionary<System.Type, CharacterState> stateCache = new();
        public CharacterData Data => characterData;
        public float CurrentGuardPower => currentGuardPower;
        private float currentGuardPower;
        private bool isInvincible = false;
        public bool IsInvincible => isInvincible;
        private bool isGuarding = false;
        public bool IsGuarding => isGuarding;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

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

        private void Start()
        {
            // 初期状態はNeutral
            stateMachine.Start(GetState<NeutralState>());
        }

        private void Update()
        {
            commandInvoker.Update();
            CustomLogger.Info($"{stateMachine.CurrentState}", LogTagUtil.TagState);
        }

        private void FixedUpdate()
        {
            IsGrounded();
            commandInvoker.FixedUpdate();
        }

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
            }else if (command is CrouchCommand crouchCommand)
            {
                if (crouchCommand.IsPress)
                    StartCrouch();
                else
                    EndCrouch();
            }

            // Debug.Log($"{command}");
            currentState.OnCommandExecuted(command);
        }

        public bool IsGrounded()
        {
            Vector3 origin = transform.position;
            Vector3 dir = Vector3.down;
            // Debug.DrawRay(origin, dir * groundCheckDistance, Color.yellow);
            return Physics.Raycast(origin, dir, out RaycastHit hit, groundCheckDistance, groundMask);
        }

        public void SetGuardPower(float amount) => currentGuardPower = amount;
        public void DecreaseGuardPower(float amount) => currentGuardPower -= amount;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Ground")) return;
            // 着地時に横方向の速度を少し減衰
            Vector3 velocity = rb.velocity;
            velocity.x *= 0.8f;
            velocity.z *= 0.8f;
            rb.velocity = velocity;
            currentJumpCount = 0;
        }
        private void OnCollisionExit(Collision collision)
        {
            if (stateMachine.CurrentState == GetState<AirState>()) return;
            stateMachine.ChangeState(GetState<AirState>());
        }

        private void OnDestroy()
        {
            stateMachine?.Cancel();
        }
    }
}