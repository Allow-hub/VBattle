# Select画面 MVC リファクタリング設計書

## 1. 現状の問題点

### 1.1 責務の不明確さ
- `SelectUIManager`がデータ管理・ロジック・UI制御を全て担当
- `CharaButton`が入力処理とデータ更新を直接実行
- `IconController`がデバイス管理とUI表示を兼任

### 1.2 データの整合性問題
- 構造体`CharacterPick`の部分更新による値の不整合
- **2P情報がDataBridgeに渡らない根本原因**
- NPC時の特別処理が複雑で追いにくい

### 1.3 テスト・保守性の低さ
- UIとロジックが密結合でテストが困難
- 変更の影響範囲が予測しづらい

---

## 2. MVC採用の理由

### 2.1 Select画面の特性
✅ **多様な入力**: マウス、キーボード、複数ゲームパッド  
✅ **複雑なUI更新**: サムネイル、名前、アニメーション、エフェクト  
✅ **明確な状態管理**: 誰がどのデバイスでどのキャラを選んだか  

→ **MVCパターンが最適**

### 2.2 期待される効果
- ✅ データの整合性保証（2P情報null問題の解決）
- ✅ 入力処理の統一的な管理
- ✅ UI更新ロジックの分離
- ✅ テスタビリティの向上

---

## 3. MVC設計方針

```
┌─────────────────────────────────────────┐
│          Controller層                    │
│  SelectSceneController                  │
│   - 全ての入力イベントを受付             │
│   - Modelの更新とViewの描画を制御       │
│   - ゲームフロー（開始・キャンセル）管理 │
└─────────────────────────────────────────┘
         ↓ 更新指示          ↑ 状態変更通知
┌──────────────────┐   ┌──────────────────┐
│   Model層         │   │    View層         │
│ SelectSceneModel  │   │  SelectSceneView  │
│  - 選択状態の保持  │   │   - UI表示のみ    │
│  - データ検証      │   │   - アニメーション │
│  - 永続性         │   │   - エフェクト     │
└──────────────────┘   └──────────────────┘
```

---

## 4. アプローチの選択

### 4.1 軽量MVC（推奨）★
Select画面の複雑さを考慮すると、これが最適解です。

**メリット:**
- シンプルで理解しやすい
- 既存コードからの移行が容易
- オーバーエンジニアリングを回避

**実装案:**

#### `SelectionData`（データ層）
```csharp
public class SelectionData
{
    public PlayerData Player1 { get; private set; } = new();
    public PlayerData Player2 { get; private set; } = new();
    
    public event Action OnDataChanged;
    
    public class PlayerData
    {
        public InputDevice Device;
        public CharacterData Character;
        public bool IsPicked;
        public bool IsNpc => Device == null;
        
        public void Clear()
        {
            Device = null;
            Character = null;
            IsPicked = false;
        }
    }
    
    public PlayerData GetPlayer(int playerId) => playerId == 1 ? Player1 : Player2;
    
    public int? GetPlayerIdByDevice(InputDevice device)
    {
        if (Player1.Device == device) return 1;
        if (Player2.Device == device) return 2;
        return null;
    }
}
```

