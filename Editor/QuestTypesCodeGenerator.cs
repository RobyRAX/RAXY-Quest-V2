using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RAXY.Quest.Editor
{
    public static class QuestTypesCodeGenerator
    {
        public static bool TryGenerate(out string absolutePath, out string error)
        {
            absolutePath = null;
            error = null;

            var settings = QuestEditorSettings.instance;
            settings.EnsureMainLocked();

            string relativePath = settings.GeneratedScriptPath;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                error = "Generated script path is empty.";
                return false;
            }

            relativePath = relativePath.Replace('\\', '/').Trim();
            if (!relativePath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase)
                && !relativePath.Equals("Assets", StringComparison.OrdinalIgnoreCase))
            {
                error = "Generated script path must be under Assets/.";
                return false;
            }

            if (!relativePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                relativePath += ".cs";

            var entries = BuildConstEntries(settings.QuestTypes, out var warnings);
            foreach (string warning in warnings)
                Debug.LogWarning($"[RAXY Hub / Quest] {warning}");

            if (entries.Count == 0)
            {
                error = "No valid quest type identifiers to generate.";
                return false;
            }

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            absolutePath = Path.GetFullPath(Path.Combine(projectRoot, relativePath));

            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string source = BuildSource(settings.GeneratedNamespace, entries);
            File.WriteAllText(absolutePath, source, Encoding.UTF8);

            settings.GeneratedScriptPath = relativePath;
            settings.SaveSettings();

            AssetDatabase.ImportAsset(relativePath);
            return true;
        }

        static List<(string Identifier, string Value)> BuildConstEntries(
            IReadOnlyList<string> types,
            out List<string> warnings)
        {
            warnings = new List<string>();
            var entries = new List<(string Identifier, string Value)>();
            var usedIdentifiers = new HashSet<string>(StringComparer.Ordinal);

            // Ensure Main is always first.
            AppendEntry(QuestTypeIds.Main, entries, usedIdentifiers, warnings);

            if (types == null)
                return entries;

            foreach (string type in types)
            {
                if (string.IsNullOrWhiteSpace(type))
                {
                    warnings.Add("Skipped empty quest type entry.");
                    continue;
                }

                if (string.Equals(type.Trim(), QuestTypeIds.Main, StringComparison.Ordinal))
                    continue;

                AppendEntry(type.Trim(), entries, usedIdentifiers, warnings);
            }

            return entries;
        }

        static void AppendEntry(
            string value,
            List<(string Identifier, string Value)> entries,
            HashSet<string> usedIdentifiers,
            List<string> warnings)
        {
            string identifier = SanitizeIdentifier(value);
            if (string.IsNullOrEmpty(identifier))
            {
                warnings.Add($"Skipped quest type '{value}' — could not form a valid C# identifier.");
                return;
            }

            if (!usedIdentifiers.Add(identifier))
            {
                warnings.Add($"Skipped duplicate identifier '{identifier}' from quest type '{value}'.");
                return;
            }

            entries.Add((identifier, value));
        }

        internal static string SanitizeIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var sb = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (i == 0)
                {
                    if (char.IsLetter(c) || c == '_')
                        sb.Append(c);
                    else if (char.IsDigit(c))
                    {
                        sb.Append('_');
                        sb.Append(c);
                    }
                    else
                        sb.Append('_');
                }
                else
                {
                    if (char.IsLetterOrDigit(c) || c == '_')
                        sb.Append(c);
                    else
                        sb.Append('_');
                }
            }

            string result = sb.ToString();
            if (string.IsNullOrEmpty(result) || result == "_")
                return null;

            if (IsCSharpKeyword(result))
                result = "@" + result;

            return result;
        }

        static bool IsCSharpKeyword(string identifier)
        {
            switch (identifier)
            {
                case "abstract":
                case "as":
                case "base":
                case "bool":
                case "break":
                case "byte":
                case "case":
                case "catch":
                case "char":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "decimal":
                case "default":
                case "delegate":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "float":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "int":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "long":
                case "namespace":
                case "new":
                case "null":
                case "object":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "ref":
                case "return":
                case "sbyte":
                case "sealed":
                case "short":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "string":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typeof":
                case "uint":
                case "ulong":
                case "unchecked":
                case "unsafe":
                case "ushort":
                case "using":
                case "virtual":
                case "void":
                case "volatile":
                case "while":
                    return true;
                default:
                    return false;
            }
        }

        static string BuildSource(string ns, List<(string Identifier, string Value)> entries)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated by RAXY Quest Project Hub>");
            sb.AppendLine("// Do not edit this file manually. Regenerate from Tools > RAXY > Project Hub > Quest.");
            sb.AppendLine();

            bool hasNamespace = !string.IsNullOrWhiteSpace(ns);
            if (hasNamespace)
            {
                sb.Append("namespace ").Append(ns.Trim()).AppendLine();
                sb.AppendLine("{");
            }

            string indent = hasNamespace ? "    " : string.Empty;
            sb.Append(indent).AppendLine("public static class QuestTypes");
            sb.Append(indent).AppendLine("{");

            foreach (var entry in entries)
            {
                sb.Append(indent).Append("    public const string ")
                    .Append(entry.Identifier)
                    .Append(" = \"")
                    .Append(EscapeString(entry.Value))
                    .AppendLine("\";");
            }

            sb.Append(indent).AppendLine("}");

            if (hasNamespace)
                sb.AppendLine("}");

            return sb.ToString();
        }

        static string EscapeString(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }
    }
}
