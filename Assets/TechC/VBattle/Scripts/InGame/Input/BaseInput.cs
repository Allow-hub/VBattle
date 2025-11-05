using System;
using System.Collections;
using System.Collections.Generic;
using TechC.VBattle.Core;
using TechC.VBattle.InGame.Character;
using UnityEngine;

namespace TechC.VBattle.InGame.Input
{
    /// <summary>
    /// キャラクター入力を管理する抽象基底クラス
    /// PlayerとNpcの共通処理を定義します
    /// </summary>
    public abstract class BaseInputManager : MonoBehaviour
    {
        // [SerializeField] protected Player.CharacterController characterController;
        // [SerializeField, ReadOnly] private string inputLogId = "input";
        // protected CharacterState characterState;
        // protected CommandHistory commandHistory;

        // protected Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

        // [Header("Guard")]
        // [SerializeField] private GameObject guardObj;

        // // 入力状態
        // protected Vector2 moveInput;
        // protected bool isMoving;
        // protected bool lastIsMoving;
        // protected bool isCrouching;
        // protected bool isGuarding;
        // protected bool isJumping;
        // protected bool isAppealing;
        // protected bool isWeakAttacking;
        // protected bool isStrongAttacking;
        // protected bool isDashing;

        // // コマンド名定義
        // protected string moveCommand = "Move";
        // protected string jumpCommand = "Jump";
        // protected string attackCommand = "Attack";
        // protected string crouchCommand = "Crouch";
        // protected string guardCommand = "Guard";

        // // ダッシュ関連変数
        // protected float lastMoveEndTime = 0f;
        // [SerializeField] protected float dashTimeWindow = 0.3f;
        // protected bool hasMoved = false;
        // protected int lastMoveDirection = 0;
        // protected int currentMoveDirection = 0;
        // // プロパティ
        // public Vector2 MoveInput => moveInput;
        // public bool IsMoving => isMoving;
        // public bool IsCrouching => isCrouching;
        // public bool IsJumping => isJumping;
        // public bool IsGuarding => isGuarding;
        // public bool IsAppealing => isAppealing;
        // public bool IsWeakAttacking => isWeakAttacking;
        // public bool IsStrongAttacking => isStrongAttacking;
        // public bool IsDashing => isDashing;

        // protected virtual void Awake()
        // {
        //     // CommandHistoryを取得
        //     commandHistory = GetComponent<CommandHistory>();
        // }

        // protected virtual void Start()
        // {
        //     if (characterController != null)
        //     {
        //         characterState = characterController.GetCharacterState();

        //         // 基本コマンドを登録
        //         RegisterCommands();
        //     }
        //     else
        //     {
        //         CustomLogger.Error("CharacterControllerが設定されていません", inputLogId);
        //     }
        // }

        // /// <summary>
        // /// 基本コマンドを登録するメソッド
        // /// </summary>
        // protected virtual void RegisterCommands()
        // {
        //     commands[moveCommand] = new MoveCommand(characterController, this);
        //     commands[jumpCommand] = new JumpCommand(characterController, this);
        //     commands[attackCommand] = new AttackCommand(characterState, characterController);
        //     commands[crouchCommand] = new CrouchCommand(characterController, this);
        //     commands[guardCommand] = new GuardCommand(characterState, characterController, this, characterController.GetCharacterData(), guardObj);
        // }

        // protected virtual void Update()
        // {
        //     CheckDash();

        //     if(moveInput.x == 0)
        //     {
        //        characterController.SetAnim(AnimatorParams.IsWalking,false);
        //        characterController.SetAnim(AnimatorParams.IsRunning,false); 
        //     }
        // }

        // /// <summary>
        // /// ダッシュ条件をチェックするメソッド
        // /// </summary>
        // protected virtual void CheckDash()
        // {
        //     if (moveInput.x != 0)
        //     {
        //         currentMoveDirection = moveInput.x > 0 ? 1 : -1;
        //         characterState.EnqueueCommand(commands[moveCommand]);

        //         if (hasMoved &&
        //             Time.time - lastMoveEndTime <= dashTimeWindow &&
        //             currentMoveDirection == lastMoveDirection)
        //         {
        //             isDashing = true;
        //             hasMoved = false;
        //         }
        //     }
        //     else
        //     {
        //         if (lastIsMoving && !isMoving)
        //         {
        //             lastMoveEndTime = Time.time;
        //             hasMoved = true;
        //             lastMoveDirection = currentMoveDirection;
        //         }

        //         isDashing = false;
        //         currentMoveDirection = 0;
        //     }

        //     lastIsMoving = isMoving;
        // }


        // /// <summary>
        // /// 全ての入力をリセット
        // /// </summary>
        // public void ResetInput()
        // {
        //     isMoving = false;
        //     isJumping = false;
        //     isGuarding = false;
        //     isAppealing = false;
        //     isCrouching = false;
        //     isDashing = false;
        //     isWeakAttacking = false;
        //     isStrongAttacking = false;
        // }

        // /// <summary>
        // /// コマンドのインスタンスを外部から取得する
        // /// </summary>
        // /// <param name="commandName"></param>
        // /// <returns></returns>
        // public ICommand GetCommandInstance(string commandName) => commands[commandName];

        // // public void OnMenu() => MenuManager.I.OpenMenu();

        // // 各入力に対する抽象メソッド - 継承先で実装
        // public abstract void OnMove(Vector2 inputValue, bool started, bool canceled);
        // public abstract void OnJump(bool started, bool canceled);
        // public abstract void OnCrouch(bool started, bool canceled);
        // public abstract void OnGuard(bool started, bool canceled);
        // public abstract void OnWeakAttack(bool started, bool canceled);
        // public abstract void OnStrongAttack(bool started, bool canceled);
        // public abstract void OnAppeal(bool started, bool canceled);
    }
}
