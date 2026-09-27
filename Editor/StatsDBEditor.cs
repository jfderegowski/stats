using System.Linq;
using fefek5.Stats.Runtime;
using fefek5.Toys.Editor.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace fefek5.Stats.Editor
{
    /// <summary>
    /// The inspector of the stats DB: its stats as a <see cref="SubAssetListElement"/>, each a foldout with the
    /// inspector of the stat (<see cref="StatEditor"/>) inside. In play mode the headers show the current values.
    /// </summary>
    [CustomEditor(typeof(StatsDB))]
    public class StatsDBEditor : UnityEditor.Editor
    {
        private const string StatsField = "_stats";
        private const string RelativePathField = "_relativePath";

        [MenuItem("Window/Stats/Stats DB")]
        public static void Open() => EditorUtility.OpenPropertyEditor(StatsDB.Instance);

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            root.Add(new PropertyField(serializedObject.FindProperty(RelativePathField)));

            var duplicates = new HelpBox(string.Empty, HelpBoxMessageType.Info);
            root.Add(duplicates);

            var list = new SubAssetListElement(serializedObject.FindProperty(StatsField), typeof(Stat)) {
                ItemInfo = stat => EditorApplication.isPlaying ? stat.ToString() : string.Empty,
                style = { marginTop = 6 }
            };
            list.Reloaded += () => ShowDuplicates(duplicates, list);
            root.Add(list);

            ShowDuplicates(duplicates, list);

            return root;
        }

        private static void ShowDuplicates(HelpBox helpBox, SubAssetListElement list)
        {
            var duplicates = list.Items.Where(stat => stat)
                .GroupBy(stat => stat.name)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            helpBox.text =
                "These stats share a name, which is the key they are saved under, so only one of them keeps its value after a save: " +
                string.Join(", ", duplicates) + ".";
            helpBox.style.display = duplicates.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
