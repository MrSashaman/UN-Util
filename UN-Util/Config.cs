using System.Collections.Generic;
using Exiled.API.Interfaces;
using Exiled.API.Enums;

namespace UN_Util
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        public CoinConfig Coin { get; set; } = new CoinConfig();
        public RoundStartConfig RoundStart { get; set; } = new RoundStartConfig();
        public MessageConfig Messages { get; set; } = new MessageConfig();
        public DecontaminationConfig Decontamination { get; set; } = new DecontaminationConfig();
    }

    public class CoinConfig
    {
        public bool Enabled { get; set; } = true;
        public bool RemoveCoin { get; set; } = true;

        public int DeathChance { get; set; } = 5;
        public int GrenadeChance { get; set; } = 10;
        public int SeveredHandsChance { get; set; } = 10;
        public int NegativeEffectChance { get; set; } = 15;
        public int NothingChance { get; set; } = 20;
        public int PositiveEffectChance { get; set; } = 20;
        public int RewardChance { get; set; } = 20;

        public float GrenadeFuse { get; set; } = 2f;
        public float EffectDuration { get; set; } = 10f;

        public List<ItemType> Rewards { get; set; } = new List<ItemType>()
        {
            ItemType.Medkit,
            ItemType.Flashlight,
            ItemType.Coin,
            ItemType.KeycardJanitor,
            ItemType.Adrenaline,
            ItemType.GunFSP9
        };
    }

    public class RoundStartConfig
    {
        public bool GiveScientistsGun { get; set; } = true;
        public ItemType ScientistGun { get; set; } = ItemType.GunRevolver;

        public List<ItemType> ClassDItems { get; set; } = new List<ItemType>()
        {
            ItemType.Flashlight,
            ItemType.Lantern,
            ItemType.Coin
        };
    }

    public class MessageConfig
    {
        public string Welcome { get; set; } = "Привет {nickname}!";
        public string DeathHint { get; set; } = "Ты умер :(";
        public string DeathBroadcast { get; set; } = "<color=orange>.res</color> чтобы возродиться";

        public string CoinBad { get; set; } = "<color=red>Что-то пошло не так...</color>";
        public string CoinGood { get; set; } = "<color=green>Тебе повезло!</color>";
        public string CoinNothing { get; set; } = "Ничего не произошло...";
    }

    public class DecontaminationConfig
    {
        public string CassieMessage { get; set; } =
            "pitch_0.7 .G3 warning . light containment zone decontamination process has begun . all personnel evacuate immediately pitch_1.0";

        public string Broadcast { get; set; } =
            "<color=red>ВНИМАНИЕ: ДЕЗИНФЕКЦИЯ ЛЗС НАЧАЛАСЬ!</color>";

        public float CassieDelay { get; set; } = 3f;
        public float LightDuration { get; set; } = 10f;
    }
}