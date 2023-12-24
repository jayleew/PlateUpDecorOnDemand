using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitchen;
using KitchenData;
using KitchenLib.References;
using KitchenLib.Utils;

namespace KitchenDecorOnDemand
{
    public class ApplianceShopSystem : GameSystemBase
    {
        public static bool GoCheat = false;
        public static int SelectedApplianceCost = 0;
        private static readonly int[] applianceIds = {            
            ApplianceReferences.Bin,
            ApplianceReferences.BlueprintCabinet,
            ApplianceReferences.BlueprintUpgradeDesk,
            ApplianceReferences.CoffeeTable,
            ApplianceReferences.Combiner,
            ApplianceReferences.Countertop,
            ApplianceReferences.DirtyPlateStack,
            ApplianceReferences.Dumbwaiter,
            ApplianceReferences.ExtraLife,
            ApplianceReferences.FloorBufferStation,
            ApplianceReferences.FloorProtector,
            ApplianceReferences.FoodDisplayStand,
            ApplianceReferences.GasLimiter,
            ApplianceReferences.GasSafetyOverride,
            ApplianceReferences.Hob,
            ApplianceReferences.HostStand,
            ApplianceReferences.Mixer,
            ApplianceReferences.MopBucket,
            ApplianceReferences.OrderingTerminal,
            ApplianceReferences.Oven,
            ApplianceReferences.PlateStack,
            ApplianceReferences.Portioner,
            ApplianceReferences.PrepStation,
            ApplianceReferences.RobotMop,
            ApplianceReferences.SinkNormal,
            ApplianceReferences.TableLarge,
            ApplianceReferences.UpgradeKit,
        };


        private static readonly int[] coffeeAppliances =
        {
            ApplianceReferences.CoffeeMachine,
            ApplianceReferences.IceDispenser,
            ApplianceReferences.MilkDispenser,
            ApplianceReferences.SourceCakeStand,            
        };

        private static readonly int[] tools =
        {
            -2070005162, // Clipboard Stand.
            ApplianceReferences.FireExtinguisherHolder,
            ApplianceReferences.RollingPinProvider,
            ApplianceReferences.ScrubbingBrushProvider,
            ApplianceReferences.SharpKnifeProvider,
            ApplianceReferences.ShoeRackTrainers,
            ApplianceReferences.ShoeRackWellies,
            ApplianceReferences.ShoeRackWorkBoots,
            ApplianceReferences.TrayStand,
        };

        private static readonly int[] consumables =
        {
            ApplianceReferences.BreadstickBox,
            ApplianceReferences.CandleBox,
            ApplianceReferences.FlowerPot,
            ApplianceReferences.NapkinBox,
            ApplianceReferences.SharpCutlery,
            ApplianceReferences.SpecialsMenuBox,
            ApplianceReferences.SupplyCabinet,
        };

        private static readonly int[] ingredients =
        {
            ApplianceReferences.SourceApple,
            ApplianceReferences.SourceBamboo,
            ApplianceReferences.SourceBananas,
            ApplianceReferences.SourceBeans,
            ApplianceReferences.SourceBonedMeat,
            ApplianceReferences.SourceBroccoli,
            ApplianceReferences.SourceBurgerBuns,
            ApplianceReferences.SourceBurgerPatty,
            ApplianceReferences.SourceCarrot,
            ApplianceReferences.SourceCheese,
            ApplianceReferences.SourceCherry,
            ApplianceReferences.SourceChocolate,
            ApplianceReferences.SourceCorn,
            ApplianceReferences.SourceCranberry,
            ApplianceReferences.SourceEgg,
            ApplianceReferences.SourceFlour,
            ApplianceReferences.SourceHotdog,
            ApplianceReferences.SourceHotdogBun,
            ApplianceReferences.SourceLemon,
            ApplianceReferences.SourceLettuce,
            ApplianceReferences.SourceMeat,
            ApplianceReferences.SourceMilk,
            ApplianceReferences.SourceMushroom,
            ApplianceReferences.SourceOlive,
            ApplianceReferences.SourceOnion,
            ApplianceReferences.SourcePotato,
            ApplianceReferences.SourcePumpkin,
            ApplianceReferences.SourceRice,
            ApplianceReferences.SourceSeaweed,
            ApplianceReferences.SourceStrawberries,
            ApplianceReferences.SourceSugar,
            ApplianceReferences.SourceTomato,
            ApplianceReferences.SourceTurkey,            
        };

        private static readonly int[] decorations =
        {
            ApplianceReferences.Painting,
            ApplianceReferences.Plant,
            ApplianceReferences.Rug,
        };

        private static readonly int[] bakingTrays =
        {
           -660310536, // Big Cake Tin
            -2135982034, // Brownie Tray
            -1723125645, // Cookie Tray
            -315287689, // Cupcake tray
            2136474391, // Doughnut Tray
        };

