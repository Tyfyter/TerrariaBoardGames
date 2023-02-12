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
using Terraria.UI;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI.Chat;
using BoardGames.Misc;
using Terraria.GameInput;
using Terraria.Localization;
using System.Reflection;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.Config;
using Newtonsoft.Json;

namespace BoardGames.UI {
	public class GameSelectorMenu : UIState {
		public static List<(string[] text, BoardGame game, AutoCastingAsset<Texture2D> texture)> Games { get; private set; }
		public static event Action AddExternalGames;
		public static int emergencyTexturedGames;
		public float totalHeight;
		internal UIElement settingsMenu;
		internal GameSettings currentSettings;
		public static void LoadTextures() {
			if (!(Games is null)) return;
			Games = new(){ };
			/*AddGame("Ur");
			AddGame("Chess");
			AddGame("Draughts", textureName: "Checkers");
			AddGame("Checkers");
			if (!(AddExternalGames is null)) {
				AddExternalGames();
				AddExternalGames = null;
			}*/
			foreach (BoardGame game in GameRegistry.Games) {
				AddGame(game, game.GetTexture());
			}
			BoardGames.UnloadTextures += UnloadTextures;
		}
		public static void UnloadTextures() {
			Games = null;
		}
		public static void AddGame(BoardGame game, AutoCastingAsset<Texture2D> texture = default) {
			if (!texture.IsLoaded) texture = ModContent.RequestIfExists<Texture2D>(game.Mod.Name + "/Textures/Icons/" + game.Name, out var tex) ? tex : default;
			bool succeeded = false;
			try {
				Games.Add((
					new string[] {
						$"Mods.{game.Mod.Name}.BoardGame.{game.Name}.Name",
						$"Mods.{game.Mod.Name}.BoardGame.{game.Name}.Description"
					}, game, texture
				));
				succeeded = true;
			} catch (Exception) {}
			if (!succeeded) {
				Games.Add(( 
					new string[] {
						$"Mods.{game.Mod.Name}.BoardGame.{game.Name}.Name",
						$"Mods.{game.Mod.Name}.BoardGame.{game.Name}.Description"
					}, game, TextureAssets.Item[++emergencyTexturedGames]
				));
			}
		}
		public override void OnActivate() {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			Main.playerInventory = false;
		}
		public override void OnDeactivate() {
			SoundEngine.PlaySound(SoundID.MenuClose);
		}
		public override void OnInitialize() {
			if (!(Elements is null)) Elements.Clear();
			Main.UIScaleMatrix.Decompose(out Vector3 scale, out Quaternion _, out Vector3 _);
			totalHeight = 39 * scale.Y;
			var gameList = Games.ToArray();//.OrderBy((v)=>v.Key).ToArray();
			GameSelectorItem element;
			for (int i = 0; i < gameList.Length; i++) {
				element = new GameSelectorItem(gameList[i].texture, gameList[i].text);
				BoardGame game = gameList[i].game;
				element.Left.Set(element.GetOuterDimensions().Width * -0.5f, 0.05f + (0.3f * (i % 3)));
				element.Width.Set(62.4f * scale.X, 0);
				element.Height.Set(62.4f * scale.Y, 0);
				if (i % 3 == 0)
					totalHeight += element.Height.Pixels;
				element.Top.Set(totalHeight - element.Height.Pixels * 1.5f, 0);
				element.OnClick += GameSelectorItem.GetClickEvent(game.FullName);
				element.OnClick += (ev, el) => this.Deactivate();
				element.OnRightClick += (ev, el) => currentSettings = game.GetSettings();
				element.OnRightClick += GameSelectorItem.GetRightClickEvent(game.FullName);
				Append(element);
			}
			Width.Set(208f * scale.X, 0);
			Height.Set(Math.Min(totalHeight + (Width.Pixels / 8), Main.screenHeight * 0.9f), 0);
			Left.Set(Width.Pixels * -0.5f, 0.5f);
			Top.Set(Height.Pixels * -0.5f, 0.5f);
		}
		public override void Click(UIMouseEvent evt) {
			Rectangle dimensions = this.GetDimensions().ToRectangle();
			int endHeight = dimensions.Width / 8;
			int margin = dimensions.Width / 10;
			Rectangle buttonRect = new Rectangle(dimensions.X + (margin / 2), dimensions.Y + dimensions.Height - (int)(dimensions.Width * 0.165f), dimensions.Width - margin, endHeight);
			if (buttonRect.Contains(Main.mouseX, Main.mouseY)) {
				BoardGames.Instance.Menu = null;
				BoardGames.Instance.UI.SetState(null);
			}
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
			}
			Rectangle dimensions = this.GetDimensions().ToRectangle();
			int endHeight = dimensions.Width / 8;
			int margin = dimensions.Width / 10;
			Color color = Color.White;
			Rectangle topRect = new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, endHeight);
			Rectangle midRect = new Rectangle(dimensions.X, dimensions.Y + endHeight, dimensions.Width, dimensions.Height - (endHeight * 2));
			Rectangle bottomRect = new Rectangle(dimensions.X, dimensions.Y + dimensions.Height - endHeight, dimensions.Width, endHeight);
			spriteBatch.Draw(BoardGames.SelectorEndTexture, topRect, new Rectangle(0, 0, 208, 26), color, 0, default, SpriteEffects.None, 0);
			spriteBatch.Draw(BoardGames.SelectorMidTexture, midRect, new Rectangle(0, 0, 208, 1), color, 0, default, SpriteEffects.None, 0);
			spriteBatch.Draw(BoardGames.SelectorEndTexture, bottomRect, new Rectangle(0, 0, 208, 26), color, 0, default, SpriteEffects.FlipVertically, 0);

