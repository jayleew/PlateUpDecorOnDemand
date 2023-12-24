using Kitchen;
using Kitchen.Modules;
using KitchenLib;
using KitchenLib.References;
using KitchenLib.Utils;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace KitchenDecorOnDemand
{
    public class ApplianceMenu<T> : KLMenu<T>
    {

        private Option<string> GroupSelector;
        private Option<bool> AutoRestockOption;
        private Option<bool> DestroyCratesOption;
        private int ApplianceId = ApplianceReferences.BlueprintCabinet;
        private string SelectedGroupName = "";

        public ApplianceMenu(Transform container, ModuleList module_list) : base(container, module_list)
        {
            
        }

        public override void Setup(int player_id)
        {
            AddLabel("What crate would you like order?");
            var applianceNames = Main.LoadedAvailableAppliances.Keys.ToList();
            applianceNames.Sort();

            GroupSelector = new Option<string>(applianceNames, applianceNames[0], applianceNames);

            GroupSelector.OnChanged += delegate (object _, string value)
                {
                    Main.LogInfo(value);
                    Redraw(Main.LoadedAvailableAppliances[value]);
                    SelectedGroupName = value;
                };


            //DestroyCratesOption = new Option<bool>(new List<bool> { true, false }, Main.DestroyCratesEnabled.Get(), new List<string> { "Enabled", "Disabled" });
            //DestroyCratesOption.OnChanged += delegate (object _, bool value)
            //    {
            //        //Main.DestroyCratesEnabled.Set(value);
            //        //Main.PreferenceManager.Save();
            //    };

            Redraw(Main.LoadedAvailableAppliances[applianceNames[0]]);
        }

        private void Redraw(Dictionary<int, string> variants)
        {
            ModuleList.Clear();

            AddLabel("What crate would you like order?");
            AddSelect(GroupSelector);

            AddLabel("Variant");

            Add(new Option<int>(variants.Keys.ToList(), variants.Keys.First(), variants.Values.ToList())).OnChanged += delegate (object _, int value)
            {
                ApplianceId = value;
            };

            AddButton("Order", delegate
            {
                if (!Enum.TryParse(Main.PrefManager.Get<string>(Main.SPAWN_AT_ID), out SpawnPositionType positionType))
                {
                    positionType = default;
                }              
                SpawnRequestSystem.Request<KitchenData.Appliance>(ApplianceId, positionType, 0, SpawnApplianceMode.Parcel);                              
            });            
            AddLabel("");
#if DEBUG
            AddButton("Cheat Toggle DEBUG", delegate
             {
                 ApplianceShopSystem.GoCheat = true;
             });
#endif
            ApplianceId = variants.Keys.First();

        }

    }
}
