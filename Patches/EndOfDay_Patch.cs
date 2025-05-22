using HarmonyLib;
using Kitchen;
using KitchenData;

namespace KitchenApplianceShop.Patches
{
    [HarmonyPatch(typeof(EndOfDayPopupView))]
    public static class EndOfDayPopupViewPatch 
    {
        //238041352 bookingdesk
        [HarmonyPatch("AddRow")]
        [HarmonyPrefix]
        public static void AddRow(ref string text, ref int value, bool is_sum_row = false)
        {
            if (text.Equals(Main.DisabledEntityName))
            {
                text = "Appliance Orders";
            }

            //switch (text)
            //{
            //    case "Booking Desk":
            //        value = (int)(value * Main.PrefManager.Get<float>(Main.REWARDS_MULTIPLIER));                    
            //        break;
            //}
        }

        [HarmonyPatch("FirstUpdate")]
        [HarmonyPrefix]
        public static void FirstUpdate(EndOfDayPopupView.ViewData view_data)
        {            
            for (int index = 0; index < view_data.Identifiers.Count; ++index)
            {
                int identifier = view_data.Identifiers[index];
                int amount = view_data.Amounts[index];
                GameDataObject output;
                GameData.Main.TryGet<GameDataObject>(identifier, out output);
                switch (output)
                {
                    case Dish dish:
                        break;
                    case Appliance appliance:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
