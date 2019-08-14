using Ensage;
using Ensage.Common.Menu;
using Ensage.SDK.Menu;
using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Broodmother
{
    internal class MenuManager
    {
        private MenuFactory factory { get; }

        public MenuItem<KeyBind> ComboKeyItem { get; }

        public MenuItem<bool> LockTarget { get; }

        public MenuItem<bool> KillStealItem { get; }
        public MenuItem<bool> QlastHit { get; }
        public MenuItem<bool> Autodenyspider { get; }
        public MenuItem<bool> Autodenycreeps { get; }



        public MenuItem<AbilityToggler> KillStealAbility { get; }

        public MenuItem<bool> AntiBladeMail { get; }

        public MenuItem<AbilityToggler> AbilityTogglerItem { get; set; }
        public MenuItem<AbilityToggler> ItemTogglerItem { get; set; }

        public MenuManager(Config config)
        {
            this.factory = MenuFactory.CreateWithTexture("Mom is here!", "npc_dota_hero_broodmother");
            this.factory.Target.SetFontColor(Color.GreenYellow);

            var comboMenu = factory.Menu("Combo");
            ComboKeyItem = comboMenu.Item(" Combo Key", new KeyBind('D'));
            AntiBladeMail = comboMenu.Item("Attack Blade Mail", false);
            LockTarget = comboMenu.Item("Lock Orbwalking to Target", false);

            var abilityMenu = factory.Menu("Abilities");
            var itemsMenu = factory.Menu("Items");

            var abilities = new List<AbilityId>
            {
                AbilityId.broodmother_spawn_spiderlings,
                AbilityId.broodmother_spin_web,
                //AbilityId.broodmother_spin_web_destroy,
                AbilityId.broodmother_insatiable_hunger

            };
            AbilityTogglerItem = abilityMenu.Item("Use: ", new AbilityToggler(abilities.ToDictionary(x => x.ToString(), x => true)));
            var items = new List<AbilityId>
            {
                AbilityId.item_mask_of_madness,
                AbilityId.item_abyssal_blade,
                AbilityId.item_soul_ring,
                AbilityId.item_orchid,
                AbilityId.item_shivas_guard,
                AbilityId.item_heavens_halberd,
                AbilityId.item_mjollnir,
                AbilityId.item_satanic,
                AbilityId.item_medallion_of_courage,
                AbilityId.item_sheepstick,
                AbilityId.item_cheese

            };
            ItemTogglerItem=itemsMenu.Item("Use: ", new AbilityToggler(items.ToDictionary(x => x.ToString(), x => true)));

            var QlastHitMenu = factory.Menu("Last Hit");
            QlastHit = QlastHitMenu.Item("Use Q Last hit",true);
            Autodenycreeps = QlastHitMenu.Item("auto spider deny creeps", false);
            

            var killStealMenu = factory.Menu("KillSteal");
            KillStealItem = killStealMenu.Item(" KillSteal", true);
            KillStealAbility = killStealMenu.Item("Use: ", new AbilityToggler(new Dictionary<string, bool>()
            {
                { "broodmother_spawn_spiderlings", true },
            }));


        }
    }
}