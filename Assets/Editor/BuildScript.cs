using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.Build.Reporting;
using System.Linq;
using System.IO;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Build/Build Android Addressables")]
    public static void BuildAndroidAddressables()
    {
        BuildAddressables();
    }

    [MenuItem("Build/Build Android Player")]
    public static void BuildAndroidPlayer()
    {
        UpdateVersion();
        BuildPlayer(BuildTarget.Android, "Builds/Android/MyGame.apk");
    }

    [MenuItem("Build/Build Android Bundle")]
    public static void BuildAndroidBundle()
    {
        UpdateVersion();
        EditorUserBuildSettings.buildAppBundle = true;
        BuildPlayer(BuildTarget.Android, "Builds/Android/MyGame.aab");
        EditorUserBuildSettings.buildAppBundle = false; // Reset to default after build
    }

    [MenuItem("Build/Build iOS Addressables")]
    public static void BuildIOSAddressables()
    {
        BuildAddressables();
    }

    [MenuItem("Build/Build iOS Player")]
    public static void BuildIOSPlayer()
    {
        UpdateVersion();
        BuildPlayer(BuildTarget.iOS, "Builds/iOS/");
    }

    private static void BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent();
    }

    private static void BuildPlayer(BuildTarget target, string locationPathName)
    {
        // Get scenes included in the build settings
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = locationPathName,
            target = target,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log(target + " Build succeeded: " + report.summary.totalSize + " bytes");
        }
        else
        {
            Debug.Log(target + " Build failed");
        }
    }

    private static void UpdateVersion()
    {
        string versionFilePath = "version.txt";
        if (File.Exists(versionFilePath))
        {
            string version = File.ReadAllText(versionFilePath).Trim();
            string[] versionParts = version.Split('.');
            int major = int.Parse(versionParts[0]);
            int minor = int.Parse(versionParts[1]);
            int build = int.Parse(versionParts[2]) + 1;

            string newVersion = $"{major}.{minor}.{build}";
            File.WriteAllText(versionFilePath, newVersion);

            PlayerSettings.bundleVersion = newVersion;
            PlayerSettings.Android.bundleVersionCode = build;
            PlayerSettings.iOS.buildNumber = newVersion;
        }
        else
        {
            Debug.LogError("Version file not found!");
        }
    }
}
