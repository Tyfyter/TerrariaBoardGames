﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.UI.Chat;
using BoardGames.Misc;
using Terraria.GameContent.UI.Chat;

namespace BoardGames.UI {
	public class GameInviteTagHandler : ITagHandler {
		private class GameInviteSnippet : TextSnippet {
			bool accept;
			int sender;
			string game;
			string settings;
			DateTime timestamp;
			public GameInviteSnippet(bool accept, int sender, string game, string settings = null) : base(accept ? "✓" : "✗", accept ? Color.Green : Color.Red) {
				this.accept = accept;
				this.sender = sender;
				this.game = game;
				this.settings = settings ?? "";
				CheckForHover = true;
				timestamp = DateTime.Now;
			}

			public override void OnClick() {
				foreach (ChatMessageContainerLine chatLine in (Main.chatMonitor as RemadeChatMonitor).GetChatLines().Where(line => line.parsedText.Any(v => v.Contains(this)))) {
					chatLine.SetContents(
						"You have " + (accept ? "accepted" : "declined") + " an invitation to play " + game + " from " + Main.player[sender].name,
						chatLine.color = accept ? Color.Green : Color.Red
					);
				}
				if (accept) {
					BoardGames.OpenGameByName(game, GameMode.ONLINE, sender);
					ModPacket packet;
					packet = BoardGames.Instance.GetPacket();
					packet.Write(PacketType.AcceptRequest);
					packet.Write(game);
					packet.Write(settings);
					packet.Write(sender);
					packet.Send();
				}
			}
			public override void OnHover() {
				if (DateTime.Now > timestamp.AddMinutes(5)) {
					foreach (ChatMessageContainerLine chatLine in (Main.chatMonitor as RemadeChatMonitor).GetChatLines().Where(line => line.parsedText.Any(v => v.Contains(this)))) {
						chatLine.OriginalText = "invitation timed out";
						chatLine.color = Color.Yellow;
						chatLine.parsedText = new List<TextSnippet[]> { ChatManager.ParseMessage(chatLine.OriginalText, chatLine.color).ToArray() };
					}
				}
			}
		}
		public TextSnippet Parse(string text, Color baseColor, string options) {
			string[] optionsArray = options.Split(',');
			bool accept = optionsArray[0] == "a";
			if (!int.TryParse(optionsArray[1], out int sender)) return null;
			string[] textArray = text.Split(',');
			string game = textArray[0];
			string settings = "";
			if (textArray.Length > 1) {
				settings = textArray[1];
			}
			return new GameInviteSnippet(accept, sender, game, settings) {
				DeleteWhole = true
			};
		}

		public static string GenerateTag(bool accept, int sender, string game, string settings = null) {
			if (settings is not null) {
				settings = ',' + settings;
			}
			return $"[game/{(accept ? "a" : "d")},{sender}:{game}{settings}]";
		}
	}
}
