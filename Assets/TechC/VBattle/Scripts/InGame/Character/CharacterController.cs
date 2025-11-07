using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// キャラクター制御（FSM管理）
    /// </summary>
    public partial class CharacterController : MonoBehaviour
    {
        [SerializeField] private CharacterData characterData;

        private Rigidbody rb;
        private StateMachine stateMachine;
        private bool isGrounded;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            stateMachine = new StateMachine();
        }

        private void Start()
        {
            SetupStates();
        }

        private void FixedUpdate()
        {
            // 簡易的な接地判定
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, LayerMask.GetMask("Default"));
        }

        private void Move(Vector3 dir)
        {
            Vector3 move = dir * characterData.MoveSpeed;
            Vector3 velocity = new Vector3(move.x, rb.velocity.y, move.z);
            rb.velocity = velocity;
        }

        private void Jump()
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * characterData.JumpPower, ForceMode.Impulse);
        }

        private void SetupStates()
        {
            // var idle = new TECH.C.Athletic_Playground.StateMachine.State();
            // var move = new TECH.C.Athletic_Playground.StateMachine.State();
            // var jump = new TECH.C.Athletic_Playground.StateMachine.State();

            // // --- Idle ---
            // idle.OnEnter = prev => Debug.Log("Enter Idle");
            // idle.OnUpdate = async ct =>
            // {
            //     while (!ct.IsCancellationRequested)
            //     {
            //         var input = GetInput();
            //         if (input.sqrMagnitude > 0.1f) return move;
            //         if (Input.GetButtonDown("Jump") && isGrounded) return jump;
            //         await UniTask.Yield(ct);
            //     }
            //     return idle;
            // };

            // // --- Move ---
            // move.OnEnter = prev => Debug.Log("Enter Move");
            // move.OnUpdate = async ct =>
            // {
            //     while (!ct.IsCancellationRequested)
            //     {
            //         var input = GetInput();
            //         if (input.sqrMagnitude < 0.1f) return idle;
            //         if (Input.GetButtonDown("Jump") && isGrounded) return jump;

            //         Move(input);
            //         await UniTask.Yield(ct);
            //     }
            //     return idle;
            // };

            // // --- Jump ---
            // jump.OnEnter = prev =>
            // {
            //     Debug.Log("Enter Jump");
            //     Jump();
            // };
            // jump.OnUpdate = async ct =>
            // {
            //     while (!isGrounded && !ct.IsCancellationRequested)
            //     {
            //         Move(GetInput());
            //         await UniTask.Yield(ct);
            //     }
            //     return idle;
            // };

            // // FSM実行開始
            // var startState = idle;
            // stateMachine.Run(startState);
        }
    }
}