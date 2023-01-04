using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using System;
using System.Linq;
using static BoardGames.UI.GameMode;
using BoardGames.Textures.Pieces;
using Terraria.Utilities;
using System.Runtime.CompilerServices;
using BoardGames.Misc;

namespace BoardGames.UI {
	public abstract class GameUI : UIState {
		public int owner = 0;
		public int currentPlayer = 0;
		public GamePieceItemSlot[,] gamePieces;
		public Point? selectedPiece;
		bool oldMouseLeft;
		public GameMode gameMode;
		public int aiMoveTimeout = 0;
		public bool gameInactive;
		public int otherPlayerId = -1;
		public static UnifiedRandom rand;
		public int endGameTimeout = 0;
		public bool JustClicked => Main.mouseLeft && !oldMouseLeft;
		public Action<GameUI> customAI = null;
		public List<Point> moveMemory;
		public override void OnInitialize() {
			Main.UIScaleMatrix.Decompose(out Vector3 scale, out Quaternion ignore, out Vector3 ignore2);
			Vector2 basePosition = new Vector2((float)(Main.screenWidth * 0.05), (float)(Main.screenHeight * 0.4));
			Vector2 slotSize = new Vector2(51 * scale.X, 51 * scale.Y);
			moveMemory = new List<Point> { };
			/**gameMode = ONLINE;
            if(Main.netMode == NetmodeID.SinglePlayer) {
                gameMode = LOCAL;
            } else {
                gameInactive = true;
                for(int i = 0; i < Main.maxPlayers; i++) {
                    if(i!=Main.myPlayer&&(Main.player[i]?.active??false)) {
                        otherPlayerId = i;
                        SyncGame(otherPlayerId);
                        /*if(i<Main.myPlayer) {
                            owner = 1;
                        }* /
                        Main.NewText("connected to "+Main.player[i].name);
                        break;
                    }
                }
            }*/
			Init(scale, basePosition, slotSize);
		}

		public void SetMode(GameMode gameMode, int otherPlayer) {
			this.gameMode = gameMode;
			if (gameMode == ONLINE) {
				gameInactive = true;
				otherPlayerId = otherPlayer;
				SyncGame(otherPlayerId);
			}
		}

		protected GamePieceItemSlot AddSlot(Item item, Vector2 position, AutoCastingAsset<Texture2D> texture, bool usePercent = false, float slotScale = 1f, Action<Point> HighlightMoves = null) {
			GamePieceItemSlot itemSlot = new GamePieceItemSlot(texture, slotScale, item) {
				HighlightMoves = HighlightMoves
			};
			if (usePercent) {
				itemSlot.Left = new StyleDimension { Percent = position.X };
				itemSlot.Top = new StyleDimension { Percent = position.Y };
			} else {
				itemSlot.Left = new StyleDimension { Pixels = position.X };
				itemSlot.Top = new StyleDimension { Pixels = position.Y };
			}
			Append(itemSlot);
			return itemSlot;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool? SlotEmpty(Point slot) {
			return SlotEmpty(slot.X, slot.Y);
		}
		public bool? SlotEmpty(int X, int Y) {
			if (X < 0 || Y < 0 || X >= gamePieces.GetLength(0) || Y >= gamePieces.GetLength(1)) {
				return null;
			}
			return (gamePieces[X, Y]?.item?.IsAir) ?? true;
		}
		public override void Update(GameTime gameTime) {
			if (gameInactive) {
				aiMoveTimeout = 0;
			}
			Point boardSize = new Point(gamePieces.GetLength(0), gamePieces.GetLength(1));
			for (int i = 0; i < gamePieces.Length; i++) {
				gamePieces[i % boardSize.X, i / boardSize.X].glowing = false;
				gamePieces[i % boardSize.X, i / boardSize.X].index = new Point(i % boardSize.X, i / boardSize.X);
			}
			for (int i = 0; i < gamePieces.Length; i++) {
				gamePieces[i % boardSize.X, i / boardSize.X].Update(gameTime);
			}
			oldMouseLeft = Main.mouseLeft;
		}
		protected abstract void Init(Vector3 scale, Vector2 basePosition, Vector2 slotSize);
		public abstract void TryLoadTextures();
		public virtual void SelectPiece(Point target) {
			if (selectedPiece == target) {
				selectedPiece = null;
			} else {
				selectedPiece = target;
			}
		}
		public virtual Color GetTileColor(bool glowing) {
			return gameInactive ? new Color(128, 128, 128, 128) : (glowing ? Color.White : Color.LightGray);
		}
		public virtual void SetupGame() { }
		public static void SyncGame(int other) {
			int seed = Main.rand.Next(int.MinValue, int.MaxValue);
			ModPacket packet = BoardGames.Instance.GetPacket(9);
			packet.Write(PacketType.StartupSync);
			packet.Write(seed);
			packet.Write(other);
			packet.Send();
			rand = new UnifiedRandom(seed);
			BoardGames.Instance.Game.owner = (other < Main.myPlayer) == rand.NextBool() ? 1 : 0;
			//Game.gameInactive = false;
		}
	}
	public enum GameMode {
		AI,
		LOCAL,
		ONLINE
	}
}