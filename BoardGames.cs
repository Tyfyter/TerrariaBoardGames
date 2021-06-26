using BoardGames.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Utilities;

namespace BoardGames {
	public class BoardGames : Mod {
        public static BoardGames Instance { get; private set; }
        public static Texture2D EmptySlotTexture { get; private set; }
        public static event Action UnloadTextures;
		internal UserInterface UI;
		internal GameUI Game;
        public override void Load() {
            Instance = this;
			if (Main.netMode!=NetmodeID.Server){
                EmptySlotTexture = ModContent.GetTexture("BoardGames/Textures/Empty");
				UI = new UserInterface();
			}
        }
        public override void Unload() {
            EmptySlotTexture = null;
            if(!(UnloadTextures is null))UnloadTextures();
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
        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            switch(Main.netMode) {
                case NetmodeID.Server:
                ModPacket packet;
                switch(reader.ReadByte()) {
                    case 0:
                    packet = Instance.GetPacket(13);
                    packet.Write((byte)0);
                    packet.Write(whoAmI);
                    packet.Write(reader.ReadInt32());
                    packet.Write(reader.ReadInt32());
                    packet.Send(reader.ReadInt32());
                    break;
                    case 1:
                    packet = Instance.GetPacket(9);
                    packet.Write((byte)1);
                    packet.Write(whoAmI);
                    packet.Write(reader.ReadInt32());
                    packet.Send(reader.ReadInt32());
                    break;
                }
                break;
                case NetmodeID.MultiplayerClient:
                switch(reader.ReadByte()) {
                    case 0:
                    if(reader.ReadInt32() == Game.otherPlayerId) {
                        Game.SelectPiece(new Point(reader.ReadInt32(), reader.ReadInt32()));
                    }
                    break;
                    case 1:
                    if(reader.ReadInt32() == Game.otherPlayerId) {
                        GameUI.rand = new UnifiedRandom(reader.ReadInt32());
                    }
                    break;
                }
                break;
            }
        }
        public static void TestUr() {
            Instance.OpenGame<UrUI>();
        }
    }
}