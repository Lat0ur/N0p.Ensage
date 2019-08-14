using Ensage;
using Ensage.SDK.Abilities;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using Ensage.SDK.Inventory;

using Ensage.SDK.Abilities.npc_dota_hero_broodmother;
    using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Inventory.Metadata;

namespace Broodmother
{
    internal class Abilities
    {
        private readonly Config config;

        private readonly AbilityFactory abilityFactory;

        private readonly IInventoryManager inventory;

        public readonly broodmother_spawn_spiderlings Q;

        public readonly broodmother_spin_web W;
        public readonly broodmother_insatiable_hunger R;

        public Abilities(Config config)
        {
            this.config = config;
            this.abilityFactory = config.Context.AbilityFactory;
            this.inventory = config.Context.Inventory;

            this.Q = new broodmother_spawn_spiderlings(config.Context.Owner.GetAbilityById(AbilityId.broodmother_spawn_spiderlings));
            this.W = new broodmother_spin_web(config.Context.Owner.GetAbilityById(AbilityId.broodmother_spin_web));
            //this.WD = new broodmother_spin_web_destroy(config.Context.Owner.GetAbilityById(AbilityId.broodmother_spin_web_destroy));
            this.R = new broodmother_insatiable_hunger(config.Context.Owner.GetAbilityById(AbilityId.broodmother_insatiable_hunger));



            UpdateManager.BeginInvoke(() =>
            {
                inventory.Attach(this);
            }, 3000);
        }
    }
}