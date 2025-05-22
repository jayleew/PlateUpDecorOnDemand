using HarmonyLib;
using Kitchen;

namespace KitchenApplianceShop.Patches
{
    [HarmonyPatch(typeof(DifficultyHelpers))]
    public static class DifficultyHelpersPatch
    {
        [HarmonyPatch("MoneyRewardPlayerModifier")]
        [HarmonyPostfix]
        public static void MoneyRewardPlayerModifier_Postfix(ref float __result, int player_count)
        {
            Main.numberOfPlayers = player_count;
            //adjust for num players (would like to adjust for recipes for balance instead)
            float rewardMultiplier = Main.PrefManager.Get<float>(Main.REWARDS_MULTIPLIER);
            if (Main.PrefManager.Get<bool>(Main.REWARDS_BOOST_PLAYERCOUNT)) rewardMultiplier += .12f * (4 - player_count);
            __result = rewardMultiplier;
        }
    }

    //[HarmonyPatch(typeof(MoneyTracker), "AddEvent")]
    //class MoneyTrackGetRecord
    //{
    //    public static bool Prefix(int identifier, int amount, MoneyTracker __instance)
    //    {
    //        if (amount != 0)
    //        {
    //            Main.LogInfo($"In money tracker add event. Adding {amount} for identifier {identifier}");
    //            if (identifier == KitchenLib.References.ApplianceReferences.BookingDesk)
    //            {
    //                Main.bookingDesk += amount;
    //            }
    //        }
    //        return true;
    //    }
    //}
}
