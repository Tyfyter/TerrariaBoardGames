using BoardGames.Misc;
using BoardGames.Textures.Chess;
using BoardGames.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Net;
using Terraria.Social;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace BoardGames {
	public class BoardGames : Mod {
		public static BoardGames Instance { get; private set; }
		public static AutoCastingAsset<Texture2D> EmptySlotTexture { get; private set; }
		public static AutoCastingAsset<Texture2D> SelectorEndTexture { get; private set; }
		public static AutoCastingAsset<Texture2D> SelectorMidTexture { get; private set; }
		public static AutoCastingAsset<Texture2D> AIIconTexture { get; private set; }
		public static AutoCastingAsset<Texture2D> ButtonEndTexture { get; private set; }
		public static AutoCastingAsset<Texture2D> ButtonMidTexture { get; private set; }
		public static event Action UnloadTextures;
		internal UserInterface UI;
		public GameUI Game;
		public UIState Menu;
		internal List<SoundSet> sounds;
		public static Dictionary<string, Action<GameMode, int>> ExternalGames { get; private set; }
		public static MultiDictionary<string, (string name, Action<GameUI> AI)> GameAI { get; private set; }
		public string selectedGame;
		public const int ai_move_time = 30;

		public override void Load() {
			Instance = this;
			string[] chessPieceNames = Chess_Piece.PieceNames;
			string pieceName;
			Chess_Piece.Pieces = new int[12];
			sounds = new List<SoundSet> { };
			for (int i = 0; i < 6; i++) {
				pieceName = chessPieceNames[i];
				AddContent(new Chess_Piece("White_" + pieceName, Chess_Piece.Moves.FromName(pieceName), true));
				AddContent(new Chess_Piece("Black_" + pieceName, Chess_Piece.Moves.FromName(pieceName), false));
			}
			if (Main.netMode != NetmodeID.Server) {
				EmptySlotTexture = ModContent.Request<Texture2D>("BoardGames/Textures/Empty");
				SelectorEndTexture = ModContent.Request<Texture2D>("BoardGames/UI/Selector_Back_End");
				SelectorMidTexture = ModContent.Request<Texture2D>("BoardGames/UI/Selector_Back_Mid");
				AIIconTexture = ModContent.Request<Texture2D>("BoardGames/Textures/Icons/AI");
				ButtonEndTexture = ModContent.Request<Texture2D>("BoardGames/UI/Button_Back_End");
				ButtonMidTexture = ModContent.Request<Texture2D>("BoardGames/UI/Button_Back_Mid");
				UI = new UserInterface();
			}
			ChatManager.Register<GameInviteTagHandler>(new string[]{
				"game"
			});
			ExternalGames = new Dictionary<string, Action<GameMode, int>> { };
			void badChessAI(GameUI b) {
				if (GameUI.rand is null) {
					GameUI.rand = new UnifiedRandom(Main.rand.Next(int.MinValue, int.MaxValue));
				}
				while (b.currentPlayer == 1) {
					b.SelectPiece(new Point(GameUI.rand.Next(8), GameUI.rand.Next(8)));
				}
			}
			void badCheckersAI(GameUI b) {
				Checkers_UI board = b as Checkers_UI;
				if (GameUI.rand is null) {
					GameUI.rand = new UnifiedRandom(Main.rand.Next(int.MinValue, int.MaxValue));
				}
				List<Point> tiles = BoardGameExtensions.BuildPointList(8, 8);
				tiles.Shuffle(GameUI.rand);
				while (b.currentPlayer == 1) {
					Point select = tiles[0];
					tiles.RemoveAt(0);
					b.SelectPiece(select);
					(Point end, Point attacked)[] moves = board.GetMoves(select);
					if (moves.Length > 0) {
						b.SelectPiece(GameUI.rand.Next(moves).end);
					} else {
						b.SelectPiece(select);
					}
				}
				b.aiMoveTimeout = 0;
			}
			Action<GameUI> getMirrorAI(Action<GameUI> fallbackAI) {
				return (GameUI b) => {
					while (b.moveMemory.Count > 0) {
						b.SelectPiece(new Point(7 - b.moveMemory[0].X, 7 - b.moveMemory[0].Y));
						b.moveMemory.RemoveAt(0);
					}
					if (b.currentPlayer != 1) {
						b.aiMoveTimeout = 0;
						return;
					}
					fallbackAI(b);
				};
			}
			GameAI = new MultiDictionary<string, (string name, Action<GameUI> AI)> {
				{ "ur", ("default ai", null) },
				{ "chess", ("quite atrocious ai", badChessAI) },
				{ "chess", ("mirror ai", getMirrorAI(badChessAI)) },
				{ "checkers", ("mirror ai", getMirrorAI(badCheckersAI)) },
				{ "draughts", ("mirror ai", getMirrorAI(badCheckersAI)) }
			};
		}
		public override void Unload() {
			EmptySlotTexture = null;
			SelectorEndTexture = null;
			SelectorMidTexture = null;
			AIIconTexture = null;
			ButtonEndTexture = null;
			ButtonMidTexture = null;
			if (!(UnloadTextures is null)) UnloadTextures();
			UnloadTextures = null;
			Chess_Piece.Pieces = null;
			sounds = null;
			BoardGamesPlayer.SteamIDs = null;
			ExternalGames = null;
			GameAI = null;
			Instance = null;
		}
		public void OpenGame<GameType>(GameMode gameMode = GameMode.LOCAL, int otherPlayer = -1) where GameType : GameUI, new() {
			Game = new GameType();
			Game.SetMode(gameMode, otherPlayer);
			Game.TryLoadTextures();
			Game.Activate();
			UI.SetState(Game);
		}
		public static void OpenGameByName(string name, GameMode gameMode = GameMode.LOCAL, int otherPlayer = -1) {
			switch (name.ToLower()) {
				case "ur":
				Instance.OpenGame<Ur_UI>(gameMode, otherPlayer);
				return;
				case "chess":
				Instance.OpenGame<Chess_UI>(gameMode, otherPlayer);
				return;
				case "draughts":
				Instance.OpenGame<Checkers_UI>(gameMode, otherPlayer);
				((Checkers_UI)Instance.Game).mandatoryJumps = true;
				return;
				case "checkers":
				Instance.OpenGame<Checkers_UI>(gameMode, otherPlayer);
				return;
			}
			if (ExternalGames?.ContainsKey(name) ?? false) {
				ExternalGames[name](gameMode, otherPlayer);
			}
		}
		public static void OpenGameSelector() {
			Instance.Menu = new GameSelectorMenu();
			GameSelectorMenu.LoadTextures();
			Instance.Menu.Activate();
			Instance.UI.SetState(Instance.Menu);
		}
		public static void OpenPlayerSelector() {
			Instance.Menu = new PlayerSelectorMenu();
			Instance.Menu.Activate();
			Instance.UI.SetState(Instance.Menu);
		}
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			ModPacket bouncePacket;
			byte packetType = reader.ReadByte();
			Logger.Info("recieved packet of type " + packetType);
			switch (Main.netMode) {
				case NetmodeID.Server:
				switch (packetType) {
					case PacketType.SelectTile:
					bouncePacket = Instance.GetPacket(13);
					bouncePacket.Write(PacketType.SelectTile);
					bouncePacket.Write(whoAmI);
					bouncePacket.Write(reader.ReadInt32());
					bouncePacket.Write(reader.ReadInt32());
					bouncePacket.Send(reader.ReadInt32());
					break;
					case PacketType.StartupSync:
					bouncePacket = Instance.GetPacket(9);
					bouncePacket.Write(PacketType.StartupSync);
					bouncePacket.Write(whoAmI);
					bouncePacket.Write(reader.ReadInt32());
					bouncePacket.Send(reader.ReadInt32());
					break;
					case PacketType.SyncedSetup:
					bouncePacket = Instance.GetPacket(5);
					bouncePacket.Write(PacketType.SyncedSetup);
					bouncePacket.Write(whoAmI);
					bouncePacket.Send(reader.ReadInt32());
					break;
					case PacketType.RequestSteamID:
					bouncePacket = Instance.GetPacket(13);
					bouncePacket.Write(PacketType.SendSteamID);
					int requestPlayer = reader.ReadInt32();
					CSteamID? requestAddress = (Netplay.Clients[requestPlayer].Socket.GetRemoteAddress() as SteamAddress)?.SteamId;
					bouncePacket.Write(requestPlayer);
					bouncePacket.Write((requestAddress?.m_SteamID) ?? (ulong)0);
					bouncePacket.Send(whoAmI);
					break;
					case PacketType.RecieveRequest:
					bouncePacket = Instance.GetPacket();
					bouncePacket.Write(PacketType.RecieveRequest);
					bouncePacket.Write(whoAmI);
					bouncePacket.Write(reader.ReadString());
					bouncePacket.Send(reader.ReadInt32());
					break;
					case PacketType.AcceptRequest:
					bouncePacket = Instance.GetPacket();
					bouncePacket.Write(PacketType.AcceptRequest);
					bouncePacket.Write(whoAmI);
					bouncePacket.Write(reader.ReadString());
					bouncePacket.Send(reader.ReadInt32());
					break;
					/*case PacketType.SendSteamID:
                    bouncePacket = Instance.GetPacket(13);
                    bouncePacket.Write(PacketType.SendSteamID);
                    bouncePacket.Write(whoAmI);
                    bouncePacket.Write(reader.ReadUInt64());
                    bouncePacket.Send(reader.ReadInt32());
                    break;*/
				}
				break;
				case NetmodeID.MultiplayerClient:
				switch (packetType) {
					case PacketType.SelectTile:
					if (reader.ReadInt32() == Game.otherPlayerId) {
						Game.SelectPiece(new Point(reader.ReadInt32(), reader.ReadInt32()));
					}
					break;
					case PacketType.StartupSync:
					if (reader.ReadInt32() == Game.otherPlayerId) {
						GameUI.rand = new UnifiedRandom(reader.ReadInt32());
						Instance.Game.owner = (Game.otherPlayerId < Main.myPlayer) == GameUI.rand.NextBool() ? 1 : 0;
						//Game.gameInactive = false;
						bouncePacket = Instance.GetPacket(5);
						bouncePacket.Write(PacketType.SyncedSetup);
						bouncePacket.Write(Game.otherPlayerId);
						bouncePacket.Send();
						goto case PacketType.SyncedSetup;
					}
					break;
					case PacketType.SyncedSetup:
					Game.gameInactive = false;
					Game.SetupGame();
					break;
					case PacketType.RecieveRequest:
					Main.NewText("recieved game invite packet");
					RecieveGameRequest(reader.ReadInt32(), reader.ReadString());
					break;
					case PacketType.AcceptRequest:
					int otherPlayer = reader.ReadInt32();
					string game = reader.ReadString();
					BoardGames.OpenGameByName(game, GameMode.ONLINE, otherPlayer);
					break;
					/*case PacketType.RequestSteamID:
                    int sender = reader.ReadInt32();
                    ModPacket packet = BoardGames.Instance.GetPacket(13);
                    packet.Write(PacketType.RequestSteamID);
                    packet.Write();
                    packet.Write(sender);
                    packet.Send();
                    break;*/
					case PacketType.SendSteamID:
					BoardGamesPlayer.SteamIDs[reader.ReadInt32()] = new CSteamID(reader.ReadUInt64());
					break;
				}
				break;
			}
		}
		public static void SendGameRequest(int playerID, string gameName) {
			ModPacket packet;
			packet = Instance.GetPacket();
			packet.Write(PacketType.RecieveRequest);
			packet.Write(gameName);
			packet.Write(playerID);
			packet.Send();
			Main.NewText("sent " + gameName + " invite packet to player" + playerID);
		}
		public static async void RecieveGameRequest(int playerID, string gameName) {
			CSteamID steamID;
			EFriendRelationship relationship;
			try {
				steamID = await GetSteamID(playerID);
				relationship = SteamFriends.GetFriendRelationship(steamID);
			} catch (Exception) {
				steamID = CSteamID.Nil;
				relationship = EFriendRelationship.k_EFriendRelationshipNone;
			}
			//Main.NewText("recieved "+gameName+" invite packet from player "+playerID+" who is a "+relationship);
			switch (relationship) {
				case EFriendRelationship.k_EFriendRelationshipBlocked:
				if (BoardGamesConfig.Instance.RequestsFrom >= RequestEnum.NotBlocked) {
					Main.NewText("rejected invitation because player " + playerID + " is blocked");
					return;
				}
				break;
				case EFriendRelationship.k_EFriendRelationshipFriend:
				if (BoardGamesConfig.Instance.RequestsFrom == RequestEnum.NoOne) {
					Main.NewText("rejected invitation because " + Main.LocalPlayer.name + " is boring");
					return;
				}
				break;
				default:
				if (BoardGamesConfig.Instance.RequestsFrom >= RequestEnum.FriendsOnly) {
					Main.NewText("rejected invitation because player " + playerID + " is not a friend");
					return;
				}
				break;
			}
			Main.NewText(Main.player[playerID].name + " has invited you to play " + gameName + ". " + GameInviteTagHandler.GenerateTag(true, playerID, gameName) + GameInviteTagHandler.GenerateTag(false, playerID, gameName));
		}
		public static async Task<CSteamID> GetSteamID(int playerID) {
			if (BoardGamesPlayer.SteamIDs[playerID].HasValue) {
				return BoardGamesPlayer.SteamIDs[playerID].Value;
			}
			string playerName = Main.player[playerID].name;
			int retryCount = 0;
			ModPacket packet = Instance.GetPacket(5);
			packet.Write(PacketType.RequestSteamID);
			packet.Write(playerID);
			packet.Send();
			repeat:
			if (BoardGamesPlayer.SteamIDs[playerID].HasValue) {
				return BoardGamesPlayer.SteamIDs[playerID].Value;
			} else if (++retryCount < 60) {
				await Task.Delay(250);
				goto repeat;
			}
			Instance.Logger.Error("Request for " + playerName + "'s Steam ID timed out");
			BoardGamesPlayer.SteamIDs[playerID] = CSteamID.Nil;
			return CSteamID.Nil;
		}
		public static void TestUr() {
			Instance.OpenGame<Ur_UI>();
		}
		public static void TestChess() {
			Instance.OpenGame<Chess_UI>();
		}
	}
	public class BoardGameSystem : ModSystem {
		public override void UpdateUI(GameTime gameTime) {
			List<SoundSet> sounds = BoardGames.Instance.sounds;
			if (!(sounds is null)) {
				for (int i = 0; i < sounds.Count; i++) {
					if (sounds[i].Update()) {
						sounds.RemoveAt(i);
						i--;
					}
				}
			}
			BoardGames.Instance.UI?.Update(gameTime);
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {
				layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					"BoardGames: GameUI",
					delegate {
						// If the current UIState of the UserInterface is null, nothing will draw. We don't need to track a separate .visible value.
						BoardGames.Instance.UI.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
	public class BoardGamesPlayer : ModPlayer {
		public static CSteamID?[] SteamIDs { get; internal set; }
		public override void OnEnterWorld(Player player) {
			SteamIDs = new CSteamID?[Main.maxPlayers + 1];
			if (!(BoardGames.Instance?.Game is null)) {
				BoardGames.Instance.Game.Deactivate();
				BoardGames.Instance.UI.SetState(null);
			}
		}
	}
	public static class PacketType {
		public const byte SelectTile = 0;
		public const byte StartupSync = 1;
		public const byte SyncedSetup = 2;
		public const byte RecieveRequest = 3;
		public const byte AcceptRequest = 4;
		public const byte RequestSteamID = 10;
		public const byte SendSteamID = 11;
	}
}