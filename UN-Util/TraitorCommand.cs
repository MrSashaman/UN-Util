using CommandSystem;
using Exiled.API.Features;
using PlayerRoles;
using System;
using System.Linq;

namespace UN_Util.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class TraitorCommand : ICommand
    {
        public string Command => "traitor";
        public string[] Aliases => new[] { "tr" };
        public string Description => "Сделать игрока предателем";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Использование: traitor <ник> <ntf/chaos>";
                return false;
            }

            string name = arguments.At(0);
            string teamArg = arguments.At(1).ToLower();

            var player = Player.List.FirstOrDefault(p =>
                p.Nickname.ToLower().Contains(name.ToLower()));

            if (player == null)
            {
                response = "Игрок не найден";
                return false;
            }

            var plugin = Plugin.Singleton;

            plugin.ForceTraitor(player, teamArg);

            response = $"Игрок {player.Nickname} теперь предатель ({teamArg})";
            return true;
        }
    }
}