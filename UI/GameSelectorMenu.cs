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
using Terraria.UI.Chat;
using BoardGames.Misc;
using Terraria.GameInput;

namespace BoardGames.UI {
    public class GameSelectorMenu : UIState {
        public static Dictionary<string, (string[] text, Texture2D texture)> Games { get; private set; }
        public static event Action AddExternalGames;
        public static int emergencyTexutredGames;
        public float totalHeight;
        public static void LoadTextures() {
            if(!(Games is null)) return;
            Games = new Dictionary<string, (string[], Texture2D)> {};
            AddGame("Ur");
            AddGame("Chess");
            AddGame("Draughts", textureName:"Checkers");
            AddGame("Checkers");
            if(!(AddExternalGames is null)) {
                AddExternalGames();
                AddExternalGames = null;
            }
            BoardGames.UnloadTextures += UnloadTextures;
        }
        public static void UnloadTextures() {
            Games = null;
        }
        public static void AddGame(string name, string modOrigin = "BoardGames", string textureName = null) {
            if(textureName is null)textureName = name;
            try {
                Games.Add(name.ToLower(), (new string[] {$"Mods.{modOrigin}.{name}.Name",$"Mods.{modOrigin}.{name}.Description"}, ModContent.GetTexture(modOrigin+"/Textures/Icons/"+textureName)) );
            } catch(Exception) {
                Games.Add(name.ToLower(), (new string[] {$"Mods.{modOrigin}.{name}.Name",$"Mods.{modOrigin}.{name}.Description"}, Main.itemTexture[++emergencyTexutredGames]) );
            }
        }
        public override void OnActivate() {
		    Main.PlaySound(SoundID.MenuOpen);
		    Main.playerInventory = false;
        }
        public override void OnDeactivate() {
		    Main.PlaySound(SoundID.MenuClose);
        }
        public override void OnInitialize() {
            if(!(Elements is null))Elements.Clear();
            Main.UIScaleMatrix.Decompose(out Vector3 scale, out Quaternion _, out Vector3 _);
            totalHeight = 39*scale.Y;
            var gameList = Games.ToArray();//.OrderBy((v)=>v.Key).ToArray();
            GameSelectorItem element;
            for(int i = 0; i < gameList.Length; i++) {
                element = new GameSelectorItem(gameList[i].Value.texture, gameList[i].Value.text);
                element.Left.Set(element.GetOuterDimensions().Width*-0.5f, 0.05f+(0.3f*(i%3)));
                element.Width.Set(62.4f*scale.X, 0);
                element.Height.Set(62.4f*scale.Y, 0);
                if(i%3==0)
                    totalHeight += element.Height.Pixels;
                element.Top.Set(totalHeight-element.Height.Pixels*1.5f, 0);
                element.OnClick += GameSelectorItem.GetClickEvent(gameList[i].Key);
                element.OnClick += (ev, el) => this.Deactivate();
                Append(element);
            }
            Height.Set(Math.Min(totalHeight, Main.screenHeight*0.9f), 0);
            Width.Set(208f*scale.X, 0);
            Left.Set(Width.Pixels*-0.5f, 0.5f);
            Top.Set(Height.Pixels*-0.5f, 0.5f);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
			}
            Rectangle dimensions = this.GetDimensions().ToRectangle();
            int endHeight = dimensions.Width / 8;
            Color color = Color.White;
            Rectangle topRect = new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, endHeight);
            Rectangle midRect = new Rectangle(dimensions.X, dimensions.Y+endHeight, dimensions.Width, dimensions.Height-(endHeight*2));
            Rectangle bottomRect = new Rectangle(dimensions.X, dimensions.Y+dimensions.Height-endHeight, dimensions.Width, endHeight);
            spriteBatch.Draw(BoardGames.SelectorEndTexture, topRect, new Rectangle(0,0,208,26), color, 0, default, SpriteEffects.None, 0);
            spriteBatch.Draw(BoardGames.SelectorMidTexture, midRect, new Rectangle(0,0,208,1), color, 0, default, SpriteEffects.None, 0);
            spriteBatch.Draw(BoardGames.SelectorEndTexture, bottomRect, new Rectangle(0,0,208,26), color, 0, default, SpriteEffects.FlipVertically, 0);
        }
        protected override void DrawChildren(SpriteBatch spriteBatch) {
            base.DrawChildren(spriteBatch);
		    foreach (UIElement el in Elements) if(el is GameSelectorItem element){
			    element.PostDrawSiblings(spriteBatch);
		    }
        }
    }
    public class GameSelectorItem : UIElement {
        public readonly Texture2D texture;
        public readonly string[] text;
        public GameSelectorItem(Texture2D texture, params string[] text) : base() {
            this.texture = texture;
            this.text = text;
        }
        protected override void DrawSelf(SpriteBatch spriteBatch) {
            Rectangle dimensions = this.GetDimensions().ToRectangle();
            spriteBatch.Draw(Main.inventoryBackTexture, dimensions, null, Color.White, 0f, default, default, 0);
            spriteBatch.Draw(texture, dimensions.Center(), null, Color.White, 0, texture.Size()*0.5f,
                Math.Min((dimensions.Width*0.8f)/texture.Width,(dimensions.Height*0.8f)/texture.Height), SpriteEffects.None, 0);

        }
        protected internal void PostDrawSiblings(SpriteBatch spriteBatch) {
            if(ContainsPoint(Main.MouseScreen)) {
                Vector2 offset = Vector2.Zero;
                float scale = 1.2f;
                for(int i = 0; i < text.Length; i++) {
                    offset += DrawString(spriteBatch, Terraria.Localization.Language.GetTextValue(text[i]), scale, offset, Main.screenWidth/3f);
                    scale -= 0.2f;
                }
            }
        }
        static Vector2 DrawString(SpriteBatch spriteBatch, string text, float scale = 1f, Vector2 posOffset = default, float maxWidth = float.PositiveInfinity) {
	        int mouseX = Main.mouseX + 10;
	        int mouseY = Main.mouseY + 10;
	        if (Main.ThickMouse) {
		        mouseX += 6;
		        mouseY += 6;
	        }
            Vector2 textSize;
            textSize = Main.fontMouseText.MeasureString(text)*scale;
            string lastText = "";
            while(textSize.X>maxWidth) {
                text = text.ReplaceLast("<SP>", "\n");
                textSize = Main.fontMouseText.MeasureString(text)*scale;
                if(text.Equals(lastText)) {
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
	        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, text.Replace("<SP>", ""), new Vector2(mouseX, mouseY)+posOffset, baseColor, 0f, Vector2.Zero, new Vector2(scale));
            return new Vector2(0, textSize.Y);
        }
        public static MouseEvent GetClickEvent(string game) {
            return (ev, el) => {
                if(Main.netMode == NetmodeID.SinglePlayer && !BoardGames.GameAI.ContainsKey(game)) {
                    BoardGames.OpenGameByName(game);
                } else {
                    BoardGames.Instance.selectedGame = game;
                    BoardGames.OpenPlayerSelector();
                }
            };
        }
    }
}
