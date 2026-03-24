using CommandSystem;
using Exiled.API.Features;
using PlayerRoles;
using System;
using System.Collections.Generic;

namespace UN_Util
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class RespawnCommand : ICommand
    {

        //Команда .res, работает в начале раунда только когда игрок находиться в spectator, имеет задержку в 4 минуты.

        public string Command => "res";
        public string[] Aliases => new string[] { "respawn" };
        public string Description => "Респавн с шансом 50/50";

        private static readonly Dictionary<string, DateTime> cooldowns = new Dictionary<string, DateTime>();

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (player == null)
            {
                response = "Только для игроков.";
                return false;
            }

            if (player.Role.Type != RoleTypeId.Spectator)
            {
                response = "Команда доступна только наблюдателям.";
                return false;
            }

            if (Round.ElapsedTime.TotalMinutes > 5)
            {
                response = "Прошло более 5 минут с начала раунда.";
                return false;
            }

            string id = player.UserId;

            if (cooldowns.TryGetValue(id, out DateTime lastUse))
            {
                double secondsLeft = (lastUse.AddMinutes(4) - DateTime.Now).TotalSeconds;

                if (secondsLeft > 0)
                {
                    response = $"Подождите {Math.Ceiling(secondsLeft)} сек.";
                    return false;
                }
            }

            cooldowns[id] = DateTime.Now;

            Random rnd = new Random();
            bool isDClass = rnd.Next(2) == 0;

            if (isDClass)
            {
                player.Role.Set(RoleTypeId.ClassD);
                player.Broadcast(5, "<color=orange>Вы возродились за Class-D</color>");
            }
            else
            {
                player.Role.Set(RoleTypeId.Scientist);
                player.Broadcast(5, "<color=green>Вы возродились за Scientist</color>");
            }

            response = "Вы возрождены.";
            return true;
        }
    }
}