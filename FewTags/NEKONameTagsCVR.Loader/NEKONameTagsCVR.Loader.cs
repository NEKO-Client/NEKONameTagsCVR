using MelonLoader;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;

namespace NEKONameTagsCVR.Loader
{
    public static class BuildInfo
    {
        public const string Name = "NEKONameTagsCVR.Loader";
        public const string Author = "ChisVR";
        public const string Company = null;
        public const string Version = "1.0";
        public const string DownloadLink = "https://github.com/NEKO-Client/NEKONameTagsCVR/releases/latest/";
    }

    internal static class GitHubInfo
    {
        public const string Author = "NEKO-Client";
        public const string Repository = "NEKONameTagsCVR";
        public const string Version = "latest";
    }

    public class ReLoader : MelonPlugin
    {

        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Checking latest version for github");
            GetLatestVersion();
        }
        private void GetLatestVersion()
        {
            DownloadFromGitHub("NEKONameTagsCVR", out _);
        }

        private void DownloadFromGitHub(string fileName, out Assembly loadedAssembly)
        {
            if (fileName == "NEKONameTagsCVR")
            {

                using var sha256 = SHA256.Create();

                byte[] bytes = null;
                if (File.Exists($"Mods/{fileName}.dll"))
                {
                    bytes = File.ReadAllBytes($"Mods/{fileName}.dll");
                }

                using var wc = new WebClient
                {
                    Headers =
                {
                    ["User-Agent"] =
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0"
                }
                };

                byte[] latestBytes = null;
                try
                {
                    latestBytes = wc.DownloadData($"https://api.chisdealhd.co.uk/v2/games/api/chilloutvrclient/NEKONameTagsCVR/assets/{fileName}");
                }
                catch (WebException e)
                {
                    MelonLogger.Error($"Unable to download latest version of ReModCE: {e}");
                }

                if (bytes == null)
                {
                    if (latestBytes == null)
                    {
                        MelonLogger.Error($"No local file exists and unable to download latest version from GitHub. {fileName} will not load!");
                        loadedAssembly = null;
                        return;
                    }
                    MelonLogger.Warning($"Couldn't find {fileName}.dll on disk. Saving latest version from GitHub.");
                    bytes = latestBytes;
                    try
                    {
                        File.WriteAllBytes($"Mods/{fileName}.dll", bytes);
                    }
                    catch (IOException)
                    {
                        ReLogger.Warning($"Failed writing {fileName} to disk. You may encounter errors while using ReModCE.");
                    }
                }

                if (latestBytes != null)
                {
                    var latestHash = ComputeHash(sha256, latestBytes);
                    var currentHash = ComputeHash(sha256, bytes);

                    if (latestHash != currentHash)
                    {
                        bytes = latestBytes;
                        try
                        {
                            File.WriteAllBytes($"Mods/{fileName}.dll", bytes);
                        }
                        catch (IOException)
                        {
                            ReLogger.Warning($"Failed writing {fileName} to disk. You may encounter errors while using ReModCE.");
                        }
                        MelonLogger.Msg(ConsoleColor.Green, $"Updated {fileName} to latest version.");
                    }
                }


                try
                {
                    loadedAssembly = Assembly.Load(bytes);
                }
                catch (BadImageFormatException e)
                {
                    MelonLogger.Error($"Couldn't load specified image: {e}");
                    loadedAssembly = null;
                }

            }
            else
            {

                using var sha256 = SHA256.Create();

                byte[] bytes = null;
                if (File.Exists($"Mods/{fileName}.dll"))
                {
                    bytes = File.ReadAllBytes($"Mods/{fileName}.dll");
                }

                using var wc = new WebClient
                {
                    Headers =
                {
                    ["User-Agent"] =
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0"
                }
                };

                byte[] latestBytes = null;
                try
                {
                    latestBytes = wc.DownloadData($"https://api.chisdealhd.co.uk/v2/games/api/chilloutvrclient/NEKONameTagsCVR/assets/{fileName}");
                }
                catch (WebException e)
                {
                    MelonLogger.Error($"Unable to download latest version of ReModCE: {e}");
                }

                if (bytes == null)
                {
                    if (latestBytes == null)
                    {
                        MelonLogger.Error($"No local file exists and unable to download latest version from GitHub. {fileName} will not load!");
                        loadedAssembly = null;
                        return;
                    }
                    MelonLogger.Warning($"Couldn't find {fileName}.dll on disk. Saving latest version from GitHub.");
                    bytes = latestBytes;
                    try
                    {
                        File.WriteAllBytes($"Mods/{fileName}.dll", bytes);
                    }
                    catch (IOException)
                    {
                        ReLogger.Warning($"Failed writing {fileName} to disk. You may encounter errors while using ReModCE.");
                    }
                }

                if (latestBytes != null)
                {
                    var latestHash = ComputeHash(sha256, latestBytes);
                    var currentHash = ComputeHash(sha256, bytes);

                    if (latestHash != currentHash)
                    {

                        bytes = latestBytes;
                        try
                        {
                            File.WriteAllBytes($"Mods/{fileName}.dll", bytes);
                        }
                        catch (IOException)
                        {
                            ReLogger.Warning($"Failed writing {fileName} to disk. You may encounter errors while using ReModCE.");
                        }
                        MelonLogger.Msg(ConsoleColor.Green, $"Updated {fileName} to latest version.");
                    }
                }


                try
                {
                    loadedAssembly = Assembly.Load(bytes);
                }
                catch (BadImageFormatException e)
                {
                    MelonLogger.Error($"Couldn't load specified image: {e}");
                    loadedAssembly = null;
                }
            }
        }


        private static string ComputeHash(HashAlgorithm sha256, byte[] data)
        {
            var bytes = sha256.ComputeHash(data);
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}