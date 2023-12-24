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
            Main.AppliancePurchases = 0;
        }
    }
}
