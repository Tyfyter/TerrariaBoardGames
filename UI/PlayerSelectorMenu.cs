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

namespace BoardGames.UI {
    public class PlayerSelectorMenu : UIState {
        int scroll = 0;
        int[] players;
        const int maxPlayers = 4;
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
            Height.Set(312f*scale.X, 0);
            Width.Set(208f*scale.X, 0);
            Left.Set(Width.Pixels*-0.5f, 0.5f);
            Top.Set(Height.Pixels*-0.5f, 0.5f);
            players = new int[maxPlayers];
        }
        public override void Update(GameTime gameTime) {
            players = new int[maxPlayers];
            int index = 0;
            for(int i = 0; i < Main.maxPlayers; i++) {
                if(i!=Main.myPlayer&&Main.player[i].active) {
                    players[index] = i;
                    if(++index>=maxPlayers) {
                        break;
                    }
                }
            }
        }
        public override void Click(UIMouseEvent evt) {
            Rectangle dimensions = this.GetDimensions().ToRectangle();
            int slotSize = dimensions.Width / 4;
            int endHeight = dimensions.Width / 8;
            Rectangle playerRect = new Rectangle(dimensions.X+endHeight/2, dimensions.Y+endHeight/2, slotSize, dimensions.Width-endHeight);
            for(int i = 0; i < maxPlayers; i++) {
                if(playerRect.Contains(Main.mouseX, Main.mouseY)) {
                    BoardGames.SendGameRequest(players[i], BoardGames.Instance.selectedGame);
                    Main.NewText($"Sent invitation to {Main.player[players[i]].name}");
                    break;
                }
                playerRect.Y += (int)(slotSize*1.2f);
            }
        }
        protected override void DrawSelf(SpriteBatch spriteBatch) {
            Rectangle dimensions = this.GetDimensions().ToRectangle();
            int endHeight = dimensions.Width / 8;
            Color color = Color.White;
            Rectangle topRect = new Rectangle(dimensions.X, dimensions.Y, dimensions.Width, endHeight);
            Rectangle midRect = new Rectangle(dimensions.X, dimensions.Y+endHeight, dimensions.Width, dimensions.Height-(endHeight*2));
            Rectangle bottomRect = new Rectangle(dimensions.X, dimensions.Y+dimensions.Height-endHeight, dimensions.Width, endHeight);
            spriteBatch.Draw(BoardGames.SelectorEndTexture, topRect, new Rectangle(0,0,208,26), color, 0, default, SpriteEffects.None, 0);
            spriteBatch.Draw(BoardGames.SelectorMidTexture, midRect, new Rectangle(0,0,208,1), color, 0, default, SpriteEffects.None, 0);
            spriteBatch.Draw(BoardGames.SelectorEndTexture, bottomRect, new Rectangle(0,0,208,26), color, 0, default, SpriteEffects.FlipVertically, 0);
            int slotSize = dimensions.Width / 4;
            Rectangle playerRect = new Rectangle(dimensions.X+endHeight/2, dimensions.Y+endHeight/2, slotSize, slotSize);
            for(int i = 0; i < maxPlayers; i++) {
                spriteBatch.Draw(Main.inventoryBackTexture, playerRect, Color.White);
	            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, Main.player[i].name, new Vector2(playerRect.X+playerRect.Width*1.2f, playerRect.Y+playerRect.Height*0.2f), Color.White, 0f, Vector2.Zero, Vector2.One);
                playerRect.Y += (int)(slotSize*1.2f);
            }
        }
    }
}