#### `SelectionController`（ロジック+制御層）
```csharp
public class SelectionController : MonoBehaviour
{
    [SerializeField] private SelectionData data = new();
    [SerializeField] private SelectionView view;
    [SerializeField] private DeviceSelectionPresenter player1DevicePresenter;
    [SerializeField] private DeviceSelectionPresenter player2DevicePresenter;
    
    // NPC関連データ
    [SerializeField] private CharacterData npcAmeData;
    [SerializeField] private CharacterData npcSyoData;
    // ... 他のNPCデータ
    
    private void Start()
    {
        data.OnDataChanged += () => view.Refresh(data);
        
        // イベント購読
        player1DevicePresenter.OnDeviceSelected += (device) => HandleDeviceSelect(1, device);
        player2DevicePresenter.OnDeviceSelected += (device) => HandleDeviceSelect(2, device);
    }
    
    public void OnCharacterClicked(InputDevice device, CharacterData character)
    {
        // 通常の選択
        int? playerId = data.GetPlayerIdByDevice(device);
        if (playerId.HasValue)
        {
            SetCharacterForPlayer(playerId.Value, character);
            return;
        }
        
        // NPC特別処理（1Pが2Pを選ぶ）
        if (CanPlayer1SelectForNpc(device))
        {
            var npcCharacter = ConvertToNpcData(character);
            SetCharacterForPlayer(2, npcCharacter);
        }
    }
    
    private void HandleDeviceSelect(int playerId, InputDevice device)
    {
        data.GetPlayer(playerId).Device = device;
        data.OnDataChanged?.Invoke();
    }
    
    private void SetCharacterForPlayer(int playerId, CharacterData character)
    {
        var player = data.GetPlayer(playerId);
        player.Character = character;
        data.OnDataChanged?.Invoke();
    }
    
    // NPC処理の明示化
    private bool CanPlayer1SelectForNpc(InputDevice device)
    {
        return data.Player1.IsPicked 
            && data.Player2.IsNpc
            && data.Player1.Device == device;
    }
    
    private CharacterData ConvertToNpcData(CharacterData playerCharacter)
    {
        if (playerCharacter.name.Contains("Ame")) return npcAmeData;
        if (playerCharacter.name.Contains("Syo")) return npcSyoData;
        // ... 他のキャラ
        return playerCharacter;
    }
    
    public void OnConfirmPick(int playerId)
    {
        var player = data.GetPlayer(playerId);
        if (player.Character == null)
            throw new InvalidOperationException($"Player{playerId}: Character not selected");
        
        player.IsPicked = true;
        data.OnDataChanged?.Invoke();
        
        if (data.Player1.IsPicked && data.Player2.IsPicked)
            OnBothPlayersReady();
    }
    
    private void OnBothPlayersReady()
    {
        // スタートボタン表示など
    }
}
```

#### `SelectionView`（表示層）
```csharp
public class SelectionView : MonoBehaviour
{
    [SerializeField] private PlayerView player1View;
    [SerializeField] private PlayerView player2View;
    
    public void Refresh(SelectionData data)
    {
        player1View.UpdateDisplay(data.Player1);
        player2View.UpdateDisplay(data.Player2);
    }
}

public class PlayerView : MonoBehaviour
{
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private Image nameImage;
    [SerializeField] private SelectPickAnim pickAnimation;
    
    public void UpdateDisplay(SelectionData.PlayerData playerData)
    {
        if (playerData.Character != null)
        {
            thumbnailImage.sprite = playerData.Character.thumbnail;
            nameImage.sprite = playerData.Character.nameSprite;
        }
        
        if (playerData.IsPicked)
            pickAnimation.PlayAnimation();
    }
}
```

#### `DeviceSelectionPresenter`（デバイス選択専用）
```csharp
public class DeviceSelectionPresenter : MonoBehaviour
{
    [SerializeField] private IconControllerView iconView;
    [SerializeField] private int playerId;
    
    public event Action<InputDevice> OnDeviceSelected;
    
    public void ShowDeviceSelection()
    {
        var devices = GetAvailableDevices();
        iconView.ShowDeviceIcons(devices, OnIconClicked);
    }
    
    private void OnIconClicked(InputDevice device)
    {
        OnDeviceSelected?.Invoke(device);
        iconView.CloseIcons();
    }
    
    private List<InputDevice> GetAvailableDevices()
    {
        var devices = new List<InputDevice>();
        
        // キーボード
        if (Keyboard.current != null)
            devices.Add(Keyboard.current);
        
        // ゲームパッド
        foreach (var gamepad in Gamepad.all)
            devices.Add(gamepad);
        
        return devices;
    }
}
```

---

### 4.2 完全MVC（将来の拡張用）
Select画面が将来的に複雑化する場合のオプション。

