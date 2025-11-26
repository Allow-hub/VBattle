# Characterの管理法

## BaseInput,PlayerInputManager
入力を受け取ってInputSnapshotという構造体で保存する<br>
この保存された構造体をCommandInvorkerが受け取り実際のCommandとしてExecuteする

## CommandInvorker(Invoker)
攻撃、ジャンプ、移動などを実行したいと通知

## それぞれのCommand(ConcreteCommand)
CommandTypeを持ち、それぞれのロジックに必要な情報を持つ<br>
情報しか持たないので構造体で定義

## CharacterController(Receiver)
FSMのStateMachineとCharacterDataを持つ。<br>
StateMachineの状態によって受け取ったCommandを受け付けるか拒否するかを決める<br>
コマンドがロジックを呼ぶよりもコマンドが通知してロジックとCharacterControllerをアダプターするクラスに任せるのが綺麗だが過剰実装なので今回はCharacterControllerにロジックを持たせる

## StateMachin,CharacterState
FSMのStateMachine、CancellationTokenSourceを使用して中断を安全に<br>

# 攻撃
AttackState の攻撃処理フロー（現状仕様まとめ）
## 状態遷移時 – OnEnter()
条件	処理<br>
空中	AttackType = Air & Direction = Neutral に強制設定、空中用 AttackData を取得<br>
地上	入力済みの AttackType / Direction を使用し AttackData を取得<br>
共通で行われること

isChainRequested = false<br>
canCancel = false<br>
Animator.IsAttacking = true（Exclusive 設定）<br>
Animator.speed = currentAttackData.animationSpeed<br>

## 攻撃ループ – OnUpdate()

攻撃は while(true) で実行 → 連鎖が成立した場合のみループ継続、成立しなければ終了。<br>
攻撃開始<br>
canCancel = false<br>
cancelStartTime まで待機<br>
この期間はキャンセル不可<br>
キャンセル可能期間開始<br>
canCancel = true<br>
cancelEndTime までの間だけ CanExecuteCommand が Attack を受け付ける<br>
Attack入力 + 連鎖可能条件付きの場合 → isChainRequested = true（予約）<br>
cancelWindow 終了後 canCancel = false<br>

## 連鎖判定
判定	結果<br>
isChainRequested == true かつ currentAttackData.canChain == true かつ currentAttackData.nextChain != null	次の攻撃へ連鎖<br>
上記以外	連鎖不可として終了処理へ<br>
連鎖成立時の処理  
currentAttackData = nextChain  
chain++  
Animator.Chain = chain  
Animator.speed = currentAttackData.animationSpeed  
continue → ループ先頭へ戻り次の攻撃を実行  

⏳ 連鎖しない場合の残り時間処理  
キャンセル受付終了後：  
残り攻撃時間 (attackDuration - cancelEndTime) を待機  
recoveryDuration（硬直）を待機  
完了 → ループ break  
🌬 攻撃終了後 – 状態遷移  
空中攻撃だった場合 → AirState へ  
地上攻撃だった場合 → NeutralState へ  

## OnExit()

AttackState から抜ける際に必ず行われる処理：  
Animator.Chain = 0  
Animator.speed = IdleAnimSpeed へ戻す  
Animator.IsAttacking = false  
canCancel = false  
isChainRequested = false  
chain = 0  

## 攻撃処理タイムライン（図解）
┌────────────── 攻撃開始 ──────────────┐
[攻撃演出区間] -----------★---------------(攻撃継続)
                         ↑ cancelStartTime
                 ← ← ← キャンセル受付時間 → → →
                         ↓ cancelEndTime
       ★ 連鎖入力成功 → 次の攻撃ループへ continue
       × 連鎖なし     → 攻撃残り時間 → 硬直 → 攻撃終了
└─────────────────────────────────────┘