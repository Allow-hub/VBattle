// CharacterController.cs (本体 - 構造と管理)
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

        private void RegisterState(CharacterState state) => stateCache[state.GetType()] = state;

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
                Debug.Log($"Command {command.Type} rejected in state {currentState?.GetType().Name}");
                return;
            }

            if (command is MoveCommand moveCmd)
                Move(moveCmd.Dir);
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

            currentState.OnCommandExecuted(command);
            TransitionByCommand(command);
        }

        /// <summary>
        /// コマンドによるステート分岐
        /// </summary>
        /// <param name="command">コマンドの種類</param>
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