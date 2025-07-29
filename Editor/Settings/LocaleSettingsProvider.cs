using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LocaleGenerator.Editor
{
    internal sealed class LocaleSettingsProvider : SettingsProvider
    {
        private LocaleSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) :
            base(path, scopes, keywords) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            var settings = LocaleSettings.instance;

            var title = new Label("Locale Generator")
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 18,
                    marginBottom = 10
                }
            };

            rootElement.Add(title);

            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginTop = 5
                }
            };

            var textField = new TextField("Target Folder")
            {
                value = settings.TargetFolder,
                isReadOnly = true,
                style =
                {
                    flexGrow = 1,
                    marginRight = 5
                },
                tooltip = "The current folder where the generated file will be placed."
            };

            row.Add(textField);

            var browseButton = new Button(() =>
            {
                var selectedFolder =
                    EditorUtility.OpenFolderPanel("Select Target Folder", Application.dataPath, string.Empty);

                if (string.IsNullOrEmpty(selectedFolder))
                {
                    return;
                }

                if (!selectedFolder.StartsWith(Application.dataPath))
                {
                    EditorUtility.DisplayDialog("Invalid Folder",
                        "Please pick a folder inside the project's Assets directory.", "OK");
                    return;
                }

                var result = "Assets" + selectedFolder[Application.dataPath.Length..];
                settings.TargetFolder = result;
                textField.value = result;
            })
            {
                text = "Browse",
                style =
                {
                    width = 80,
                },
                tooltip = "Select the generated file location"
            };

            row.Add(browseButton);

            rootElement.Add(row);
            
            var prefixField = Utility.CreateTextField("Locale Class Prefix", settings.Prefix,
                "The prefix assigned to each Locale class");

            var field = prefixField.Q<TextField>();

            field?.RegisterCallback<FocusOutEvent>(_ =>
            {
                var name = Utility.SanitizeName(field.value, true);

                if (name is null)
                {
                    field.value = settings.Prefix;
                    return;
                }

                settings.Prefix = name;
            });

            rootElement.Add(prefixField);

            var generateButton = new Button(LocaleGenerator.GenerateLocaleClasses)
            {
                text = "Generate",
                style =
                {
                    marginTop = 20,
                    marginLeft = new Length(30, LengthUnit.Percent),
                    marginRight = new Length(30, LengthUnit.Percent),
                },
                tooltip = "Generate Locale Classes"
            };

            rootElement.Add(generateButton);
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new LocaleSettingsProvider("Leo's Tools/Locale Generator", SettingsScope.Project);
        }
    }
}
