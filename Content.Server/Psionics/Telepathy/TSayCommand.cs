using Content.Server.Chat.Systems;
using Content.Shared.Administration;
<<<<<<< HEAD
<<<<<<< HEAD:Content.Server/Nyanotrasen/Chat/TSayCommand.cs
<<<<<<< HEAD:Content.Server/Psionics/Telepathy/TSayCommand.cs
using Content.Shared.Chat;
=======
>>>>>>> parent of 462e91c2cc (aaaaaaaaa):Content.Server/Nyanotrasen/Chat/TSayCommand.cs
using Robust.Server.Player;
=======
>>>>>>> parent of d439c5a962 (Revert "Merge branch 'VMSolidus-Psionic-Power-Refactor'"):Content.Server/Psionics/Telepathy/TSayCommand.cs
=======
using Robust.Server.Player;
>>>>>>> parent of 0e11da6316 (Final power update)
using Robust.Shared.Console;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server.Psionics.Telepathy
{
    [AnyCommand]
    internal sealed class TSayCommand : IConsoleCommand
    {
        public string Command => "tsay";
        public string Description => "Send chat messages to the telepathic.";
        public string Help => "tsay <text>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (shell.Player is not ICommonSession player)
            {
                shell.WriteError("This command cannot be run from the server.");
                return;
            }

            if (player.Status != SessionStatus.InGame)
                return;

            if (player.AttachedEntity is not {} playerEntity)
            {
                shell.WriteError("You don't have an entity!");
                return;
            }

            if (args.Length < 1)
                return;

            var message = string.Join(" ", args).Trim();
            if (string.IsNullOrEmpty(message))
                return;
            //Not sure if I should hide the logs from this. Default is false.
            EntitySystem.Get<ChatSystem>().TrySendInGameICMessage(playerEntity, message, InGameICChatType.Telepathic, ChatTransmitRange.Normal, false, shell, player);
        }
    }
}
