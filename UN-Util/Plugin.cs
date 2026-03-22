using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;

namespace UN_Util
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "UnitedUtil";
        public override string Prefix => "UN_Utils";
        public override string Author => "mrSashaman";
        public override Version Version => new Version(0, 0, 1);

        public override void OnEnabled()
        {
            // Подписка на события
            Exiled.Events.Handlers.Server.WaitingForPlayers += Hello;
            Exiled.Events.Handlers.Player.Verified += PlayerVerified;
            Exiled.Events.Handlers.Player.Died += OnDead;
            Exiled.Events.Handlers.Server.RoundStarted += RoundStart;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= Hello;
            Exiled.Events.Handlers.Player.Verified -= PlayerVerified;
            Exiled.Events.Handlers.Player.Died -= OnDead;
            Exiled.Events.Handlers.Server.RoundStarted -= RoundStart;

            base.OnDisabled();
        }

        private void Hello()
        {
            Log.Info("****WELCOME*****");
            Log.Info("АВТОР: " + Author);
            Log.Info("Версия: " + Version);
        }

        private void PlayerVerified(VerifiedEventArgs ev)
        {
            ev.Player.Broadcast(10, "Welcome " + ev.Player.Nickname + "!");
        }

        private void OnDead(DiedEventArgs ev)
        {
            ev.Player.ShowHint("Ты умер :(");
        }

        private void RoundStart()
        {
            List<Player> Dboys = new List<Player>();
            foreach (var player in Player.List)
                if (player.Role == RoleTypeId.ClassD)
                    Dboys.Add(player);

            Random rnd = new Random();

            foreach (var player in Dboys)
            {
                int roll = rnd.Next(0, 3);
                if (roll == 0)
                    player.AddItem(ItemType.Flashlight);
                else if (roll == 1)
                    player.AddItem(ItemType.Lantern);
                else
                    player.AddItem(ItemType.Coin);
            }
        }
    }
}