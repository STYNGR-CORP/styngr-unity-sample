#if (UNITY_EDITOR)

using Newtonsoft.Json.Linq;
using Packages.StyngrSDK.Runtime.ScriptableObjects;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Packages.StyngrSDK.Editor
{
    /// <summary>
    /// Reads the package version and writes it to a specified ScriptableObject.
    /// </summary> 
    /// <remarks>
    /// IPreprocessBuildWithReport is temporarily commented out until STD-3432 is completed.
    /// Once STD-3432 is completed, this code should be uncommented and development should be continued.
    internal class PreBuildPackageVersionScriptableObjectUpdater //: IPreprocessBuildWithReport
    {
        private const string packageName = "com.styngr.styngr-sdk";
        private const string fileName = "package.json";
        private const string versionKey = "version";
        private static readonly string assetPath = $"Packages/{packageName}/Runtime/Assets/PackageVersion.asset";
        private static readonly string packagePath = $"Packages/{packageName}/{fileName}";

        /// <inheritdoc/>
        public int callbackOrder => 0;

        /// <inheritdoc/>
        public void OnPreprocessBuild(BuildReport report)
        {
            var package = AssetDatabase.LoadAssetAtPath<TextAsset>(packagePath);

            if (package == null)
            {
                Debug.LogError($"{packagePath} does not exist.");
                return;
            }

            var packageText = package.text;

            var packageParsed = JObject.Parse(packageText);

            string versionValue;
            if (packageParsed.ContainsKey(versionKey))
            {
                versionValue = packageParsed[versionKey].ToString();
                Debug.Log($"The {versionKey} for the package name: {packageName} is {versionValue}.");
            }
            else
            {
                versionValue = string.Empty;
                Debug.LogError($"{fileName} for the package ({packageName}) does not contain \"{versionKey}\" parameter");
            }

            var packageVersionScriptableObject = CreateScriptableObject(versionValue);

            Debug.Log($"The scriptabe object is created. \n" +
                $" Package name is \"{packageVersionScriptableObject.package_name}\". " +
                $" Package version is \"{packageVersionScriptableObject.package_version}\".");

            Debug.Log(nameof(PreBuildPackageVersionScriptableObjectUpdater) + ".OnPreprocessBuild for target "
                + report.summary.platform + " at path " + report.summary.outputPath);
        }

        private static PackageVersionScriptableObject CreateScriptableObject(string packageVersion)
        {
            PackageVersionScriptableObject packageVersionScriptableObject = ScriptableObject.CreateInstance<PackageVersionScriptableObject>();

            packageVersionScriptableObject.package_name = packageName;
            packageVersionScriptableObject.package_version = packageVersion;

            AssetDatabase.CreateAsset(packageVersionScriptableObject, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return packageVersionScriptableObject;
        }
    }
}

#endif