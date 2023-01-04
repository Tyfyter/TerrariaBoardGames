using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using static BoardGames.UI.GameMode;
using BoardGames.Textures.Pieces;
using Microsoft.Xna.Framework.Graphics;
using BoardGames.Misc;
using Terraria.ID;

namespace BoardGames.UI {
	public class Ur_UI : GameUI {
		public override void TryLoadTextures() => LoadTextures();
		public static int[] GamePieceTypes { get; private set; }
		public static DieSpriteSet[] DieSet { get; private set; }
		int roll = 0;
		bool rolled = false;
		int[][] allRolls;
		Vector2[] rollOffsets;
		public int[] remainingPieces;
		public int[] activePieces;
		int endTurnTimeout = 0;
		static char[,] Grid => new char[8, 2] {
				{'r','n'},
				{'n','5'},
				{'5','n'},
				{'n','r'},
				{'o','5'},
				{'o','n'},
				{'r','n'},
				{'n','n'}
			};
		public static void LoadTextures() {
			if (RosetteTexture.HasValue) return;
			RosetteTexture = ModContent.Request<Texture2D>("BoardGames/Textures/Ur_Rosette");
			OtherTextures = new AutoCastingAsset<Texture2D>[] {
				ModContent.Request<Texture2D>("BoardGames/Textures/Ur_Base"),
				ModContent.Request<Texture2D>("BoardGames/Textures/Ur_5")
			};
			GamePieceTypes = new int[] {
				ModContent.ItemType<Textures.Pieces.Red>(),
				ModContent.ItemType<Textures.Pieces.Blue>()
			};
			DieSet = new DieSpriteSet[] {
				new DieSpriteSet("BoardGames/Textures/Pieces/Ur_Die_1", startIndex:1, endIndex:4),
				new DieSpriteSet("BoardGames/Textures/Pieces/Ur_Die_2", startIndex:1, endIndex:4)
			};
			BoardGames.UnloadTextures += UnloadTextures;
		}
		public static void UnloadTextures() {
			RosetteTexture = null;
			OtherTextures = null;
			GamePieceTypes = null;
			DieSet = null;
		}
		public static AutoCastingAsset<Texture2D> RosetteTexture { get; private set; }
		public static AutoCastingAsset<Texture2D>[] OtherTextures { get; private set; }
		protected override void Init(Vector3 scale, Vector2 basePosition, Vector2 slotSize) {
			basePosition = new Vector2((float)(Main.screenWidth * 0.5), (float)(Main.screenHeight * 0.5));
			basePosition -= slotSize * new Vector2(1.5f, 4);
			gamePieces = new GamePieceItemSlot[3, 8];
			remainingPieces = new int[] { 7, 7 };
			activePieces = new int[] { 0, 0 };
			allRolls = new int[4][];
			rollOffsets = new Vector2[4];
			if (rand is null)
				rand = new Terraria.Utilities.UnifiedRandom(Main.rand.Next(int.MinValue, int.MaxValue));
			try {
				for (int j = 0; j < 8; j++) {
					for (int i = 0; i < 3; i++) {
						gamePieces[i, j] = AddSlot(null,
							basePosition + slotSize * new Vector2(i, j),
							GetTexture(Grid[j, i % 2]),
							slotScale: scale.X,
							HighlightMoves: HighlightMoves
						);
					}
				}
			} catch (Exception e) {
				BoardGames.Instance.Logger.Warn(e);
			}
		}
		public override void Update(GameTime gameTime) {
			if (endGameTimeout > 0) {
				if (--endGameTimeout < 1) {
					this.Deactivate();
					BoardGames.Instance.UI.SetState(null);
					endGameTimeout = 0;
					return;
				}
			}
			if (remainingPieces[0] == 0 || remainingPieces[1] == 0) {
				if (!gameInactive) {
					switch (gameMode) {
						case AI:
						if (remainingPieces[0] == 0) {
							Main.NewText("Player wins", Color.Firebrick);
						} else {
							Main.NewText("AI wins", Color.DodgerBlue);
						}
						break;
						case LOCAL:
						if (remainingPieces[0] == 0) {
							Main.NewText("Player 1 wins", Color.Firebrick);
						} else {
							Main.NewText("Player 2 wins", Color.DodgerBlue);
						}
						break;
						case ONLINE:
						int notOwner = owner ^ 1;
						if (remainingPieces[0] == 0) {
							Main.NewText(Main.player[(owner * otherPlayerId) + (notOwner * Main.myPlayer)].name + " wins", Color.Firebrick);
						} else {
							Main.NewText(Main.player[(notOwner * otherPlayerId) + (owner * Main.myPlayer)].name + " wins", Color.DodgerBlue);
						}
						break;
					}
					endGameTimeout = 180;
				}
				gameInactive = true;
				return;
			}
			if (endTurnTimeout > 0) {
				if (++endTurnTimeout > 45) {
					EndTurn();
					endTurnTimeout = 0;
				}
			}
			if (aiMoveTimeout > 0) {
				if (++aiMoveTimeout > BoardGames.ai_move_time) {
					AIMove();
				}
			}
			base.Update(gameTime);
		}
		public void HighlightMoves(Point target) {
			if (!rolled || (target.X ^ 2) == (currentPlayer * 2) || Grid[target.Y, target.X % 2] == 'o') {
				return;
			}
			Point[] move;
			if (CanMoveFrom(target)) {
				move = GetMoveFrom(target);
				if (move.Length > 0 && CanMoveTo(move[0])) {
					gamePieces.Index(target).glowing = true;
					gamePieces.Index(move[0]).glowing = true;
				}
			} else if (CanMoveTo(target)) {
				move = GetMoveTo(target);
				if (move.Length > 0 && CanMoveFrom(move[0])) {
					gamePieces.Index(target).glowing = true;
					gamePieces.Index(move[0]).glowing = true;
				}
			}
		}
		public override void SelectPiece(Point target) {
			if ((target.X ^ 2) == (currentPlayer * 2)) {
				return;
			}
			if (gameMode == ONLINE && currentPlayer == owner) {
				ModPacket packet = BoardGames.Instance.GetPacket(13);
				packet.Write(PacketType.SelectTile);
				packet.Write(target.X);
				packet.Write(target.Y);
				packet.Write(otherPlayerId);
				packet.Send();
			} else if (gameMode == AI && currentPlayer == 0) {
				moveMemory.Add(target);
			}
			bool noAction = true;
			if (Grid[target.Y, target.X % 2] == 'o') {
				if (!rolled) {
					RollDice();
					noAction = false;
				}
			} else if (rolled) {
				Point slotA = new Point(-1, -1);
				Point slotB = new Point(-1, -1);
				Point[] move = GetMoveFrom(target);
				if (move.Length != 0 && CanMoveFrom(target) && CanMoveTo(move[0])) {
					slotA = new Point(target.X, target.Y);
					slotB = new Point(move[0].X, move[0].Y);
				} else {
					move = GetMoveTo(target);
					if (move.Length != 0) {
						slotA = new Point(move[0].X, move[0].Y);
						slotB = new Point(target.X, target.Y);
					}
				}
				if (slotA != new Point(-1, -1) && CanMoveFrom(slotA) && CanMoveTo(slotB)) {
					noAction = false;
					if (Grid[slotA.Y, slotA.X % 2] == 'o') {
						gamePieces.Index(slotB).SetItem(GamePieceTypes[currentPlayer]);
						activePieces[currentPlayer]++;
					} else if (Grid[slotB.Y, slotB.X % 2] == 'o') {
						gamePieces.Index(slotA).SetItem(null);
						remainingPieces[currentPlayer]--;
						activePieces[currentPlayer]--;
					} else {
						if (gamePieces[slotB.X, slotB.Y].item?.type == GamePieceTypes[currentPlayer ^ 1]) {
							activePieces[currentPlayer ^ 1]--;
						}
						gamePieces.Index(slotB).SetItem(gamePieces[slotA.X, slotA.Y].item);
						gamePieces.Index(slotA).SetItem(null);
					}
					SoundEngine.PlaySound(SoundID.Tink.WithPitchOffset(1), Main.LocalPlayer.MountedCenter);
					if (Grid[slotB.Y, slotB.X % 2] == 'r') {
						rolled = false;
						if (gameMode == AI && currentPlayer != 0) {
							aiMoveTimeout = 1;
						}
					} else {
						EndTurn();
					}
				}
			}
			if (noAction && gameMode == AI && endTurnTimeout == 0 && aiMoveTimeout == 0 && currentPlayer != 0) {
				return;
			}
		}
		public void EndTurn() {
			rolled = false;
			if (gameMode == AI && currentPlayer == 0) {
				aiMoveTimeout = 1;
			}
			currentPlayer ^= 1;
			if (gameMode == LOCAL) owner = currentPlayer;
		}
		public void RollDice() {
			roll = 0;
			int[] rolls;
			for (int i = 0; i < 4; i++) {
				int set = Main.rand.Next(2);
				rolls = DieSet[set].GetRolls(2, random: rand);
				rollOffsets[i] = new Vector2(rand.NextFloat(-0.6f, 0.6f), rand.NextFloat(-0.6f, 0.6f));
				allRolls[i] = new int[] { set, rand.Next(2) }.Concat(rolls).ToArray();
				if (rolls[0] == 0 || rolls[1] == 0) {
					roll++;
				}
				BoardGames.Instance.sounds.Add(SoundSet.Dice);
			}
			if (roll == 0) {
				endTurnTimeout = 1;
			} else {
				Point[] move;
				int totalMoves = 0;
				for (int j = 0; j < 8; j++) {
					for (int i = 0; i < 3; i++) {
						if (i == (currentPlayer ^ 2)) {
							continue;
						}
						if (CanMoveFrom(new Point(i, j))) {
							move = GetMoveFrom(new Point(i, j));
							if (move.Length != 0 && CanMoveTo(move[0])) {
								totalMoves++;
							}
						}
					}
				}
				if (totalMoves == 0) {
					endTurnTimeout = 1;
				}
			}
			rolled = true;
		}
		public void DefaultAI() {
			Point AiMove = new Point();
			aiMoveTimeout = 0;
			List<Point> allMoves = new List<Point> { };
			List<(int priority, Point target)> captureMoves = new List<(int, Point)> { };
			List<Point> safetyMoves = new List<Point> { };
			Point? offMove = null;
			if (!rolled) {
				SelectPiece(new Point(2, 5));
				if (endTurnTimeout < 1)
					aiMoveTimeout = 1;
			} else {
				Point[] move;
				Item targetItem;
				for (int j = 0; j < 8; j++) {
					for (int i = 0; i < 3; i++) {
						if ((i ^ 2) == (currentPlayer * 2)) {
							continue;
						}
						if (i == 2 && j == 5) {
							move = null;
						}
						if (CanMoveTo(new Point(i, j))) {
							move = GetMoveTo(new Point(i, j));
							if (move.Length != 0 && CanMoveFrom(move[0])) {
								if (Grid[j, i % 2] == 'o') {
									if (Grid[move[0].Y, i % 2] != 'o') {
										offMove = move[0];
										allMoves.Add(offMove.Value);
									}
								} else {
									allMoves.Add(new Point(i, j));
									targetItem = gamePieces[i, j].item;
									if (!(targetItem is null) && !targetItem.IsAir) {
										captureMoves.Add((j, new Point(i, j)));
									}
									if (move[0].X == 1 && i != 1) {
										safetyMoves.Add(new Point(i, j));
									}
								}
							}
						}
					}
				}
				if (offMove.HasValue) {
					AiMove = offMove.Value;
				} else if (safetyMoves.Count > 0) {
					AiMove = Main.rand.Next(safetyMoves);
				} else if (captureMoves.Count > 0) {
					captureMoves = captureMoves.OrderBy(v => v.priority).ToList();
					AiMove = captureMoves[0].target;
				} else if (allMoves.Count > 0) {
					AiMove = Main.rand.Next(allMoves);
				} else {
					for (int j = 0; j < 8; j++) {
						for (int i = 0; i < 3; i++) {
							AiMove = new Point();
							SelectPiece(AiMove);
							if (currentPlayer != 1) {
								return;
							}
						}
					}
					endTurnTimeout = 1;
				}
				SelectPiece(AiMove);
			}
			if (endTurnTimeout == 0 && aiMoveTimeout == 0 && currentPlayer != 0) {
				string warning = $"ai attempted invalid move {AiMove} with roll {roll}";
				if (offMove.HasValue) {
					warning += " believing it would move a piece off the board";
				} else if (captureMoves.Count == 0) {
					warning += " believing it would capture an opponent's piece";
				}
				BoardGames.Instance.Logger.Warn(warning);
				Main.NewText(warning, Color.OrangeRed);
				endTurnTimeout = 1;
			}
		}
		public void AIMove() {
			if (customAI is null) {
				DefaultAI();
			} else {
				customAI(this);
			}
		}
		public bool CanMoveFrom(Point value) {
			if (Grid[value.Y, value.X % 2] == 'o') {
				return remainingPieces[currentPlayer] > activePieces[currentPlayer];
			}
			GamePieceItemSlot slot = gamePieces.Index(value);
			if (!(slot.item is null) && slot.item.type == GamePieceTypes[currentPlayer]) {
				return true;
			}
			return false;
		}
		public bool CanMoveTo(Point value) {
			GamePieceItemSlot slot = gamePieces.Index(value);
			if ((SlotEmpty(value) ?? true) || (slot.item.type != GamePieceTypes[currentPlayer] && Grid[value.Y, value.X % 2] != 'r')) {
				return true;
			}
			return false;
		}
		public Point[] GetMoveFrom(Point current) {
			try {
				for (int i = roll; i-- > 0;) {
					current = GetNextSquare(current);
					if (Grid[current.Y, current.X % 2] == 'o' && i > 0) {
						return new Point[0];
					} else if (i == 0 && Grid[current.Y, current.X % 2] == 'r' && gamePieces[current.X, current.Y].item?.type == GamePieceTypes[currentPlayer ^ 1]) {
						current = GetNextSquare(current);
					}
				}
			} catch (Exception) {
				return new Point[0];
			}
			return new Point[] { current };
		}
		public Point[] GetMoveTo(Point current) {
			Point target = current;
			for (int i = roll; i-- > 0;) {
				current = GetLastSquare(current);
				if (Grid[current.Y, current.X % 2] == 'o' && i > 0) {
					return new Point[0];
				} else if (i == roll && Grid[current.Y, current.X % 2] == 'r' && gamePieces[current.X, current.Y].item?.type == GamePieceTypes[currentPlayer ^ 1]) {
					current = GetLastSquare(current);
				}
			}
			return new Point[] { current };
		}
		public Point GetNextSquare(Point current) {
			if (current.Y == 0 && current.X != 1) {
				return new Point(1, 0);
			}
			if (current.Y == 7 && current.X == 1) {
				return new Point(currentPlayer * 2, 7);
			}
			if (current.X == 1) {
				return new Point(1, current.Y + 1);
			} else {
				return new Point(current.X, current.Y - 1);
			}
		}
		public Point GetLastSquare(Point current) {
			if (current.Y == 0 && current.X == 1) {
				return new Point(currentPlayer * 2, 0);
			}
			if (current.Y == 7 && current.X != 1) {
				return new Point(1, 7);
			}
			if (current.X == 1) {
				return new Point(1, current.Y - 1);
			} else {
				return new Point(current.X, current.Y + 1);
			}
		}
		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			if (gameInactive) return;
			Vector2 rollPos = gamePieces[currentPlayer * 2, 5].GetDimensions().ToRectangle().Center();
			var font = FontAssets.CombatText[1].Value;
			string text = "roll";
			if (rolled) {
				text = "" + roll;
			}
			rollPos -= font.MeasureString(text) / 2;
			Utils.DrawBorderStringFourWay(spriteBatch, font, text, rollPos.X, rollPos.Y, Color.White, Color.Black, Vector2.Zero, 1);
			Rectangle pieceSquare = gamePieces[1, 7].GetDimensions().ToRectangle();
			Vector2 centerPos = pieceSquare.Center();
			for (int i = remainingPieces[0]; i-- > 0;)
				spriteBatch.Draw(TextureAssets.Item[GamePieceTypes[0]].Value,
					centerPos + new Vector2(pieceSquare.Width * -3, pieceSquare.Height * (remainingPieces[0] - i) * -0.5f),
					(i < activePieces[0]) ? new Color(150, 150, 150, 150) : Color.White);
			for (int i = remainingPieces[1]; i-- > 0;)
				spriteBatch.Draw(TextureAssets.Item[GamePieceTypes[1]].Value,
					centerPos + new Vector2(pieceSquare.Width * 3, pieceSquare.Height * (remainingPieces[1] - i) * -0.5f),
					(i < activePieces[1]) ? new Color(150, 150, 150, 150) : Color.White);
			DieSpriteSet spriteSet;
			int[] cRoll;
			Vector2 pos;
			if (rolled) {
				for (int i = allRolls.Length; i-- > 0;) {
					cRoll = allRolls[i];
					if (cRoll is null)
						break;
					spriteSet = DieSet[cRoll[0]];
					pos = centerPos + new Vector2(pieceSquare.Width * -2.5f * (1 - (2 * currentPlayer)), pieceSquare.Height * 0.5f) + (rollOffsets[i] * new Vector2(pieceSquare.Width, pieceSquare.Height));
					spriteBatch.Draw(spriteSet.BaseTexture,
						pos,
						null, Color.White, 0, new Vector2(32, 32), 0.5f, (SpriteEffects)cRoll[1], 0);
					for (int i2 = cRoll.Length; i2-- > 2;)
						spriteBatch.Draw(spriteSet.LayerTextures[cRoll[i2]],
							pos,
							null, Color.White, 0, new Vector2(32, 32), 0.5f, (SpriteEffects)cRoll[1], 0);
				}
			}
		}
		public static AutoCastingAsset<Texture2D> GetTexture(char type) {
			switch (type) {
				case 'r':
				return RosetteTexture;
				case '5':
				return OtherTextures[1];
				case 'o':
				return BoardGames.EmptySlotTexture;
				default:
				return OtherTextures[0];
			}
		}
	}
}
