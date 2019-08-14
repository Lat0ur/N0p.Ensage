

namespace BroodMother
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Ensage;
    using Ensage.Common;
    using Ensage.Common.Enums;
    using Ensage.Common.Threading;
    //using Ensage.Common.Extensions;
    using Ensage.SDK.Abilities.Items;
    using Ensage.SDK.Abilities.npc_dota_hero_broodmother;
    using Ensage.SDK.Extensions;
    using Ensage.SDK.Geometry;
    using Ensage.SDK.Handlers;
    using Ensage.SDK.Helpers;
    using Ensage.SDK.Inventory.Metadata;
    using Ensage.SDK.Orbwalker.Modes;
    using Ensage.SDK.Prediction;
    using Ensage.SDK.Renderer.Particle;
    using Ensage.SDK.Service;
    using Ensage.SDK.TargetSelector;

    using log4net;

    using PlaySharp.Toolkit.Logging;

    using SharpDX;
    using AbilityId = Ensage.Common.Enums.AbilityId;

    internal class ComboMode : KeyPressOrbwalkingModeAsync
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static broodmother_spawn_spiderlings SpellQ;

        private readonly IUpdateHandler hookUpdateHandler;

        private readonly IParticleManager particleManager;

        private static broodmother_spin_web web;

        private readonly Settings settings;

        private readonly IUpdateHandler targetParticleUpdateHandler;

        private readonly ITargetSelectorManager targetSelector;

        private static broodmother_insatiable_hunger ult;

        //private static broodmother_spin_web_destroy

        private Vector3 hookCastPosition;

        private float hookStartCastTime;

        private Unit target;

        public ComboMode(IServiceContext context, Settings settings)
            : base(context, settings.ComboKey)
        {
            var abilities = context.AbilityFactory;
            SpellQ = abilities.GetAbility<broodmother_spawn_spiderlings>();
            web = abilities.GetAbility<broodmother_spin_web>();
            ult = abilities.GetAbility<broodmother_insatiable_hunger>();
            this.targetSelector = context.TargetSelector;
            this.settings = settings;
            //this.particleManager = this.Context.Particle;
            //this.hookUpdateHandler = UpdateManager.Subscribe(this.HookHitCheck, 0, false);
            //this.targetParticleUpdateHandler = UpdateManager.Subscribe(this.UpdateTargetParticle, 0, false);
        }
        #region Items

        [ItemBinding]
        private item_mask_of_madness mom { get; set; }
        [ItemBinding]
        private item_abyssal_blade abyssal { get; set; }
        [ItemBinding]
        private item_soul_ring Soul { get; set; }
        [ItemBinding]
        private item_orchid orchid { get; set; }
        [ItemBinding]
        private item_shivas_guard shiva { get; set; }
        [ItemBinding]
        private item_heavens_halberd halberd { get; set; }
        [ItemBinding]
        private item_mjollnir mjollnir { get; set; }
        [ItemBinding]
        private item_satanic satanic { get; set; }

        [ItemBinding]
        private item_medallion_of_courage medall { get; set; }

        [ItemBinding]
        private item_sheepstick sheep { get; set; }
        [ItemBinding]
        private item_cheese cheese { get; set; }
        #endregion
        public override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                this.target = this.targetSelector.Active.GetTargets()
                    .FirstOrDefault(x => x.Distance2D(this.Owner) <= 1900);

                if (this.target == null)
                {
                    this.Orbwalker.OrbwalkTo(null);
                    return;
                }
                if (target != null && target.IsAlive && !Owner.IsVisibleToEnemies)
                {
                    if (Owner.Distance2D(this.target) <= 1100 && (!Owner.IsAttackImmune() || !target.IsAttackImmune())
                        && Owner.NetworkActivity != NetworkActivity.Attack && Owner.CanAttack() && Utils.SleepCheck("attack"))
                    {
                        Owner.Attack(target);
                        Orbwalker.Attack(target);
                        //Utils.Sleep(150, "attack");
                        //await Task.Delay(150, token);

                    }
                }
                if (target != null && target.IsAlive && Owner.IsVisibleToEnemies)
                {
                    var Son = ObjectManager.GetEntities<Unit>().Where(spiderlings => spiderlings.NetworkName == "CDOTA_Unit_Broodmother_Spiderling").ToList();
                    for (int i = 0; i < Son.Count(); i++)
                    {
                        if (Son[i].Distance2D(target) <= 1500 && Utils.SleepCheck(Son[i].Handle.ToString() + "Sons"))
                        {
                            Son[i].Attack(target);
                            Utils.Sleep(350, Son[i].Handle.ToString() + "Sons");
                        }
                    }
                    for (int j = 0; j < Son.Count(); j++)
                    {
                        if (Son[j].Distance2D(target) >= 1500 && Utils.SleepCheck(Son[j].Handle.ToString() + "Sons"))
                        {
                            Son[j].Move(Game.MousePosition);
                            Utils.Sleep(350, Son[j].Handle.ToString() + "Sons");

                        }
                    }

                    var linkens = target.Modifiers.Any(x => x.Name == "modifier_item_spheretarget") || target.Inventory.Items.Any(x => x.Name == "item_sphere");
                    //var linkens = EntityManager<Hero>.Entities.Where(x => x.HasModifier ("modifier_item_spheretarget") || x.Inventory.Items.Any(x => x.Name == "item_sphere"));
                    var enemies = EntityManager<Hero>.Entities.Where(x => x.IsValidTarget());
                    if (target != null && target.IsAlive && !target.IsIllusion && Owner.Distance2D(target) <= 1000)
                    {
                        var Q = SpellQ;
                        if (Q != null && Q.CanBeCasted && !target.IsMagicImmune() && settings.Items.Value.IsEnabled(Q.Ability.Name) && Owner.Distance2D(target) <= 600
                           && Utils.SleepCheck("Q"))
                        {
                            Q.UseAbility(target);
                            //Utils.Sleep(250, "Q");
                            await Await.Delay(Q.GetCastDelay(target), token);
                        }
                        var R = ult;
                        if (R != null && R.CanBeCasted && Owner.Distance2D(target) <= 350 && settings.Items.Value.IsEnabled(R.Ability.Name) && Utils.SleepCheck("R"))
                        {
                            R.UseAbility();
                            //Utils.Sleep(250, "R");
                            await Await.Delay(250, token);
                        }
                        if ( // orchid
                            orchid != null &&
                            orchid.CanBeCasted &&
                            !target.IsMagicImmune() &&
                            !linkens &&
                            orchid.CanHit(target) &&
                            Owner.Distance2D(target) <= 1000
)
                        {
                            orchid.UseAbility(target);
                            await Await.Delay(orchid.GetCastDelay(target), token);
                        } // orchid Item end

                        if (sheep != null &&
                            sheep.CanBeCasted &&
                            !target.IsMagicImmune() &&
                            !linkens &&
                            Owner.Distance2D(target) <= 600)
                        {
                            sheep.UseAbility(target);
                            await Await.Delay(sheep.GetCastDelay(target), token);
                        }

                        if (Soul != null &&
                            Owner.Health / Owner.MaximumHealth <= 0.5 &&
                           Owner.Mana <= Q.ManaCost &&
                            Soul.CanBeCasted)
                        {
                            Soul.UseAbility();
                        }
                        if (shiva != null &&
                            shiva.CanBeCasted &&
                            !target.IsMagicImmune() &&
                            Owner.Distance2D(target) <= 600)
                        {
                            shiva.UseAbility();
                            await Await.Delay(shiva.GetCastDelay(), token);
                        }
                        if (mom != null &&
                            mom.CanBeCasted &&
                            Owner.Distance2D(target) <= 700)
                        {
                            mom.UseAbility();
                            await Await.Delay(mom.GetCastDelay(), token);
                        }
                        if (medall != null &&
                            medall.CanBeCasted &&
                            Owner.Distance2D(target) <= 500)
                        {
                            medall.UseAbility(target);
                            await Await.Delay(medall.GetCastDelay(target), token);
                        }
                        if (abyssal != null &&
                            abyssal.CanBeCasted &&
                            !target.IsMagicImmune() &&
                            Owner.Distance2D(target) <= 400)
                        {
                            abyssal.UseAbility(target);
                            await Await.Delay(abyssal.GetCastDelay(target), token);

                        }
                        if (halberd != null &&
                            halberd.CanBeCasted &&
                            !target.IsMagicImmune() &&
                            Owner.Distance2D(target) <= 700)
                        {
                            halberd.UseAbility(target);
                            await Await.Delay(halberd.GetCastDelay(target), token);
                        }
                        if (mjollnir != null &&
                            mjollnir.CanBeCasted &&
                            !target.IsMagicImmune() &&
                            Owner.Distance2D(target) <= 600)
                        {
                            mjollnir.UseAbility(Owner);
                            await Await.Delay(mjollnir.GetCastDelay(Owner), token);
                        }
                        if (satanic != null &&
                            Owner.Health / Owner.MaximumHealth <= 0.4 &&
                            satanic.CanBeCasted &&
                            Owner.Distance2D(target) <= 300)
                        {
                            satanic.UseAbility();
                            await Await.Delay(satanic.GetCastDelay(), token);
                        }
                        if ((!Owner.CanAttack() || Owner.Distance2D(target) >= 0) && Owner.NetworkActivity != NetworkActivity.Attack &&
                            Owner.Distance2D(target) <= 600)
                        {
                            Owner.Move(Game.MousePosition);
                        }
                        if(Owner.Distance2D(target) <= Owner.AttackRange + 100 && (!Owner.IsAttackImmune() || !target.IsAttackImmune())
                            && Owner.NetworkActivity != NetworkActivity.Attack && Owner.CanAttack())
                        {
                            Owner.Attack(target);

                        }

                        //============== use web
                        var W= ObjectManager.GetEntities<Unit>().Where(unit => unit.NetworkName == "CDOTA_Unit_Broodmother_Web").ToList();
                        var me = ObjectManager.LocalHero;
                        var SpinWeb = GetClosestToWeb(W, me);
                        if (web != null && web.CanBeCasted && settings.Items.Value.IsEnabled(web.Ability.Name))
                        {
                            if ((me.Distance2D(SpinWeb) >= 900) && me.Distance2D(target) <= 800 )
                            {
                                web.UseAbility(target);
                                await Await.Delay(web.GetCastDelay(target), token);
                            }
                        }
                    }

                }



            }
            catch (TaskCanceledException)
            {
                // ignore
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            this.Context.Inventory.Attach(this);
            Entity.OnBoolPropertyChange += this.EntityOnBoolPropertyChange;
            this.MenuKey.PropertyChanged += this.MenuKeyOnPropertyChanged;

        }

        protected override void OnDeactivate()
        {
            this.Context.Inventory.Detach(this);
            Entity.OnBoolPropertyChange -= this.EntityOnBoolPropertyChange;
            this.MenuKey.PropertyChanged -= this.MenuKeyOnPropertyChanged;

            base.OnDeactivate();
        }

        private void EntityOnBoolPropertyChange(Entity sender, BoolPropertyChangeEventArgs args)
        {
            if (!this.CanExecute)
            {
                return;
            }

            if (sender != web || args.NewValue == args.OldValue || args.PropertyName != "m_bInAbilityPhase")
            {
                return;
            }

            if (args.NewValue)
            {
                // start HookHitCheck updater when we casted it
                this.hookStartCastTime = Game.RawGameTime;
                this.hookUpdateHandler.IsEnabled = true;
            }
            else
            {
                this.hookUpdateHandler.IsEnabled = false;
            }
        }



        private void MenuKeyOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (this.MenuKey)
            {
                if (this.settings.DrawTargetParticle)
                {
                    this.targetParticleUpdateHandler.IsEnabled = true;
                }
            }
            else
            {
                if (this.settings.DrawTargetParticle)
                {
                    this.particleManager.Remove("pudgeTarget");
                    this.targetParticleUpdateHandler.IsEnabled = false;
                }

                var anyEnemyNear = EntityManager<Hero>.Entities.Any(
                    x => x.IsValid && x.IsAlive && !x.IsIllusion && x.IsEnemy(this.Owner) &&  x.Distance2D(this.Owner) < web.Radius);

                /*if (!anyEnemyNear)
                {
                    web.e = false;
                }*/
            }
        }


        private static Unit GetClosestToWeb(List<Unit> units, Hero x)
        {
            Unit closestHero = null;
            foreach (var b in units.Where(v => closestHero == null || closestHero.Distance2D(x) > v.Distance2D(x)))
            {
                closestHero = b;
            }
            return closestHero;
        }


    }
}