#### `PlayerSelectModel`（データ層）
```csharp
public class PlayerSelectModel
{
    public int PlayerId { get; }
    public InputDevice InputDevice { get; private set; }
    public CharacterData SelectedCharacter { get; private set; }
    public bool IsPicked { get; private set; }
    public bool IsNpc => InputDevice == null;
    
    public event Action OnStateChanged;
    
    // シンプルなセッター（検証はControllerで）
    public void SetDevice(InputDevice device) 
    {
        InputDevice = device;
        OnStateChanged?.Invoke();
    }
    
    public void SetCharacter(CharacterData character)
    {
        SelectedCharacter = character;
        OnStateChanged?.Invoke();
    }
    
    public void ConfirmPick()
    {
        if (SelectedCharacter == null) 
            throw new InvalidOperationException("Character not selected");
        IsPicked = true;
        OnStateChanged?.Invoke();
    }
    
    public void Reset()
    {
        InputDevice = null;
        SelectedCharacter = null;
        IsPicked = false;
        OnStateChanged?.Invoke();
    }
}
```

#### `SelectSceneModel`（シーン全体の状態管理）
```csharp
public class SelectSceneModel
{
    private PlayerSelectModel player1 = new PlayerSelectModel(1);
    private PlayerSelectModel player2 = new PlayerSelectModel(2);
    
    public event Action<int> OnPlayerStateChanged;
    public event Action OnBothPlayersPicked;
    
    public PlayerSelectModel GetPlayer(int playerId) 
        => playerId == 1 ? player1 : player2;
    
    public int? GetPlayerIdByDevice(InputDevice device)
    {
        if (player1.InputDevice == device) return 1;
        if (player2.InputDevice == device) return 2;
        return null;
    }
    
    public CharacterPick[] GetAllPicks()
    {
        return new[]
        {
            new CharacterPick 
            { 
                playerId = 1, 
                characterData = player1.SelectedCharacter,
                inputDevice = player1.InputDevice
            },
            new CharacterPick 
            { 
                playerId = 2, 
                characterData = player2.SelectedCharacter,
                inputDevice = player2.InputDevice
            }
        };
    }
}
```

---

### 4.3 共通コンポーネント

#### `CharaButton`（リファクタリング）
```csharp
public class CharaButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private CharacterData characterData;
    [SerializeField] private Sprite player1Sprite;
    [SerializeField] private Sprite player2Sprite;
    
    // Controllerに通知するだけ
    public event Action<InputDevice, CharacterData> OnCharacterHovered;
    public event Action<InputDevice, CharacterData> OnCharacterClicked;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        var device = ResolveDevice(eventData);
        OnCharacterHovered?.Invoke(device, characterData);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        var device = ResolveDevice(eventData);
        OnCharacterClicked?.Invoke(device, characterData);
    }
    
    private InputDevice ResolveDevice(PointerEventData eventData)
    {
        // GamepadPointerからデバイスを特定
        // ... 既存ロジック
    }
}
```

#### `IconControllerView`（表示専用）
```csharp
public class IconControllerView : MonoBehaviour
{
    [SerializeField] private Transform iconParent;
    [SerializeField] private GameObject iconPrefab;
    
    private List<GameObject> activeIcons = new List<GameObject>();
    
    public void ShowDeviceIcons(List<InputDevice> devices, Action<InputDevice> onSelected)
    {
        ClearIcons();
        
        foreach (var device in devices)
        {
            var icon = Instantiate(iconPrefab, iconParent);
            var button = icon.GetComponent<Button>();
            button.onClick.AddListener(() => onSelected?.Invoke(device));
            
            // アイコン画像設定
            SetIconSprite(icon, device);
            
            activeIcons.Add(icon);
        }
    }
    
    public void CloseIcons()
    {
        // アニメーション付きで閉じる
        foreach (var icon in activeIcons)
            Destroy(icon);
        activeIcons.Clear();
    }
    
    private void ClearIcons()
    {
        foreach (var icon in activeIcons)
            Destroy(icon);
        activeIcons.Clear();
    }
    
    private void SetIconSprite(GameObject icon, InputDevice device)
    {
        // デバイスタイプに応じたスプライト設定
    }
}
```

