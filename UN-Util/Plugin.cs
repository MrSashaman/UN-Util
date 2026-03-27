using Exiled.API.Enums;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Server;
using MEC;
using Exiled.Events.EventArgs.Scp096;

using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MapEvents = Exiled.Events.Handlers.Map;

namespace UN_Util
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "UnitedUtil";
        public override string Prefix => "UN_Utils";

        public override string Author => "mrSashaman";
        public override Version Version => new Version(0, 1, 1);

        private readonly System.Random rnd = new System.Random();
        private Dictionary<Player, CoroutineHandle> choosingPlayers = new Dictionary<Player, CoroutineHandle>();
        private Player traitor;
        private string traitorId;
        private bool traitorActive = false;
        public static Plugin Singleton;

        public override void OnEnabled()
        {
            Singleton = this;
            Exiled.Events.Handlers.Server.WaitingForPlayers += Hello;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Player.Verified += PlayerVerified;
            Exiled.Events.Handlers.Player.Died += OnDead;
            Exiled.Events.Handlers.Server.RoundStarted += RoundStart;
            Exiled.Events.Handlers.Scp049.StartingRecall += OnStartingRecall;
            Exiled.Events.Handlers.Player.FlippingCoin += ThanksCoin;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.Escaping += OnEscaping;
            Exiled.Events.Handlers.Scp096.AddingTarget += OnAddingTarget;

            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawningTeam;
            MapEvents.Decontaminating += OnDecontaminating;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= Hello;
            Exiled.Events.Handlers.Player.Verified -= PlayerVerified;
            Exiled.Events.Handlers.Player.Died -= OnDead;
            Exiled.Events.Handlers.Scp049.StartingRecall -= OnStartingRecall;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.RoundStarted -= RoundStart;
            Exiled.Events.Handlers.Player.FlippingCoin -= ThanksCoin;
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Scp096.AddingTarget -= OnAddingTarget;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawningTeam;
            MapEvents.Decontaminating -= OnDecontaminating;
            Exiled.Events.Handlers.Player.Escaping -= OnEscaping;


            base.OnDisabled();
        }

        private void Hello()
        {
            Log.Info("****WELCOME*****");
            Log.Info("Dev Discord: https://discord.gg/5pBt7cj8B9 ");
            Log.Info("Version: " + Version);
        }

        private void PlayerVerified(VerifiedEventArgs ev)
        {
            ev.Player.Broadcast(4,
                Config.Messages.Welcome.Replace("{nickname}", ev.Player.Nickname));
        }

        private void OnDead(DiedEventArgs ev)
        {
            ev.Player.ShowHint(Config.Messages.DeathHint);
            ev.Player.Broadcast(7, Config.Messages.DeathBroadcast);
        }
        

        private void OnStartingRecall(StartingRecallEventArgs ev)
            {
                var doctor = ev.Player;

                var target = ev.Target;

                doctor.ShowHint("Ты начинаешь оживлять игрока!");

                if (target != null)
                {
                    target.ShowHint("Тебя пытаются оживить...");
                }
            }

        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            Server.FriendlyFire = true;
            traitorId = null;
            traitor = null;
            traitorActive = false;
        }


        private void OnAddingTarget(AddingTargetEventArgs ev)
            {
                var scp096 = ev.Player;   
                var target = ev.Target;   

                target.ShowHint("Ты видел его лицо...");
                scp096.ShowHint($"Новая цель: {target.Nickname}");
            }



        //ПОБЕГ ИГРОКА



        private void OnEscaping(EscapingEventArgs ev)
        {
            ev.IsAllowed = false;

            Player player = ev.Player;

            player.Role.Set(RoleTypeId.Tutorial);

            player.ClearInventory();

            var ntfCard = player.AddItem(ItemType.KeycardMTFPrivate);
            var chaosCard = player.AddItem(ItemType.KeycardChaosInsurgency);

            player.Broadcast(10, "Выбери сторону:\nДержи карту в руках 10 секунд");

            choosingPlayers[player] = Timing.RunCoroutine(ChooseSide(player));
        }

        private IEnumerator<float> ChooseSide(Player player)
        {
            float time = 10f;

            while (time > 0)
            {
                if (player == null || !player.IsAlive)
                    yield break;

                player.ShowHint($"Выбор стороны: {time:F0} сек");

                time--;
                yield return Timing.WaitForSeconds(1f);
            }

            var item = player.CurrentItem;

            if (item == null)
            {
                player.Broadcast(5, "Ты не выбрал сторону!");
                yield break;
            }

            if (item.Type == ItemType.KeycardMTFPrivate)
            {
                SpawnNTF(player);
            }
            else if (item.Type == ItemType.KeycardChaosInsurgency)
            {
                SpawnChaos(player);
            }
            else
            {
                player.Broadcast(5, "Неверный предмет!");
            }
        }


        private void SpawnNTF(Player player)
        {
            player.Role.Set(RoleTypeId.NtfPrivate);
            player.Broadcast(5, "Ты выбрал NTF!");
        }

        private void SpawnChaos(Player player)
        {
            player.Role.Set(RoleTypeId.ChaosRifleman);
            player.Broadcast(5, "Ты выбрал Chaos!");
        }


        public void OnDecontaminating(DecontaminatingEventArgs ev)
        {
            Timing.CallDelayed(Config.Decontamination.CassieDelay, () =>
            {
                Exiled.API.Features.Cassie.Message(Config.Decontamination.CassieMessage, isNoisy: true);
            });

            Map.Broadcast(15, Config.Decontamination.Broadcast);

            Map.ChangeLightsColor(Color.red);

            Timing.CallDelayed(Config.Decontamination.LightDuration, () =>
            {
                Map.ResetLightsColor();
            });
        }


        private void ThanksCoin(FlippingCoinEventArgs ev)
        {
            if (!Config.Coin.Enabled)
                return;

            ev.IsAllowed = false;

            var player = ev.Player;

            var coin = player.Items.FirstOrDefault(i => i.Type == ItemType.Coin);
            if (Config.Coin.RemoveCoin && coin != null)
                player.RemoveItem(coin);

            int roll = rnd.Next(100);
            int current = 0;

            if (roll < (current += Config.Coin.DeathChance))
            {
                player.Kill("Монетка решила твою судьбу");
                return;
            }

            if (roll < (current += Config.Coin.GrenadeChance))
            {
                var grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                grenade.FuseTime = Config.Coin.GrenadeFuse;
                grenade.SpawnActive(player.Position);

                player.ShowHint("<color=red>ОЙ...</color>", 3);
                return;
            }

            if (roll < (current += Config.Coin.SeveredHandsChance))
            {
                player.EnableEffect(EffectType.SeveredHands, Config.Coin.EffectDuration);
                return;
            }

            if (roll < (current += Config.Coin.NegativeEffectChance))
            {
                player.ApplyRandomEffect(EffectCategory.Negative);
                player.ShowHint(Config.Messages.CoinBad, 3);
                return;
            }

            if (roll < (current += Config.Coin.NothingChance))
            {
                player.ShowHint(Config.Messages.CoinNothing, 3);
                return;
            }

            if (roll < (current += Config.Coin.PositiveEffectChance))
            {
                player.ApplyRandomEffect(EffectCategory.Positive);
                player.ShowHint(Config.Messages.CoinGood, 3);
                return;
            }

            var reward = Config.Coin.Rewards[rnd.Next(Config.Coin.Rewards.Count)];
            player.AddItem(reward);
            player.ShowHint($"<color=yellow>Вам выпало: {reward}</color>", 3);
        }



        private void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (ev.Players == null || ev.Players.Count == 0)
                return;

            if (rnd.Next(100) > 40)
                return;

            traitorActive = false;

            traitor = ev.Players.ElementAt(rnd.Next(ev.Players.Count));
            traitorId = traitor.UserId;

            Timing.CallDelayed(0.5f, () =>
            {
                var pl = Player.Get(traitorId);

                if (pl == null || !pl.IsAlive)
                    return;

                SetupTraitor(ev);
            });
        }


        public void ForceTraitor(Player player, string team)
        {

            traitorId = player.UserId;

            traitor = player;
            traitorActive = false;

            Timing.CallDelayed(0.5f, () =>
            {
                if (traitor == null || !traitor.IsAlive)
                    return;

                traitor.ClearInventory();

                if (team == "ntf")
                {
                    traitor.Role.Set(RoleTypeId.NtfPrivate);

                    traitor.AddItem(ItemType.GunAK);
                    traitor.AddItem(ItemType.ArmorCombat);
                    traitor.AddItem(ItemType.Medkit);
                }
                else if (team == "chaos")
                {
                    traitor.Role.Set(RoleTypeId.ChaosRifleman);

                    traitor.AddItem(ItemType.GunCrossvec);
                    traitor.AddItem(ItemType.ArmorHeavy);
                    traitor.AddItem(ItemType.Medkit);
                }

                traitor.Broadcast(10, "<color=red>Ты предатель (принудительно)</color>");

                Timing.RunCoroutine(TraitorTimer());
            });
        }


        private void SetupTraitor(RespawningTeamEventArgs ev)
        {
            var pl = Player.Get(traitorId);

            if (pl == null)
                return;

            pl.Role.Set(RoleTypeId.ChaosRifleman);

            pl.ClearInventory();

            pl.AddItem(ItemType.GunAK);
            pl.AddItem(ItemType.ArmorCombat);
            pl.AddItem(ItemType.Medkit);

            pl.Broadcast(10, "<color=red>Ты предатель. Активация через 60 секунд</color>");

            Timing.RunCoroutine(TraitorTimer());
        }

        private IEnumerator<float> TraitorTimer()
        {
            float time = 60f;

            while (time > 0)
            {
                if (traitor == null)
                    yield break;

                traitor.ShowHint($"Активация через {Mathf.CeilToInt(time)} сек", 1f);

                time -= 1f;
                yield return Timing.WaitForSeconds(1f);
            }

            traitorActive = true;

            if (traitor != null)
                traitor.Broadcast(5, "<color=green>Ты активирован. Действуй.</color>");
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (traitorId == null)
                return;

            bool isTraitorInvolved =
                ev.Attacker?.UserId == traitorId ||
                ev.Player?.UserId == traitorId;

            if (!isTraitorInvolved)
                return;

            if (!traitorActive)
            {
                ev.IsAllowed = false;
                return;
            }

            Log.Info("TRAITOR CAN DAMAGE NOW");
        }

        private void RoundStart()
        {
            foreach (var player in Player.List)
            {
                if (player.Role == RoleTypeId.Scientist && Config.RoundStart.GiveScientistsGun)
                {
                    player.AddItem(Config.RoundStart.ScientistGun);
                }

                if (player.Role == RoleTypeId.ClassD)
                {
                    var item = Config.RoundStart.ClassDItems[
                        rnd.Next(Config.RoundStart.ClassDItems.Count)];

                    player.AddItem(item);
                }
            }
        }
    }
}