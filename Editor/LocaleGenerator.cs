using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LocaleGenerator.UnityLocaleGenerator.Editor.Settings;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

#nullable enable
namespace LocaleGenerator.UnityLocaleGenerator.Editor
{
    public static class LocaleGenerator
    {
        private const string ClassPrefix = "Locale";
        private const string TableGuidName = "TableGuid";
        private const string LocaleClassesName = "LocaleClasses.cs";

        [InitializeOnLoadMethod]
        private static void InitializeEditorEvents()
        {
            LocalizationEditorSettings.EditorEvents.TableEntryAdded += OnTableModified;
            LocalizationEditorSettings.EditorEvents.TableEntryRemoved += OnTableModified;

            LocaleSettings.instance.TargetChanged += OnTargetChanged;
        }

        [MenuItem("Tools/LocaleGenerator/Generate")]
        public static void GenerateLocaleClasses()
        {
            var output = Path.Combine(LocaleSettings.instance.TargetFolder, LocaleClassesName);
            GenerateLocaleClasses(output);
        }

        private static void OnTargetChanged(string oldPath, string newPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(oldPath))
                {
                    var oldFile = Path.Combine(oldPath, LocaleClassesName);
                    if (File.Exists(oldFile))
                    {
                        AssetDatabase.DeleteAsset(oldFile);
                    }
                }

                GenerateLocaleClasses(Path.Combine(newPath, LocaleClassesName));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to regenerate locale classes: {e}");
            }
        }

        private static void OnTableModified(LocalizationTableCollection table, SharedTableData.SharedTableEntry entry)
        {
            GenerateLocaleClasses();
        }

        public static void GenerateLocaleClasses(string outputPath)
        {
            var collections = LocalizationEditorSettings.GetStringTableCollections();
            var writer = new StringWriter();
            var builder = new IndentedTextWriter(writer);

            BuildUsing(builder);
            builder.WriteLine();

            foreach (var collection in collections)
            {
                BuildClass(builder, collection.TableCollectionName,
                    collection.TableCollectionNameReference.TableCollectionNameGuid, collection.SharedData.Entries);
            }

            builder.WriteLine();

            BuildExtensions(builder);

            try
            {
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(outputPath, writer.ToString(), Encoding.UTF8);
                AssetDatabase.ImportAsset(outputPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to generate localized strings: {e}");
            }
        }

        private static void BuildUsing(IndentedTextWriter builder)
        {
            builder.WriteLine("//Auto-Generated file. Do not modify this file!");
            builder.WriteLine("using System;");
            builder.WriteLine("using UnityEngine.Localization;");
            builder.WriteLine("using System.Diagnostics.Contracts;");
        }

        private static void BuildClass(IndentedTextWriter builder, string tableName, Guid tableGuid,
            IEnumerable<SharedTableData.SharedTableEntry> entries)
        {
            var className = SanitizeName(tableName);
            if (className is null)
            {
                return;
            }

            builder.WriteLine($"public static class {ClassPrefix}{className}");
            builder.WriteLine("{");

            builder.Indent++;

            builder.WriteLine($"private static readonly Guid {TableGuidName} = new(\"{tableGuid}\");");

            builder.WriteLine();

            foreach (var entry in entries.OrderBy(entry => entry.Key))
            {
                BuildProperty(builder, entry);
            }

            builder.Indent--;
            builder.WriteLine("}");
        }

        private static void BuildProperty(IndentedTextWriter builder, SharedTableData.SharedTableEntry entry)
        {
            var keyName = SanitizeName(entry.Key);
            if (keyName is null)
            {
                return;
            }

            var localizedString = $"new {nameof(LocalizedString)}({TableGuidName}, {entry.Id})";

            builder.WriteLine(
                $"private static readonly Lazy<{nameof(LocalizedString)}> _{keyName.ToLower()} = new( () => {localizedString});");
            builder.WriteLine(
                $"public static {nameof(LocalizedString)} {keyName} => _{keyName.ToLower()}.Value;");
        }

        private static void BuildExtensions(IndentedTextWriter builder)
        {
            builder.WriteLine("public static class LocalizationExtensions");
            builder.WriteLine("{");
            builder.Indent++;
            builder.WriteLine("[Pure]");
            builder.WriteLine("public static LocalizedString Clone(this LocalizedString localeString)");
            builder.WriteLine("{");
            builder.Indent++;
            builder.WriteLine(
                "return new LocalizedString(localeString.TableReference, localeString.TableEntryReference);");
            builder.Indent--;
            builder.WriteLine("}");
            builder.Indent--;
            builder.WriteLine("}");
        }

        private static string? SanitizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (var c in name)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    builder.Append(c);
                }
                else
                {
                    builder.Append(string.Empty);
                }
            }

            if (char.IsDigit(builder[0]))
            {
                builder.Insert(0, '_');
            }

            return builder.ToString();
        }
    }
}