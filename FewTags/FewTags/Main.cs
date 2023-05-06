using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MelonLoader;
using Newtonsoft.Json;
using System.IO;
using NEKONameTagsCVR.Utils;
using System.Net;
using ABI_RC.Core.Player;
using ABI_RC.Core.UI;
using ABI_RC.Core;
using Harmony;
using UnityEngine;
using System.Collections;
using ABI_RC.Core.Base.Jobs;
using UnityEngine.UI;
using System.Reflection;
using System.Security.Cryptography;
using System.Timers;

//Thanks To Edward7 For The Original Base

namespace NEKONameTagsCVR
{
    public class Main : MelonMod
    {
        public static bool CustomNameplatesLoaded { get; private set; }

        private static float a;
        private static float b;
        private static float g;
        private static float r;

        private static List<Json.User> _userArr { get; set; }
        private static GameObject s_namePlate { get; set; }
        private static GameObject s_dev { get; set; }
        private static GameObject s_MainPlateHolder { get; set; }
        private static GameObject s_BigPlateHolder { get; set; }
        private static GameObject s_textMeshProGmj { get; set; }
        private static GameObject s_textMeshProGmj2 { get; set; }
        private static Transform s_plateTransform { get; set; }
        private static TMPro.TextMeshProUGUI s_Logo { get; set; }

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
            MelonLogger.Msg(@"                   NEKONameTagsCVR is Nametag MOD (OLD VRC MOD)- By NekoSuneVR, Bison, Aries                 ");
            MelonLogger.Msg(@"=============================================================================================================");
            Console.ForegroundColor = System.ConsoleColor.White;
        }

        [Obsolete]
        public override void OnApplicationStart()
        { 
            ShowLogo();
            //NocturnalTagsLoaded = MelonHandler.Mods.Any(m => m.Info.Name == "Nocturnal Plates");
            MelonLogger.Msg("Initializing.");
            MelonLogger.Msg("NEKONameTagsCVR Loaded. Press Slash To Reload Tags");
            DownloadString();
            _hInstance.Patch(typeof(PlayerNameplate).GetMethod(nameof(PlayerNameplate.UpdateNamePlate)), null, typeof(Main).GetMethod(nameof(OnPlayerJoin),System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).ToNewHarmonyMethod());
            MelonCoroutines.Start(WaitForNamePlate());

        }

