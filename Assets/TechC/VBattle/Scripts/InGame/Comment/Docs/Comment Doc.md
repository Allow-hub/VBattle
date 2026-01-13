# Commentドキュメント

## 各クラスの役割（簡易まとめ）

## TODO
・GrassAbility内に書かれている、投げる処理と、コメントを持つ処理に分離してそれぞれ書く
→そうすることでアビリティを組み合わせるだけで特殊コメントができるようになる

ICommentAbilityを継承して書くこと

- **CommentDisplay**
  - コメント全体の表示・管理の中枢。SpawnerやMoverなどを統括。
- **CommentSpawner**
  - コメントの生成・出現位置の管理。
- **CommentMover**
  - コメントの移動・消滅処理を担当。
- **CommentMaterialApplier**
  - コメントや文字へのマテリアル適用を担当。
- **CommentFactory**
  - コメント本体や文字オブジェクトの生成・プール返却を管理。
- **BuffCommentTrigger / FreezeCommentTrigger**
  - コメント取得時のバフ・特殊効果の発動トリガー。
- **BuffManager**
  - プレイヤーに付与されているバフの管理。
- **BuffBase / SpeedBuff / AttackBuff / MapChangeBuff**
  - バフの基底クラスと各種バフの具体的な効果処理。
- **BuffFactory**
  - バフタイプに応じてバフインスタンスを生成するファクトリ。
- **BuffCommentData**
  - バフコメントの種類・テキストをScriptableObjectで管理。
- **SpecialCommentManager**
  - 特殊コメント（例：フリーズ）の返却処理を管理。
- **SpecialCommentTrigger**
  - 特殊コメントの当たり判定・効果発動を担当。
- **FreezeController**
  - 固定コメントの返却処理を担当。
- **CharPrefabDatabase**
  - 文字ごとのPrefabを管理するデータベース（ScriptableObject）。
- **NormalCommentData / SpecialCommentData**
  - 通常/特殊コメントのテキストをScriptableObjectで管理。
- **GrassController**
  - 草コメントのエフェクト・返却処理を担当。