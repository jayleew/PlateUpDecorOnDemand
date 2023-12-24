using Kitchen;
using KitchenData;
using KitchenMods;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System.Linq;

namespace KitchenApplianceShop
{
    public enum SpawnType
    {
        Decor,
        Appliance
    }

    public enum SpawnPositionType
    {
        Door,
        Player
    }

    public abstract class SpawnHandlerSystemBase : GenericSystemBase
    {
        protected abstract Type GDOType { get; }
        protected virtual bool UseFallbackTile => false;
        EntityQuery Players;
        protected override void Initialise()
        {
            base.Initialise();
            Players = GetEntityQuery(typeof(CPlayer), typeof(CPosition));
        }

        internal static int GetKnownPrice(EntityManager entityManager, int applianceID)
        {
            var buyPrice = GameData.Main.Get<Appliance>(applianceID).PurchaseCost;

            // We look for CApplianceInfo with CImmovable
            // These are our price trackers
            // Why? Because this mod should easily be removable, if we use entities without views, they will never get one, easy!
            var query = entityManager.CreateEntityQuery(new QueryHelper().All(typeof(CApplianceInfo), typeof(CImmovable)).None(typeof(CLinkedView), typeof(CRequiresView)).Build());
            var appliances = query.ToComponentDataArray<CApplianceInfo>(Allocator.Temp);
            try
            {
                foreach (var appliance in appliances.Where(appliance => appliance.ID == applianceID))
                {
                    buyPrice = Math.Min(buyPrice, appliance.Price);
                }
            }
            finally
            {
                appliances.Dispose();
            }

            return buyPrice;
        }

        protected override void OnUpdate()
        {
            if (GDOType != null && !SpawnRequestSystem.IsHandled && GDOType == SpawnRequestSystem.Current?.GDO?.GetType())
            {
                if (HasSingleton<SMoney>())
                {                    
                    var appliancePrice = GetKnownPrice(EntityManager, SpawnRequestSystem.Current.GDO.ID);

                    SMoney money = GetSingleton<SMoney>();
                    int thisPurchaseCost = (int)(money.Amount * Main.PrefManager.Get<float>(Main.SHOP_SERVICEFEE_ID))
                        + 30
                        + (int)(appliancePrice * Main.PrefManager.Get<float>(Main.APPLIANCE_BLUEPRINT_COST_ID));                    
                    if (
                        money.Amount - thisPurchaseCost
                        < 0
                        //|| money.Amount < 250 
                        )
                    {
                        string description = string.Format("Too expensive.\nAppliance cost = {0}\nSales Tax = {1}\nTotal = {2}"
                            , (int)(appliancePrice * Main.PrefManager.Get<float>(Main.APPLIANCE_BLUEPRINT_COST_ID))
                            , (int)(money.Amount * Main.PrefManager.Get<float>(Main.SHOP_SERVICEFEE_ID) + 30)
                            , thisPurchaseCost);                        
                        ApplianceShopSystem.SelectedApplianceCost = thisPurchaseCost;
                        KitchenLib.UI.GenericPopupManager.CreatePopup("Appliance Shop", description);                        
                        SpawnRequestSystem.RequestHandled();                        
                        return;
                    }
                    money.Amount -= thisPurchaseCost;
                    if (money.Amount < 0)
                        money.Amount = 0;
                    Main.AppliancePurchases += thisPurchaseCost;
                    SetSingleton(money);
                }                

                Vector3 position = UseFallbackTile ? GetFallbackTile() : GetFrontDoor(get_external_tile: true);
                using NativeArray<CPlayer> players = Players.ToComponentDataArray<CPlayer>(Allocator.Temp);
                using NativeArray<CPosition> playerPositions = Players.ToComponentDataArray<CPosition>(Allocator.Temp);
                switch (SpawnRequestSystem.Current.PositionType)
                {
                    case SpawnPositionType.Player:
                        bool positionSet = false;
                        for (int i = 0; i < players.Length; i++)
                        {
                            CPlayer player = players[i];
                            CPosition playerPosition = playerPositions[i];
                            bool match = player.InputSource == SpawnRequestSystem.Current.InputIdentifier;
                            if (!positionSet || match)
                            {
                                positionSet = true;
                                position = playerPosition;
                            }
                            if (match)
                                break;
                        }
                        break;
                    case SpawnPositionType.Door:
                    default:
                        break;
                }
                SpawnRequestSystem.RequestHandled();
                Spawn(SpawnRequestSystem.Current.GDO, position, SpawnRequestSystem.Current.SpawnApplianceMode);
            }
        }
        protected abstract void Spawn(GameDataObject gameDataObject, Vector3 position, SpawnApplianceMode spawnApplianceMode);
    }

    public class SpawnRequest
    {
        public GameDataObject GDO;
        public SpawnPositionType PositionType;
        public int InputIdentifier;
        public SpawnApplianceMode SpawnApplianceMode;
    }

    public class SpawnRequestSystem : GenericSystemBase, IModSystem
    {
        static Queue<SpawnRequest> requests = new Queue<SpawnRequest>();
        public static SpawnRequest Current { get; private set; } = null;
        public static bool IsHandled { get; private set; } = false;
        protected override void OnUpdate()
        {
            if (requests.Count > 0)
            {
                IsHandled = false;
                Current = requests.Dequeue();
                
                return;
            }
            Current = null;
            IsHandled = true;
        }
        public static void Request<T>(int gdoID, SpawnPositionType positionType, int inputIdentifier = 0, SpawnApplianceMode spawnApplianceMode = default) where T : GameDataObject, new()
        {
            if (gdoID != 0 && GameInfo.CurrentScene == SceneType.Kitchen && GameData.Main.TryGet(gdoID, out T gdo, warn_if_fail: true))
            {
                requests.Enqueue(new SpawnRequest()
                {
                    GDO = gdo,
                    PositionType = positionType,
                    InputIdentifier = inputIdentifier,
                    SpawnApplianceMode = spawnApplianceMode
                });
            }
        }
        public static void RequestHandled()
        {
            IsHandled = true;
        }
    }
}