#### `CharacterSelectManager`（簡素化）
```csharp
public class CharacterSelectManager : MonoBehaviour
{
    [SerializeField] private SelectionController controller;
    
    private void OnStartGameRequested()
    {
        var data = controller.GetSelectionData();
        
        // DataBridgeに設定
        GameDataBridge.I.SetupPlayer(1, CreatePlayerData(data.Player1));
        GameDataBridge.I.SetupPlayer(2, CreatePlayerData(data.Player2));
        
        // シーン遷移
        SceneLoader.I.LoadBattleSceneAsync();
    }
    
    private PlayerData CreatePlayerData(SelectionData.PlayerData playerData)
    {
        return new PlayerData
        {
            characterData = playerData.Character,
            inputDevice = playerData.Device,
            isNpc = playerData.IsNpc
        };
    }
}
```

---

## 5. データフロー

### 5.1 キャラクター選択フロー
```
1. ユーザー入力
   CharaButton.OnPointerClick
         ↓
2. Controllerで受付
   SelectSceneController.HandleCharacterClick()
         ↓
3. Modelを更新
   SelectSceneModel.TrySetCharacter()
         ↓
4. イベント通知
   OnPlayerStateChanged.Invoke()
         ↓
5. Viewを更新
   PlayerSelectView.UpdateThumbnail()
```

### 5.2 デバイス選択フロー
```
1. アイコンクリック
   IconControllerView.OnDeviceSelected
         ↓
2. Controllerで受付
   SelectSceneController.HandleDeviceSelect()
         ↓
3. Modelを更新
   SelectSceneModel.TryAssignDevice()
         ↓
4. Viewを更新
   IconControllerView.UpdateCurrentDevice()
```

---

## 6. 段階的な移行計画

### Phase 1: Model層の構築
1. `PlayerSelectModel`クラスを作成
2. `SelectSceneModel`クラスを作成
3. 既存の`SelectUIManager`からデータ部分を移行

### Phase 2: View層の分離
1. `PlayerSelectView`を作成
2. `CharaButton`をイベント駆動に変更
3. `IconControllerView`を表示専用に変更
（ストラングラーパターン）

**重要原則**: 一度に全て変更せず、機能単位で新旧並行稼働させながら移行

### Phase 1: 並行稼働の準備（リスク最小化）
```csharp
// 1-1. 新しいデータクラスを作成（既存コードは触らない）
public class SelectionData { /*...*/ }

// 1-2. SelectUIManagerで新旧両方を更新
public class SelectUIManager
{
    private SelectionData newData = new(); // 追加
    private CharacterPick[] currentPicks; // 既存
    
    public void SetCharacterPick(int playerId, CharacterData character, InputDevice device)
    {
        // 旧システム（既存の動作を保証）
        currentPicks[playerId - 1] = new CharacterPick 
        { 
            playerId = playerId, 
            characterData = character,
            inputDevice = device
        };
        
        // 新システム（並行稼働）
        var player = newData.GetPlayer(playerId);
        player.Character = character;
        player.Device = device;
        newData.OnDataChanged?.Invoke();
    }
}

// 1-3. 動作確認（新データが正しく更新されているか検証）
```

**Phase 1のゴール**: 新クラスが存在するが、既存の動作に影響を与えない状態

---

