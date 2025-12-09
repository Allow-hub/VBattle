using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Allow.EditorTools
{
    /// <summary>
    /// インターフェースを実装した具象クラスのリストを
    /// Unityインスペクター上で操作可能にする抽象エディター基底クラス
    /// </summary>
    /// <typeparam name="TTarget">編集対象のUnityオブジェクトの型</typeparam>
    /// <typeparam name="TInterface">リスト要素が実装すべきインターフェースの型</typeparam>
    public abstract class PolymorphicListEditor<TTarget, TInterface> : Editor
        where TTarget : UnityEngine.Object
    {
        #region プロパティ

        /// <summary>対象リストのSerializedProperty名を取得します。</summary>
        protected abstract string PropertyName { get; }

        /// <summary>インスペクター上に表示するインターフェース名を取得します。</summary>
        protected virtual string InterfaceDisplayName => typeof(TInterface).Name;

        /// <summary>検索対象とするアセンブリ名の配列を取得します。nullの場合は全アセンブリが対象です。</summary>
        protected virtual string[] TargetAssemblies => null;

        /// <summary>型フィルターの条件を判定します。trueを返す型のみ候補になります。</summary>
        /// <param name="type">判定対象の型</param>
        /// <returns>条件を満たす場合はtrue</returns>
        protected virtual bool FilterType(Type type) => true;

        /// <summary>メニュー上に表示する型名を取得</summary>
        /// <param name="type">型</param>
        /// <returns>表示用の名前</returns>
        protected virtual string GetMenuItemName(Type type) => type.Name;

        /// <summary>指定した型のインスタンスを生成</summary>
        /// <param name="type">生成対象の型</param>
        /// <returns>生成したインスタンス</returns>
        protected new virtual object CreateInstance(Type type) => Activator.CreateInstance(type);

        /// <summary>要素が削除されたときに呼ばれる</summary>
        /// <param name="index">削除された要素のインデックス</param>
        protected virtual void OnElementRemoved(int index) { }

        /// <summary>要素が追加されたときに呼ばれます。</summary>
        /// <param name="instance">追加されたインスタンス</param>
        /// <param name="index">追加された要素のインデックス</param>
        protected virtual void OnElementAdded(object instance, int index) { }

        #endregion

        #region  フィールド 

        private SerializedProperty listProperty;
        private static readonly Dictionary<Type, List<Type>> typeCache = new();
        private bool isDeletingElement = false;

        #endregion

        #region Unityのコールバック

        /// <summary>エディターが有効になったときに呼ばれます。型キャッシュを初期化</summary>
        protected virtual void OnEnable()
        {
            listProperty = serializedObject.FindProperty(PropertyName);
            CacheConcreteTypes();
        }

        /// <summary>インスペクターGUIを描画</summary>
        public override void OnInspectorGUI()
        {
            if (listProperty == null)
            {
                EditorGUILayout.HelpBox($"Property '{PropertyName}' not found.", MessageType.Error);
                return;
            }

            serializedObject.Update();

            if (GUILayout.Button("🔄 Refresh Types"))
            {
                RebuildTypeCache();
            }

            DrawHeader();
            DrawElements();
            DrawAddButton();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region  型の探索のキャッシュ

        /// <summary>型キャッシュを強制的に再構築</summary>
        protected void RebuildTypeCache() => CacheConcreteTypes(true);

        /// <summary>型キャッシュを構築または更新</summary>
        /// <param name="force">強制的に再構築するかどうか</param>
        private void CacheConcreteTypes(bool force = false)
        {
            var interfaceType = typeof(TInterface);
            if (force || !typeCache.ContainsKey(interfaceType))
                typeCache[interfaceType] = GetConcreteTypes();
        }

        /// <summary>有効な具象型のリストを取得</summary>
        /// <returns>具象型のリスト</returns>
        private List<Type> GetConcreteTypes()
        {
            var assemblies = TargetAssemblies != null
                ? AppDomain.CurrentDomain.GetAssemblies().Where(asm => TargetAssemblies.Contains(asm.GetName().Name))
                : AppDomain.CurrentDomain.GetAssemblies();

            return assemblies
                .SelectMany(GetTypesFromAssembly)
                .Where(IsValidType)
                .Where(FilterType)
                .OrderBy(t => t.Name)
                .ToList();
        }

        /// <summary>指定されたアセンブリから型の配列を取得</summary>
        /// <param name="assembly">対象アセンブリ</param>
        /// <returns>型配列</returns>
        private Type[] GetTypesFromAssembly(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch { return new Type[0]; }
        }

        /// <summary>指定された型が有効な具象型かどうか判定</summary>
        /// <param name="type">判定対象の型</param>
        /// <returns>有効ならtrue</returns>
        private bool IsValidType(Type type)
        {
            try
            {
                return typeof(TInterface).IsAssignableFrom(type)
                    && !type.IsAbstract
                    && !type.IsInterface
                    && (type.IsClass || type.IsValueType)
                    && HasValidConstructor(type);
            }
            catch { return false; }
        }

        /// <summary>型にパラメータなしコンストラクタがあるか、値型かどうかを判定</summary>
        /// <param name="type">判定対象の型</param>
        /// <returns>条件を満たす場合はtrue</returns>
        private bool HasValidConstructor(Type type) =>
            type.GetConstructor(Type.EmptyTypes) != null || type.IsValueType;

        #endregion

        #region  ヘッダー / 追加ボタン

        /// <summary>リストのヘッダーを描画</summary>
        private new void DrawHeader()
        {
            EditorGUILayout.LabelField($"{InterfaceDisplayName} List", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Count: {listProperty.arraySize}", EditorStyles.miniLabel);
            EditorGUILayout.Space();
        }

        /// <summary>要素追加ボタンを描画</summary>
        private void DrawAddButton()
        {
            var interfaceType = typeof(TInterface);
            var concreteTypes = typeCache.TryGetValue(interfaceType, out var types) ? types : new List<Type>();

            EditorGUI.BeginDisabledGroup(concreteTypes.Count == 0);
            if (GUILayout.Button("+ Add Element"))
            {
                ShowAddMenu(concreteTypes);
            }
            EditorGUI.EndDisabledGroup();

            if (concreteTypes.Count == 0)
            {
                EditorGUILayout.HelpBox($"No concrete implementations of {InterfaceDisplayName} found.", MessageType.Info);
            }
        }

        /// <summary>追加メニューを表示</summary>
        /// <param name="concreteTypes">追加可能な型のリスト</param>
        private void ShowAddMenu(List<Type> concreteTypes)
        {
            var menu = new GenericMenu();

            if (concreteTypes.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No types available"));
            }
            else
            {
                AddMenuItems(menu, concreteTypes);
            }

            menu.ShowAsContext();
        }

        /// <summary>追加メニューに項目を追加</summary>
        /// <param name="menu">メニュー</param>
        /// <param name="concreteTypes">型リスト</param>
        private void AddMenuItems(GenericMenu menu, List<Type> concreteTypes)
        {
            var groupedTypes = GroupTypesByNamespace(concreteTypes);
            var hasMultipleNamespaces = groupedTypes.Count() > 1;

            foreach (var group in groupedTypes)
            {
                foreach (var type in group.OrderBy(t => t.Name))
                {
                    var menuPath = hasMultipleNamespaces
                        ? $"{group.Key}/{GetMenuItemName(type)}"
                        : GetMenuItemName(type);

                    menu.AddItem(new GUIContent(menuPath), false, () => AddElement(type));
                }
            }
        }

        /// <summary>型リストを名前空間でグループ化してソート</summary>
        /// <param name="types">型リスト</param>
        /// <returns>グループ化された型コレクション</returns>
        private IOrderedEnumerable<IGrouping<string, Type>> GroupTypesByNamespace(List<Type> types) =>
            types.GroupBy(t => string.IsNullOrEmpty(t.Namespace) ? "Global" : t.Namespace)
                 .OrderBy(g => g.Key);

        /// <summary>指定された型の新しい要素を追加</summary>
        /// <param name="type">追加する型</param>
        private void AddElement(Type type)
        {
            try
            {
                var instance = CreateInstance(type);
                var newIndex = AddNewElement(instance);
                OnElementAdded(instance, newIndex);
                serializedObject.ApplyModifiedProperties();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create instance of {type.Name}: {ex.Message}");
            }
        }

        /// <summary>新しい要素をリストに追加し、インデックスを返す</summary>
        /// <param name="instance">追加するインスタンス</param>
        /// <returns>追加した要素のインデックス</returns>
        private int AddNewElement(object instance)
        {
            Undo.RecordObject(target, "Add Element");

            listProperty.arraySize++;
            var newIndex = listProperty.arraySize - 1;
            var element = listProperty.GetArrayElementAtIndex(newIndex);
            element.managedReferenceValue = instance;

            EditorUtility.SetDirty(target);
            return newIndex;
        }

        #endregion

        #region  UI:要素の描画 

        /// <summary>全要素を描画</summary>
        private void DrawElements()
        {
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                DrawElement(i);

                if (isDeletingElement)
                {
                    isDeletingElement = false; // 次回ループへ影響しないようリセット
                    return; // 要素削除直後に描画中断（破棄済みに触れない）
                }
            }
        }

        /// <summary>指定インデックスの要素を描画</summary>
        /// <param name="index">要素インデックス</param>
        /// <returns>削除処理で描画中断した場合はtrue</returns>
        private bool DrawElement(int index)
        {
            var element = listProperty.GetArrayElementAtIndex(index);

            EditorGUILayout.BeginVertical("box");
            DrawElementHeader(index, element);

            if (isDeletingElement)
            {
                EditorGUILayout.EndVertical();
                return true;
            }

            DrawElementContent(element);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
            return false;
        }

        /// <summary>指定インデックスの要素のヘッダーを描画</summary>
        /// <param name="index">要素インデックス</param>
        /// <param name="element">描画対象のSerializedProperty</param>
        private void DrawElementHeader(int index, SerializedProperty element)
        {
            EditorGUILayout.BeginHorizontal();

            var typeName = GetElementTypeName(element);
            EditorGUILayout.LabelField($"[{index}] {typeName}", EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();
            DrawMoveButtons(index);
            DrawDeleteButton(index);

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>指定された要素の内容を描画</summary>
        /// <param name="element">描画対象のSerializedProperty</param>
        private void DrawElementContent(SerializedProperty element)
        {
            if (element.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(element, GUIContent.none, true);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("Null reference.\nThis may happen when the type has been renamed or deleted.", MessageType.Warning);
            }
        }

        /// <summary>指定された要素の型名を取得</summary>
        /// <param name="element">対象のSerializedProperty</param>
        /// <returns>型名（nullの場合は"Unknown"）</returns>
        private string GetElementTypeName(SerializedProperty element) =>
            element.managedReferenceValue?.GetType().Name ?? "Unknown";

        /// <summary>指定されたインデックスの要素を上に移動するボタンを描画</summary>
        /// <param name="index">要素インデックス</param>
        private void DrawMoveButtons(int index)
        {
            if (index > 0 && GUILayout.Button("↑", GUILayout.Width(25)))
            {
                listProperty.MoveArrayElement(index, index - 1);
            }
            if (index < listProperty.arraySize - 1 && GUILayout.Button("↓", GUILayout.Width(25)))
            {
                listProperty.MoveArrayElement(index, index + 1);
            }
        }

        /// <summary>指定されたインデックスの要素を削除するボタンを描画</summary>
        /// <param name="index">要素インデックス</param>
        private void DrawDeleteButton(int index)
        {
            if (GUILayout.Button("×", GUILayout.Width(25)))
            {
                Undo.RecordObject(target, "Remove Element");

                var element = listProperty.GetArrayElementAtIndex(index);

                if (element.propertyType == SerializedPropertyType.ManagedReference && element.managedReferenceValue != null)
                {
                    element.managedReferenceValue = null;
                }

                listProperty.DeleteArrayElementAtIndex(index);
                OnElementRemoved(index);
                EditorUtility.SetDirty(target);

                isDeletingElement = true;
            }
        }

        #endregion
    }
}