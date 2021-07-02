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
using Terraria.Localization;

namespace BoardGames.UI {
    public class PlayerSelectorMenu : UIState {
        string selectedGame;
        int scroll = 0;
        int[] players;
        int totalPlayers = 0;
        const int maxPlayers = 4;
        List<(string name, Action<GameUI> AI)> AIs;
        int AICount;
        int MaxScroll => totalPlayers + AICount - 4;
        public override void OnActivate() {
		    Main.PlaySound(SoundID.MenuOpen);
		    Main.playerInventory = false;
        }
        public override void OnDeactivate() {
		    Main.PlaySound(SoundID.MenuClose);
        }
        public override void OnInitialize() {
            if(!(Elements is null))Elements.Clear();
            selectedGame = BoardGames.Instance.selectedGame;
            Main.UIScaleMatrix.Decompose(out Vector3 scale, out Quaternion _, out Vector3 _);
            Height.Set(312f*scale.X, 0);
            Width.Set(208f*scale.X, 0);
            Left.Set(Width.Pixels*-0.5f, 0.5f);
            Top.Set(Height.Pixels*-0.5f, 0.5f);
            players = new int[maxPlayers];
            if(BoardGames.GameAI.TryGetValue(selectedGame, out AIs)) {
                AICount = AIs.Count;
            }
        }
        public override void Update(GameTime gameTime) {
            players = new int[maxPlayers] {-1,-1,-1,-1};
            totalPlayers = 0;
            int index = 0;
            int scrolled = scroll;
            for(int i = 0; i < Main.maxPlayers; i++) {
                if(Main.player[i].active) {
                    if(scrolled==0) {
                        if(index<maxPlayers)players[index] = i;
                        index++;
                    } else {
                        scrolled--;
                    }
                    totalPlayers++;
                }
            }
            for(int i = 0; i < AICount && index < maxPlayers; i++) {
                if(scrolled==0) {
                    players[index] = (-2)-i;
                    index++;
                } else {
                    scrolled--;
                }
            }
        }
        public override void Click(UIMouseEvent evt) {
            Rectangle dimensions = this.GetDimensions().ToRectangle();
            int slotSize = dimensions.Width / 4;
            int endHeight = dimensions.Width / 8;
            int margin = dimensions.Width / 10;
            Rectangle playerRect = new Rectangle(dimensions.X+endHeight/2, dimensions.Y+endHeight/2, dimensions.Width-endHeight, slotSize);
            for(int i = 0; i < maxPlayers; i++) {
                if(playerRect.Contains(Main.mouseX, Main.mouseY)) {
                    if(players[i]<0) {
                        if(players[i]==-1) {
                            break;
                        }
                        int index = (-2) - players[i];
                        BoardGames.OpenGameByName(selectedGame, GameMode.AI);
                        BoardGames.Instance.Game.customAI = AIs[index].AI;
                        return;
                    }
                    BoardGames.SendGameRequest(players[i], selectedGame);
                    Main.NewText($"Sent invitation to {Main.player[players[i]].name}");
                    BoardGames.Instance.Menu = null;
                    BoardGames.Instance.UI.SetState(null);
                    return;
                }
                playerRect.Y += (int)(slotSize*1.2f);
            }
            Rectangle buttonRect = new Rectangle(dimensions.X+(margin/2), dimensions.Y+(int)(dimensions.Height*0.89f), (dimensions.Width-margin)-(dimensions.Width/2), endHeight);
            if(buttonRect.Contains(Main.mouseX, Main.mouseY)) {
                BoardGames.OpenGameByName(selectedGame, GameMode.LOCAL);
            }
            buttonRect.X += (dimensions.Width / 2);
            if(buttonRect.Contains(Main.mouseX, Main.mouseY)) {
                BoardGames.OpenGameSelector();
            }
        }
        public override void ScrollWheel(UIScrollWheelEvent evt) {
            if(evt.Target==this) {
                scroll -= Math.Sign(evt.ScrollWheelValue);
                scroll = BoardGameExtensions.Clamp(scroll, 0, MaxScroll);
            }
        }
        protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
			}
            Rectangle dimensions = this.GetDimensions().ToRectangle();
            int endHeight = dimensions.Width / 8;
            int slotSize = dimensions.Width / 4;
            int margin = dimensions.Width / 10;