### Phase 2: デバイス選択だけ新システムに移行
```csharp
// 2-1. DeviceSelectionPresenterを作成してPrefabに配置
public class DeviceSelectionPresenter { /*...*/ }

// 2-2. SelectUIManagerのデバイス選択部分を新システムに委譲
public class SelectUIManager
{
    [SerializeField] private DeviceSelectionPresenter player1DevicePresenter; // 追加
    
    private vo・その他の課題

### 8.1 既存コードへの影響
- ✅ Prefabの参照が変わる可能性 → Phase 2以降で段階的に更新
- ✅ 既存のイベント購読コードの修正が必要 → 各Phase内で対応
- ⚠️ GamepadPointerとの連携 → 既存の仕組みを維持

### 8.2 移行期間中のリスク
- ⚠️ **最大のリスク**: Phase 3まで動作確認できない（旧計画）
- ✅ **対策**: ストラングラーパターンで各Phase毎に動作確認
- ✅ **ロールバック**: 問題があれば前のPhaseに戻せる

### 8.3 パフォーマンス
- イベント駆動によるオーバーヘッドは微小（Select画面なので問題なし）
- 必要に応じて最適化を検討（基本的には不要）

### 8.4 完全MVCが必要になるケース
将来的に以下の要件が出た場合は、軽量MVCから完全MVCに移行：
- オンライン対戦実装（状態同期が必要）
- リプレイ機能（状態の記録・再生）
- 3人以上のプレイヤー対応
- 複雑なマッチング処理
- キャラクター選択のアニメーション演出が大幅に増える

### 8.5 GamepadPointerの今後
現状の`GamepadPointer`は複雑なので、別途リファクタリングを検討：
```csharp
// 提案: GamepadPointerをシンプルなInputResolverに
public static class InputDeviceResolver
{
    public static InputDevice GetDeviceFromPointerEvent(PointerEventData eventData)
    {
        // GamepadPointerのロジックを移植
        // テスタブルなstatic メソッドに
    }
}
```

### 8.6 テストの重要性
特に以下のロジックはユニットテストを書くべき：
- ✅ デバイス割り当て（同じデバイスを2人に割り当てない）
- ✅ NPC処理（1Pが2Pのキャラを選べる条件）
- ✅ DataBridgeへのデータ受け渡し（2P情報がnullにならない）

```csharp
[Test]
public void Player2DataShouldNotBeNull_WhenBothPlayersSelectCharacters()
{
    var controller = new SelectionController();
    controller.OnCharacterClicked(device1, character1);
    controller.OnCharacterClicked(device2, character2);
    
    var data = controller.GetSelectionData();
    
    Assert.IsNotNull(data.Player1.Character);
    Assert.IsNotNull(data.Player2.Character); // ★重要★
}
```

---

## 9. 最終推奨事項

### 9.1 推奨アプローチ
**✅ 軽量MVC（Option A）から始める**
- Select画面の複雑さには十分
- 既存コードからの移行が容易
- オーバーエンジニアリングを回避

### 9.2 実装順序
**✅ ストラングラーパターンで段階的に**
1. Phase 1: 新クラス作成（並行稼働）
2. Phase 2: デバイス選択だけ移行
3. Phase 3: キャラ選択だけ移行
4. Phase 4: View層分離
5. Phase 5: Controller完成
6. Phase 6: NPC処理明示化

各Phase終了時に必ず動作確認 → リスク最小化

### 9.3 特に重要なポイント
🔴 **2P情報null問題の根本原因**
```csharp
// ❌ 既存コード（構造体の部分更新）
currentPicks[1].characterData = character; // 他のフィールドが消える

// ✅ 新コード（クラスで完全管理）
data.Player2.Character = character; // 他のフィールドは保持される
```

🔴 **NPC処理の明示化**
```csharp
// ❌ 既存コード（複雑な条件が散在）
if (iconController_2p.GetCurrentDevice() == null && CheckPicked(1)) { /*...*/ }

// ✅ 新コード（意図が明確）
if (CanPlayer1SelectForNpc(device))
{
    var npcCharacter = ConvertToNpcData(character);
    SetCharacterForPlayer(2, npcCharacter);
}
```

### 9.4 次のステップ
このプランに同意いただけたら、Phase 1から実装を開始します。
質問や懸念点があれば、遠慮なくお伝えください。バイス選択が正しく動くか）
// 2-5. 問題なければIconControllerを削除
```

**Phase 2のゴール**: デバイス選択のみ新システム、他は既存のまま

---

