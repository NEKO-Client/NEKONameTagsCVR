using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using NEKOClient.Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using NEKOClient.Compatibility;

[assembly: MelonInfo(typeof(NEKOClient.NEKOClient), "NEKOClientCVR", "1.0.1", "ChisVR")]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]

namespace NEKOClient
{
    public static class NEKOClient
    {
        internal static Queue<Action> _Queue;

        // this prevents some garbage collection bullshit
        private static List<object> ourPinnedDelegates = new List<object>();
        public static HarmonyLib.Harmony Harmony { get; private set; }

        private static MelonPreferences_Entry<bool> _paranoidMode;

        public static NameplateManager? NameplateManager;

        private static void DownloadFiles(string path, string fileName, string fileext)
        {
            

                using var sha256 = SHA256.Create();

                byte[] bytes = null;
                if (File.Exists($"{Path.Combine(Environment.CurrentDirectory, path + "/" + fileName + "." + fileext)}"))
                {
                    bytes = File.ReadAllBytes($"{Path.Combine(Environment.CurrentDirectory, path + "/" + fileName + "." + fileext)}");
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
                    latestBytes = wc.DownloadData($"https://api.chisdealhd.co.uk/v2/games/api/vrchatclient/nekoclientcvr/assets/{fileName}");
                }
                catch (WebException e)
                {
                    MelonLogger.Error($"Unable to download latest version of ReModCE: {e}");
                }

                if (bytes == null)
                {
                    if (latestBytes == null)
                    {
                        MelonLogger.Error($"No local file exists and unable to download latest version from GitHub. {fileName}.{fileext} will not load!");
                        return;
                    }
                    MelonLogger.Warning($"Couldn't find {path}/{fileName}.{fileext} on disk. Saving latest version from GitHub.");
                    bytes = latestBytes;
                    try
                    {
                        File.WriteAllBytes($"{Path.Combine(Environment.CurrentDirectory, path + "/" + fileName + "." + fileext)}", bytes);
                    }
                    catch (IOException)
                    {
                        ReLogger.Warning($"Failed writing {fileName}.{fileext} to disk. You may encounter errors while using ReModCE.");
                    }
                }

                if (latestBytes != null)
                {
                    var latestHash = ComputeHash(sha256, latestBytes);
                    var currentHash = ComputeHash(sha256, bytes);

                    if (latestHash != currentHash)
                    {
                        if (_paranoidMode.Value)
                        {
                            MelonLogger.Msg(ConsoleColor.Cyan,
                                $"There is a new version of ReModCE available. You can either delete the \"{fileName}.{fileext}\" from your {path}.");
                        }
                        else
                        {
                            bytes = latestBytes;
                            try
                            {
                                File.WriteAllBytes($"{Path.Combine(Environment.CurrentDirectory, path + "/" + fileName + "." + fileext)}", bytes);
                            }
                            catch (IOException)
                            {
                                ReLogger.Warning($"Failed writing {fileName}.{fileext} to disk. You may encounter errors while using ReModCE.");
                            }
                            MelonLogger.Msg(ConsoleColor.Green, $"Updated {fileName}.{fileext} to latest version.");
                        }
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

        public static void OnApplicationStart()
        {
            Harmony = MelonHandler.Mods.First(m => m.Info.Name == "NEKOClient").HarmonyInstance;
            Directory.CreateDirectory("UserData/NEKOClient");
            Directory.CreateDirectory("UserData/NEKOClient/Background");
            Directory.CreateDirectory("UserData/NEKOClient/LoadingScreenMusic");

            if (File.Exists("NEKOCLIENT/"))
            {
                Directory.Delete("NEKOCLIENT/");
            }

            if (File.Exists("LoadingScreenMusic/"))
            {
                Directory.Delete("LoadingScreenMusic/");
            }

            ReLogger.Msg("Initializing...");

            _Queue = new Queue<Action>();

            if (!File.Exists("UserData/NEKOClient/LoadingScreenMusic/Music.ogg"))
            {
                DownloadFiles("UserData/NEKOClient/LoadingScreenMusic", "Music", "ogg");
            }
            else
            {
                DownloadFiles("UserData/NEKOClient/LoadingScreenMusic", "Music", "ogg");
            }


        }
        private static void ShowLogo()
        {
            Console.Title = "NEKO CLIENT";
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(@"=============================================================================================================");
            Console.WriteLine(@"  /$$   /$$ /$$$$$$$$ /$$   /$$  /$$$$$$         /$$$$$$  /$$       /$$$$$$ /$$$$$$$$ /$$   /$$ /$$$$$$$$    ");
            Console.WriteLine(@" | $$$ | $$| $$_____/| $$  /$$/ /$$__  $$       /$$__  $$| $$      |_  $$_/| $$_____/| $$$ | $$|__  $$__/    ");
            Console.WriteLine(@" | $$$$| $$| $$      | $$ /$$/ | $$  \ $$      | $$  \__/| $$        | $$  | $$      | $$$$| $$   | $$       ");
            Console.WriteLine(@" | $$ $$ $$| $$$$$   | $$$$$/  | $$  | $$      | $$      | $$        | $$  | $$$$$   | $$ $$ $$   | $$       ");
            Console.WriteLine(@" | $$  $$$$| $$__/   | $$  $$  | $$  | $$      | $$      | $$        | $$  | $$__/   | $$  $$$$   | $$       ");
            Console.WriteLine(@" | $$\  $$$| $$      | $$\  $$ | $$  | $$      | $$    $$| $$        | $$  | $$      | $$\  $$$   | $$       ");
            Console.WriteLine(@" | $$ \  $$| $$$$$$$$| $$ \  $$|  $$$$$$/      |  $$$$$$/| $$$$$$$$ /$$$$$$| $$$$$$$$| $$ \  $$   | $$       ");
            Console.WriteLine(@" | __/  \__/|________/|__/  \__/ \______/        \______/ |________/|______/|________/|__/  \__/   |__/      ");
            Console.WriteLine("                                                                                                              ");
            Console.WriteLine(@"                                                     /\__ /\                                                 ");
            Console.WriteLine(@"                                                    /`     '\                                                ");
            Console.WriteLine(@"                                                    === 0  0 ===                                             ");
            Console.WriteLine(@"                                                     \   --  /                                               ");
            Console.WriteLine(@"                                                     /       \                                               ");
            Console.WriteLine(@"                                                    /         \                                              ");
            Console.WriteLine(@"                                                   |           |                                             ");
            Console.WriteLine(@"                                                   \   ||  ||  /                                             ");
            Console.WriteLine(@"                                                    \_oo__oo_ /#######o                                      ");
            Console.WriteLine("                                                                                                              ");
            Console.WriteLine(@"                                    NEKO Client - By ChisVR, Bison, Aries                    ");
            Console.WriteLine(@"                                                         HotKeys                                             ");
            Console.WriteLine(@"=============================================================================================================");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void OnInitializeMelon()
        {
            ShowLogo();

            AssetManager.Init();
            Settings.Init();
            Compat.Init();

            MelonCoroutines.Start(UIManagerInit());
        }

        private static IEnumerator UIManagerInit()
        {
            while (GameObject.Find("Cohtml/QuickMenu") == null) yield return null;
            try
            {
                NameplateManager = new NameplateManager();
                Patching.Patching.Init();
            }
            catch (Exception obj)
            {
                MelonLogger.Error("Unable to Apply Patches: " + obj);
            }
        }

        public static void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (NameplateManager?.Nameplates == null) return;
            if (NameplateManager.Nameplates.Count <= 0) return;
            NameplateManager.ClearNameplates();
        }

        public static void OnPreferencesSaved()
        {
            if (NameplateManager?.Nameplates == null) return;

            foreach (var plate in NameplateManager.Nameplates)
            {
                MelonDebug.Msg("Applying Settings for user: " + plate.Key);

                if (plate.Value != null && plate.Value.Nameplate != null)
                {
                    plate.Value.ApplySettings();
                }
            }
        }

        internal static void Log(object msg) => ReLogger.Msg(msg);

        internal static void Debug(object msg)
        {
            if (MelonDebug.IsEnabled())
                ReLogger.Msg(ConsoleColor.Cyan, msg);
        }

        internal static void Error(object obj) => ReLogger.Error(obj);

        internal static void DebugError(object obj)
        {
            if (MelonDebug.IsEnabled())
                ReLogger.Error($"[DEBUG] {obj}");
        }

        public static void Warning(string s)
        {
            ReLogger.Warning(s);
        }
    }
}
