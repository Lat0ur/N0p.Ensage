using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
//using Ensage.SDK.Extensions;
using Ensage.SDK.Geometry;
using Ensage.SDK.Helpers;
using Ensage.SDK.Orbwalker.Modes;
using Ensage.SDK.Prediction;
using Ensage.SDK.TargetSelector;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Broodmother
{
    internal class Mode : KeyPressOrbwalkingModeAsync
    {
        private readonly ITargetSelectorManager targetSelector;

        private readonly IPredictionManager prediction;

        private readonly Unit owner;

        private readonly Abilities abilities;
        private Bootstrapper Main { get; }

        private readonly MenuManager menu;

        private readonly ILog log;

        public Mode(Config config, Key key) : base(config.Context, key)
        {
            this.log = config.Log;
            this.targetSelector = config.Context.TargetSelector;
            this.owner = config.Context.Owner;
            this.abilities = config.Abilities;
            this.prediction = config.Context.Prediction;
            this.menu = config.Menu;
        }

        public override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                var target = targetSelector.Active.GetTargets().FirstOrDefault(x => x.IsValidTarget()) as Hero;

                if (target == null || !menu.AntiBladeMail && target.HasModifier("modifier_item_blade_mail_reflect"))
                {
                    if (Orbwalker.CanMove())
                        Orbwalker.Move(Game.MousePosition);

                    return;
                }
                if (target != null && target.IsAlive && !Owner.IsVisibleToEnemies && !menu.LockTarget || menu.LockTarget)
                {

                    Orbwalker.OrbwalkTo(target);

                }
                else
                {
                    if (Orbwalker.CanMove())
                        Orbwalker.Move(Game.MousePosition);
                }
                if (target != null && target.IsAlive && Owner.IsVisibleToEnemies)
                {
                    var Son = EntityManager<Unit>.Entities.Where(x => x.NetworkName == "CDOTA_Unit_Broodmother_Spiderling").ToList();
                    for (int i = 0; i < Son.Count(); i++)
                    {
                        if (Son[i].Distance2D(target) <= 2000 && Utils.SleepCheck(Son[i].Handle.ToString() + "Sons"))//&& Utils.SleepCheck(Son[i].Handle.ToString() + "Sons")
                        {
                            Son[i].Attack(target);
                            //Utils.Sleep(350, Son[i].Handle.ToString() + "Sons");


                        }
                    }
                    for (int i = 0; i < Son.Count(); i++)
                    {
                        if (Son[i].Distance2D(target) >= 2000 && Utils.SleepCheck(Son[i].Handle.ToString() + "Sons"))
                        {
                            Son[i].Move(Game.MousePosition);
                            //Utils.Sleep(350, Son[i].Handle.ToString() + "Sons");

                        }
                    }
                }

                var me = ObjectManager.LocalHero;
                var linkens = target.Modifiers.Any(x => x.Name == "modifier_item_spheretarget") || target.Inventory.Items.Any(x => x.Name == "item_sphere");
                var enemies = ObjectManager.GetEntities<Hero>().Where(hero => hero.IsAlive && !hero.IsIllusion && hero.IsVisible && hero.Team != me.Team).ToList();
                if (target != null && target.IsAlive && !target.IsIllusion && me.Distance2D(target) <= 2000)
                {
                    var W = abilities.W;
                    var web = EntityManager<Unit>.Entities.Where(unit => unit.NetworkName == "CDOTA_Unit_Broodmother_Web").ToList();
                    var SpinWeb = GetClosestToWeb(web, me);
                    if (menu.AbilityTogglerItem.Value.IsEnabled(W.Ability.Name) && W.CanBeCasted && !W.Ability.IsHidden)
                    {
                        if ((me.Distance2D(SpinWeb) >= 900) && me.Distance2D(target) <= 800 && Utils.SleepCheck(SpinWeb.Handle.ToString() + "SpideWeb"))
                        {
                            W.UseAbility(target.Predict(1100));
                            //await Task.Delay(W.GetCastDelay(), token);
                            Utils.Sleep(300, SpinWeb.Handle.ToString() + "SpideWeb");
                        }
                    }


                    if (!target.IsMagicImmune())
                    {
                        var Q = abilities.Q;
                        if (menu.AbilityTogglerItem.Value.IsEnabled(Q.Ability.Name) && Q.CanBeCasted && !Q.Ability.IsHidden && !target.IsMagicImmune() && Owner.Distance2D(target) <= 600)
                        {
                            Q.UseAbility(target);
                            await Task.Delay(Q.GetCastDelay(target), token);
                        }
                        var R = abilities.R;
                        if (menu.AbilityTogglerItem.Value.IsEnabled(R.Ability.Name) && R.CanBeCasted && target.IsValidTarget() && Owner.Distance2D(target) <= 350)
                        {
                            R.UseAbility();
                            await Task.Delay(R.GetCastDelay(), token);
                        }

                        var orchid = me.GetItemById(ItemId.item_orchid) ??
                            me.GetItemById(ItemId.item_bloodthorn);
                        if (orchid !=null &&
                             orchid.CanBeCasted() &&
                             !linkens &&
                             orchid.CanHit(target) &&
                        Utils.SleepCheck("orchid")&&me.Distance2D(target) <= 1000)
                        {
                           orchid.UseAbility(target);
                            Utils.Sleep(250, "orchid");
                        }
                        var sheep = me.GetItemById(ItemId.item_sheepstick);
                            if (sheep != null &&
                                menu.ItemTogglerItem.Value.IsEnabled(sheep.ToString())&&
                                sheep.CanBeCasted() &&
                                !linkens &&
                                sheep.CanHit(target) &&
                        Utils.SleepCheck("sheep") && me.Distance2D(target) <= 600)
                            {
                                sheep.UseAbility(target);
                            Utils.Sleep(250, "sheep");
                        }
                        var Soul = me.GetItemById(ItemId.item_soul_ring);
                            if (Soul != null &&
                            menu.ItemTogglerItem.Value.IsEnabled(Soul.ToString()) &&
                                Owner.Health / Owner.MaximumHealth <= 0.5 &&
                               Owner.Mana <= Q.ManaCost &&
                                Soul.CanBeCasted())
                            {
                                Soul.UseAbility();
                            }
                        var shiva = me.GetItemById(ItemId.item_shivas_guard);
                            if (shiva != null &&
                            menu.ItemTogglerItem.Value.IsEnabled(shiva.ToString()) &&
                                shiva.CanBeCasted() &&
                                shiva.CanHit(target) && Utils.SleepCheck("shiva") &&
                        me.Distance2D(target) <= 600)
                            {
                                shiva.UseAbility();
                                //await Task.Delay(shiva.GetCastDelay(), token);
                                Utils.Sleep(250, "shiva");
                            }
                        var mom = me.GetItemById(ItemId.item_mask_of_madness);
                            if (mom != null &&
                            menu.ItemTogglerItem.Value.IsEnabled(mom.ToString()) &&
                                mom.CanBeCasted() && Utils.SleepCheck("mom") &&
                        me.Distance2D(target) <= 700)
                            {
                                mom.UseAbility();
                            Utils.Sleep(250, "mom");

                            }
                        var medall = me.GetItemById(ItemId.item_solar_crest) ??
                             me.GetItemById(ItemId.item_medallion_of_courage);
                            if (medall != null &&
                                medall.CanBeCasted() && Utils.SleepCheck("medall") &&
                        me.Distance2D(target) <= 500)
                            {
                                medall.UseAbility(target);
                            Utils.Sleep(250, "Medall");
                        }
                        var abyssal = me.GetItemById(ItemId.item_abyssal_blade);
                            if (abyssal != null &&
                            menu.ItemTogglerItem.Value.IsEnabled(abyssal.ToString()) &&
                                abyssal.CanBeCasted() && Utils.SleepCheck("abyssal") &&
                        me.Distance2D(target) <= 400)
                            {
                                abyssal.UseAbility(target);
                            Utils.Sleep(250, "abyssal");

                        }
                        var halberd = me.GetItemById(ItemId.item_heavens_halberd);
                            if (halberd != null &&
                            menu.ItemTogglerItem.Value.IsEnabled(halberd.ToString()) &&
                                halberd.CanBeCasted() && Utils.SleepCheck("halberd") &&
                        me.Distance2D(target) <= 700)
                            {
                                halberd.UseAbility(target);
                            Utils.Sleep(250, "halberd");
                        }
                        var mjollnir = me.GetItemById(ItemId.item_mjollnir);
                            if (mjollnir != null &&
                            menu.ItemTogglerItem.Value.IsEnabled(mjollnir.ToString()) &&
                                mjollnir.CanBeCasted() && Utils.SleepCheck("mjollnir") &&
                        me.Distance2D(target) <= 600)
                            {
                                mjollnir.UseAbility(Owner);
                            Utils.Sleep(250, "mjollnir");
                        }
                        var satanic = me.GetItemById(ItemId.item_satanic);
                            if (satanic != null &&
                            menu.ItemTogglerItem.Value.IsEnabled(satanic.ToString()) &&
                                Owner.Health / Owner.MaximumHealth <= 0.4 &&
                                satanic.CanBeCasted() && Utils.SleepCheck("Satanic") &&
                        me.Distance2D(target) <= 300)
                            {
                                satanic.UseAbility();
                            Utils.Sleep(250, "Satanic");
                        }

                        if ((!Owner.CanAttack() || Owner.Distance2D(target) >= 0) && Owner.NetworkActivity != NetworkActivity.Attack &&
                            Owner.Distance2D(target) <= 600 && Utils.SleepCheck("Move"))
                        {
                            Orbwalker.Move(target.Predict(500));
                            //Utils.Sleep(390, "Move");
                        }
                        if (Owner.Distance2D(target) <= Owner.AttackRange + 100 && (!Owner.IsAttackImmune() || !target.IsAttackImmune())
                            && Owner.NetworkActivity != NetworkActivity.Attack && Owner.CanAttack() && Utils.SleepCheck("attack"))
                        {
                            Orbwalker.Attack(target);
                           // Utils.Sleep(160, "attack");

                        }
                    }




                }
                await Task.Delay(290, token);
            }
            catch (TaskCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                log.Error(ex);
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