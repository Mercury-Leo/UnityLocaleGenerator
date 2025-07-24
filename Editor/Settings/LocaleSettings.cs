using System;
using UnityEditor;
using UnityEngine;

#nullable enable
namespace LocaleGenerator.UnityLocaleGenerator.Editor.Settings
{
    [FilePath("ProjectSettings/LocaleGenerator.asset", FilePathAttribute.Location.ProjectFolder)]
    public class LocaleSettings : ScriptableSingleton<LocaleSettings>
    {
        [SerializeField] private string? targetFolder;

        private const string DefaultTargetFolder = "Assets";

        public event Action<string, string>? TargetChanged; 

        public string TargetFolder
        {
            get => targetFolder ?? DefaultTargetFolder;
            set
            {
                if (targetFolder == value)
                {
                    return;
                }
                
                TargetChanged?.Invoke(TargetFolder, value);
                targetFolder = value;
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
        }

        private void SaveDirty()
        {
            Save(this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}