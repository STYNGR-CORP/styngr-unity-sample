using UnityEngine;

namespace Packages.StyngrSDK.Runtime.ScriptableObjects
{
    /// <summary>
    /// Stores the package name and version. 
    /// Package name and version are updated using PreBuildPackageVersionScriptableObjectUpdater.cs
    /// Stored data should only be read at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "PackageVersion.asset", menuName = "ScriptableObjects/PackageVersion.asset")]
    public class PackageVersionScriptableObject : ScriptableObject
    {
        /// <summary>
        /// Stores the package name.
        /// </summary>
        public string package_name;

        /// <summary>
        /// Stores the package version.
        /// </summary>
        public string package_version;
    }
}