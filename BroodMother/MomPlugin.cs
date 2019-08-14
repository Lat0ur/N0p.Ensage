// <copyright file="PudgePlugin.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace BroodMother
{
    using System.ComponentModel.Composition;

    using Ensage;
    using Ensage.SDK.Service;
    using Ensage.SDK.Service.Metadata;

    [ExportPlugin("Simple.BroodMother", HeroId.npc_dota_hero_broodmother)]
    internal class MomPlugin : Plugin
    {
        private readonly IServiceContext context;

        private readonly Unit owner;

        private ComboMode orbwalkingMode;

        private Settings settings;

        [ImportingConstructor]
        public MomPlugin(IServiceContext context)
        {
            this.context = context;
            this.owner = context.Owner;
        }

        protected override void OnActivate()
        {
            this.settings = new Settings(this.owner.Name);
            this.orbwalkingMode = new ComboMode(this.context, this.settings);
            this.context.Orbwalker.RegisterMode(this.orbwalkingMode);
        }

        protected override void OnDeactivate()
        {
            this.context.Orbwalker.UnregisterMode(this.orbwalkingMode);
            this.settings.Dispose();
        }
    }
}