			Rectangle labelRect = new Rectangle(dimensions.X, dimensions.Y - (dimensions.Width / 10), dimensions.Width, endHeight);
			string labelText = Language.GetTextValue("Mods.BoardGames.GameSelector");
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, labelText, labelRect.Center.ToVector2(), Color.White, 0f, FontAssets.MouseText.Value.MeasureString(labelText) * 0.5f, Vector2.One);

			Rectangle buttonRect = new Rectangle(dimensions.X + (margin / 2), dimensions.Y + dimensions.Height - (int)(dimensions.Width * 0.165f), dimensions.Width - margin, endHeight);

			Rectangle buttonLeftRect = buttonRect;
			buttonLeftRect.Width = buttonLeftRect.Height / 2;

			Rectangle buttonMidRect = buttonRect;
			buttonMidRect.X += buttonMidRect.Height / 2;
			buttonMidRect.Width -= buttonMidRect.Height - 1;

			Rectangle buttonBottomRect = buttonRect;
			buttonBottomRect.X += buttonBottomRect.Width - buttonLeftRect.Width;
			buttonBottomRect.Width = buttonBottomRect.Height / 2;

			Color buttonColor = buttonRect.Contains(Main.mouseX, Main.mouseY) ? Color.White : Color.LightGray;

			spriteBatch.Draw(BoardGames.ButtonEndTexture, buttonLeftRect, new Rectangle(0, 0, 26, 52), buttonColor, 0, default, SpriteEffects.None, 0);
			spriteBatch.Draw(BoardGames.ButtonMidTexture, buttonMidRect, new Rectangle(0, 0, 1, 52), buttonColor, 0, default, SpriteEffects.None, 0);
			spriteBatch.Draw(BoardGames.ButtonEndTexture, buttonBottomRect, new Rectangle(0, 0, 26, 52), buttonColor, 0, default, SpriteEffects.FlipHorizontally, 0);

			string localText = Lang.menu[6].Value;
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, localText, buttonRect.Center.ToVector2() + new Vector2(0, buttonRect.Height * 0.15f), buttonColor, 0f, FontAssets.MouseText.Value.MeasureString(localText) * 0.5f, Vector2.One);
		}
		protected override void DrawChildren(SpriteBatch spriteBatch) {
			base.DrawChildren(spriteBatch);
			foreach (UIElement el in Elements) if (el is GameSelectorItem element) {
					element.PostDrawSiblings(spriteBatch);
			}
		}
	}
	public class GameSelectorItem : UIElement {
		public readonly AutoCastingAsset<Texture2D> texture;
		public readonly string[] tooltipText;
		public GameSelectorItem(AutoCastingAsset<Texture2D> texture, params string[] tooltipText) : base() {
			this.texture = texture;
			this.tooltipText = tooltipText;
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Rectangle dimensions = this.GetDimensions().ToRectangle();
			Color color = ContainsPoint(Main.MouseScreen) ? Color.White : Color.LightGray;
			spriteBatch.Draw(TextureAssets.InventoryBack.Value, dimensions, null, color, 0f, default, default, 0);
			spriteBatch.Draw(texture,
				dimensions.Center(),
				null,
				color,
				0, texture.Value.Size() * 0.5f,
				Math.Min((dimensions.Width * 0.8f) / texture.Value.Width, (dimensions.Height * 0.8f) / texture.Value.Height),
				SpriteEffects.None,
			0);
		}
		protected internal void PostDrawSiblings(SpriteBatch spriteBatch) {
			if (ContainsPoint(Main.MouseScreen)) {
				Vector2 offset = Vector2.Zero;
				float scale = 1.2f;
				for (int i = 0; i < tooltipText.Length; i++) {
					offset += DrawString(spriteBatch, Language.GetTextValue(tooltipText[i]), scale, offset, Main.screenWidth / 2f);
					scale -= 0.2f;
				}
			}
		}
		protected internal static Vector2 DrawString(SpriteBatch spriteBatch, string text, float scale = 1f, Vector2 posOffset = default, float maxWidth = float.PositiveInfinity) {
			int mouseX = Main.mouseX + 10;
			int mouseY = Main.mouseY + 10;
			if (Main.ThickMouse) {
				mouseX += 6;
				mouseY += 6;
			}
			Vector2 textSize;
			textSize = FontAssets.MouseText.Value.MeasureString(text) * scale;
			string lastText = "";
			while (textSize.X > maxWidth) {
				text = text.ReplaceMiddle("<SP>", "\n");
				textSize = FontAssets.MouseText.Value.MeasureString(text) * scale;
				if (text.Equals(lastText)) {
					break;
				}
				lastText = text;
			}
			if (mouseX + textSize.X + 4f > Main.screenWidth) {
				mouseX = (int)(Main.screenWidth - textSize.X - 4f);
			}
			if (mouseY + textSize.Y + 4f > Main.screenHeight) {
				mouseY = (int)(Main.screenHeight - textSize.Y - 4f);
			}
			float mouseTextColor = Main.mouseTextColor / 255f;
			Color baseColor = new Color(mouseTextColor, mouseTextColor, mouseTextColor, Main.mouseTextColor);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text.Replace("<SP>", ""), new Vector2(mouseX, mouseY) + posOffset, baseColor, 0f, Vector2.Zero, new Vector2(scale));
			return new Vector2(0, textSize.Y);
		}
		public static MouseEvent GetRightClickEvent(string game) {
			return (ev, el) => {
				if (el.Parent is GameSelectorMenu gameSelectorMenu) {
					if (gameSelectorMenu.settingsMenu is not null) {
						gameSelectorMenu.Deactivate();
						gameSelectorMenu.RemoveChild(gameSelectorMenu.settingsMenu);
					}
					UIElement settingsMenu = gameSelectorMenu.settingsMenu = new GameSettingsMenu("bees");
					gameSelectorMenu.Append(settingsMenu);
					settingsMenu.Initialize();
				}
			};
		}
		public static MouseEvent GetClickEvent(string game) {
			return (ev, el) => {
				if (Main.netMode == NetmodeID.SinglePlayer && !BoardGames.GameAI.ContainsKey(game)) {
					BoardGames.OpenGameByName(game);
				} else {
					BoardGames.Instance.selectedGame = game;
					BoardGames.Instance.selectedGameSettings = "";
					if (el.Parent is GameSelectorMenu gameSelectorMenu && gameSelectorMenu.currentSettings is GameSettings gameSettings) {
						BoardGames.Instance.selectedGameSettings = gameSettings.Serialize();
					}
					BoardGames.OpenPlayerSelector();
				}
			};
		}
	}
	public class GameSettingsMenu : UIElement {
		public float totalHeight;
		public static int testSettingValue = 4;
		public static bool testSettingBool = true;
		public GameSettingsMenu(string text) : base() {

		}
		public override void OnActivate() {
			SoundEngine.PlaySound(SoundID.MenuOpen);
		}
		public override void OnInitialize() {
			if (!(Elements is null)) Elements.Clear();
			Main.UIScaleMatrix.Decompose(out Vector3 scale, out Quaternion _, out Vector3 _);
			totalHeight = 39 * scale.Y;
			UIElement element;
			int top = (int)-totalHeight;
			GameSettings currentSettings = (Parent as GameSelectorMenu)?.currentSettings;
			if (currentSettings is null) {
				this.Deactivate();
				Remove();
				return;
			}
			PropertyFieldWrapper[] settingList = currentSettings.GetType()
				.GetFields().Select(v => new PropertyFieldWrapper(v)).Union(
					currentSettings.GetType()
					.GetProperties().Select(v => new PropertyFieldWrapper(v))
			).Where(v => v.MemberInfo.GetCustomAttribute<JsonIgnoreAttribute>() is null)
			.ToArray();
			Width.Set(208f * scale.X, 0);
			for (int i = 0; i < settingList.Length; i++) {
				element = ConfigManager.WrapIt(this, ref top, settingList[i], currentSettings, i, index: i).Item2;
				//if (element is RangeElement) Width.Set(416f * scale.X, 0);
				element.Top.Set(element.Top.Pixels + top, element.Top.Percent);
				Append(element);
			}
			Height.Set(Math.Min(totalHeight + (Width.Pixels / 8), Main.screenHeight * 0.9f), 0);
			Left.Set(Width.Pixels * 0.1f, 1f);
			Top.Set(Height.Pixels * -0.5f, 0.5f);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
			}
			Rectangle dimensions = this.GetDimensions().ToRectangle();
			int endHeight = dimensions.Width / 8;
			Color color = Color.Lime;
			Rectangle topRect = new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, endHeight);
			Rectangle midRect = new Rectangle(dimensions.X, dimensions.Y + endHeight, dimensions.Width, dimensions.Height - (endHeight * 2));
			Rectangle bottomRect = new Rectangle(dimensions.X, dimensions.Y + dimensions.Height - endHeight, dimensions.Width, endHeight);
			spriteBatch.Draw(BoardGames.SelectorEndTexture, topRect, new Rectangle(0, 0, 208, 26), color, 0, default, SpriteEffects.None, 0);
			spriteBatch.Draw(BoardGames.SelectorMidTexture, midRect, new Rectangle(0, 0, 208, 1), color, 0, default, SpriteEffects.None, 0);
			spriteBatch.Draw(BoardGames.SelectorEndTexture, bottomRect, new Rectangle(0, 0, 208, 26), color, 0, default, SpriteEffects.FlipVertically, 0);

			Rectangle labelRect = new Rectangle(dimensions.X, dimensions.Y - (dimensions.Width / 10), dimensions.Width, endHeight);
			string labelText = Language.GetTextValue("Mods.BoardGames.GameSettings");
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, labelText, labelRect.Center.ToVector2(), Color.White, 0f, FontAssets.MouseText.Value.MeasureString(labelText) * 0.5f, Vector2.One);
		}
	}
}
