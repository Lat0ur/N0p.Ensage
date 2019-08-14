using Ensage;
using Ensage.SDK.Abilities.Items;
using Ensage.SDK.Inventory.Metadata;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using System.ComponentModel.Composition;

namespace Broodmother
{
    [ExportPlugin(name: "Mom is here !!!!", author: "nop", units: HeroId.npc_dota_hero_broodmother)]
    internal class Bootstrapper : Plugin
    {
        private IServiceContext context { get; set; }

        private Config Config { get; set; }

        [ImportingConstructor]
        public Bootstrapper([Import] IServiceContext context)
        {
            this.context = context;
        }
        [ItemBinding]
        public item_mask_of_madness mom { get; set; }
        [ItemBinding]
        public item_abyssal_blade abyssal { get; set; }
        [ItemBinding]
        public item_soul_ring Soul { get; set; }
        [ItemBinding]
        public item_orchid orchid { get; set; }
        [ItemBinding]
        public item_shivas_guard shiva { get; set; }
        [ItemBinding]
        public item_heavens_halberd halberd { get; set; }
        [ItemBinding]
        public item_mjollnir mjollnir { get; set; }
        [ItemBinding]
        public item_satanic satanic { get; set; }

        [ItemBinding]
        public item_medallion_of_courage medall { get; set; }

        [ItemBinding]
        public item_sheepstick sheep { get; set; }
        [ItemBinding]
        public item_cheese cheese { get; set; }
        protected override void OnActivate()
        {
            Config = new Config(context);
        }

        protected override void OnDeactivate()
        {

        }
    }
}
