# Characterの管理法

## BaseInput,PlayerInputManager
入力を受け取ってInputSnapshotという構造体で保存する<br>
この保存された構造体をCommandInvorkerが受け取り実際のCommandとしてExecuteする

## CommandInvorker(Invoker)
攻撃、ジャンプ、移動などを実行したいと通知

## それぞれのCommand(ConcreteCommand)
CommandTypeを持ち、それぞれのロジックに必要な情報を持つ

## CharacterController(Receiver)
FSMのStateMachineとCharacterDataを持つ。<br>
StateMachineの状態によって受け取ったCommandを受け付けるか拒否するかを決める<br>