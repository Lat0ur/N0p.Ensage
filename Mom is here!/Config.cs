using Ensage.Common.Menu;
using Ensage.SDK.Service;
using log4net;
using PlaySharp.Toolkit.Logging;
using System.Reflection;
using System.Windows.Input;

namespace Broodmother
{
    internal class Config
    {
        public IServiceContext Context { get; }

        public MenuManager Menu { get; }

        public Mode Mode { get; }

        public Abilities Abilities { get; }

        public KillSteal KillSteal { get; }
        public LastHit LastHit { get; }

        public ILog Log { get; } = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Config(IServiceContext context)
        {
            this.Context = context;
            ActivateDependencies();

            this.Menu = new MenuManager(this);
            this.Abilities = new Abilities(this);
            this.KillSteal = new KillSteal(this);
            this.LastHit = new LastHit(this);


            Menu.ComboKeyItem.Item.ValueChanged += ComboKeyChanged;
            this.Mode = new Mode(this, KeyInterop.KeyFromVirtualKey((int)Menu.ComboKeyItem.Value.Key));
            Context.Orbwalker.RegisterMode(Mode);
        }

        private void ActivateDependencies()
        {
            if (!Context.Orbwalker.IsActive)
                Context.Orbwalker.Activate();

            if (!Context.TargetSelector.IsActive)
                Context.TargetSelector.Activate();

            if (!Context.Prediction.IsActive)
                Context.Prediction.Activate();
        }

        private void ComboKeyChanged(object sender, OnValueChangeEventArgs e)
        {
            var keyCode = e.GetNewValue<KeyBind>().Key;
            if (keyCode == e.GetOldValue<KeyBind>().Key)
                return;

            Mode.Key = KeyInterop.KeyFromVirtualKey((int)keyCode);
        }
    }
}