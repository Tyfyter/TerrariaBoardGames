using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Steamworks;

namespace BoardGames {
    public class InviteCommand : ModCommand {
        public override string Command => "game";

        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args) {
            string game = args[0].ToLower();
            string mode = args.Length > 1 ?args[1]:(Main.netMode==NetmodeID.SinglePlayer?"local":"online");
            switch(mode) {
                case "local":
                BoardGames.Instance.OpenGameByName(game, UI.GameMode.LOCAL);
                break;
                case "ai":
                BoardGames.Instance.OpenGameByName(game, UI.GameMode.AI);
                break;
                case "online":
                if(Main.netMode == NetmodeID.SinglePlayer) {
                    Main.NewText("Online multiplayer is only available in online multiplayer");
                    break;
                }
                Main.NewText("usage: /game <game> <player name|player id>");
                break;
                default:
                if(Main.netMode == NetmodeID.SinglePlayer) {
                    Main.NewText("Online multiplayer is only available in online multiplayer");
                    break;
                }
                if(int.TryParse(mode, out int otherID)) {

                } else {

                }
                break;
            }
        }
    }
}
