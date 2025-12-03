namespace TechC.CommentSystem
{
   /// <summary>
    /// コメントタイプを定義
    /// </summary>
    public enum CommentType
    {
        Normal,
        SpeedBuff,
        AttackBuff,
        Grass,
        Freeze
    }

    public class CommentData
    {
        public CommentType type;
        public string text;
        public BuffType? buffType;

        /* コンストラクタでコメントタイプ、テキスト、バフタイプを設定 */
        public CommentData(CommentType type, string text, BuffType? buffType)
        {
            this.type = type;
            this.text = text;
            this.buffType = buffType;
        }
    }
}
