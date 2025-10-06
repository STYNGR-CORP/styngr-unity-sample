#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Packages.StyngrSDK.Editor.PackageManager
{
    public class PackageUpdater : EditorWindow
    {
        private static readonly Queue<string> packageQueue = new Queue<string>();
        private static AddRequest addRequest;

        [MenuItem("StyngrSDK/Restore Packages")]
        public static void UpdateAllPackages()
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");

            if (!File.Exists(manifestPath))
            {
                Debug.LogError("manifest.json not found!");
                return;
            }

            // Read manifest.json
            string jsonText = File.ReadAllText(manifestPath);
            JObject manifest = JObject.Parse(jsonText);

            if (manifest["dependencies"] is JObject dependencies)
            {
                foreach (var package in dependencies)
                {
                    packageQueue.Enqueue($"{package.Key}@{package.Value}"); // Add package to queue
                }
            }

            InstallNextPackage();
        }

        private static void InstallNextPackage()
        {
            if (packageQueue.Count == 0)
            {
                Debug.Log("All packages installed/updated.");
                return;
            }

            string packageName = packageQueue.Dequeue();
            Debug.Log($"Installing/Updating: {packageName}");

            addRequest = Client.Add(packageName);
            EditorApplication.update += MonitorAddRequest;
        }

        private static void MonitorAddRequest()
        {
            if (addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"Installed/Updated: {addRequest.Result.name}");
                }
                else
                {
                    Debug.LogError($"Failed to install {addRequest.Result.name}: {addRequest.Error.message}");
                }

                EditorApplication.update -= MonitorAddRequest;
                InstallNextPackage(); // Continue with the next package
            }
        }
    }
}
#endif
