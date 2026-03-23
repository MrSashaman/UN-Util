using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UN_Util
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "UnitedUtil";
        public override string Prefix => "UN_Utils";
        public override string Author => "mrSashaman";
        public override Version Version => new Version(0, 0, 1);

        private readonly List<ItemType> rewards = new List<ItemType>()
        {
            ItemType.Medkit,
            ItemType.Flashlight,
            ItemType.Coin,
            ItemType.KeycardJanitor
        };

        private readonly Random rnd = new Random();

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += Hello;
            Exiled.Events.Handlers.Player.Verified += PlayerVerified;
            Exiled.Events.Handlers.Player.Died += OnDead;
            Exiled.Events.Handlers.Server.RoundStarted += RoundStart;
            Exiled.Events.Handlers.Player.FlippingCoin += ThanksCoin;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= Hello;
            Exiled.Events.Handlers.Player.Verified -= PlayerVerified;
            Exiled.Events.Handlers.Player.Died -= OnDead;
            Exiled.Events.Handlers.Server.RoundStarted -= RoundStart;
            Exiled.Events.Handlers.Player.FlippingCoin -= ThanksCoin;

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
            ev.Player.Broadcast(4, "Привет " + ev.Player.Nickname + "!");
        }

        private void OnDead(DiedEventArgs ev)
        {
            ev.Player.ShowHint("Ты умер :(");
        }

        private void ThanksCoin(FlippingCoinEventArgs ev)
        {
            ev.IsAllowed = false;

            var coin = ev.Player.Items.FirstOrDefault(i => i.Type == ItemType.Coin);
            if (coin != null)
                ev.Player.RemoveItem(coin);

            int roll = rnd.Next(100);

            if (roll < 10)
            {
                ev.Player.ApplyRandomEffect();
                return;
            }

            if (roll < 40)
            {
                ev.Player.ShowHint("Ничего не произошло...", 3);
                return;
            }

            ItemType reward = rewards[rnd.Next(rewards.Count)];
            ev.Player.AddItem(reward);
            ev.Player.ShowHint($"Вам выпало: {reward}", 3);
        }

        private void RoundStart()
        {
            List<Player> dBoys = new List<Player>();

            foreach (var player in Player.List)
                if (player.Role == RoleTypeId.ClassD)
                    dBoys.Add(player);

            foreach (var player in dBoys)
            {
                int roll = rnd.Next(3);

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