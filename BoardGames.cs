using BoardGames.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace BoardGames {
	public class BoardGames : Mod {
        public static BoardGames Instance { get; private set; }
        public static Texture2D EmptySlotTexture { get; private set; }
        public static event Action UnloadTextures;
		internal UserInterface UI;
		internal GameUI Game;
        public override void Load() {
            Instance = this;
			if (!Main.dedServ){
                EmptySlotTexture = ModContent.GetTexture("BoardGames/Textures/Empty");
				UI = new UserInterface();
			}
        }
        public override void Unload() {
            EmptySlotTexture = null;
            UnloadTextures();
            UnloadTextures = null;
            Instance = null;
        }
        public void OpenGame<GameType>() where GameType : GameUI, new(){
            Game = new GameType();
            Game.TryLoadTextures();
            Game.Activate();
            UI.SetState(Game);
        }
		public override void UpdateUI(GameTime gameTime) {
			UI?.Update(gameTime);
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {
				layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					"BoardGames: GameUI",
					delegate {
						// If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
						UI.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
        public static void TestUr() {
            Instance.OpenGame<UrUI>();
        }
    }
}