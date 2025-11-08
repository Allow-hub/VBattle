using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    public partial class CharacterController : MonoBehaviour
    {
        [SerializeField] private CharacterData characterData;

        private Rigidbody rb;
        private StateMachine stateMachine;
        private CommandInvoker commandInvoker;
        private bool isGrounded;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            stateMachine = new StateMachine();
            commandInvoker = new CommandInvoker(this);
        }

        private void Update()
        {
            commandInvoker.Update(); // コマンドの実行
            // stateMachine.Run(stateMachine.CurrentState).Forget(); // 状態の更新
        }

        
        public void Move(Vector2 direction)
        {
            // 移動ロジックの実装
            Vector3 move = new Vector3(direction.x, 0, 0) * characterData.MoveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + move);
        }

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
        }
    }
}