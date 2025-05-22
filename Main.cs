using HarmonyLib;
using Kitchen;
using KitchenLib;
using KitchenLib.UI.PlateUp.PreferenceMenus;
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
    public class Main : BaseMod, IModInitializer
    {
        public const string MOD_GUID = "jayleew.plateup.applianceshop";
        public const string MOD_NAME = "Appliance Shop";
        public const string MOD_VERSION = "0.2.10";
        public const string MOD_GAMEVERSION = "";

        internal const string MENU_START_OPEN_ID = "menuStartOpen";
        internal const string MENU_START_TAB_ID = "menuStartTab";
        internal const string HOST_ONLY_ID = "hostOnly2";
        internal const string APPLIANCE_SPAWN_AS_ID = "applianceSpawnAs";
        internal const string APPLIANCE_BLUEPRINT_COST_ID = "applianceBlueprintCost";
        internal const string SHOP_SERVICEFEE_ID = "shopServiceFee";
        internal const string SHOP_TAX_ID = "shopTaxRate";
        internal const string SPAWN_AT_ID = "spawnAt";
        internal const string REWARDS_MULTIPLIER = "rewardsMultiplier";
        internal const string REWARDS_BOOST_PLAYERCOUNT = "rewardsByPlayerCount";
        internal const string DEVELOPER_LOGGING_LEVEL = "developerLoggingLevel";

        internal static PreferenceSystemManager PrefManager;
        internal static readonly float salePrices = 0.5f;
        internal static readonly float[] blueprintCostMultiplier = new float[] { 0, 0.5f, 1, 1.5f, 2 };
        internal static readonly float[] rewardsMultiplierValues = new float[] { 1.25f, 1.5f, 1.75f, 2.0f };
        internal static readonly string[] rewardsMultiplerLabels = new string[] { "Vanilla", "2X", "3X", "4X" };

        internal static int purchaseNumber = 0;
        internal static int numberOfPlayers = 0;
        internal static bool isComplete = false;
        internal static int wishlistApplianceID = -1;
        internal static int bookingDesk;

        //hiding the base entitymanager just cause im dumb. don't know if it just needs to be instantiated
        internal static new Unity.Entities.EntityManager EntityManager { get; set; }

        internal static ViewType SpawnRequestViewType = (ViewType)Main.GetInt32HashCode("SpawnRequestViewType");

        Harmony harmony;
        static List<Assembly> PatchedAssemblies = new List<Assembly>();
        public static Dictionary<string, Dictionary<int, string>> LoadedAvailableAppliances = new();

        SpawnGUI _spawnGUI;

        public const int DisabledEntityID = 1836107598;
        public static string DisabledEntityName;
        internal static int AppliancePurchases;
        internal static int saleApplianceID = KitchenLib.References.ApplianceReferences.BlueprintCabinet;
        internal static string saleApplianceName = "Blueprint Cabinet";

        public Main() : base(MOD_GUID, MOD_NAME, "jayleew", MOD_VERSION, $"{MOD_GAMEVERSION}", Assembly.GetExecutingAssembly())
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

        public static void PickNewSaleAppliance()
        {
            //pick a new item on sale
            int categoryIndex = UnityEngine.Random.Range(0, Main.LoadedAvailableAppliances.Count);
            int i = 0;
            bool shouldChooseWishlist = UnityEngine.Random.Range(1, 13) < 5 && wishlistApplianceID != -1;

            if (shouldChooseWishlist) categoryIndex = -1;//don't choose a random category

            foreach (var category in Main.LoadedAvailableAppliances)
            {
                if (i == categoryIndex || shouldChooseWishlist && category.Value.ContainsKey(wishlistApplianceID))
                //then this category index is the one selected or contains a wishlisted appliance
                {
                    LogInfo($"Have I found a wishlist category? {category.Value.ContainsKey(wishlistApplianceID)}");
                    int itemIndex = UnityEngine.Random.Range(0, category.Value.Count);
                    LogInfo($"{itemIndex} was selected");
                    int j = 0;
                    foreach (var appliance in category.Value)
                    {
                        if (j == itemIndex)
                        {
                            LogInfo($"{appliance.Value} will be on sale today!");
                            saleApplianceID = appliance.Key;
                            saleApplianceName = appliance.Value;
                            if (shouldChooseWishlist) wishlistApplianceID = -1;
                            break;
                        }
                        j++;
                    }
                    break;
                }
                else i++;
            }
        }

        public void PostActivate(KitchenMods.Mod mod)
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");

            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);
            PrefManager
            #region Host Options
            #region Spawn At Location
                .AddConditionalBlocker(() => Session.CurrentGameNetworkMode != GameNetworkMode.Host)
                    .AddLabel("Spawn At")
                    .AddOption<string>(
                        SPAWN_AT_ID,
                        SpawnPositionType.Door.ToString(),
                        Enum.GetNames(typeof(SpawnPositionType)),
                        Enum.GetNames(typeof(SpawnPositionType)))
            #endregion
                    .AddSpacer()
            #region Appliance SubMenu
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
                            blueprintCostMultiplier,
                            new string[] { "Free", "Half Price", "Original Price", "One&One Half", "Double Price" })
                        .AddSpacer()
                        .AddLabel("Shop Service Fee")
                        .AddOption<float>(
                            SHOP_SERVICEFEE_ID,
                            0,
                            new float[] { 0, 10f, 20f, 30f },
                            new string[] { "No Fee", "10", "20", "30" })
                        .AddSpacer()
                        .AddOption<float>(
                            SHOP_TAX_ID,
                            0,
                            new float[] { 0, 0.1f, 0.2f },
                            new string[] { "No Tax", "10% Tax", "20% Tax" })
                        .AddSpacer()
                    .SubmenuDone()
            #endregion
            #region Difficulty Settings
                    .AddSubmenu("Difficulty Settings", "difficulty")
                    .AddLabel("Rewards Multiplier")
                    .AddOption<float>(REWARDS_MULTIPLIER, rewardsMultiplierValues[0], rewardsMultiplierValues, rewardsMultiplerLabels)
                    .AddSpacer()
                    .AddLabel("Boost for number of players")
                    .AddOption<bool>(REWARDS_BOOST_PLAYERCOUNT, false, new bool[] { true, false }, new string[] { "True", "False" })
                    .AddSpacer()
                    .SubmenuDone()
            #endregion
            #region Developer
                    .AddSubmenu("Developer Options", "developer")
                    .AddLabel("Logging (Update EOD)")
                    .AddOption<int>(DEVELOPER_LOGGING_LEVEL, 1, new int[] { 1, 2, 3 }, new string[] { "Errors Only", "Add Warnings", "Everything" })
                    .AddSpacer()
                    .SubmenuDone()
            #endregion
            #region Decor SubMenu
                    .AddSubmenu("Decor", "decor")
                        .AddButtonWithConfirm("Remove Applied Decor", "Strip applied wallpapers and flooring? This only works for the host.",
                            delegate (GenericChoiceDecision decision)
                            {
                                if (Session.CurrentGameNetworkMode == GameNetworkMode.Host && decision == GenericChoiceDecision.Accept)
                                {
                                    StripRequestSystem.Request();
                                }
                            })
                        .AddSpacer()
                        .AddSpacer()
                    .SubmenuDone()
            #endregion
                    .AddSpacer()
                .ConditionalBlockerDone()
            #endregion

            #region Menu Settings SubMenu
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
            #endregion
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.MainMenu);
            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
            AddGameDataObject<EndOfDayTrackerAppliance>();
            Debug.Log($"LogLevel set to {Main.PrefManager.Get<int>(Main.DEVELOPER_LOGGING_LEVEL)} from preferences");
            Main.logLevel = Main.PrefManager.Get<int>(Main.DEVELOPER_LOGGING_LEVEL);
#if DEBUG
            Log("DEBUG Binaries Installed.");
            logLevel = 3;
#endif


        }

        private void initPauseMenu()
        {
            PauseMenuPreferencesesMenu.RegisterMenu("Appliance Store", typeof(ApplianceMenu));
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
        public static void LogInfo(string _log) { if (logLevel > 2) Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { if (logLevel >= 2) Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }

        internal static int logLevel = 1;//errors only
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
