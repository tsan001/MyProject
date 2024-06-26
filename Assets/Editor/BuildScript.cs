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
    public static void BuildAndroidAddressables()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        string buildNumber = GetCommandLineArg("-buildNumber") ?? System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        Debug.Log("BuildAndroidAddressables - Build Version: " + buildVersion);
        Debug.Log("BuildAndroidAddressables - Build Number: " + buildNumber);

        SetAddressablePaths("Android", buildNumber);
        EnableBuildRemoteCatalog();
        CleanAddressables();
        BuildAddressables();
    }

    public static void UpdateAndroidAddressables()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        string buildNumber = GetCommandLineArg("-buildNumber") ?? System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        Debug.Log("UpdateAndroidAddressables - Build Version: " + buildVersion);
        Debug.Log("UpdateAndroidAddressables - Build Number: " + buildNumber);

        SetAddressablePaths("Android", buildNumber);
        EnableBuildRemoteCatalog();
        BuildAddressables();
    }

    public static void BuildAndroidPlayer()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        string buildNumber = GetCommandLineArg("-buildNumber") ?? System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        Debug.Log("BuildAndroidPlayer - Build Version: " + buildVersion);
        Debug.Log("BuildAndroidPlayer - Build Number: " + buildNumber);

        SetVersion(buildVersion);
        DisableBuildRemoteCatalog();
        BuildPlayer(BuildTarget.Android, "Builds/Android/MyGame.apk");
    }

    public static void BuildAndroidBundle()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        string buildNumber = GetCommandLineArg("-buildNumber") ?? System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        Debug.Log("BuildAndroidBundle - Build Version: " + buildVersion);
        Debug.Log("BuildAndroidBundle - Build Number: " + buildNumber);

        SetVersion(buildVersion);
        DisableBuildRemoteCatalog();
        EditorUserBuildSettings.buildAppBundle = true;
        BuildPlayer(BuildTarget.Android, "Builds/Android/MyGame.aab");
        EditorUserBuildSettings.buildAppBundle = false; // Reset to default after build
    }

    public static void BuildIOSAddressables()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        string buildNumber = GetCommandLineArg("-buildNumber") ?? System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        Debug.Log("BuildIOSAddressables - Build Version: " + buildVersion);
        Debug.Log("BuildIOSAddressables - Build Number: " + buildNumber);

        SetAddressablePaths("iOS", buildNumber);
        EnableBuildRemoteCatalog();
        CleanAddressables();
        BuildAddressables();
    }

    public static void UpdateIOSAddressables()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        string buildNumber = GetCommandLineArg("-buildNumber") ?? System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        Debug.Log("UpdateIOSAddressables - Build Version: " + buildVersion);
        Debug.Log("UpdateIOSAddressables - Build Number: " + buildNumber);

        SetAddressablePaths("iOS", buildNumber);
        EnableBuildRemoteCatalog();
        BuildAddressables();
    }

    public static void BuildIOSPlayer()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        string buildNumber = GetCommandLineArg("-buildNumber") ?? System.Environment.GetEnvironmentVariable("BUILD_NUMBER");
        Debug.Log("BuildIOSPlayer - Build Version: " + buildVersion);
        Debug.Log("BuildIOSPlayer - Build Number: " + buildNumber);

        SetVersion(buildVersion);
        DisableBuildRemoteCatalog();
        BuildPlayer(BuildTarget.iOS, "Builds/iOS");
    }

    private static void SetAddressablePaths(string platform, string buildNumber)
    {
        Debug.Log("SetAddressablePaths - Platform: " + platform);
        Debug.Log("SetAddressablePaths - Build Number: " + buildNumber);

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("AddressableAssetSettings is null. Ensure Addressable Assets is properly configured.");
            return;
        }

        var profileSettings = settings.profileSettings;
        if (profileSettings == null)
        {
            Debug.LogError("ProfileSettings is null. Ensure Addressable Assets is properly configured.");
            return;
        }

        var profileId = settings.activeProfileId;
        if (string.IsNullOrEmpty(profileId))
        {
            Debug.LogError("ActiveProfileId is null or empty. Ensure Addressable Assets is properly configured.");
            return;
        }

        string remoteBuildPath = $"ServerData/{platform}/{buildNumber}";
        string remoteLoadPath = $"ServerData/{platform}/{buildNumber}";

        Debug.Log("SetAddressablePaths - RemoteBuildPath: " + remoteBuildPath);
        Debug.Log("SetAddressablePaths - RemoteLoadPath: " + remoteLoadPath);

        profileSettings.SetValue(profileId, "Remote.BuildPath", remoteBuildPath);
        profileSettings.SetValue(profileId, "Remote.LoadPath", remoteLoadPath);

        Debug.Log("SetAddressablePaths - Profile settings updated.");

        // Save the modified settings
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Addressable paths set to: {remoteBuildPath}");
    }

    private static void EnableBuildRemoteCatalog()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        settings.BuildRemoteCatalog = true;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    private static void DisableBuildRemoteCatalog()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        settings.BuildRemoteCatalog = false;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
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

    private static void SetVersion(string version)
    {
        PlayerSettings.bundleVersion = version;
        PlayerSettings.Android.bundleVersionCode = int.Parse(version.Split('.')[2]);
        PlayerSettings.iOS.buildNumber = version;
    }

    private static string GetCommandLineArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
