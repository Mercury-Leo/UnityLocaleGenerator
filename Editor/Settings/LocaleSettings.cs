#nullable enable
using UnityEditor;
using UnityEngine;

namespace LocaleGenerator.Editor
{
    [FilePath("ProjectSettings/LocaleGenerator.asset", FilePathAttribute.Location.ProjectFolder)]
    public class LocaleSettings : ScriptableSingleton<LocaleSettings>
    {
        [SerializeField] private string? targetFolder;
        [SerializeField] private string? prefix;

        private const string DefaultTargetFolder = "Assets";
        private const string DefaultPrefix = "Locale_";
        
        public string TargetFolder
        {
            get => targetFolder ?? DefaultTargetFolder;
            set
            {
                if (targetFolder == value)
                {
                    return;
                }

                targetFolder = value;
                SaveDirty();
            }
        }
        
        public string Prefix
        {
            get => prefix ?? DefaultPrefix;
            set
            {
                prefix = value;
                SaveDirty();
            }
        }

        private void Awake()
        {
            SetDefaultFolder();
        }

        private void SetDefaultFolder()
        {
            if (targetFolder is null)
            {
                TargetFolder = DefaultTargetFolder;
            }

            if (prefix is null)
            {
                Prefix = DefaultPrefix;
            }
        }

        private void SaveDirty()
        {
            Save(this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}