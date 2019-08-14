using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Threading;
//using Ensage.SDK.Extensions;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using log4net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Broodmother
{
    internal class LastHit
    {
        private readonly Config config;

        private readonly Abilities abilities;

        private readonly Mode mode;

        private Unit owner;
        private static int spiderDenies = 65;
        private static int spiderDmg;

        private readonly TaskHandler handler;

        private readonly MenuManager menu;

        private readonly ILog log;

        public LastHit(Config config)
        {
            this.config = config;
            this.abilities = config.Abilities;
            this.mode = config.Mode;
            this.owner = config.Context.Owner;
            this.log = config.Log;
            this.menu = config.Menu;

            this.handler = UpdateManager.Run(ExecuteAsync, true, false);

            if (menu.QlastHit)
            {
                handler.RunAsync();
            }

            menu.QlastHit.PropertyChanged += QlastHitChanged;

        }

        private void QlastHitChanged(object sender, PropertyChangedEventArgs e)
        {
            if (menu.QlastHit)
                handler.RunAsync();
            else
                handler.Cancel();
        }

        // TODO: Redo whole system.
        // TODO: Create core assembly for checking damage reduction and amplifiers.
        // TODO: Add Heroes spellblocks.Like
        private async Task ExecuteAsync(CancellationToken token)
        {

            try
            {
                if (Game.IsPaused || !owner.IsValid || !owner.IsAlive || owner.IsStunned())
                    return;

                var me = ObjectManager.LocalHero;

                var enemies = EntityManager<Hero>.Entities.Where(x => x.IsValidTarget() && x.IsAlive && !x.IsIllusion && x.IsVisible && x.Team != me.Team).ToList();
                var Q = abilities.Q;
                var soul = me.FindItem("item_soul_ring");
                var creeps = EntityManager<Creep>.Entities.Where(creep => (creep.NetworkName == "CDOTA_BaseNPC_Creep_Lane"
                  || creep.NetworkName == "CDOTA_BaseNPC_Creep_Siege" || creep.NetworkName == "CDOTA_BaseNPC_Creep_Neutral"
                  || (creep.NetworkName == "CDOTA_Unit_SpiritBear" && creep.Team != me.Team)
                  || (creep.NetworkName == "CDOTA_BaseNPC_Invoker_Forged_Spirit" && creep.Team != me.Team)
                  || creep.NetworkName == "CDOTA_BaseNPC_Creep" && creep.IsAlive && creep.IsVisible && creep.IsSpawned) && creep.Health <= 259).ToList();
                var creepQ = EntityManager<Creep>.Entities.Where(creep => (creep.NetworkName == "CDOTA_BaseNPC_Creep_Lane"
                || creep.NetworkName == "CDOTA_BaseNPC_Creep_Siege"
                || creep.NetworkName == "CDOTA_BaseNPC_Creep_Neutral"
                || creep.NetworkName == "CDOTA_Unit_SpiritBear"
                || creep.NetworkName == "CDOTA_BaseNPC_Invoker_Forged_Spirit"
                || creep.NetworkName == "CDOTA_BaseNPC_Creep" && creep.IsAlive && creep.IsVisible && creep.IsSpawned)).ToList();
                var Spiderlings = EntityManager<Unit>.Entities.Where(spiderlings => spiderlings.NetworkName == "CDOTA_Unit_Broodmother_Spiderling").ToList();
                foreach (var creep in creepQ)
                {
                    if (Q.CanBeCasted && creep.Health - Q.GetDamage(creep) <= 0 && creep.Health > 45 && creep.Team != me.Team)
                    {
                        if (creep.Position.Distance2D(me.Position) <= 600)//&& Utils.SleepCheck("QQQ")
                        {
                            if (soul != null && soul.CanBeCasted() && me.Health >= 400)
                            {
                                soul.UseAbility();
                                //Utils.Sleep(300, "QQQ");
                                //await Task.Delay()
                            }
                            else
                                Q.UseAbility(creep);
                            //Utils.Sleep(300, "QQQ");
                            await Task.Delay(Q.GetCastDelay(creep), token);

                        }
                    }
                }
                foreach (var creep in creepQ)
                {
                    if (me.Mana < Q.ManaCost && creep.Health - Q.GetDamage(creep) <= 0 && creep.Health > 55 && creep.Team != me.Team)
                    {
                        if (creep.Position.Distance2D(me.Position) <= 600)//&& Utils.SleepCheck("QQQ")
                        {
                            if (soul != null && soul.CanBeCasted() && me.Health >= 400)
                            {
                                soul.UseAbility();
                                //Utils.Sleep(300, "QQQ");
                            }
                        }
                    }
                }

                var Spiderling = EntityManager<Unit>.Entities.Where(x => x.NetworkName == "CDOTA_Unit_Broodmother_Spiderling" && x.IsAlive && x.IsControllable && x.Team == me.Team).ToList();
                if (Spiderling.Count <= 0)
                    return;
                for (int s = 0; s < Spiderlings.Count(); s++)//auto deny
                {
                    if (Spiderlings[s].Health > 0 && Spiderlings[s].Health - spiderDenies <= 0)
                    {
                        for (int z = 0; z < Spiderlings.Count(); z++)
                        {
                            if (Spiderlings[s].Position.Distance2D(Spiderlings[z].Position) <= 500
                                && Utils.SleepCheck(Spiderlings[z].Handle.ToString() + "Spiderlings"))
                            {
                                Spiderlings[z].Attack(Spiderlings[s]);
                                Utils.Sleep(300, Spiderlings[z].Handle.ToString() + "Spiderlings");
                            }
                        }
                    }
                }
                if (menu.Autodenycreeps)
                {
                    for (int c = 0; c < creeps.Count(); c++)//auto deny creeps
                    {
                        for (int s = 0; s < Spiderlings.Count(); s++)
                        {
                            if (creeps != null)
                            {
                                if (creeps[c].Position.Distance2D(Spiderlings[s].Position) <= 500 &&
                                    creeps[c].Team != me.Team && creeps[c].Health > 0 && creeps[c].Health - Q.GetDamage(creeps[c]) <= 0
                                    && Utils.SleepCheck(Spiderlings[s].Handle.ToString() + "Spiderling"))
                                {
                                    {
                                        Spiderlings[s].Attack(creeps[c]);
                                        Utils.Sleep(300, Spiderlings[s].Handle.ToString() + "Spiderling");
                                    }
                                }
                                else if (creeps[c].Position.Distance2D(Spiderlings[s].Position) <= 500 &&
                                    creeps[c].Team == me.Team && creeps[c].Health > 0 && creeps[c].Health - Q.GetDamage(creeps[c]) <= 0
                                    && Utils.SleepCheck(Spiderlings[s].Handle.ToString() + "Spiderlings"))
                                {
                                    Spiderlings[s].Attack(creeps[c]);
                                    Utils.Sleep(300, Spiderlings[s].Handle.ToString() + "Spiderlings");
                                }

                            }

                        }
                    }

                }

                /*for (int t = 0; t < enemies.Count(); t++)// auto last hit enemies
                {
                    for (int s = 0; s < Spiderlings.Count(); s++)
                    {
                        if (enemies != null)
                        {
                            spiderDmg = Spiderlings.Count(y => y.Distance2D(enemies[t]) < 800) * Spiderlings[s].MinimumDamage;
                            if ((enemies[t].Position.Distance2D(Spiderlings[s].Position)) <= 800 &&
                                enemies[t].Team != me.Team && enemies[t].Health > 0 && enemies[t].Health - Q.GetDamage(creeps[t]) <= 0
                                && Utils.SleepCheck(Spiderlings[t].Handle.ToString() + "AttackEnemies"))
                            {
                                Spiderlings[s].Attack(enemies[t]);
                                Utils.Sleep(350, Spiderlings[t].Handle.ToString() + "AttackEnemies");

                            }
                        }
                    }
                }*/


            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        public void Dispose()
        {
            menu.QlastHit.PropertyChanged -= QlastHitChanged;
            if (menu.QlastHit)
                handler?.Cancel();
        }
    }
}