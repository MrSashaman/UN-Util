using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Cassie;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using CassieAPI = Exiled.API.Features.Cassie;
using MapEvents = Exiled.Events.Handlers.Map;

namespace UN_Util
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "UnitedUtil";
        public override string Prefix => "UN_Utils";
        public override string Author => "mrSashaman";
        public override Version Version => new Version(0, 0, 3);

        private readonly List<ItemType> rewards = new List<ItemType>()
        {
            ItemType.Medkit,
            ItemType.Flashlight,
            ItemType.Coin,
            ItemType.KeycardJanitor,
            ItemType.Adrenaline,
            ItemType.GunFSP9
        };

        private readonly Random rnd = new Random();

        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += Hello;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Player.Verified += PlayerVerified;
            Exiled.Events.Handlers.Player.Died += OnDead;
            Exiled.Events.Handlers.Server.RoundStarted += RoundStart;
            Exiled.Events.Handlers.Player.FlippingCoin += ThanksCoin;
            MapEvents.Decontaminating += OnDecontaminating;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= Hello;
            Exiled.Events.Handlers.Player.Verified -= PlayerVerified;
            Exiled.Events.Handlers.Player.Died -= OnDead;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.RoundStarted -= RoundStart;
            MapEvents.Decontaminating -= OnDecontaminating;

            Exiled.Events.Handlers.Player.FlippingCoin -= ThanksCoin;

            base.OnDisabled();
        }



        public void OnDecontaminating(DecontaminatingEventArgs ev)
        {
            Timing.CallDelayed(3f, () =>
            {
                Exiled.API.Features.Cassie.Message(
                    "pitch_0.7 .G3 warning . light containment zone decontamination process has begun . all personnel evacuate immediately pitch_1.0",
                    isNoisy: true
                );
            });

            Exiled.API.Features.Map.Broadcast(15,
                "<color=red>ВНИМАНИЕ: ДЕЗИНФЕКЦИЯ ЛЗС НАЧАЛАСЬ! НЕМЕДЛЕННО ПОКИНЬТЕ ЗОНУ!</color>");

            Map.ChangeLightsColor(new UnityEngine.Color(1, 0, 0));

            Timing.CallDelayed(10f, () =>
            {
                Map.ResetLightsColor();
            });
        }
        private void Hello()
        {
            Log.Info("****WELCOME*****");
            Log.Info("Dev Discord: https://discord.gg/5pBt7cj8B9 ");
            Log.Info("Version: " + Version);
        }

        private void PlayerVerified(VerifiedEventArgs ev)
        {
            ev.Player.Broadcast(4, "Привет " + ev.Player.Nickname + "!");
        }


        private void OnDead(DiedEventArgs ev)
        {
            ev.Player.ShowHint("Ты умер :(");
        }

        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            Server.FriendlyFire = true;
            Log.Info("FF Включен");
            Map.Broadcast(4, "<color=orange>FRIENDLY FIRE ВКЛЮЧЕН</color>");
        }

        private void ThanksCoin(FlippingCoinEventArgs ev)
        {
            ev.IsAllowed = false;

            var player = ev.Player;

            var coin = player.Items.FirstOrDefault(i => i.Type == ItemType.Coin);
            if (coin != null)
                player.RemoveItem(coin);

            int roll = rnd.Next(100);

            if (roll < 5)
            {
                player.Kill("Монетка решила твою судьбу");
                return;
            }

            if (roll < 15)
            {
                ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                grenade.FuseTime = 2f;
                grenade.SpawnActive(player.Position);

                player.ShowHint("<color=red>ОЙ... </color>", 3);
                return;
            }

            if (roll < 25)
            {
                player.EnableEffect(EffectType.SeveredHands, 10f); 
                player.ShowHint("<color=red>Ты потерял контроль над руками...</color>", 3);
                return;
            }

            if (roll < 40)
            {
                player.ApplyRandomEffect(EffectCategory.Negative);
                player.ShowHint("<color=red>Что-то пошло не так...</color>", 3);
                return;
            }

            if (roll < 60)
            {
                player.ShowHint("Ничего не произошло...", 3);
                return;
            }

            if (roll < 80)
            {
                player.ApplyRandomEffect(EffectCategory.Positive);
                player.ShowHint("<color=green>Тебе повезло!</color>", 3);
                return;
            }

            ItemType reward = rewards[rnd.Next(rewards.Count)];
            player.AddItem(reward);
            player.ShowHint($"<color=yellow>Вам выпало: {reward}</color>", 3);
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