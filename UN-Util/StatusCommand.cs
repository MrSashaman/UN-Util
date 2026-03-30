using CommandSystem;
using Exiled.API.Features;
using System;
using System.Linq;

namespace UN_Util.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class StatusCommand : ICommand
    {
        public string Command => "unut_status";
        public string[] Aliases => new string[] { "unstatus", "un_st" };
        public string Description => "STATUS UnitedUtil";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var plugin = Plugin.Singleton;

            if (plugin == null)
            {
                response = "❌ Плагин не найден или не загружен!";
                return false;
            }

            bool isEnabled = plugin.IsPluginEnabled;

            var traitor = plugin.GetType()
                .GetField("traitor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(plugin) as Player;

            var traitorActive = (bool)(plugin.GetType()
                .GetField("traitorActive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(plugin) ?? false);

            var radiationRooms = plugin.GetType()
                .GetField("radiationRooms", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(plugin) as System.Collections.ICollection;

            var choosingPlayers = plugin.GetType()
                .GetField("choosingPlayers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(plugin) as System.Collections.IDictionary;

            int online = Player.List.Count();

            response =
                "=== UnitedUtil Status ===\n" +
                $"Включён: {(isEnabled ? "🟢 Да" : "🔴 Нет")}\n\n" +

                "👥 Игроки:\n" +
                $"- Онлайн: {online}\n" +
                $"- Выбор стороны: {choosingPlayers?.Count ?? 0}\n\n" +

                "🕵️ Предатель:\n" +
                $"- Есть: {(traitor != null ? "Да" : "Нет")}\n" +
                $"- Активен: {(traitorActive ? "Да" : "Нет")}\n" +
                $"- Игрок: {(traitor != null ? traitor.Nickname : "Нет")}\n\n" +

                "☢ Радиация:\n" +
                $"- Зон: {radiationRooms?.Count ?? 0}\n\n" +

                $"📦 Версия: {plugin.Version}";

            return true;
        }
    }
}