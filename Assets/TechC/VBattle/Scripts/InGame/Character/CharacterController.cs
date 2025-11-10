using UnityEngine;
using System.Collections.Generic;

namespace TechC.VBattle.InGame.Character
{
    public partial class CharacterController : MonoBehaviour, ITakeDamageable
    {
        [SerializeField] private CharacterData characterData;

        private Rigidbody rb;
        private StateMachine stateMachine;
        private CommandInvoker commandInvoker;
        private bool isGrounded;

        // 状態のキャッシュ
        private Dictionary<System.Type, CharacterState> stateCache = new();

        // 外部から参照可能なプロパティ
        public bool IsGrounded => isGrounded;
        public CharacterData Data => characterData;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            // すべての状態を登録してキャッシュ
            RegisterState(new NeutralState(this));
            RegisterState(new AirState(this));
            RegisterState(new AttackState(this));
            RegisterState(new DamageState(this));
            RegisterState(new GuardState(this));

            stateMachine = new StateMachine();
            commandInvoker = new CommandInvoker(this);
        }

        private void Start()
        {
            // 初期状態はNeutral
            stateMachine.Start(GetState<NeutralState>());
        }

        private void Update()
        {
            commandInvoker.Update();
        }

        private void RegisterState(CharacterState state)
        {
            stateCache[state.GetType()] = state;
        }

        /// <summary>
        /// 状態取得用のヘルパーメソッド
        /// </summary>
        public T GetState<T>() where T : CharacterState
        {
            if (stateCache.TryGetValue(typeof(T), out var state))
            {
                return state as T;
            }
            Debug.LogError($"State {typeof(T).Name} not found!");
            return null;
        }

        // ==========================================
        // 実際のアクション実行メソッド
        // ==========================================

        public void Move(Vector2 direction)
        {
            Vector3 move = new Vector3(direction.x, 0, 0) * characterData.MoveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + move);
        }

        public void Jump()
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * characterData.JumpPower, ForceMode.Impulse);
                isGrounded = false;
                
                // 空中状態へ遷移
                stateMachine.ChangeState(GetState<AirState>());
            }
        }

        public void Attack(AttackType type, AttackDirection direction)
        {
        }

        public void StartGuard()
        {
        }

        public void EndGuard()
        {
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(float damage, float stunDuration = 0.3f)
        {
            var damageState = GetState<DamageState>();
            damageState.SetStunDuration(stunDuration);
            stateMachine.ChangeState(damageState);
        }

        // ==========================================
        // コマンド実行（状態に判断を委譲）
        // ==========================================
        public void ExecuteCommand(ICommand command)
        {
            var currentState = stateMachine.CurrentState;

            // 現在の状態がコマンドを受け付けるかチェック
            if (currentState == null || !currentState.CanExecuteCommand(command))
            {
                Debug.Log($"Command {command.Type} rejected in state {currentState?.GetType().Name}");
                return;
            }

            // コマンド実行
            command.Execute();

            // 状態にコマンド実行を通知
            currentState.OnCommandExecuted(command);

            // コマンドに応じて状態遷移
            TransitionByCommand(command);
        }

        private void TransitionByCommand(ICommand command)
        {
            CharacterState nextState = null;

            switch (command.Type)
            {
                case CommandType.Jump:
                    // ジャンプは内部でAirStateへ遷移
                    break;

                case CommandType.Attack:
                    nextState = GetState<AttackState>();
                    break;

                case CommandType.Guard:
                    var currentState = stateMachine.CurrentState;
                    if (currentState is GuardState)
                    {
                        // ガード解除
                        EndGuard();
                        nextState = GetState<NeutralState>();
                    }
                    else
                    {
                        // ガード開始
                        StartGuard();
                        nextState = GetState<GuardState>();
                    }
                    break;

                case CommandType.Move:
                    // 移動は状態遷移しない（NeutralやAirで継続）
                    break;
            }

            if (nextState != null)
            {
                stateMachine.ChangeState(nextState);
            }
        }

        // ==========================================
        // 物理判定
        // ==========================================

        private void OnCollisionEnter(Collision collision)
        {
            isGrounded = true;
        }
        
        private void OnCollisionExit(Collision collision)
        {
            isGrounded = false;
            if (stateMachine.CurrentState == GetState<AirState>()) return;
            stateMachine.ChangeState(GetState<AirState>());
        }

        private void OnDestroy()
        {
            stateMachine?.Cancel();
        }
    }
}