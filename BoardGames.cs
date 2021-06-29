using BoardGames.Textures.Chess;
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
		public GameUI Game;
        public override void Load() {
            Instance = this;
            string[] chessPieceNames = Chess_Piece.PieceNames;
            string pieceName;
            Chess_Piece.Pieces = new int[12];
            for(int i = 0; i < 6; i++) {
                pieceName = chessPieceNames[i];
                AddItem("White_"+pieceName, new Chess_Piece(Chess_Piece.Moves.FromName(pieceName), true));
                AddItem("Black_"+pieceName, new Chess_Piece(Chess_Piece.Moves.FromName(pieceName), false));
            }
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
            Chess_Piece.Pieces = null;
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
            ModPacket bouncePacket;
            switch(Main.netMode) {
                case NetmodeID.Server:
                switch(reader.ReadByte()) {
                    case 0:
                    bouncePacket = Instance.GetPacket(13);
                    bouncePacket.Write((byte)0);
                    bouncePacket.Write(whoAmI);
                    bouncePacket.Write(reader.ReadInt32());
                    bouncePacket.Write(reader.ReadInt32());
                    bouncePacket.Send(reader.ReadInt32());
                    break;
                    case 1:
                    bouncePacket = Instance.GetPacket(9);
                    bouncePacket.Write((byte)1);
                    bouncePacket.Write(whoAmI);
                    bouncePacket.Write(reader.ReadInt32());
                    bouncePacket.Send(reader.ReadInt32());
                    break;
                    case 2:
                    bouncePacket = Instance.GetPacket(5);
                    bouncePacket.Write((byte)2);
                    bouncePacket.Write(whoAmI);
                    bouncePacket.Send(reader.ReadInt32());
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
                        Instance.Game.owner = (Game.otherPlayerId<Main.myPlayer)==GameUI.rand.NextBool()?1:0;
                        Game.gameInactive = false;
                        bouncePacket = Instance.GetPacket(5);
                        bouncePacket.Write((byte)2);
                        bouncePacket.Write(Game.otherPlayerId);
                        bouncePacket.Send();
                    }
                    break;
                    case 2:
                    Game.gameInactive = false;
                    Game.SetupGame();
                    break;
                }
                break;
            }
        }
        public static void TestUr() {
            Instance.OpenGame<Ur_UI>();
        }
        public static void TestChess() {
            Instance.OpenGame<Chess_UI>();
        }
    }
}