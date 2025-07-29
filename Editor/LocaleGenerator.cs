#nullable enable
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace LocaleGenerator.Editor
{
    public static class LocaleGenerator
    {
        private const string TableGuidName = "TableGuid";
        private const string LocaleClassesName = "LocaleClasses.g.cs";
        private static readonly LocaleSettings Settings = LocaleSettings.instance;

        [InitializeOnLoadMethod]
        private static void InitializeEditorEvents()
        {
            LocalizationEditorSettings.EditorEvents.TableEntryAdded += OnTableModified;
            LocalizationEditorSettings.EditorEvents.TableEntryRemoved += OnTableModified;
        }

        [MenuItem("Tools/Leo's Tools/Generate Locale")]
        public static void GenerateLocaleClasses()
        {
            var output = Path.Combine(Settings.TargetFolder, LocaleClassesName);
            GenerateLocaleClasses(output);
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
            builder.WriteLine("using System.Collections.Generic;");
            builder.WriteLine("using System.Linq;");
        }

        private static void BuildClass(IndentedTextWriter builder, string tableName, Guid tableGuid,
            IEnumerable<SharedTableData.SharedTableEntry> entries)
        {
            var className = Utility.SanitizeName(tableName);
            if (className is null)
            {
                return;
            }

            builder.WriteLine($"public static class {Settings.Prefix}{className}");
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
            var keyName = Utility.SanitizeName(entry.Key, false, true);
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

            // Clone extension
            builder.WriteLine("[Pure]");
            builder.WriteLine("public static LocalizedString Clone(this LocalizedString localeString)");
            builder.WriteLine("{");
            builder.Indent++;
            builder.WriteLine(
                "return new LocalizedString(localeString.TableReference, localeString.TableEntryReference);");
            builder.Indent--;
            builder.WriteLine("}");

            // With Arguments
            builder.WriteLine("[Pure]");
            builder.WriteLine(
                "public static LocalizedString WithArguments(this LocalizedString text, params string[] args)");
            builder.WriteLine("{");
            builder.Indent++;
            builder.WriteLine("var textWithArgs = text.Clone();");
            builder.WriteLine("textWithArgs.Arguments = args;");
            builder.WriteLine("return textWithArgs;");
            builder.Indent--;
            builder.WriteLine("}");

            // With Arguments
            builder.WriteLine("[Pure]");
            builder.WriteLine(
                "public static LocalizedString WithArguments(this LocalizedString text, IDictionary<string, string> args)");
            builder.WriteLine("{");
            builder.Indent++;
            builder.WriteLine("var textWithArgs = text.Clone();");
            builder.WriteLine("textWithArgs.Arguments = new object[] { args };");
            builder.WriteLine("return textWithArgs;");
            builder.Indent--;
            builder.WriteLine("}");

            // With Arguments
            builder.WriteLine("[Pure]");
            builder.WriteLine(
                "public static LocalizedString WithArguments(this LocalizedString text, params (string Key, string Value)[] args)");
            builder.WriteLine("{");
            builder.Indent++;
            builder.WriteLine("var dict = args.ToDictionary(pair => pair.Key, pair => pair.Value);");
            builder.WriteLine("return text.WithArguments(dict);");
            builder.Indent--;
            builder.WriteLine("}");

            builder.Indent--;
            builder.WriteLine("}");
        }
    }
}
