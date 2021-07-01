using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace BoardGames.UI {
    public class GameSelectorMenu : UIState {
        public static Dictionary<string, (string[], Texture2D)> Games { get; private set; }
        public static event Action AddExternalGames;
        public static void LoadTextures() {
            if(!(Games is null)) return;
            Games = new Dictionary<string, (string[], Texture2D)>{};
            AddGame("Ur");
            AddGame("Chess");
            if(!(AddExternalGames is null)) {
                AddExternalGames();
                AddExternalGames = null;
            }
            BoardGames.UnloadTextures += UnloadTextures;
        }
        public static void UnloadTextures() {
            Games = null;
        }
        public static void AddGame(string name, string modOrigin = "BoardGames") {
            Games.Add(name.ToLower(), (new string[]{$"Mods.{modOrigin}.{name}.Name",$"Mods.{modOrigin}.{name}.Description"}, ModContent.GetTexture(modOrigin+"/Textures/Icons/"+name)) );
        }
        public override void OnActivate() {
		    Main.PlaySound(SoundID.MenuOpen);
		    Main.playerInventory = false;
        }
        public override void OnDeactivate() {
		    Main.PlaySound(SoundID.MenuClose);
        }
    }
    public class GameSelectorItem : UIElement {

    }
}
