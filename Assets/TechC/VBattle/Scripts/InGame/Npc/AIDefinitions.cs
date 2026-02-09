namespace TechC.VBattle.InGame.Npc
{
    /// <summary>
    /// 戦闘距離の定義
    /// 相手との距離に応じて戦闘戦略を変更するための列挙型
    /// </summary>
    public enum BattleRange
    {
        /// <summary>
        /// 近距離戦闘（0-2.0f）
        /// - 攻撃が当たりやすい距離
        /// - 相手の攻撃も受けやすい
        /// - 攻撃・ガード
        /// </summary>
        Close,

        /// <summary>
        /// 中距離戦闘（2.0f-5.0f）
        /// - 攻撃の間合いを測る距離
        /// - 接近か後退かの判断が重要
        /// </summary>
        Medium,

        /// <summary>
        /// 遠距離戦闘（5.0f以上）
        /// - 攻撃が届かない距離
        /// - 基本的に接近が必要
        /// </summary>
        Far
    }

    /// <summary>
    /// AI行動タイプの定義
    /// AIが実行できる基本的な行動パターン
    /// </summary>
    public enum AIActionType
    {
        /// <summary>
        /// 接近行動 - 相手に近づく
        /// 使用場面：遠距離・中距離で攻撃範囲に入りたい時
        /// </summary>
        Approach,

        /// <summary>
        /// 後退行動 - 相手から離れる
        /// 使用場面：近距離で危険を感じた時、間合いを取り直したい時
        /// </summary>
        Retreat,

        /// <summary>
        /// 攻撃行動 - 弱攻撃または強攻撃
        /// 使用場面：攻撃範囲内にいる時、積極的に攻める時
        /// </summary>
        Attack,

        /// <summary>
        /// ガード行動 - 防御姿勢を取る
        /// 使用場面：相手の攻撃を予測した時、守勢に回る時
        /// </summary>
        Guard,

        /// <summary>
        /// ジャンプ行動 - 上方向への移動
        /// 使用場面：相手の攻撃を避ける時、位置を変える時
        /// </summary>
        Jump,

        /// <summary>
        /// しゃがみ行動 - 低い姿勢を取る
        /// 使用場面：上段攻撃を避ける時、下段攻撃の準備
        /// </summary>
        Crouch,

        /// <summary>
        /// 待機行動 - 何もしない
        /// 使用場面：相手の出方を見る時、次の行動を考える時
        /// </summary>
        Wait
    }

    /// <summary>
    /// 敵AIの難易度設定
    /// </summary>
    public enum EnemyDifficulty
    {
        Debug,   // インスペクターの値をそのまま使う
        Easy,    // 簡単
        Normal,  // 普通

        // 今後実装予定
        // Hard,    // 難しい
        // Expert   // 上級者向け
    }
}
