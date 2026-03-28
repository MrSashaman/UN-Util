using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Lockers;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using MapEvents = Exiled.Events.Handlers.Map;

namespace UN_Util
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "UnitedUtil";
        public override string Prefix => "UN_Utils";
        public override string Author => "mrSashaman";
        public override Version Version => new Version(0, 1, 3);

        private readonly System.Random rnd = new System.Random();
        private Dictionary<Player, CoroutineHandle> choosingPlayers = new Dictionary<Player, CoroutineHandle>();

        private Player traitor;
        private string traitorId;
        private Locker anomalousLocker;
        private Room anomalousRoom;
        private readonly System.Random lockerRnd = new System.Random();
        private bool traitorActive = false;

        public static Plugin Singleton;
        public bool IsPluginEnabled { get; private set; }

        public override void OnEnabled()
        {
            Singleton = this;
            IsPluginEnabled = true;

            Exiled.Events.Handlers.Server.WaitingForPlayers += Hello;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.RoundStarted += RoundStart;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawningTeam;

            Exiled.Events.Handlers.Player.Verified += PlayerVerified;
            Exiled.Events.Handlers.Player.Died += OnDead;
            MapEvents.Generated += OnMapGenerated;
            Exiled.Events.Handlers.Player.InteractingLocker += OnInteractingLocker;
            Exiled.Events.Handlers.Player.FlippingCoin += ThanksCoin;
            Exiled.Events.Handlers.Player.Hurting += OnHurting;
            Exiled.Events.Handlers.Player.Escaping += OnEscaping;

            Exiled.Events.Handlers.Scp049.StartingRecall += OnStartingRecall;
            Exiled.Events.Handlers.Player.TriggeringTesla += OnTriggeringTesla;
            Exiled.Events.Handlers.Scp096.AddingTarget += OnAddingTarget;

            MapEvents.Decontaminating += OnDecontaminating;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            IsPluginEnabled = false;

            Exiled.Events.Handlers.Server.WaitingForPlayers -= Hello;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.RoundStarted -= RoundStart;
            Exiled.Events.Handlers.Player.TriggeringTesla -= OnTriggeringTesla;
            MapEvents.Generated -= OnMapGenerated;
            Exiled.Events.Handlers.Player.InteractingLocker -= OnInteractingLocker;

            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawningTeam;

            Exiled.Events.Handlers.Player.Verified -= PlayerVerified;
            Exiled.Events.Handlers.Player.Died -= OnDead;
            Exiled.Events.Handlers.Player.FlippingCoin -= ThanksCoin;
            Exiled.Events.Handlers.Player.Hurting -= OnHurting;
            Exiled.Events.Handlers.Player.Escaping -= OnEscaping;

            Exiled.Events.Handlers.Scp049.StartingRecall -= OnStartingRecall;
            Exiled.Events.Handlers.Scp096.AddingTarget -= OnAddingTarget;

            MapEvents.Decontaminating -= OnDecontaminating;

            base.OnDisabled();
        }

        private void Hello()
        {
            Log.Info("****WELCOME*****");
            Log.Info("Dev Discord: https://discord.gg/5pBt7cj8B9 ");
            Log.Info("Version: " + Version);
        }


        private void OnMapGenerated()
        {
            var room = Room.List.FirstOrDefault(r => r.Type == RoomType.LczGlassBox);
            anomalousRoom = Room.List.FirstOrDefault(r => r.Type == RoomType.LczGlassBox);
            if (room == null)
            {
                Log.Warn("GR-18 не найдена!");
                return;
            }

            var lockers = Locker.List.Where(l => l.Room == room).ToList();

            if (lockers.Count == 0)
            {
                Log.Warn("В GR-18 нет шкафов!");
                return;
            }

            anomalousLocker = lockers[lockerRnd.Next(lockers.Count)];

            Log.Info("Аномальный шкаф выбран!");
        }



        private void OnInteractingLocker(InteractingLockerEventArgs ev)
        {
            if (ev.Player == null || ev.InteractingLocker == null) return;

            if (anomalousLocker == null || ev.InteractingLocker.Base != anomalousLocker.Base)
                return;

            if (ev.Player.Role.Team == Team.SCPs)
                return;

            if (anomalousRoom == null) return;

            int roll = lockerRnd.Next(100);

            anomalousRoom.Color = Color.yellow;

            Timing.CallDelayed(3f, () =>
            {
                if (anomalousRoom != null)
                    anomalousRoom.ResetColor();
            });

            if (roll < 10)
            {
                ev.Player.EnableEffect(EffectType.SeveredHands, 10f);
                ev.Player.ShowHint("Аномалия.. Руки оторваны", 3f);
            }
            else if (roll < 20)
            {
                ev.Player.EnableEffect(EffectType.Blinded, 5f);
                ev.Player.ShowHint("Аномалия.. Темнота", 3f);
            }
            else if (roll < 30)
            {
                ev.Player.EnableEffect(EffectType.Ensnared, 6f);
                ev.Player.ShowHint("Аномалия.. Ты не можешь двигаться", 3f);
            }
            else if (roll < 40)
            {
                ev.Player.EnableEffect(EffectType.Poisoned, 8f);
                ev.Player.ShowHint("Аномалия.. Тебе плохо", 3f);
            }
            else if (roll < 50)
            {
                ev.Player.EnableEffect(EffectType.Slowness, 10f);
                ev.Player.ShowHint("Аномалия.. Ты замедлен", 3f);
            }
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

        private void OnAddingTarget(AddingTargetEventArgs ev)
        {
            ev.Target.ShowHint("Ты видел его лицо...");
            ev.Player.ShowHint($"Новая цель: {ev.Target.Nickname}");
        }

        private void OnStartingRecall(StartingRecallEventArgs ev)
        {
            ev.Player.ShowHint("Ты начинаешь оживлять игрока!");
            ev.Target?.ShowHint("Тебя пытаются оживить...");
        }

        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            Server.FriendlyFire = true;
            traitorId = null;
            traitor = null;
            traitorActive = false;

            Log.Info("FF Включен");
        }

        private void OnEscaping(EscapingEventArgs ev)
        {
            ev.IsAllowed = false;

            var player = ev.Player;

            player.Role.Set(RoleTypeId.Tutorial);
            player.ClearInventory();

            player.AddItem(ItemType.KeycardMTFPrivate);
            player.AddItem(ItemType.KeycardChaosInsurgency);

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
                SpawnNTF(player);
            else if (item.Type == ItemType.KeycardChaosInsurgency)
                SpawnChaos(player);
            else
                player.Broadcast(5, "Неверный предмет!");
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


        private void OnTriggeringTesla(TriggeringTeslaEventArgs ev)
        {
            if (ev.Player == null) return;

            if (ev.Player.Role.Team != Team.SCPs)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnDecontaminating(DecontaminatingEventArgs ev)
        {
            Timing.CallDelayed(Config.Decontamination.CassieDelay,
                () => Exiled.API.Features.Cassie.Message(Config.Decontamination.CassieMessage, true));

            Map.Broadcast(15, Config.Decontamination.Broadcast);
            Map.ChangeLightsColor(Color.red);

            Timing.CallDelayed(Config.Decontamination.LightDuration,
                () => Map.ResetLightsColor());
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
                return;
            }

            if (roll < (current += Config.Coin.NothingChance))
                return;

            if (roll < (current += Config.Coin.PositiveEffectChance))
            {
                player.ApplyRandomEffect(EffectCategory.Positive);
                return;
            }

            var reward = Config.Coin.Rewards[rnd.Next(Config.Coin.Rewards.Count)];
            player.AddItem(reward);
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
                if (pl == null || !pl.IsAlive) return;

                SetupTraitor();
            });
        }

        private void SetupTraitor()
        {
            var pl = Player.Get(traitorId);
            if (pl == null) return;

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

            traitor?.Broadcast(5, "<color=green>Ты активирован. Действуй.</color>");
        }

        private void OnHurting(HurtingEventArgs ev)
        {
            if (traitorId == null) return;

            bool involved =
                ev.Attacker?.UserId == traitorId ||
                ev.Player?.UserId == traitorId;

            if (!involved) return;

            if (!traitorActive)
                ev.IsAllowed = false;
        }

        private void RoundStart()
        {
            foreach (var player in Player.List)
            {
                if (player.Role == RoleTypeId.Scientist && Config.RoundStart.GiveScientistsGun)
                    player.AddItem(Config.RoundStart.ScientistGun);

                if (player.Role == RoleTypeId.ClassD)
                    player.AddItem(Config.RoundStart.ClassDItems[
                        rnd.Next(Config.RoundStart.ClassDItems.Count)]);
            }
        }
    }
}