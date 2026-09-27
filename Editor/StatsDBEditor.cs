using System;
using System.Collections.Generic;
using System.Linq;
using fefek5.Stats.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace fefek5.Stats.Editor
{
    /// <summary>
    /// The inspector of the stats DB: the list of its stats, adding and removing them as
    /// sub-assets, and the inspector of the selected one under it. In play mode the list shows the
    /// current values.
    /// </summary>
    [CustomEditor(typeof(StatsDB))]
    public class StatsDBEditor : UnityEditor.Editor
    {
        private const string StatsField = "_stats";
        private const string RelativePathField = "_relativePath";

        private readonly List<Stat> _stats = new();

        private ListView _list;
        private HelpBox _duplicates;
        private TextField _name;
        private VisualElement _detail;
        private Stat _selected;

        [MenuItem("Window/Stats/Stats DB")]
        public static void Open() => EditorUtility.OpenPropertyEditor(StatsDB.Instance);

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(RelativePathField)));

            _duplicates = new HelpBox(string.Empty, HelpBoxMessageType.Info);
            root.Add(_duplicates);

            _list = new ListView(_stats, 20, MakeRow, BindRow)
            {
                selectionType = SelectionType.Single,
                showBorder = true,
                style = { marginTop = 6, maxHeight = 20 * 16 }
            };
            _list.selectionChanged += selection => Select(selection.OfType<Stat>().FirstOrDefault());
            root.Add(_list);

            var buttons = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd } };
            buttons.Add(new Button(ShowAddMenu) { text = "Add Stat" });
            buttons.Add(new Button(() => { if (_selected) RemoveStat(_selected); }) { text = "Remove" });
            root.Add(buttons);

            _name = new TextField("Name") { isDelayed = true, style = { marginTop = 10 } };
            _name.RegisterValueChangedCallback(evt => Rename(_selected, evt.newValue));
            root.Add(_name);

            _detail = new VisualElement();
            root.Add(_detail);

            // Values live outside the serialized data, so nothing else tells the list they moved.
            root.schedule.Execute(() => { if (EditorApplication.isPlaying) _list.RefreshItems(); }).Every(200);

            root.RegisterCallback<AttachToPanelEvent>(_ => Undo.undoRedoPerformed += Refresh);
            root.RegisterCallback<DetachFromPanelEvent>(_ => Undo.undoRedoPerformed -= Refresh);

            Refresh();
            Select(null);

            return root;
        }

        #region Rows

        private static VisualElement MakeRow()
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, paddingLeft = 4, paddingRight = 4 } };

            row.Add(new Label { name = "name", style = { flexGrow = 1 } });
            row.Add(new Label { name = "type", style = { width = 90, color = Color.gray } });
            row.Add(new Label { name = "value", style = { width = 90, unityTextAlign = TextAnchor.MiddleRight } });

            return row;
        }

        private void BindRow(VisualElement row, int index)
        {
            var stat = _stats[index];

            row.Q<Label>("name").text = stat ? stat.name : "(missing)";
            row.Q<Label>("type").text = stat ? ObjectNames.NicifyVariableName(stat.GetType().Name) : string.Empty;
            row.Q<Label>("value").text = stat && EditorApplication.isPlaying ? stat.ToString() : string.Empty;
        }

        #endregion

        private void Refresh()
        {
            if (!target)
                return;

            serializedObject.Update();

            _stats.Clear();
            _stats.AddRange(((StatsDB)target).Stats);
            _list.RefreshItems();

            var duplicates = _stats.Where(stat => stat)
                .GroupBy(stat => stat.name)
                .Where(group => group.Count() > 1)
                .Select(group => string.Join(", ", group.Select(stat => stat.name)))
                .ToList();

            _duplicates.text = "These stats share a name, which is the key they are saved under, so only one of them keeps its value after a save: " +
                               string.Join("; ", duplicates) + ".";
            _duplicates.style.display = duplicates.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            if (_selected && !_stats.Contains(_selected))
                Select(null);
        }

        private void Select(Stat stat)
        {
            _selected = stat;
            _detail.Clear();

            _name.style.display = stat ? DisplayStyle.Flex : DisplayStyle.None;
            _name.SetValueWithoutNotify(stat ? stat.name : string.Empty);

            if (stat)
                _detail.Add(new InspectorElement(stat));
        }

        #region Assets

        private void ShowAddMenu()
        {
            var menu = new GenericMenu();

            foreach (var type in TypeCache.GetTypesDerivedFrom<Stat>()
                         .Where(type => !type.IsAbstract && !type.IsGenericType)
                         .OrderBy(type => type.Name))
            {
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () => AddStat(type));
            }

            menu.ShowAsContext();
        }

        private void AddStat(Type type)
        {
            var stat = (Stat)CreateInstance(type);

            stat.name = $"New {ObjectNames.NicifyVariableName(type.Name)}";

            AssetDatabase.AddObjectToAsset(stat, target);
            Undo.RegisterCreatedObjectUndo(stat, "Add Stat");

            serializedObject.Update();

            var stats = serializedObject.FindProperty(StatsField);

            stats.arraySize++;
            stats.GetArrayElementAtIndex(stats.arraySize - 1).objectReferenceValue = stat;

            serializedObject.ApplyModifiedProperties();
            Save(target);

            Refresh();
            _list.SetSelection(_stats.IndexOf(stat));
        }

        private void RemoveStat(Stat stat)
        {
            var statName = stat.name;

            if (!EditorUtility.DisplayDialog("Remove Stat",
                    $"Remove '{statName}' from the stats DB?\n\nFields that reference it lose it. Undo brings it back.",
                    "Remove", "Cancel"))
                return;

            Undo.SetCurrentGroupName($"Remove Stat {statName}");

            var group = Undo.GetCurrentGroup();

            serializedObject.Update();

            var stats = serializedObject.FindProperty(StatsField);

            for (var i = stats.arraySize - 1; i >= 0; i--)
            {
                if (stats.GetArrayElementAtIndex(i).objectReferenceValue != stat)
                    continue;

                // Cleared first: deleting an element that still references an object has only
                // cleared it in some versions of Unity.
                stats.GetArrayElementAtIndex(i).objectReferenceValue = null;
                stats.DeleteArrayElementAtIndex(i);
            }

            serializedObject.ApplyModifiedProperties();
            Undo.DestroyObjectImmediate(stat);
            Undo.CollapseUndoOperations(group);
            Save(target);

            _list.ClearSelection();
            Refresh();
        }

        private void Rename(Stat stat, string newName)
        {
            if (!stat || string.IsNullOrWhiteSpace(newName) || stat.name == newName)
                return;

            Undo.RecordObject(stat, "Rename Stat");
            stat.name = newName;

            EditorUtility.SetDirty(stat);
            Save(stat);

            Refresh();
        }

        /// <summary>
        /// Writes the asset and imports it again. The Project window lists sub-assets as of the last
        /// import, so without it a change shows only once the project is saved.
        /// </summary>
        private static void Save(Object asset)
        {
            AssetDatabase.SaveAssetIfDirty(asset);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
        }

        #endregion
    }
}
