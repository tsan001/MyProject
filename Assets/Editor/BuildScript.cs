using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.Build.Reporting;
using System.Linq;
using System.IO;
using UnityEditor.AddressableAssets;
using UnityEngine;

public class BuildScript
{
    private static string buildVersion;
    private static string buildNumber;

    public static void BuildAndroidAddressables(string version, string number)
    {
        buildVersion = version;
        buildNumber = number;
        Debug.Log("Build Number: " + buildNumber);
        SetAddressablePaths("Android", buildNumber);
        CleanAddressables();
        BuildAddressables();
    }

    public static void UpdateAndroidAddressables(string version, string number)
    {
        buildVersion = version;
        buildNumber = number;
        Debug.Log("Build Number: " + buildNumber);
        SetAddressablePaths("Android", buildNumber);
        BuildAddressables();
    }

    public static void BuildAndroidPlayer(string version, string number)
    {
        buildVersion = version;
        buildNumber = number;
        SetVersion(buildVersion);
        BuildPlayer(BuildTarget.Android, "Builds/Android/MyGame.apk", buildNumber);
    }

    public static void BuildAndroidBundle(string version, string number)
    {
        buildVersion = version;
        buildNumber = number;
        SetVersion(buildVersion);
        EditorUserBuildSettings.buildAppBundle = true;
        BuildPlayer(BuildTarget.Android, "Builds/Android/MyGame.aab", buildNumber);
        EditorUserBuildSettings.buildAppBundle = false; // Reset to default after build
    }

    public static void BuildIOSAddressables(string version, string number)
    {
        buildVersion = version;
        buildNumber = number;
        Debug.Log("Build Number: " + buildNumber);
        SetAddressablePaths("iOS", buildNumber);
        CleanAddressables();
        BuildAddressables();
    }

    public static void UpdateIOSAddressables(string version, string number)
    {
        buildVersion = version;
        buildNumber = number;
        Debug.Log("Build Number: " + buildNumber);
        SetAddressablePaths("iOS", buildNumber);
        BuildAddressables();
    }

    public static void BuildIOSPlayer(string version, string number)
    {
        buildVersion = version;
        buildNumber = number;
        SetVersion(buildVersion);
        BuildPlayer(BuildTarget.iOS, "Builds/iOS", buildNumber);
    }

    private static void SetAddressablePaths(string platform, string buildNumber)
    {
        if (string.IsNullOrEmpty(buildNumber))
        {
            Debug.LogError("Build number not provided. Using 'Version' as default.");
            buildNumber = "Version";
        }

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var profileSettings = settings.profileSettings;
        var profileId = settings.activeProfileId;

        string remoteBuildPath = $"ServerData/{platform}/{buildNumber}";
        string remoteLoadPath = $"ServerData/{platform}/{buildNumber}";

        profileSettings.SetValue(profileId, "RemoteBuildPath", remoteBuildPath);
        profileSettings.SetValue(profileId, "RemoteLoadPath", remoteLoadPath);

        // Save the modified settings
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log($"Addressable paths set to: {remoteBuildPath}");
    }

    private static void CleanAddressables()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressableAssetSettings.CleanPlayerContent(settings.ActivePlayerDataBuilder);
        Debug.Log("Cleaned Addressables content");
    }

    private static void BuildAddressables()
    {
        AddressableAssetSettings.BuildPlayerContent();
    }

    private static void BuildPlayer(BuildTarget target, string locationPathName, string buildNumber)
    {
        SetAddressablePaths(target.ToString(), buildNumber); // Ensure paths are correct before build

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

    private static void SetVersion(string version)
    {
        PlayerSettings.bundleVersion = version;
        PlayerSettings.Android.bundleVersionCode = int.Parse(version.Split('.')[2]);
        PlayerSettings.iOS.buildNumber = version;
    }
}
