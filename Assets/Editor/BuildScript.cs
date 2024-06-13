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
        SetVersion(buildVersion);
        SetAddressablePaths(BuildTarget.Android, buildVersion);
        BuildAddressables();
    }

    public static void BuildAndroidPlayer()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        SetVersion(buildVersion);
        BuildPlayer(BuildTarget.Android, "Builds/Android/MyGame.apk");
    }

    public static void BuildAndroidBundle()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        SetVersion(buildVersion);
        EditorUserBuildSettings.buildAppBundle = true;
        BuildPlayer(BuildTarget.Android, "Builds/Android/MyGame.aab");
        EditorUserBuildSettings.buildAppBundle = false; // Reset to default after build
    }

    public static void BuildIOSAddressables()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        SetVersion(buildVersion);
        SetAddressablePaths(BuildTarget.iOS, buildVersion);
        BuildAddressables();
    }

    public static void BuildIOSPlayer()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        SetVersion(buildVersion);
        BuildPlayer(BuildTarget.iOS, "Builds/iOS");
    }

    private static void SetAddressablePaths(BuildTarget target, string version)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var profileSettings = settings.profileSettings;
        var profileId = settings.activeProfileId;

        string platform = target.ToString();
        string remoteBuildPath = $"ServerData/{platform}/{version}";
        string remoteLoadPath = $"ServerData/{platform}/{version}";

        profileSettings.SetValue(profileId, "Remote.BuildPath", remoteBuildPath);
        profileSettings.SetValue(profileId, "Remote.LoadPath", remoteLoadPath);

        // Save the modified settings
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        Debug.Log($"Addressable paths set to: {remoteBuildPath}");
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
