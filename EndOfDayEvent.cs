using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenApplianceShop
{
    [UpdateBefore(typeof(DestroyAppliancesAtDay))]
    public class EndOfDayEvent : StartOfDaySystem, IModSystem
    {
        protected override void Initialise()
        {
            base.Initialise();
        }

        protected override void OnUpdate()
        {
            MoneyTracker.AddEvent(new EntityContext(this.EntityManager), Main.DisabledEntityID, -1* Main.AppliancePurchases);
            if (Main.AppliancePurchases > 0) Main.purchaseNumber++;
            Main.AppliancePurchases = 0;            
            
            Main.PickNewSaleAppliance();

            SDay day = GetSingleton<SDay>();
            if (day.Day == 15 && !Main.isComplete)
            {
                if (!System.Enum.TryParse(Main.PrefManager.Get<string>(Main.SPAWN_AT_ID), out SpawnPositionType positionType))
                {
                    positionType = default;
                }
                SpawnRequestSystem.Request<KitchenData.Appliance>(Main.saleApplianceID, positionType, 0, SpawnApplianceMode.Parcel, true);
                KitchenLib.UI.GenericPopupManager.CreatePopup("Appliance Shop", "Thanks for playing! Enjoy the day's sale item on us!");
                Main.isComplete = true;
            }

            Debug.Log($"LogLevel set to {Main.PrefManager.Get<int>(Main.DEVELOPER_LOGGING_LEVEL)} from preferences");
            Main.logLevel = Main.PrefManager.Get<int>(Main.DEVELOPER_LOGGING_LEVEL);
        }
    }
}
