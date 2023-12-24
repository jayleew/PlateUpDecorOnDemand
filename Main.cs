using HarmonyLib;
using Kitchen;
using KitchenLib;
using KitchenLib.Event;
using KitchenData;
using KitchenApplianceShop.Utils;
using KitchenMods;
using PreferenceSystem;
using PreferenceSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenApplianceShop
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = "jayleew.plateup.applianceshop";
        public const string MOD_NAME = "Appliance Shop";
        public const string MOD_VERSION = "0.2.9";

        internal const string MENU_START_OPEN_ID = "menuStartOpen";
        internal const string MENU_START_TAB_ID = "menuStartTab";
        internal const string HOST_ONLY_ID = "hostOnly2";
        internal const string APPLIANCE_SPAWN_AS_ID = "applianceSpawnAs";
        internal const string APPLIANCE_BLUEPRINT_COST_ID = "applianceBlueprintCost";
        internal const string SHOP_SERVICEFEE_ID = "shopServiceFee";
        internal const string SPAWN_AT_ID = "spawnAt";
        internal static PreferenceSystemManager PrefManager;

        internal static ViewType SpawnRequestViewType = (ViewType)Main.GetInt32HashCode("SpawnRequestViewType");

        Harmony harmony;
        static List<Assembly> PatchedAssemblies = new List<Assembly>();
        public static Dictionary<string, Dictionary<int, string>> LoadedAvailableAppliances = new();

        SpawnGUI _spawnGUI;

        public const int DisabledEntityID = 1836107598;
        public static string DisabledEntityName;
        internal static int AppliancePurchases; 
        public Main()
        {
            if (harmony == null)
                harmony = new Harmony(MOD_GUID);
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null && !PatchedAssemblies.Contains(assembly))
            {
                harmony.PatchAll(assembly);
                PatchedAssemblies.Add(assembly);
            }
        }

        public void PostActivate(KitchenMods.Mod mod)
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");            

            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);
            PrefManager
                .AddConditionalBlocker(() => Session.CurrentGameNetworkMode != GameNetworkMode.Host)
                    .AddLabel("Spawn At")
                    .AddOption<string>(
                        SPAWN_AT_ID,
                        SpawnPositionType.Door.ToString(),
                        Enum.GetNames(typeof(SpawnPositionType)),
                        Enum.GetNames(typeof(SpawnPositionType)))
                    .AddSpacer()
                    .AddSubmenu("Appliance", "appliance")
                        .AddLabel("Spawn As")
                        .AddOption<string>(
                            APPLIANCE_SPAWN_AS_ID,
                            SpawnApplianceMode.Blueprint.ToString(),
                            Enum.GetNames(typeof(SpawnApplianceMode)),
                            Enum.GetNames(typeof(SpawnApplianceMode)))
                        .AddLabel("Blueprint Cost")
                        .AddOption<float>(
                            APPLIANCE_BLUEPRINT_COST_ID,
                            0,
                            new float[] { 0, 0.5f, 1, 1.5f, 2 },
                            new string[] { "Free", "Half Price", "Original Price", "One&One Half", "Double Price" })
                        .AddSpacer()
                        .AddLabel("Shop Service Fee")
                        .AddOption<float>(
                            SHOP_SERVICEFEE_ID,
                            0,
                            new float[] { 0, 0.1f, 0.2f },
                            new string[] { "30", "30 + 10% Tax", "30 + 20% Tax" })                        
                        .AddSpacer()
                        .AddSpacer()
                    .SubmenuDone()
                    .AddSubmenu("Decor", "decor")
                        .AddButtonWithConfirm("Remove Applied Decor", "Strip applied wallpapers and flooring? This only works for the host.",
                            delegate(GenericChoiceDecision decision)
                            {
                                if (Session.CurrentGameNetworkMode == GameNetworkMode.Host && decision == GenericChoiceDecision.Accept)
                                {
                                    StripRequestSystem.Request();
                                }
                            })
                        .AddSpacer()
                        .AddSpacer()
                    .SubmenuDone()
                    .AddSpacer()
                .ConditionalBlockerDone()
                .AddSubmenu("Menu Settings", "menuSettings")
                    .AddConditionalBlocker(() => Session.CurrentGameNetworkMode != GameNetworkMode.Host)
                        .AddLabel("Can Spawn")
                        .AddOption<bool>(
                            HOST_ONLY_ID,
                            true,
                            new bool[] { false, true },
                            new string[] { "Everyone", "Only Host" })
                    .ConditionalBlockerDone()
                    .AddSpacer()
                    .AddSpacer()
                .SubmenuDone()
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.MainMenu);
            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }

        private void initPauseMenu()
        {                        
            ModsPreferencesMenu<PauseMenuAction>.RegisterMenu("Appliance Store", typeof(ApplianceMenu<PauseMenuAction>), typeof(PauseMenuAction));
            Events.PreferenceMenu_PauseMenu_CreateSubmenusEvent += (s, args) =>
            {
                args.Menus.Add(typeof(ApplianceMenu<PauseMenuAction>), new ApplianceMenu<PauseMenuAction>(args.Container, args.Module_list));
            };            
        }

        public void PreInject()
        {
            if (GameObject.FindObjectOfType<SpawnGUI>() == null)
            {
                GameObject gameObject = new GameObject(MOD_NAME);
                _spawnGUI = gameObject.AddComponent<SpawnGUI>();
                _spawnGUI.showMenu = PrefManager.Get<bool>(MENU_START_OPEN_ID);
            }
        }

        public static int GetInt32HashCode(string strText)
        {
            SHA1 hash = new SHA1CryptoServiceProvider();
            if (string.IsNullOrEmpty(strText))
            {
                return 0;
            }

            byte[] bytes = Encoding.Unicode.GetBytes(strText);
            byte[] value = hash.ComputeHash(bytes);
            uint num = BitConverter.ToUInt32(value, 0);
            uint num2 = BitConverter.ToUInt32(value, 8);
            uint num3 = BitConverter.ToUInt32(value, 16);
            uint num4 = num ^ num2 ^ num3;
            return BitConverter.ToInt32(BitConverter.GetBytes(uint.MaxValue - num4), 0);
        }

        public void PostInject() { DisabledEntityName = GameData.Main.Get<Appliance>(DisabledEntityID).Name; initPauseMenu(); }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }

    public class SpawnGUI : MonoBehaviour
    {
        private const int MAX_DUPLICATE_NAMES = 100;
        private const float WINDOW_WIDTH = 250f;
        private const float WINDOW_HEIGHT = 600f;

        private static Dictionary<string, int> decors = new Dictionary<string, int>();
        private static List<string> decorNames;
        private static Dictionary<string, int> appliances = new Dictionary<string, int>();
        private static List<string> applianceNames;

        public Rect windowRect = new Rect(10, 10, 250, 600);
        private Vector2 scrollPosition;
        private string searchText = string.Empty;
        private SpawnType currentMode = SpawnType.Decor;
        public bool showMenu = true;

        private string _hoveredName = null;
        Texture2D _hoveredTexture = null;

        //private SpawnRequestView spawnRequestView;

        private readonly HashSet<int> DISABLED_APPLIANCES = new HashSet<int>()
        {
            -349733673,
            1836107598,
            369884364,
            -699013948
        };

        private void Start()
        {
            if (Enum.TryParse(Main.PrefManager?.Get<string>(Main.MENU_START_TAB_ID), out SpawnType spawnType))
            {
                currentMode = spawnType;
            }
        }

        public void Update()
        {

        }

        private int? _windowID = null;
        public void OnGUI()
        {

        }

        public void SpawnWindow(int windowID)
        {
        }
    }
}
