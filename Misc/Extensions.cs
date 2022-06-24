using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Utilities;

namespace BoardGames.Misc {
    public static class BoardGameExtensions {
        public static T Index<T>(this T[,] array, Point index) {
            return array[index.X, index.Y];
        }
        public static string ReplaceLast(this string source, string oldValue, string newValue){
            int place = source.LastIndexOf(oldValue);
            if(place == -1)return source;
            string result = source.Remove(place, oldValue.Length).Insert(place, newValue);
            return result;
        }
        public static string ReplaceMiddle(this string source, string oldValue, string newValue){
            int[] places = source.AllIndexesOf(oldValue);
            if(places.Length==0)return source;
            int halfSourceLength = source.Length/2;
            int place = places.OrderBy(v=>Math.Abs(v-halfSourceLength)).First();
            string result = source.Remove(place, oldValue.Length).Insert(place, newValue);
            return result;
        }
        public static int[] AllIndexesOf(this string source, string value){
            List<int> result = new List<int> { };
            int i = source.IndexOf(value);
            while(i!=-1) {
                result.Add(i);
                i = source.IndexOf(value, i+1);
            }
            return result.ToArray();
        }
        public static void DrawPlayerHead(SpriteBatch spriteBatch, Player drawPlayer, Vector2 position, Color color = default, float Scale = 1f) {
			PlayerHeadDrawInfo drawInfo = default(PlayerHeadDrawInfo);
			drawInfo.spriteBatch = spriteBatch;
			drawInfo.drawPlayer = drawPlayer;
			drawInfo.alpha = 1f;
			drawInfo.scale = Scale;
			int helmetDye = 0;
			int skinVariant = drawPlayer.skinVariant;
			short hairDye = drawPlayer.hairDye;
			if (drawPlayer.head == 0 && hairDye == 0) {
				hairDye = 1;
			}
            Rectangle bodyFrame = new Rectangle(0, 0, 40, 56);
			drawInfo.hairShader = hairDye;
			for (int i = 0; i < 16 + drawPlayer.extraAccessorySlots * 2; i++) {
				int num3 = i % 10;
				if (drawPlayer.dye[num3] != null && drawPlayer.armor[i].type > ItemID.None && drawPlayer.armor[i].stack > 0 && drawPlayer.armor[i].faceSlot > 0) {
					_ = drawPlayer.dye[num3].dye;
				}
			}
			if (drawPlayer.face > 0 && drawPlayer.face < 9) {
				if (!Main.accFaceLoaded[drawPlayer.face]) {
		            Main.accFaceTexture[drawPlayer.face] = Main.instance.OurLoad<Texture2D>("Images/Acc_Face_" + drawPlayer.face.ToString());
		            Main.accFaceLoaded[drawPlayer.face] = true;
	            }
			}
			if (drawPlayer.dye[0] != null) {
				helmetDye = drawPlayer.dye[0].dye;
			}
			drawInfo.armorShader = helmetDye;
	        if (!Main.hairLoaded[drawPlayer.hair]) {
		        Main.playerHairTexture[drawPlayer.hair] = Main.instance.OurLoad<Texture2D>("Images" + Path.DirectorySeparatorChar.ToString() + "Player_Hair_" + (drawPlayer.hair + 1).ToString());
		        Main.playerHairAltTexture[drawPlayer.hair] = Main.instance.OurLoad<Texture2D>("Images" + Path.DirectorySeparatorChar.ToString() + "Player_HairAlt_" + (drawPlayer.hair + 1).ToString());
		        Main.hairLoaded[drawPlayer.hair] = true;
	        }
            Color eyeWhiteColor = drawInfo.eyeWhiteColor = color;
            Color eyeColor = drawInfo.eyeColor = drawPlayer.eyeColor.MultiplyRGBA(color);
            Color hairColor = drawInfo.hairColor = drawPlayer.GetHairColor(useLighting: false).MultiplyRGBA(color);
            Color skinColor = drawInfo.skinColor = drawPlayer.skinColor.MultiplyRGBA(color);
            Color armorColor = drawInfo.armorColor = color;
			SpriteEffects spriteEffects = drawInfo.spriteEffects = SpriteEffects.None;
			Vector2 origin = drawInfo.drawOrigin = new Vector2(drawPlayer.legFrame.Width * 0.5f, drawPlayer.legFrame.Height * 0.35f);
			if (drawPlayer.head > 0 && drawPlayer.head < 216) {
                if (!Main.armorHeadLoaded[drawPlayer.head]) {
		            Main.armorHeadTexture[drawPlayer.head] = Main.instance.OurLoad<Texture2D>("Images" + Path.DirectorySeparatorChar.ToString() + "Armor_Head_" + drawPlayer.head.ToString());
		            Main.armorHeadLoaded[drawPlayer.head] = true;
	            }
			}
			bool drawHair = false;
			if (drawPlayer.head == 10 || drawPlayer.head == 12 || drawPlayer.head == 28 || drawPlayer.head == 62 || drawPlayer.head == 97 || drawPlayer.head == 106 || drawPlayer.head == 113 || drawPlayer.head == 116 || drawPlayer.head == 119 || drawPlayer.head == 133 || drawPlayer.head == 138 || drawPlayer.head == 139 || drawPlayer.head == 163 || drawPlayer.head == 178 || drawPlayer.head == 181 || drawPlayer.head == 191 || drawPlayer.head == 198) {
				drawHair = true;
			}
			bool drawAltHair = false;
			if (drawPlayer.head == 161 || drawPlayer.head == 14 || drawPlayer.head == 15 || drawPlayer.head == 16 || drawPlayer.head == 18 || drawPlayer.head == 21 || drawPlayer.head == 24 || drawPlayer.head == 25 || drawPlayer.head == 26 || drawPlayer.head == 40 || drawPlayer.head == 44 || drawPlayer.head == 51 || drawPlayer.head == 56 || drawPlayer.head == 59 || drawPlayer.head == 60 || drawPlayer.head == 67 || drawPlayer.head == 68 || drawPlayer.head == 69 || drawPlayer.head == 114 || drawPlayer.head == 121 || drawPlayer.head == 126 || drawPlayer.head == 130 || drawPlayer.head == 136 || drawPlayer.head == 140 || drawPlayer.head == 145 || drawPlayer.head == 158 || drawPlayer.head == 159 || drawPlayer.head == 184 || drawPlayer.head == 190 || (double)drawPlayer.head == 92.0 || drawPlayer.head == 195) {
				drawAltHair = true;
			}
			ItemLoader.DrawHair(drawPlayer, ref drawHair, ref drawAltHair);
			drawInfo.drawHair = drawHair;
			drawInfo.drawAltHair = drawAltHair;
			List<PlayerHeadLayer> drawHeadLayers = PlayerLoader.GetDrawHeadLayers(drawPlayer);
			int headLayerIndex = -1;
            DrawData drawData;
			while (true) {
				headLayerIndex++;
				if (headLayerIndex >= drawHeadLayers.Count) {
					break;
				}
				if (!drawHeadLayers[headLayerIndex].ShouldDraw(drawHeadLayers)) {
					continue;
				}
				if (drawHeadLayers[headLayerIndex] != PlayerHeadLayer.Head) {
					if (drawHeadLayers[headLayerIndex] != PlayerHeadLayer.Hair) {
						if (drawHeadLayers[headLayerIndex] != PlayerHeadLayer.AltHair) {
							if (drawHeadLayers[headLayerIndex] != PlayerHeadLayer.Armor) {
								if (drawHeadLayers[headLayerIndex] != PlayerHeadLayer.FaceAcc) {
									drawHeadLayers[headLayerIndex].Draw(ref drawInfo);
								} else if (drawPlayer.face > 0) {
									DrawData value = (drawPlayer.face == 7) ? new DrawData(Main.accFaceTexture[drawPlayer.face], position, bodyFrame, new Color(200, 200, 200, 150), 0f, origin, Scale, spriteEffects, 0) : new DrawData(Main.accFaceTexture[drawPlayer.face], position, bodyFrame, armorColor, 0f, origin, Scale, spriteEffects, 0);
									GameShaders.Armor.Apply(helmetDye, drawPlayer, value);
									value.Draw(spriteBatch);
									Main.pixelShader.CurrentTechnique.Passes[0].Apply();
								}
							} else if (drawPlayer.head == 23) {
								drawData = new DrawData(Main.playerHairTexture[drawPlayer.hair], position, bodyFrame, hairColor, 0f, origin, Scale, spriteEffects, 0);
								GameShaders.Hair.Apply(hairDye, drawPlayer, drawData);
								drawData.Draw(spriteBatch);
								Main.pixelShader.CurrentTechnique.Passes[0].Apply();
								drawData = new DrawData(Main.armorHeadTexture[drawPlayer.head], position, bodyFrame, armorColor, 0f, origin, Scale, spriteEffects, 0);
								GameShaders.Armor.Apply(helmetDye, drawPlayer, drawData);
								drawData.Draw(spriteBatch);
								Main.pixelShader.CurrentTechnique.Passes[0].Apply();
							} else if (drawPlayer.head == 14 || drawPlayer.head == 56 || drawPlayer.head == 158) {
								drawData = new DrawData(Main.armorHeadTexture[drawPlayer.head], position, bodyFrame, armorColor, 0f, origin, Scale, spriteEffects, 0);
								GameShaders.Armor.Apply(helmetDye, drawPlayer, drawData);
								drawData.Draw(spriteBatch);
								Main.pixelShader.CurrentTechnique.Passes[0].Apply();
							} else if (drawPlayer.head > 0 && drawPlayer.head != 28) {
								drawData = new DrawData(Main.armorHeadTexture[drawPlayer.head], position, bodyFrame, armorColor, 0f, origin, Scale, spriteEffects, 0);
								GameShaders.Armor.Apply(helmetDye, drawPlayer, drawData);
								drawData.Draw(spriteBatch);
								Main.pixelShader.CurrentTechnique.Passes[0].Apply();
							} else {
								drawData = new DrawData(Main.playerHairTexture[drawPlayer.hair], position, bodyFrame, hairColor, 0f, origin, Scale, spriteEffects, 0);
								GameShaders.Hair.Apply(hairDye, drawPlayer, drawData);
								drawData.Draw(spriteBatch);
								Main.pixelShader.CurrentTechnique.Passes[0].Apply();
							}
						} else if (drawAltHair) {
							drawData = new DrawData(Main.playerHairAltTexture[drawPlayer.hair], position, bodyFrame, hairColor, 0f, origin, Scale, spriteEffects, 0);
							GameShaders.Hair.Apply(hairDye, drawPlayer, drawData);
							drawData.Draw(spriteBatch);
							Main.pixelShader.CurrentTechnique.Passes[0].Apply();
						}
					} else {
						if (!drawHair) {
							continue;
						}
						drawData = new DrawData(Main.armorHeadTexture[drawPlayer.head], position, bodyFrame, armorColor, 0f, origin, Scale, spriteEffects, 0);
						GameShaders.Armor.Apply(helmetDye, drawPlayer, drawData);
						drawData.Draw(spriteBatch);
						Main.pixelShader.CurrentTechnique.Passes[0].Apply();
						drawData = new DrawData(Main.playerHairTexture[drawPlayer.hair], position, bodyFrame, hairColor, 0f, origin, Scale, spriteEffects, 0);
						GameShaders.Hair.Apply(hairDye, drawPlayer, drawData);
						drawData.Draw(spriteBatch);
						Main.pixelShader.CurrentTechnique.Passes[0].Apply();
					}
				} else if (drawPlayer.head != 38 && drawPlayer.head != 135 && ItemLoader.DrawHead(drawPlayer)) {
					spriteBatch.Draw(Main.playerTextures[skinVariant, 0], position, bodyFrame, skinColor, 0f, origin, Scale, spriteEffects, 0f);
					spriteBatch.Draw(Main.playerTextures[skinVariant, 1], position, bodyFrame, eyeWhiteColor, 0f, origin, Scale, spriteEffects, 0f);
					spriteBatch.Draw(Main.playerTextures[skinVariant, 2], position, bodyFrame, eyeColor, 0f, origin, Scale, spriteEffects, 0f);
				}
			}
		}
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>{
            if(value.CompareTo(min)<0) {
                return min;
            }
            if(value.CompareTo(max)>0) {
                return max;
            }
            return value;
        }
        public static List<Point> BuildPointList(int width, int height){
            List<Point> result = new List<Point> { };
            for(int y = 0; y < height; y++) {
                for(int x = 0; x < width; x++) {
                    result.Add(new Point(x,y));
                }
            }
            return result;
        }
        public static void Shuffle<T>(this IList<T> list, UnifiedRandom rng = null) {
            if(rng is null)rng = Main.rand;

            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
    public static class Reflected {
        public static void Load() {

        }
        public static void Unload() {

        }
    }
}
