using MelonLoader;
using NEKONameTagsCVR.Loader;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

// Thanks To Edward7 For The Original Base

namespace NekoNameTagsCVR.Loader
{
    public static class BuildInfo
    {
        public const string Name = "NEKONameTagsCVR.Loader";
        public const string Author = "NekoSuneVR";
        public const string Company = null;
        public const string Version = "1.1";
        public const string DownloadLink = "https://github.com/NEKO-Client/NEKONameTagsCVR/releases/latest/";
    }

    internal static class GitHubInfo
    {
        public const string Author = "NEKO-Client";
        public const string Repository = "NEKONameTagsCVR";
        public const string Version = "latest";
        public const string Loader = "NEKONameTagsCVR.Loader.dll";
        public const string Mod = "NEKONameTagsCVR.dll";
    }

    public class ReLoader : MelonPlugin
    {
        private string shortpathLoader = "NEKONameTagsCVR.Loader.dll";
        private string filepathLoader = "Plugins/NEKONameTagsCVR.Loader.dll";

        private string shortpath = "NEKONameTagsCVR.dll";
        private string filepath = "Mods/NEKONameTagsCVR.dll";

        private void GetLatestVersion()
        {
            DownloadFromGitHub("NEKONameTagsCVR.Loader.dll");
            DownloadFromGitHub("NEKONameTagsCVR");
        }


#pragma warning disable CS0672 // Member overrides obsolete member
        public override void OnApplicationStart()
#pragma warning restore CS0672 // Member overrides obsolete member
        {
            MelonLogger.Msg("Checking latest version for github");
            GetLatestVersion();
        }

        private void DownloadFromGitHub(string fileName)
        {
            if (fileName == "NEKONameTagsCVR")
            {
                SHA256 sha = SHA256.Create();
                if (File.Exists(shortpath))
                {
                    File.Delete(shortpath);
                    ReLogger.Msg("Yeeted NEKONameTagsCVR From Folder!");
                }
                byte[] bytes;
                if (!File.Exists(filepath))
                {
                    bytes = new WebClient().DownloadData($"{BuildInfo.DownloadLink}download/{GitHubInfo.Mod}");
                    File.WriteAllBytes(filepath, bytes);
                    ReLogger.Msg("NEKONameTagsCVR Downloaded!");
                    return;
                }
                bytes = new WebClient().DownloadData($"{BuildInfo.DownloadLink}download/{GitHubInfo.Mod}");
                string text = ComputeHash(sha, bytes);
                byte[] data = File.ReadAllBytes(filepath);
                if (ComputeHash(sha, data) != text)
                {
                    File.WriteAllBytes(filepath, bytes);
                    ReLogger.Msg("NEKONameTagsCVR Updated!");
                }
                else
                {
                    ReLogger.Msg("NEKONameTagsCVR Is Already Up To Date!");
                }
            } else {
                SHA256 sha = SHA256.Create();
                if (File.Exists(shortpathLoader))
                {
                    File.Delete(shortpathLoader);
                    ReLogger.Msg("Yeeted NEKONameTagsCVR.Loader From Folder!");
                }
                byte[] bytes;
                if (!File.Exists(filepathLoader))
                {
                    bytes = new WebClient().DownloadData($"{BuildInfo.DownloadLink}download/{GitHubInfo.Loader}");
                    File.WriteAllBytes(filepathLoader, bytes);
                    ReLogger.Msg("NEKONameTagsCVR.Loader Downloaded!");
                    return;
                }
                bytes = new WebClient().DownloadData($"{BuildInfo.DownloadLink}download/{GitHubInfo.Loader}");
                string text = ComputeHash(sha, bytes);
                byte[] data = File.ReadAllBytes(filepathLoader);
                if (ComputeHash(sha, data) != text)
                {
                    File.WriteAllBytes(filepathLoader, bytes);
                    ReLogger.Msg("NEKONameTagsCVR.Loader Updated!");
                }
                else
                {
                    ReLogger.Msg("NEKONameTagsCVR.Loader Is Already Up To Date!");
                }
            }
        }


        private static string ComputeHash(HashAlgorithm sha256, byte[] data)
        {
            byte[] array = sha256.ComputeHash(data);
            StringBuilder stringBuilder = new StringBuilder();
            byte[] array2 = array;
            foreach (byte b in array2)
            {
                stringBuilder.Append(b.ToString("x2"));
            }
            return stringBuilder.ToString();
        }
    }
}