            Rectangle topRect = new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, endHeight);
            Rectangle midRect = new Rectangle(dimensions.X, dimensions.Y+endHeight, dimensions.Width, dimensions.Height-(endHeight*2));
            Rectangle bottomRect = new Rectangle(dimensions.X, dimensions.Y+dimensions.Height-endHeight, dimensions.Width, endHeight);

            spriteBatch.Draw(BoardGames.SelectorEndTexture, topRect, new Rectangle(0,0,208,26), Color.White, 0, default, SpriteEffects.None, 0);
            spriteBatch.Draw(BoardGames.SelectorMidTexture, midRect, new Rectangle(0,0,208,1), Color.White, 0, default, SpriteEffects.None, 0);
            spriteBatch.Draw(BoardGames.SelectorEndTexture, bottomRect, new Rectangle(0,0,208,26), Color.White, 0, default, SpriteEffects.FlipVertically, 0);

            Rectangle playerRect = new Rectangle(dimensions.X+endHeight/2, dimensions.Y+endHeight/2, slotSize, slotSize);
            Rectangle playerRect2 = new Rectangle(dimensions.X+endHeight/2, dimensions.Y+endHeight/2, dimensions.Width-endHeight, slotSize);
            for(int i = 0; i < maxPlayers; i++) {
                playerRect2.Y = playerRect.Y;
                if(players[i]<0) {
                    if(players[i]==-1) {
                        break;
                    }
                    spriteBatch.Draw(BoardGames.AIIconTexture, playerRect, playerRect2.Contains(Main.mouseX, Main.mouseY)?Color.White:Color.LightGray);
	                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, AIs[(-2)-players[i]].name, new Vector2(playerRect.X+playerRect.Width*1.2f, playerRect.Y+playerRect.Height*0.2f), Color.White, 0f, Vector2.Zero, Vector2.One);
                    playerRect.Y += (int)(slotSize*1.2f);
                    continue;
                }
                Color color = playerRect2.Contains(Main.mouseX, Main.mouseY) ? Color.White : Color.LightGray;
                spriteBatch.Draw(Main.inventoryBackTexture, playerRect, color);
                BoardGameExtensions.DrawPlayerHead(spriteBatch, Main.player[players[i]], playerRect.Center.ToVector2(), color, slotSize/39f);
	            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Main.player[players[i]].name, new Vector2(playerRect.X+playerRect.Width*1.2f, playerRect.Y+playerRect.Height*0.2f), Main.teamColor[Main.player[players[i]].team], 0f, Vector2.Zero, Vector2.One);
                playerRect.Y += (int)(slotSize*1.2f);
            }

            Rectangle labelRect = new Rectangle(dimensions.X+(margin/2), dimensions.Y-margin, dimensions.Width-margin, endHeight);
            string labelText = Language.GetTextValue("Mods.BoardGames.PlayerSelector");
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, labelText, labelRect.Center.ToVector2(), Color.White, 0f, Main.fontMouseText.MeasureString(labelText)*0.5f, Vector2.One);

            Rectangle buttonRect = new Rectangle(dimensions.X+(margin/2), dimensions.Y+(int)(dimensions.Height*0.89f), (dimensions.Width-margin)-(dimensions.Width/2), endHeight);

            Rectangle buttonLeftRect = buttonRect;
            buttonLeftRect.Width = buttonLeftRect.Height / 2;

            Rectangle buttonMidRect = buttonRect;
            buttonMidRect.X += buttonMidRect.Height / 2;
            buttonMidRect.Width -= buttonMidRect.Height-1;

            Rectangle buttonBottomRect = buttonRect;
            buttonBottomRect.X += buttonBottomRect.Width-buttonLeftRect.Width;
            buttonBottomRect.Width = buttonBottomRect.Height / 2;

            Color buttonColor = buttonRect.Contains(Main.mouseX, Main.mouseY) ? Color.White : Color.LightGray;

            spriteBatch.Draw(BoardGames.ButtonEndTexture, buttonLeftRect, new Rectangle(0,0,26,52), buttonColor, 0, default, SpriteEffects.None, 0);
            spriteBatch.Draw(BoardGames.ButtonMidTexture, buttonMidRect, new Rectangle(0,0,1,52), buttonColor, 0, default, SpriteEffects.None, 0);
            spriteBatch.Draw(BoardGames.ButtonEndTexture, buttonBottomRect, new Rectangle(0,0,26,52), buttonColor, 0, default, SpriteEffects.FlipHorizontally, 0);

            string localText = Language.GetTextValue("Mods.BoardGames.Local");
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, localText, buttonRect.Center.ToVector2()+new Vector2(0,buttonRect.Height*0.15f), buttonColor, 0f, Main.fontMouseText.MeasureString(localText)*0.5f, Vector2.One);

            buttonRect.X += dimensions.Width / 2;

            buttonLeftRect = buttonRect;
            buttonLeftRect.Width = buttonLeftRect.Height / 2;

            buttonMidRect = buttonRect;
            buttonMidRect.X += buttonMidRect.Height / 2;
            buttonMidRect.Width -= buttonMidRect.Height-1;

            buttonBottomRect = buttonRect;
            buttonBottomRect.X += buttonBottomRect.Width-buttonLeftRect.Width;
            buttonBottomRect.Width = buttonBottomRect.Height / 2;

            buttonColor = buttonRect.Contains(Main.mouseX, Main.mouseY) ? Color.White : Color.LightGray;

            spriteBatch.Draw(BoardGames.ButtonEndTexture, buttonLeftRect, new Rectangle(0,0,26,52), buttonColor, 0, default, SpriteEffects.None, 0);
            spriteBatch.Draw(BoardGames.ButtonMidTexture, buttonMidRect, new Rectangle(0,0,1,52), buttonColor, 0, default, SpriteEffects.None, 0);
            spriteBatch.Draw(BoardGames.ButtonEndTexture, buttonBottomRect, new Rectangle(0,0,26,52), buttonColor, 0, default, SpriteEffects.FlipHorizontally, 0);

            localText = Lang.menu[5].Value;
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, localText, buttonRect.Center.ToVector2()+new Vector2(0,buttonRect.Height*0.15f), buttonColor, 0f, Main.fontMouseText.MeasureString(localText)*0.5f, Vector2.One);
        }
    }
}