        private static IEnumerator WaitForNamePlate()
        {
            while (Resources.FindObjectsOfTypeAll<PuppetMaster>() == null)
                yield return null;
            s_namePlate = Resources.FindObjectsOfTypeAll<PuppetMaster>().FirstOrDefault(x => x.name == "_NetworkedPlayerObject").transform.Find("[NamePlate]/Canvas/Content").gameObject;
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Slash))
            {
                ReloadString();
                MelonLogger.Msg("Reloaded Tags, Please Rejoin World.");
                CohtmlHud.Instance.ViewDropText("NEKONameTagsCVR", "Connected", "Connected To NEKONameTagsCVR");
            }
        }

        private void ReloadString()
        {
            _userArr.Clear();
            DownloadString();
        }

        private static string s_uId { get; set; }
        private static Json.User s_user { get; set; }

        /*private static void OnTimedEvent(PlayerNameplate __instance, ElapsedEventArgs e)
        {

            _userArr.Clear();
            DownloadString();

            s_uId = __instance.transform.parent.name;
            s_user = _userArr.FirstOrDefault(x => x.UserId == s_uId);
            if (s_user == null) return;
            for (int i = 0; i < s_user.NamePlatesText.Length; i++)
                GeneratePlate(s_uId, s_user.NamePlatesText[i], i, new Color32(byte.Parse(s_user.Color[0].ToString()), byte.Parse(s_user.Color[1].ToString()), byte.Parse(s_user.Color[2].ToString()), byte.Parse(s_user.Color[3].ToString())));
            //CreateLogo(s_uId);
        }*/

        private static void OnPlayerJoin(PlayerNameplate __instance)
        {
            if (__instance.player.ownerId == "c270aa22-6705-9859-3c8f-a542d16e7c3b" || __instance.player.ownerId == "996f7dee-09a6-11db-9d55-34c451a46b05" || __instance.player.ownerId == "fce3b701-5560-8ec4-38f8-5382be7799ea")
            {
                CohtmlHud.Instance.ViewDropText("NEKONameTagsCVR", "ALERT", "NEKONameTagCVR STAFF: " + __instance.player.userName + " Has Joined Your Session!");
            }

            s_uId = __instance.transform.parent.name;

            DeletePlate(s_uId);
            _userArr.Clear();
            DownloadString();

            s_user = _userArr.FirstOrDefault(x => x.UserId == s_uId);
            if (s_user == null) return;
            for (int i = 0; i < s_user.NamePlatesText.Length; i++)
                GeneratePlate(s_uId, s_user.NamePlatesText[i], i, new Color32(byte.Parse(s_user.Color[0].ToString()), byte.Parse(s_user.Color[1].ToString()), byte.Parse(s_user.Color[2].ToString()), byte.Parse(s_user.Color[3].ToString())));
            //for (int i = 0; i < s_user.BigPlatesText.Length; i++)
            //    GenerateBigPlate(s_uId, s_user.BigPlatesText[i], i);
            //CreateLogo(s_uId);
        }

        private static void OnPlayerLeave(PlayerNameplate __instance)
        {
            s_uId = __instance.transform.parent.name;

            DeletePlate(s_uId);
            _userArr.Clear();
            DownloadString();

            s_user = _userArr.FirstOrDefault(x => x.UserId == s_uId);
            if (s_user == null) return;
            for (int i = 0; i < s_user.NamePlatesText.Length; i++)
                GeneratePlate(s_uId, s_user.NamePlatesText[i], i, new Color32(byte.Parse(s_user.Color[0].ToString()), byte.Parse(s_user.Color[1].ToString()), byte.Parse(s_user.Color[2].ToString()), byte.Parse(s_user.Color[3].ToString())));
            //for (int i = 0; i < s_user.BigPlatesText.Length; i++)
            //    GenerateBigPlate(s_uId, s_user.BigPlatesText[i], i);
            //CreateLogo(s_uId);
        }

        private static float s_textCount { get; set; }
        private static GameObject s_imageHolder { get; set; }

        private static void DeletePlate(string uid)
        {
            try
            {
                s_MainPlateHolder = GameObject.Instantiate(s_namePlate, GameObject.Find("/" + uid + "[NamePlate]/Canvas").transform);
                GameObject.Destroy(s_MainPlateHolder.transform.Find("Image").gameObject);
                GameObject.Destroy(s_MainPlateHolder.transform.Find("Image/FriendsIndicator").gameObject);
                GameObject.Destroy(s_MainPlateHolder.transform.Find("Image/ObjectMaskSlave").gameObject);
                GameObject.Destroy(s_MainPlateHolder.transform.Find("Disable with Menu").gameObject);
                GameObject.Destroy(s_textMeshProGmj = s_MainPlateHolder.transform.Find("TMP:Username").gameObject);
            }
            catch { }
        }

        private static void GeneratePlate(string uid, string plateText, int multiplier,Color32 color)
        {
            // This Was Used For Testing Mainly To Check Lengths Of Things (Sorta Math Related I Guess)
            // MelonLogger.Msg("---PlateText---");
            // MelonLogger.Msg(plateText);
            // MelonLogger.Msg("---PlateText Length---");
            // MelonLogger.Msg(plateText.Length);

            try  // Try Catch For Incase The Tag Somehow Manages To Mess Up
            {
                s_textCount = plateText.Contains("<color=") ? plateText.Length - (Regex.Matches(plateText, "<color=").Count != 1 ? Regex.Matches(plateText, "<color=").Count * 23 - 3 : 20) : plateText.Length;
                s_MainPlateHolder = GameObject.Instantiate(s_namePlate, GameObject.Find("/" + uid + "[NamePlate]/Canvas").transform);
                s_MainPlateHolder.transform.localPosition = new Vector3(0, -0.155f - (multiplier) * 0.0778f, 0);
                s_imageHolder = s_MainPlateHolder.transform.Find("Image").gameObject;
                s_imageHolder.GetComponent<UnityEngine.UI.Image>().color = color;
                GameObject.Destroy(s_MainPlateHolder.transform.Find("Image/FriendsIndicator").gameObject);
                GameObject.Destroy(s_MainPlateHolder.transform.Find("Image/ObjectMaskSlave").gameObject);
                GameObject.Destroy(s_MainPlateHolder.transform.Find("Disable with Menu").gameObject);
                s_MainPlateHolder.transform.localScale = new Vector3(0.3f, 0.3f, 1);
                s_imageHolder.transform.localScale = new Vector3(1, 0.5f, 1);
                s_imageHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(s_textCount / 10, 0.5f);
                s_textMeshProGmj = s_MainPlateHolder.transform.Find("TMP:Username").gameObject;
                s_textMeshProGmj.transform.localScale = new Vector3(0.58f, 0.58f, 1);
                s_textMeshProGmj.transform.localPosition = Vector3.zero;
                s_textMeshProGmj.GetComponent<TMPro.TextMeshProUGUI>().text = plateText;
                s_textMeshProGmj.GetComponent<TMPro.TextMeshProUGUI>().alignment = TMPro.TextAlignmentOptions.Center;
                s_textMeshProGmj.GetComponent<TMPro.TextMeshProUGUI>().autoSizeTextContainer = true;
                s_textMeshProGmj.gameObject.GetComponent<UnityEngine.RectTransform>().anchoredPosition = new Vector2(-0.05f, 0f);


                // Done Just For Removing The Text Under Devs/Mods - Doesn't Effect Being Able To See Who Is A Dev/Mod ect. (Done For Personal Preference To Make Things Cleaner)
                //s_dev = GameObject.Find("/" + uid + "[NamePlate]/Canvas/Content/Disable with Menu").gameObject.GetComponent<RectTransform>().gameObject;
                //s_dev.transform.gameObject.SetActive(false);
            }
            catch { }
        }

        //Just Gonna Duplicate It For Big Text Because Im Lazy Asf
        private static void GenerateBigPlate(string uid, string plateText, int multiplier)
        {
            try
            {
                s_BigPlateHolder = GameObject.Instantiate(s_namePlate, GameObject.Find("/" + uid + "[NamePlate]/Canvas").transform);
                string[] splited = plateText.Split(new string[] { "<size=" }, StringSplitOptions.None);
                string sizeString = string.Empty;
                for (int i = 0; i < splited[1].Length; i++)
                {
                    if (!char.IsDigit(splited[1][i])) break;
                    sizeString += splited[1][i];
                }
                s_BigPlateHolder.transform.localPosition = CustomNameplatesLoaded ? new Vector3(0, 0.758f + (int.Parse(sizeString)) * 0.0075f, 0) : new Vector3(0, 0.45f + (int.Parse(sizeString)) * 0.0035f, 0);
                GameObject.Destroy(s_BigPlateHolder.transform.Find("Image").gameObject.GetComponent<UnityEngine.UI.Image>());
                GameObject.Destroy(s_BigPlateHolder.transform.Find("Image/FriendsIndicator").gameObject);
                GameObject.Destroy(s_BigPlateHolder.transform.Find("Image/ObjectMaskSlave").gameObject);
                GameObject.Destroy(s_BigPlateHolder.transform.Find("Disable with Menu").gameObject);
                s_textMeshProGmj2 = s_BigPlateHolder.transform.Find("TMP:Username").gameObject;
                s_textMeshProGmj2.transform.localPosition = Vector3.zero;
                s_textMeshProGmj2.GetComponent<TMPro.TextMeshProUGUI>().text = plateText;
                s_textMeshProGmj2.GetComponent<TMPro.TextMeshProUGUI>().autoSizeTextContainer = true;
                s_textMeshProGmj2.GetComponent<TMPro.TextMeshProUGUI>().color = new Color(r, g, b, 0.55f);
                s_textMeshProGmj2.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
            }
            catch { }
        }

        //At Some Point Will Make This Show Up For Each Person Running The Mod If I Figure Out How To Since I Don't Have A Server
        public static void CreateLogo(string uid)
        {
            s_plateTransform = GameObject.Find("/" + uid + "[NamePlate]/Canvas").transform;
            s_Logo = GameObject.Instantiate(s_plateTransform.transform.Find("Content/TMP:Username").gameObject, s_plateTransform.transform.Find("Content").transform).gameObject.GetComponent<TMPro.TextMeshProUGUI>();
            s_Logo.text = "<b><i><color=#00FFFF>NT</color>";
            s_Logo.outlineWidth = 0.23f;
            s_Logo.outlineColor = new Color32(0, 0, 0, 255);
            s_Logo.autoSizeTextContainer = true;
            s_Logo.enableAutoSizing = false;
            s_Logo.fontSize = 0.19f;
            s_Logo.transform.localPosition = new Vector3(-1.35f, -0.45f);
            GameObject.Find("/" + uid + "[NamePlate]/Canvas").transform.localScale = new Vector3(0.45f, 0.45f, 1);
        }

        private static void DownloadString()
        {
            using (WebClient wc = new WebClient())
                _userArr = JsonConvert.DeserializeObject<List<Json.User>>(wc.DownloadString("https://api.nekosunevr.co.uk/v3/games/api/chilloutvrclient/NEKONameTagsCVR/assets/NameTags"));
        }

        internal class TagRainbow : MonoBehaviour
        {
            public string _Text { get; set; }
            public string _CurentText { get; set; }
            public int _TextPoz { get; set; }
            public bool _Deacreasing { get; set; }
            public List<string> colors = new List<string>();
            public int numColors = 255;


            public TMPro.TextMeshProUGUI _TextMeshPro;

            void Start()
            {
                var random = new System.Random();
                for (int i = 0; i < numColors; i++)
                {
                    colors.Add(String.Format("#{0:X6}", random.Next(0x1000000)));
                }

                _TextMeshPro = this.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                InvokeRepeating(nameof(updatehud), -1, 0.5f);
            }

            void updatehud()
            {
                _CurentText = "";
                var random = new System.Random();
                foreach (char c in _Text)
                {
                    _CurentText += String.Format("<color={0}>{1}</color>", colors[random.Next(numColors)], c.ToString());
                }

                _TextMeshPro.text = _CurentText;
            }
        }

        internal class TagAnimation : MonoBehaviour
        {
            public string _Color { get; set; }
            public string _Text { get; set; }
            public string _CurentText { get; set; }
            public int _TextPoz { get; set; }
            public bool _Deacreasing { get; set; }

            public TMPro.TextMeshProUGUI _TextMeshPro;

            private void Start()
            {
                _Deacreasing = false;
                _TextPoz = 0;
                _Color = "";
                if (_Text.StartsWith("<color="))
                {
                    _Color = _Text.Substring(0, 15) + " ";
                    _Text = _Text.Substring(15).Trim();
                }
                _TextMeshPro = this.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                InvokeRepeating(nameof(updatehud), -1, 0.5f);
            }

            private void updatehud()
            {
                if (_TextPoz >= _Text.Length)
                    _Deacreasing = true;
                else if (!_Deacreasing)
                {
                    _CurentText = _CurentText + _Text[_TextPoz];
                    _TextPoz += 1;
                }
                if (_Deacreasing)
                {
                    _CurentText = _CurentText.Substring(0, _CurentText.Length - 1);
                    _TextPoz -= 1;

                    if (_TextPoz == 0)
                        _Deacreasing = false;
                }

                if (_CurentText.Length == 0)
                {
                    _TextMeshPro.text = "";
                    return;
                }

                _TextMeshPro.text = _Color + _CurentText;
            }
        }

    }
}