### Phase 3: キャラクター選択を新システムに移行
```csharp
// 3-1. CharaButtonをイベント駆動に変更
public class CharaButton
{
    public event Action<InputDevice, CharacterData> OnCharacterClicked;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        var device = ResolveDevice(eventData);
        OnCharacterClicked?.Invoke(device, characterData); // イベント発行のみ
    }
}

// 3-2. SelectUIManagerでイベントを購読
private void Start()
{
    foreach (var button in charaButtons)
    {
        button.OnCharacterClicked += (device, character) =>
        {
            // 既存メソッドを呼ぶ
            SetCharacterPick(GetPlayerIdByDevice(device), character, device);
        };
    }
}

// 3-3. 動作確認（キャラ選択が正しく動くか）
```

**Phase 3のゴール**: デバイス選択+キャラ選択が新システム

---

### Phase 4: View層の分離
```csharp
// 4-1. PlayerViewを作成
public class PlayerView : MonoBehaviour { /*...*/ }

// 4-2. SelectUIManagerのUI更新部分をPlayerViewに委譲
public class SelectUIManager
{
    [SerializeField] private PlayerView player1View;
    
    private void UpdatePlayerDisplay(int playerId)
    {
        var playerData = newData.GetPlayer(playerId);
        
        if (playerId == 1)
            player1View.UpdateDisplay(playerData);
        else
            player2View.UpdateDisplay(playerData);
    }
}

// 4-3. 動作確認（UI更新が正しく動くか）
// 4-4. SelectUIManagerからUI更新コードを削除
```

**Phase 4のゴール**: データ・ロジック・UIが分離

---

### Phase 5: Controller層の完成
```csharp
// 5-1. SelectionControllerを作成
public class SelectionController : MonoBehaviour
{
    [SerializeField] private SelectionData data;
    [SerializeField] private SelectionView view;
    // ... イベントハンドラを実装
}

// 5-2. SelectUIManagerの処理を段階的にSelectionControllerに移動
// 5-3. 動作確認（全機能が新システムで動くか）
// 5-4. SelectUIManagerを削除
```

**Phase 5のゴール**: SelectUIManager完全削除、新システムのみ稼働

---

### Phase 6: NPC処理の明示化
```csharp
// 6-1. NPC処理を専用メソッドに抽出
private void HandleNpcSelection(InputDevice device, CharacterData character)
{
    if (!CanPlayer1SelectForNpc(device)) return;
    
    var npcCharacter = ConvertToNpcData(character);
    SetCharacterForPlayer(2, npcCharacter);
}

// 6-2. 動作確認（NPC選択が正しく動くか）
// 6-3. 必要に応じてNpcSelectionHandlerクラスに分離
```

**Phase 6のゴール**: NPC処理が明示的で理解しやすい

---

### 各Phaseでの検証ポイント
✅ **Phase 1**: `Debug.Log`で新データが正しく更新されているか  
✅ **Phase 2**: マウス/キーボード/ゲームパッドでデバイス選択できるか  
✅ **Phase 3**: 選んだキャラがサムネイルに表示されるか  
✅ **Phase 4**: アニメーション/エフェクトが正しく動くか  
✅ **Phase 5**: ゲームスタートして2P情報がnullでないか ★最重要★  
✅ **Phase 6**: NPCとして選んだキャラがNPC専用データになっているか切に制御

### 7.2 保守性向上
- クラスの責務が明確
- 変更の影響範囲が限定的
- テストが容易

### 7.3 拡張性
- 新しい入力デバイスの追加が容易
- UI変更がロジックに影響しない
- キャラクター追加が簡単

---

## 8. 注意事項・リスク

### 8.1 既存コードへの影響
- Prefabの参照が変わる可能性
- 既存のイベント購読コードの修正が必要

### 8.2 移行期間中のリスク
- 段階的な移行が必要（一度に全て変更しない）
- 各Phase終了時に動作確認を徹底

### 8.3 パフォーマンス
- イベント駆動によるオーバーヘッドは微小
- 必要に応じて最適化を検討
