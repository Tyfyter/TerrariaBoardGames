using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.UI.Chat;

namespace BoardGames.UI {
    public class GameInviteTagHandler : ITagHandler {
        private class GameInviteSnippet : TextSnippet {
            bool accept;
            int sender;
            string game;
            public GameInviteSnippet(bool accept, int sender, string game) : base(accept ? "✓" : "✗", accept ? Color.Green : Color.Red) {
                this.accept = accept;
                this.sender = sender;
                this.game = game;
                CheckForHover = true;
            }

            public override void OnClick() {
                IngameOptions.Close();
                foreach(ChatLine chatLine in Main.chatLine.Where(line => line.parsedText.Contains(this))) {
                    chatLine.text = "You have "+(accept?"accepted":"declined")+" a " + game + " invitation from " + Main.player[sender].name;
                    chatLine.color = accept?Color.Green:Color.Red;
                    chatLine.parsedText = ChatManager.ParseMessage(chatLine.text, chatLine.color).ToArray();
                }

            }
        }
        public TextSnippet Parse(string text, Color baseColor, string options) {
            bool accept = true;
            string game = "";
            string[] array = options.Split(',');
            accept = array[0]=="a";
            int.TryParse(array[1], out int sender);
            game = text;
		    return new GameInviteSnippet(accept, sender, game) {
			    DeleteWhole = true
		    };
	    }

	    public static string GenerateTag(bool accept, int sender, string game) {
		    return $"[game/{(accept?"a":"d")},{sender}:{game}]";
	    }
    }
}
