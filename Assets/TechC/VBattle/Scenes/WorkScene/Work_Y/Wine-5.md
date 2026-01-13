# Wine-5の残りやるべきこと

- コメント形式の統一
- 子階層のRendererが時々切れる
ここの部分では子階層に反映されないからここを修正する
``` cs
private void OnEnable()
        {
            // Pool から取得時に状態をリセット
            var renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = true;
            
            var boxCollider = GetComponent<BoxCollider>();
            if (boxC
            ollider != null) boxCollider.enabled = true;
        }
```
