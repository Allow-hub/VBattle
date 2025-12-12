# Wine-5のやるべきリスト

## コメント関連の移植作業
- [ ] PRの修正

## カメラの制御
- [ ] Shake
- [ ] ズーム
- [ ] パン

### 注意点
- イベントバスから攻撃のリザルトイベントを購読してShakeをする

## 必要なクラス一覧

- **CameraController** - カメラエフェクトの統合管理とEventBusからのイベント購読

- **ICameraEffect** - カメラエフェクトの共通インターフェース定義（State、Apply、Stop）
- **CameraShake** - カメラを振動させるエフェクトの実装
- **CameraPan** - カメラの位置を上下左右に移動させるエフェクトの実装  
- **CameraZoom** - カメラのFOVを変更してズーム効果を実装

- **CameraEffectState** - エフェクトの状態を表すEnum（Idle、Active、Completed）