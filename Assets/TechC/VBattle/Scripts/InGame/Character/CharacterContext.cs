using UnityEngine;

namespace TechC.VBattle.InGame.Character
{
    /// <summary>
    /// バトル中のキャラクターの状態情報（データのみ）
    /// </summary>
    public class CharacterContext
    {
        // 基本情報
        public CharacterData Data { get; }
        public CharacterController Controller { get; }
        public int PlayerIndex { get; } // 0: P1, 1: P2
        public string DeviceName { get; }
        public bool IsNPC { get; }
        
        // バトル中の状態
        public int CurrentHP { get; set; }
        public float CurrentGuardPower { get; set; }
        
        
        // 状態フラグ（Controllerから参照）
        // public bool IsGrounded => Controller.IsGrounded;
        // public bool IsGuarding => Controller.IsGuarding;
        // public bool IsInvincible => Controller.IsInvincible;
        
        public CharacterContext(
            CharacterData data, 
            CharacterController controller, 
            int playerIndex,
            string deviceName,
            bool isNPC)
        {
            Data = data;
            Controller = controller;
            PlayerIndex = playerIndex;
            DeviceName = deviceName;
            IsNPC = isNPC;
            
            // 初期値設定
            CurrentHP = data.MaxHP;
            CurrentGuardPower = data.GuardPower;
        }
        
        public bool IsDead => CurrentHP <= 0;
        public bool IsGuardBroken => CurrentGuardPower <= 0;
        public float HPRatio => (float)CurrentHP / Data.MaxHP;
        public float GuardPowerRatio => CurrentGuardPower / Data.GuardPower;
    }

}