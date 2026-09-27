using fefek5.Stats.Runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace fefek5.Stats.Editor
{
    /// <summary>
    /// The inspector of every stat: its name first, since that is the key it is saved under, and the
    /// default inspector under it.
    /// </summary>
    [CustomEditor(typeof(Stat), true)]
    public class StatEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var stat = (Stat)target;

            var nameField = new TextField("Name") { isDelayed = true, value = stat.name };
            // Lines the label up with the labels of the default inspector below.
            nameField.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            nameField.RegisterValueChangedCallback(evt => Rename(stat, evt.newValue, nameField));
            // Catches an undone rename and one made in the Project window.
            nameField.TrackSerializedObjectValue(serializedObject, _ => nameField.SetValueWithoutNotify(stat.name));
            root.Add(nameField);

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            return root;
        }

        private static void Rename(Stat stat, string newName, TextField nameField)
        {
            if (!stat || string.IsNullOrWhiteSpace(newName) || stat.name == newName)
            {
                nameField.SetValueWithoutNotify(stat ? stat.name : string.Empty);

                return;
            }

            var path = AssetDatabase.GetAssetPath(stat);

            // A stat of its own is named after its file, which only a rename of the file changes.
            if (AssetDatabase.IsMainAsset(stat))
            {
                var error = AssetDatabase.RenameAsset(path, newName);

                if (!string.IsNullOrEmpty(error))
                    UnityEngine.Debug.LogWarning($"Could not rename stat '{stat.name}': {error}", stat);

                nameField.SetValueWithoutNotify(stat.name);

                return;
            }

            Undo.RecordObject(stat, "Rename Stat");
            stat.name = newName;

            EditorUtility.SetDirty(stat);

            // The Project window lists sub-assets as of the last import, so without it the new name
            // shows only once the project is saved.
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.SaveAssetIfDirty(stat);
                AssetDatabase.ImportAsset(path);
            }
        }
    }
}