        private static readonly int[] cooking =
        {
            ApplianceReferences.PotStack,
            ApplianceReferences.ServingBoardStack,
            ApplianceReferences.WokStack,
        };

        private static readonly int[] magic = {
            -292467039, // Enchanting Desk
            782648278, // Cauldron
            -1992638820 , // Enchanted Broom
            540526865, // Enchanted Plates
            -1946127856, // Ghostly Clipboard
            1313278365, // Ghostly Knife
            689268680, // Ghostly Rolling Pin
            -560953757, // Ghost Scrubber
            -1780646993 , // Illusion Wall
            1150470926 , // Instant Wand
            2044081363 , // Levitation Line
            119166501 , // Levitation Station
            267288096 , // Magic Apple Tree
            744482650 , // Magic Mirror
            -1696198539 , // Magic Spring
            29164230 , // Pouch of Holding
            423254987 , // Preserving Station
            -1688921160 , // Table - Sharing Cauldron
            2000892639 , // Table - Stone
            1492264331 , // Vanishing Circle
        };

        protected override void OnUpdate()
        {
            CheatMoney();
            Main.LogInfo("Loading available appliances...");

            Main.LoadedAvailableAppliances.Clear();


            foreach (int applianceId in applianceIds)
            {
                Appliance appliance = (Appliance)GDOUtils.GetExistingGDO(applianceId);


                if (appliance == null)
                {
                    continue;
                }

                var variants = new Dictionary<int, string>();
                variants.Add(applianceId, appliance.Name);

                Main.LoadedAvailableAppliances.Add(appliance.Name, variants);


                foreach (var upgrade in appliance.Upgrades)
                {
                    Main.LogInfo($"{appliance.Name} - {upgrade.Name}");
                    // The deconstructor mod causes an issue because it loops the blueprint cabinet to a deconstructor and back
                    if (!variants.ContainsKey(upgrade.ID))
                    {
                        variants.Add(upgrade.ID, upgrade.Name);
                    }
                }
            }

            try
            {
                // Belts
                var belts = new Dictionary<int, string>();

                Appliance belt = (Appliance)GDOUtils.GetExistingGDO(ApplianceReferences.Belt);
                belts.Add(belt.ID, belt.Name);

                Main.LoadedAvailableAppliances.Add(belt.Name, belts);

                foreach (var upgrade in belt.Upgrades)
                {
                    Main.LogInfo($"{belt.Name} - {upgrade.Name}");
                    if (!belts.ContainsKey(upgrade.ID))
                    {
                        belts.Add(upgrade.ID, upgrade.Name);
                    }
                    var upgradeAppliance = (Appliance)GDOUtils.GetExistingGDO(upgrade.ID);

                    foreach (var nestedUpgrade in upgradeAppliance.Upgrades)
                    {
                        if (!belts.ContainsKey(nestedUpgrade.ID))
                        {
                            belts.Add(nestedUpgrade.ID, nestedUpgrade.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            Main.LoadedAvailableAppliances.Add("Baking", CreateApplianceDictionary(bakingTrays));
            Main.LoadedAvailableAppliances.Add("Coffee", CreateApplianceDictionary(coffeeAppliances));
            Main.LoadedAvailableAppliances.Add("Cooking", CreateApplianceDictionary(cooking));
            Main.LoadedAvailableAppliances.Add("Consumables", CreateApplianceDictionary(consumables));
            Main.LoadedAvailableAppliances.Add("Decorations", CreateApplianceDictionary(decorations));         
            Main.LoadedAvailableAppliances.Add("Magic", CreateApplianceDictionary(magic));
            Main.LoadedAvailableAppliances.Add("Tools", CreateApplianceDictionary(tools));
            Main.LoadedAvailableAppliances.Add("Ingredients", CreateApplianceDictionary(ingredients));

            Main.LogInfo("Found all appliances");
        }

        private static Dictionary<int, string> CreateApplianceDictionary(int[] applianceIds)
        {
            var appliances = new Dictionary<int, string>();

            try
            {
                foreach (var applianceId in applianceIds)
                {
                    Appliance appliance = (Appliance)GDOUtils.GetExistingGDO(applianceId);
                    if (appliance != null && !appliances.ContainsKey(appliance.ID))
                    {
                        appliances.Add(appliance.ID, appliance.Name.Replace("Source -", "").Replace("Provider", ""));
                    }
                }
            }
            catch { };

            return appliances;
        }

        private void CheatMoney()
        {
            if (HasSingleton<SMoney>() && GoCheat)
            {
                SMoney money = GetSingleton<SMoney>();
                if (money.Amount > 120) money.Amount = 120;
                else money.Amount = 1000;
                SetSingleton(money);
                GoCheat = false;
            }
        }
    }
}
