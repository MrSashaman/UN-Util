using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using System;

namespace UN_Util
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class SuicideCommand : ICommand
    {

        //Команда .suicide

        public string Command => "suicide";
        public string[] Aliases => new string[] { "selfkill" };
        public string Description => "Убить себя";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (player == null)
            {
                response = "Команда только для игроков.";
                return false;
            }

            if (!player.IsAlive)
            {

                player.ShowHint("Команду можно использовать только будучи живым.", 5);
                response = "Вы должны быть живы.";
                return false;
            }

            player.Kill("Суицид - не выход");
            player.ShowHint("Вы совершили самоубийство.", 5);

            response = "Вы убили себя.";
            return true;
        }
    }
}