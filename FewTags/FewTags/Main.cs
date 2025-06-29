﻿using ABI_RC.Core.InteractionSystem;
using ABI_RC.Core.Networking.IO.Instancing;
using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.UI;
using NekoNameTagsCVR.Utils;
using Harmony;
using MelonLoader;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Threading.Tasks;

// Thanks To Edward7 For The Original Base

namespace NekoNameTagsCVR
{
    public class Main : MelonMod
    {
        public static bool ChatBoxLoaded { get; private set; }

        private static float a;
        private static float b;
        private static float g;
        private static float r;

        static bool isOverlay = false;
        private static List<Json.User> _userArr { get; set; }
        private static GameObject s_namePlate { get; set; }
        private static GameObject s_dev { get; set; }
        private static GameObject s_MainPlateHolder { get; set; }
        private static GameObject s_BigPlateHolder { get; set; }
        private static GameObject s_textMeshProGmj { get; set; }
        private static GameObject s_textMeshProGmj2 { get; set; }
        private static Transform s_plateTransform { get; set; }
        private static TMPro.TextMeshProUGUI s_Logo { get; set; }

        [Obsolete]
        private HarmonyInstance _hInstance { get; } = new HarmonyInstance(Guid.NewGuid().ToString());

        private static void ShowLogo()
        {
            Console.Title = "NEKONameTagsCVR";
            Console.ForegroundColor = System.ConsoleColor.DarkMagenta;
            MelonLogger.Msg(@"=============================================================================================================");
            MelonLogger.Msg(@"                                  /$$   /$$ /$$$$$$$$ /$$   /$$  /$$$$$$      ");
            MelonLogger.Msg(@"                                  | $$$ | $$| $$_____/| $$  /$$/ /$$__  $$     ");
            MelonLogger.Msg(@"                                  | $$$$| $$| $$      | $$ /$$/ | $$  \ $$     ");
            MelonLogger.Msg(@"                                  | $$ $$ $$| $$$$$   | $$$$$/  | $$  | $$     ");
            MelonLogger.Msg(@"                                  | $$  $$$$| $$__/   | $$  $$  | $$  | $$     ");
            MelonLogger.Msg(@"                                  | $$\  $$$| $$      | $$\  $$ | $$  | $$     ");
            MelonLogger.Msg(@"                                  | $$ \  $$| $$$$$$$$| $$ \  $$|  $$$$$$/     ");
            MelonLogger.Msg(@"                                  | __/  \__/|________/|__/  \__/ \______/     ");
            MelonLogger.Msg(@"                                                                                                              ");
            MelonLogger.Msg(@"                                                     /\__ /\                                                 ");
            MelonLogger.Msg(@"                                                    /`     '\                                                ");
            MelonLogger.Msg(@"                                                    === 0  0 ===                                             ");
            MelonLogger.Msg(@"                                                     \   --  /                                               ");
            MelonLogger.Msg(@"                                                     /       \                                               ");
            MelonLogger.Msg(@"                                                    /         \                                              ");
            MelonLogger.Msg(@"                                                   |           |                                             ");
            MelonLogger.Msg(@"                                                   \   ||  ||  /                                             ");
            MelonLogger.Msg(@"                                                    \_oo__oo_ /#######o                                      ");
            MelonLogger.Msg(@"                                                                                                              ");
            MelonLogger.Msg(@"                   NEKONameTagsCVR is Nametag MOD (OLD VRC MOD NEKONameTagsVRC)- By NekoSuneVR                 ");
            MelonLogger.Msg(@"                   Version: 1.0.9 | Twitch: NekoSuneVR | TikTok: NekoSuneVR | YouTube: NekoSuneVR              ");
            MelonLogger.Msg(@"=============================================================================================================");
            Console.ForegroundColor = System.ConsoleColor.White;
        }

