using KitchenLib.Customs;
using KitchenData;

namespace KitchenApplianceShop
{
    class EndOfDayTrackerAppliance : CustomAppliance
    {
        public override string UniqueNameID => "ApplianceShop-EoDMoneyTracker";

        public static int ApplianceId;

        public override void OnRegister(GameDataObject gameDataObject)
        {
            ((Appliance)gameDataObject).Name = Main.MOD_NAME;
            ((Appliance)gameDataObject).IsPurchasable = false;
            ApplianceId = gameDataObject.ID;
        }
    }
}
