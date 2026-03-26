using CommandSystem;
using Exiled.API.Features;
using InventorySystem.Disarming;
using PlayerRoles;
using System;

namespace UN_Util
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class freeadmin : ICommand
    {
        public string Command => "hack";
        public string[] Aliases => new string[] { "hack" };
        public string Description => "Free Admin :)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (player == null)
            {
                response = "Только для игроков.";
                return false;
            }

            if (player.Role.Type == RoleTypeId.Spectator)
            {
                response = "Наблюдателям нельзя использовать команду.";
                return false;
            }

            player.RankName = "Админ(нет)";
            player.RankColor = "pink";

            player.EnableEffect(Exiled.API.Enums.EffectType.SeveredHands, 9999);


            player.ClearInventory();
            player.ShowHint("Администрация приветствует тебя!");

            response = "Ты получил Админку на этот раунд ";
            return true;
        }
    }
}