        [Obsolete]
        public override void OnApplicationStart()
        {
            ShowLogo();
            MelonLogger.Msg("Initializing.");
            MelonLogger.Msg("NekoNameTagsCVR Loaded. Press Slash To Reload Tags");
            MelonLogger.Msg(ConsoleColor.Magenta, "Nameplate Overlay/Nameplate ESP - Keybind: RightCTRL + O");
            DownloadString();
            _hInstance.Patch(typeof(PlayerNameplate).GetMethod(nameof(PlayerNameplate.UpdateNamePlate)), null, typeof(Main).GetMethod(nameof(OnPlayerJoin), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).ToNewHarmonyMethod());
            MelonCoroutines.Start(WaitForNamePlate());
        }

        private static IEnumerator WaitForNamePlate()
        {
            while (Resources.FindObjectsOfTypeAll<PuppetMaster>() == null)
                yield return null;
            s_namePlate = Resources.FindObjectsOfTypeAll<PuppetMaster>().FirstOrDefault(x => x.name == "_NetworkedPlayerObject").transform.Find("[NamePlate]/Canvas/Content").gameObject;
        }

        // Keybind To Re-Fetch The Tags (Prevents The Need Of Having To Restart You're Game)
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Slash))
            {
                if (CVR_MenuManager.Instance.quickMenuCollider.enabled == true || ViewManager.Instance.isGameMenuOpen() == true) { return; } // we don't wanna accidently reload tags while menu's are open
                else
                {
                    ReloadString();
                    MelonLogger.Msg("Reloaded Tags, Please Rejoin World If Needed.");
                    CohtmlHud.Instance.ViewDropText("NekoNameTagsCVR", "Connected", "Downloading Tags");
                }
            }

            // Overlay Toggle On Off // -- I'd rewrite the entire mod if I did this the way I'd personally like to but whatever I guess lol
            if (isOverlay == false)
            {
                if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.O))
                {
                    isOverlay = true;
                    MelonLogger.Msg(ConsoleColor.Green, "Enabled Nameplate Overlay/Nameplate ESP");
                    if (CohtmlHud.Instance != null)
                    {
                        try // Incase it fails in whatever case
                        {
                            CohtmlHud.Instance.ViewDropText("NekoNameTagsCVR", "Enabled Nameplate Overlay/ESP", "Rejoining World");
                        }
                        catch { MelonLogger.Msg(ConsoleColor.DarkRed, $"Failed To Display CohtmlHud Message"); }
                    }
                    RejoinWorld();
                }
            }
            else if (isOverlay == true)
            {
                if (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.O))
                {
                    isOverlay = false;
                    MelonLogger.Msg(ConsoleColor.DarkGray, "Disabled Nameplate Overlay/Nameplate ESP");
                    if (CohtmlHud.Instance != null)
                    {
                        try // Incase it fails in whatever case
                        {
                            CohtmlHud.Instance.ViewDropText("NekoNameTagsCVR", "Disabled Nameplate Overlay/ESP", "Rejoining World");
                        }
                        catch { MelonLogger.Msg(ConsoleColor.DarkRed, $"Failed To Display CohtmlHud Message"); }
                    }
                    RejoinWorld();
                }
            }
        }

        // Used For The Keybind To Update The Tags
        private void ReloadString()
        {
            try
            {
                _userArr.Clear();
            }
            catch // not gonna let yourself know if there was an issue clearing?? let me fix that for you
            {
                MelonLogger.Msg(ConsoleColor.Yellow, "Couldn't Clear Tags List - Probably Because There Were No Tags To Clear");
            }
            DownloadString();
        }

        private static string s_uId { get; set; }
        private static Json.User s_user { get; set; }
        private static void OnPlayerJoin(PlayerNameplate __instance)
        {
            s_uId = __instance.transform.parent.name;
            s_user = _userArr.FirstOrDefault(x => x.UserId == s_uId);
            if (s_user == null) return;
            if (GameObject.Find($"{s_uId}/[NamePlate]/Canvas/NekoNameTagsCVR-NamePlate") == null) // why wasnt there a check for if it already exists?
            {
                for (int i = 0; i < s_user.NamePlatesText.Length; i++)
                {
                    GeneratePlateAsync(s_uId, s_user.NamePlatesText[i], i, new Color32(byte.Parse(s_user.Color[0].ToString()), byte.Parse(s_user.Color[1].ToString()), byte.Parse(s_user.Color[2].ToString()), byte.Parse(s_user.Color[3].ToString())));
                }
            }
            if (GameObject.Find($"{s_uId}/[NamePlate]/Canvas/NekoNameTagsCVR-BigNamePlate") == null) // again why wasnt there a check for if it already exists?
            {
                for (int i = 0; i < s_user.BigPlatesText.Length; i++)
                {
                    GenerateBigPlate(s_uId, s_user.BigPlatesText[i], i);
                }
            }
            /*CreateLogo(s_uId);*/
        }

        private static float s_textCount { get; set; }
        private static GameObject s_imageHolder { get; set; }

        private static async Task GeneratePlateAsync(string uid, string plateText, int multiplier, Color32 color)
        {
            // This Was Used For Testing Mainly To Check Lengths Of Things (Sorta Math Related I Guess)
             //MelonLogger.Msg("---PlateText---");
             //MelonLogger.Msg(plateText);
             //MelonLogger.Msg("---PlateText Length---");
             //MelonLogger.Msg(plateText.Length);

            if (plateText == null) { return; } // why make the plate if it has no text to start with?
            else if (uid == null) { return; } // this one isnt really needed tbh but personally I like to check to prevent code that shouldn't be running to run
            else
            {
                try  // Try Catch For Incase The Tag Somehow Manages To Mess Up -- Improved For You <3
                {
                    s_textCount = plateText.Contains("<color=") ? plateText.Length - (Regex.Matches(plateText, "<color=").Count != 1 ? Regex.Matches(plateText, "<color=").Count * 23 - 3 : 20) : plateText.Length;
                    s_MainPlateHolder = GameObject.Instantiate(s_namePlate, GameObject.Find("/" + uid + "[NamePlate]/Canvas").transform);
                    s_MainPlateHolder.transform.localPosition = new Vector3(0, 0.4f - (multiplier) * 0.0778f, 0);
                    s_MainPlateHolder.name = "NekoNameTagsCVR-NamePlate"; // why were you not naming the object to be able to find it if needed?
                    s_MainPlateHolder.layer = 69; // ;)
                    s_imageHolder = s_MainPlateHolder.transform.Find("Image").gameObject;
                    s_imageHolder.GetComponent<UnityEngine.UI.Image>().color = color;
                    try // This Is Just Here Incase The Paths of Objects To Destroy Change (A Try Catch In A Try Catch May Seem Repetitive & Stupid, But It's For The Sake Of Knowing Whats Wrong Easily)
                    {
                        GameObject.Destroy(s_MainPlateHolder.transform.Find("Disable with Menu").gameObject);
                        GameObject.Destroy(s_MainPlateHolder.transform.Find("Image").gameObject);
                    }
                    catch { MelonLogger.Msg(ConsoleColor.DarkRed, $"Failed To Destroy One Or More Objects On Created NekoNameTagsCVR-Nameplate ({uid})"); }
                    s_MainPlateHolder.transform.localScale = new Vector3(0.3f, 0.3f, 1);
                    s_imageHolder.transform.localScale = new Vector3(1, 0.5f, 1);

                    // Calculate the desired sizeDelta for s_imageHolder based on text character count
                    int maxChars = 1000; // Maximum number of characters you want to consider
                    float charWidth = 100f; // Example average width per character (adjust according to your text size)
                    float desiredWidth = Mathf.Min(s_textCount * charWidth, maxChars * charWidth); // Limit to maxChars


                    s_imageHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(desiredWidth / 10, 0.5f);
                    s_textMeshProGmj = s_MainPlateHolder.transform.Find("TMP:Username").gameObject;
                    s_textMeshProGmj.transform.localScale = new Vector3(0.58f, 0.58f, 1);
                    s_textMeshProGmj.transform.localPosition = Vector3.zero;
                    s_textMeshProGmj.gameObject.GetComponent<UnityEngine.RectTransform>().anchoredPosition = new Vector2(-0.05f, 0f);
                    var tmpc = s_textMeshProGmj.GetComponent<TMPro.TextMeshProUGUI>(); // why make life more diffucult and not just do this?
                    tmpc.text = plateText;
                    tmpc.alignment = TMPro.TextAlignmentOptions.Center;
                    tmpc.autoSizeTextContainer = true;
                    tmpc.enableCulling = true;
                    tmpc.material.enableInstancing = true;
                    tmpc.isOverlay = isOverlay;

                    
                    // Done Just For Removing The Text Under Devs/Mods - Doesn't Effect Being Able To See Who Is A Dev/Mod ect. (Done For Personal Preference To Make Things Cleaner)
                    s_dev = GameObject.Find("/" + uid + "[NamePlate]/Canvas/Content/Disable with Menu").gameObject.GetComponent<RectTransform>().gameObject;
                    s_dev.transform.gameObject.SetActive(false);
                    if (plateText.Contains("#rainbow#"))
                    {
                        string rainbowText = plateText.Replace("#rainbow#", "");
                        await Animations.DisplayRainbowText(rainbowText, s_MainPlateHolder, tmpc); // Call the rainbow text animation method
                    } else if (plateText.Contains("#animationtag#"))
                    {
                        string animatedText = plateText.Replace("#animationtag#", "");
                        await Animations.DisplayAnimatedText(animatedText, tmpc); // Call the animation text method
                    }
                }
                catch // why not have it tell us where something went wrong instead of just basically catching it each time it fails
                {
                    if (uid != null)
                    {
                        MelonLogger.Msg(ConsoleColor.DarkRed, $"Failed To Create Nameplate On {uid}");
                    }
                    else
                    {
                        MelonLogger.Msg(ConsoleColor.DarkRed, $"Failed To Create Nameplate");
                    }
                }
            }
        }

        // Duplicated GeneratePlate And Changed/Added A Bit Because I Was Lazy And Wanted A Specific Spot For Big Text -- You're just like me fr stop being lazy tho :p
        private static void GenerateBigPlate(string uid, string plateText, int multiplier)
        {
            if (plateText == null) { return; } // again why make the plate if it has no text to start with?
            else if (uid == null) { return; } // again this one isnt really needed tbh but personally I like to check to prevent code that shouldn't be running to run
            else
            {
                try  // Try Catch For Incase The Tag Somehow Manages To Mess Up -- Improved For You <3
                {
                    s_BigPlateHolder = GameObject.Instantiate(s_namePlate, GameObject.Find("/" + uid + "[NamePlate]/Canvas").transform);
                    s_BigPlateHolder.name = "NekoNameTagsCVR-BigNamePlate"; // again why were you not naming the object to be able to find it if needed?
                    s_BigPlateHolder.layer = 69; // ;)
                    string[] splited = plateText.Split(new string[] { "<size=" }, StringSplitOptions.None);
                    string sizeString = string.Empty;
                    for (int i = 0; i < splited[1].Length; i++)
                    {
                        if (!char.IsDigit(splited[1][i])) break;
                        sizeString += splited[1][i];
                    }
                    // Moves Big Text Based On Weather Or Not ChatBox Is Loaded -- Idk Why You Were Checking For A Deprecated Mod
                    s_BigPlateHolder.transform.localPosition = new Vector3(0, 0.45f + (int.Parse(sizeString)) * 0.0075f, 0);
                    try // This Is Just Here Incase The Paths of Objects To Destroy Change (A Try Catch In A Try Catch May Seem Repetitive & Stupid, But It's For The Sake Of Knowing Whats Wrong Easily)
                    {
                        GameObject.Destroy(s_BigPlateHolder.transform.Find("Disable with Menu").gameObject);
                        GameObject.Destroy(s_BigPlateHolder.transform.Find("Image").gameObject);
                    }
                    catch { MelonLogger.Msg(ConsoleColor.DarkRed, $"Failed To Destroy One Or More Objects On Created FewTags-BigNameplate ({uid})"); }
                    s_textMeshProGmj2 = s_BigPlateHolder.transform.Find("TMP:Username").gameObject;
                    s_textMeshProGmj2.transform.localPosition = Vector3.zero;
                    s_textMeshProGmj2.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                    var bptmpc = s_textMeshProGmj2.GetComponent<TMPro.TextMeshProUGUI>(); // again why make life more diffucult and not just do this?
                    bptmpc.text = plateText;
                    bptmpc.autoSizeTextContainer = true;
                    bptmpc.color = new Color(r, g, b, 0.55f);
                    bptmpc.enableCulling = true;
                    bptmpc.material.enableInstancing = true;
                    bptmpc.isOverlay = isOverlay;
                }
                catch // again why not have it tell us where something went wrong instead of just basically catching it each time it fails
                {
                    if (uid != null)
                    {
                        MelonLogger.Msg(ConsoleColor.DarkRed, $"Failed To Create BigNameplate On {uid}");
                    }
                    else
                    {
                        MelonLogger.Msg(ConsoleColor.DarkRed, $"Failed To Create BigNameplate");
                    }
                }
            }
        }

        // At Some Point Will Make This Show Up For Each Person Running The Mod If I Figure Out How To Since I Don't Have A Server Nor Do I Plan On Using One // Small Update On This, Probably Not, Or Atleast Not Anytime Soon
        public static void CreateLogo(string uid)
        {
            s_plateTransform = GameObject.Find("/" + uid + "[NamePlate]/Canvas").transform;
            s_Logo = GameObject.Instantiate(s_plateTransform.transform.Find("Content/TMP:Username").gameObject, s_plateTransform.transform.Find("Content").transform).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
            s_Logo.text = "<b><i><color=#00FFFF>NNT</color>";
            s_Logo.outlineWidth = 0.23f;
            s_Logo.outlineColor = new Color32(0, 0, 0, 255);
            s_Logo.autoSizeTextContainer = true;
            s_Logo.enableAutoSizing = false;
            s_Logo.fontSize = 0.19f;
            s_Logo.transform.localPosition = new Vector3(-1.35f, -0.45f);
            GameObject.Find("/" + uid + "[NamePlate]/Canvas").transform.localScale = new Vector3(0.45f, 0.45f, 1);
        }

        // Downloads The String Of The Json Aka Tags
        private static void DownloadString()
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    _userArr = JsonConvert.DeserializeObject<List<Json.User>>(wc.DownloadString("https://nekont.nekosunevr.co.uk/v1/nametags/chilloutvr/NekoNametagsCVR/assets/VitalityPlates"));
                    MelonLogger.Msg(ConsoleColor.Green, "Downloaded Tags");
                }
            }
            catch { MelonLogger.Msg(ConsoleColor.DarkRed, "Error Downloading Tags (Likely A API Issue or A Internet/Service Issue)"); } // see like this you gave a reason for a possible issue. you should do that way more often when doing try catch

        }

        // Will Rejoin The Current World You Are In For You
        public static void RejoinWorld()
        {
            var id = MetaPort.Instance.CurrentWorldId.ToString() + ":" + MetaPort.Instance.CurrentInstanceId.ToString();
            string[] parts = id.Split(new[] { ":" }, StringSplitOptions.None);
            string instanceid = parts[1].ToString();
            string worldid = parts[0].ToString();
            try
            {
                Instances.GetInstanceJoinTokenTask(instanceid, worldid);
            }
            catch
            {
                MelonLogger.Msg(System.ConsoleColor.Red, "Failed To Join World: " + worldid + ":" + instanceid);
            }
        }
    }
}
