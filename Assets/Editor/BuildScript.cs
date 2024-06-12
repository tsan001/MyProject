using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.Build.Reporting;
using System.Linq;
using System.IO;
using UnityEngine;

public class BuildScript
{
    public static void BuildAndroidAddressables()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        SetVersion(buildVersion);
        BuildAddressables(buildVersion, "Android");
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
        BuildAddressables(buildVersion, "iOS");
    }

    public static void BuildIOSPlayer()
    {
        string buildVersion = GetCommandLineArg("-buildVersion");
        SetVersion(buildVersion);
        BuildPlayer(BuildTarget.iOS, "Builds/iOS");
    }

    private static void BuildAddressables(string version, string platform)
    {
        AddressableAssetSettings.BuildPlayerContent();
        string sourceFolder = $"Library/com.unity.addressables/aa/{platform.ToLower()}";
        string targetFolder = $"serverdata/{platform.ToLower()}/{version}";

        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        CopyFilesRecursively(sourceFolder, targetFolder);
    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
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
