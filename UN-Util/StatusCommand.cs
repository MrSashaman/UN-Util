using CommandSystem;
using Exiled.API.Features;
using System;

namespace UN_Util.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class StatusCommand : ICommand
    {
        public string Command => "status";
        public string[] Aliases => new string[] { "st" };
        public string Description => "Показывает статус плагина";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var plugin = Plugin.Singleton;

            if (plugin == null)
            {
                response = "Плагин не найден или не загружен!";
                return false;
            }

            bool isEnabled = plugin.IsPluginEnabled;


            response = $"Статус плагина:\n" +
                       $"Включён: {(isEnabled ? "Да" : "Нет")}\n";
            return true;
        }
    }
}