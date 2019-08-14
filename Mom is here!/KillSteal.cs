using Ensage;
using Ensage.Common.Threading;
using Ensage.SDK.Extensions;
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
    internal class KillSteal
    {
        private readonly Config config;

        private readonly Abilities abilities;

        private Unit owner;

        private readonly TaskHandler handler;

        private readonly MenuManager menu;

        private readonly ILog log;

        public KillSteal(Config config)
        {
            this.config = config;
            this.abilities = config.Abilities;
            this.owner = config.Context.Owner;
            this.log = config.Log;
            this.menu = config.Menu;

            this.handler = UpdateManager.Run(ExecuteAsync, true, false);

            if (menu.KillStealItem)
            {
                handler.RunAsync();
            }

            menu.KillStealItem.PropertyChanged += KillStealChanged;

        }

        private void KillStealChanged(object sender, PropertyChangedEventArgs e)
        {
            if (menu.KillStealItem)
                handler.RunAsync();
            else
                handler.Cancel();
        }

        // TODO: Creat a web_destroy combo.

        private async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                if (Game.IsPaused || !owner.IsValid || !owner.IsAlive || owner.IsStunned())
                    return;

                var heroes = EntityManager<Hero>.Entities.Where(x => x.IsValidTarget());

                foreach (var target in heroes)
                {
                    var Spiderlings = abilities.Q;
                    if (menu.KillStealAbility.Value.IsEnabled(Spiderlings.Ability.Name) &&
                        !Spiderlings.Ability.IsHidden &&
                        Spiderlings.CanBeCasted &&
                        (int)target.Health - (int)DamageHelpers.GetSpellDamage(Spiderlings.GetDamage(target)) <= 0)
                    {
                        if (owner.Animation.Name.Contains("cast"))
                            owner.Stop();

                        Spiderlings.UseAbility(target);
                        await Task.Delay(Spiderlings.GetCastDelay(target), token);
                    }

                  
                }

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
            menu.KillStealItem.PropertyChanged -= KillStealChanged;
            if (menu.KillStealItem)
                handler?.Cancel();
        }
    }
}