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
			if (args.Length == 0) {
				BoardGames.OpenGameSelector();
				return;
			}
			string game = args[0].ToLower();
			if (args.Length < 2) {
				BoardGames.Instance.selectedGame = game;
				return;
			}
			string mode = args[1].ToLower();
			switch (mode) {
				case "local":
				BoardGames.OpenGameByName(game, UI.GameMode.LOCAL);
				break;
				case "ai":
				BoardGames.OpenGameByName(game, UI.GameMode.AI);
				break;
				case "online":
				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText("Online multiplayer is only available in online multiplayer");
					break;
				}
				Main.NewText("usage: /game <game> <player name|player id>");
				break;
				default:
				if (Main.netMode == NetmodeID.SinglePlayer) {
					Main.NewText("Online multiplayer is only available in online multiplayer");
					break;
				}
				int otherID = -1;
				if (int.TryParse(mode, out otherID)) {
					if (otherID == Main.myPlayer) {
						Main.NewText("You can't invite yourself to a game");
						break;
					}
				} else {
					mode = mode.ToLower();
					otherID = -1;
					float dist = float.PositiveInfinity;
					float curDist;
					for (int i = 0; i <= Main.maxPlayers; i++) {
						if (i == Main.myPlayer || !mode.Equals(Main.player[i].name.ToLower())) {
							continue;
						}
						curDist = Main.LocalPlayer.Distance(Main.player[i].Center);
						if (curDist < dist) {
							dist = curDist;
							otherID = i;
						}
					}
				}
				if (otherID != -1) {
					string settings = "";
					if (args.Length > 2) {
						settings = args[2].ToLower();
					}
					BoardGames.SendGameRequest(otherID, game, settings);
					Main.NewText($"Sent invitation to {Main.player[otherID].name}");
				} else {
					Main.NewText($"Could not find player \"{mode}\"");
				}
				break;
			}
		}
	}
}
