

namespace BroodMother
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ensage;
    using Ensage.Common.Menu;
    using Ensage.SDK.Menu;

    internal class Settings : IDisposable
    {
        private readonly MenuFactory factory;

        public Settings(string ownerName)
        {
            this.factory = MenuFactory.CreateWithTexture("Your mom is here!", ownerName);
            this.ComboKey = this.factory.Item("Combo", new KeyBind('D', KeyBindType.Press));
            this.ChaseKey = this.factory.Item("Chase", new KeyBind('E', KeyBindType.Press));
            this.Lasthit = this.factory.Item("LastHitCreeps", new KeyBind('F', KeyBindType.Toggle));
            //this.DestroyKey = this.factory.Item("Combo", new KeyBind('E'));
            this.UseQ = this.factory.Item("Kill creep Q Spell", true);
           

            var abilities = new List<AbilityId>
            {
                AbilityId.broodmother_spawn_spiderlings,
                AbilityId.broodmother_spin_web,
                //AbilityId.broodmother_spin_web_destroy,
                AbilityId.broodmother_insatiable_hunger

            };
            this.Items = this.factory.Item("Abilities", new AbilityToggler(abilities.ToDictionary(x => x.ToString(), x => true)));
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
           
           this.Abilities = this.factory.Item("Items", new AbilityToggler(items.ToDictionary(x => x.ToString(), x => true)));
        }

        public MenuItem<KeyBind> ComboKey { get; }
        public MenuItem<KeyBind> ChaseKey { get; }
        public MenuItem<KeyBind> DestroyKey { get; }
        public MenuItem<KeyBind> Lasthit { get; }
        public MenuItem<Slider> Distance { get; }

        public MenuItem<bool> DrawTargetParticle { get; }
        public MenuItem<bool> UseQ { get; }

        public MenuItem<AbilityToggler> Items { get; }
        public MenuItem<AbilityToggler> Abilities { get; }

        public void Dispose()
        {
            this.factory.Dispose();
        }
    }
}