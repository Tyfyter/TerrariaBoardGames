using BoardGames.Misc;
using BoardGames.UI;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BoardGames {
	public class GameRegistry : ILoadable {
		readonly Dictionary<string, int> gameIDs;
		readonly List<BoardGame> games;
		public static Dictionary<string, int> GameIDs => Instance.gameIDs;
		public static List<BoardGame> Games => Instance.games;
		/// <summary>
		/// </summary>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">
		/// The key does not exist in QuestIDs.
		/// </exception>
		public static BoardGame GetGameByKey(string key) => 
			GameIDs.TryGetValue(key, out int type) ?
				Games[type] : 
				throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
		public static GameRegistry Instance { get; private set; }
		[Obsolete("Should only be called by the loading process", true)]
		public GameRegistry() {
			Instance = this;
			gameIDs = new();
			games = new();
		}
		public void Load(Mod mod) {}
		public void Unload() {
			Instance = null;
		}
		public static int Add(BoardGame game) {
			int type = Games.Count;
			GameIDs.Add(game.FullName, type);
			Games.Add(game);
			return type;
		}
	}
	public abstract class BoardGame : ModType {
		public int Type { get; private set; }
		public virtual Asset<Texture2D> GetTexture() {
			return ModContent.Request<Texture2D>(Mod.Name + "/Textures/Icons/" + Name);
		}
		public virtual GameSettings GetSettings() => null;
		public void StartGame(GameMode gameMode = GameMode.LOCAL, int otherPlayer = -1, string settings = null) {
			GameSettings _settings = GetSettings();
			if (_settings is not null) _settings.Deserialize(settings);
			StartGame(gameMode, otherPlayer, _settings);
		}
		public abstract void StartGame(GameMode gameMode = GameMode.LOCAL, int otherPlayer = -1, GameSettings settings = null);
		protected sealed override void Register() {
			ModTypeLookup<BoardGame>.Register(this);
			Type = GameRegistry.Add(this);
		}
	}
}
