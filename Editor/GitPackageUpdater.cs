#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GitPackageUpdater.Editor
{
    public static class GitPackageUpdater
    {
        [MenuItem("Tools/Update all git packages")]
        public static void UpdateAllGitPackages()
        {
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath))
            {
                Debug.LogError($"manifest.json not found at: {manifestPath}");
                return;
            }

            // Read all lines from manifest.json
            var lines = File.ReadAllLines(manifestPath);
            var inDependenciesBlock = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // Detect the start of the "dependencies" block
                if (line.Contains("\"dependencies\":"))
                {
                    inDependenciesBlock = true;
                }
                else switch (inDependenciesBlock)
                {
                    // Detect the end of the "dependencies" block
                    case true when line.TrimStart().StartsWith("}"):
                        inDependenciesBlock = false;
                        break;
                    case true:
                    {
                        // Example line:  "com.mycompany.somepackage": "https://git-something.git",
                        // We'll parse out the packageName (key) and URL (value).
                        var trimmedLine = line.Trim();

                        // Basic sanity check that we have something like `"key": "value",`
                        if (!trimmedLine.StartsWith("\""))
                            continue;

                        var colonIndex = trimmedLine.IndexOf(':');
                        if (colonIndex >= 0)
                        {
                            // Extract the "com.mycompany" portion
                            var rawKey =
                                trimmedLine.Substring(0, colonIndex).Trim(); // e.g. `"com.mycompany.somepackage"`
                            // Then remove leading/trailing quotes
                            var packageName = rawKey.Trim('"');

                            // Extract the URL portion (the value)
                            var rawValue =
                                trimmedLine.Substring(colonIndex + 1).Trim(); // e.g. `"https://git-something.git",`
                            // Remove trailing comma if present
                            if (rawValue.EndsWith(","))
                                rawValue = rawValue.Substring(0, rawValue.Length - 1).Trim();

                            // Remove surrounding quotes if present
                            if (rawValue.StartsWith("\"") && rawValue.EndsWith("\""))
                                rawValue = rawValue.Substring(1, rawValue.Length - 2);

                            // Now let's decide if we need to modify it:
                            // 1. Skip any com.unity.* packages
                            // 2. If it’s a .git URL, append ?<random> to force an update
                            if (!packageName.StartsWith("com.unity.", StringComparison.OrdinalIgnoreCase) &&
                                rawValue.Contains(".git", StringComparison.OrdinalIgnoreCase))
                            {
                                // Remove any old ? query
                                var questionIndex = rawValue.IndexOf('?', StringComparison.OrdinalIgnoreCase);
                                if (questionIndex >= 0)
                                {
                                    rawValue = rawValue.Substring(0, questionIndex);
                                }

                                // Append new random query param
                                var forcedUpdateUrl = rawValue + "?" + Guid.NewGuid().ToString("N");

                                // Rebuild the line with the changed URL
                                // Note the indentation is optional. This just matches typical Unity formatting.
                                line = $"    \"{packageName}\": \"{forcedUpdateUrl}\",";
                            }
                        }

                        break;
                    }
                }

                // Update our lines array
                lines[i] = line;
            }

            // Write back out to manifest.json
            File.WriteAllLines(manifestPath, lines);

            // Force Unity to detect changes
            AssetDatabase.Refresh();

            Debug.Log("Git packages have been updated in manifest.json; please wait for the project to be updated!");
        }
    }
}
#endif