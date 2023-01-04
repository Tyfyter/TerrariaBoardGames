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
using ReLogic.Content;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;
using System.Collections;

namespace BoardGames.Misc {
	public class ChatMessageContainerLine {
		internal static FieldInfo _color;
		internal static FieldInfo _parsedText;
		public string OriginalText {
			get => chatMessageContainer.OriginalText;
			set => chatMessageContainer.OriginalText = value;
		}
		public Color color {
			get => (Color)_color.GetValue(chatMessageContainer);
			set => _color.SetValue(chatMessageContainer, value);
		}
		public List<TextSnippet[]> parsedText {
			get => ((List<TextSnippet[]>)_parsedText.GetValue(chatMessageContainer));
			set => _parsedText.SetValue(chatMessageContainer, value);
		}
		ChatMessageContainer chatMessageContainer;
		public ChatMessageContainerLine(ChatMessageContainer chatMessageContainer) {
			this.chatMessageContainer = chatMessageContainer;
		}
		public void SetContents(string text, Color color, int widthLimitInPixels = -1) => chatMessageContainer.SetContents(text, color, widthLimitInPixels);
	}
	public struct AutoCastingAsset<T> where T : class {
		public bool HasValue => asset is not null;
		public bool IsLoaded => asset?.IsLoaded ?? false;
		public T Value => asset.Value;

		readonly Asset<T> asset;
		AutoCastingAsset(Asset<T> asset) {
			this.asset = asset;
		}
		public static implicit operator AutoCastingAsset<T>(Asset<T> asset) => new(asset);
		public static implicit operator T(AutoCastingAsset<T> asset) => asset.Value;
	}
	public static class BoardGameExtensions {
		internal static FieldInfo _messages;
		public static List<ChatMessageContainerLine> GetChatLines(this RemadeChatMonitor chatMonitor) {
			return ((List<ChatMessageContainer>)_messages.GetValue(chatMonitor)).Select(v => new ChatMessageContainerLine(v)).ToList();
		}
		public static T Index<T>(this T[,] array, Point index) {
			return array[index.X, index.Y];
		}
		public static string ReplaceLast(this string source, string oldValue, string newValue) {
			int place = source.LastIndexOf(oldValue);
			if (place == -1) return source;
			string result = source.Remove(place, oldValue.Length).Insert(place, newValue);
			return result;
		}
		public static string ReplaceMiddle(this string source, string oldValue, string newValue) {
			int[] places = source.AllIndexesOf(oldValue);
			if (places.Length == 0) return source;
			int halfSourceLength = source.Length / 2;
			int place = places.OrderBy(v => Math.Abs(v - halfSourceLength)).First();
			string result = source.Remove(place, oldValue.Length).Insert(place, newValue);
			return result;
		}
		public static int[] AllIndexesOf(this string source, string value) {
			List<int> result = new List<int> { };
			int i = source.IndexOf(value);
			while (i != -1) {
				result.Add(i);
				i = source.IndexOf(value, i + 1);
			}
			return result.ToArray();
		}
		public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> {
			if (value.CompareTo(min) < 0) {
				return min;
			}
			if (value.CompareTo(max) > 0) {
				return max;
			}
			return value;
		}
		public static List<Point> BuildPointList(int width, int height) {
			List<Point> result = new List<Point> { };
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					result.Add(new Point(x, y));
				}
			}
			return result;
		}
		public static void Shuffle<T>(this IList<T> list, UnifiedRandom rng = null) {
			if (rng is null) rng = Main.rand;

			int n = list.Count;
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
		public static void DrawPlayerHead(Player drawPlayer, Vector2 position, Color color = default, float Scale = 1f) {
			position += Main.screenPosition;
			PlayerDrawSet drawinfo = default(PlayerDrawSet);
			List<DrawData> drawData = new List<DrawData>();
			drawPlayer.chatOverhead.NewMessage(drawPlayer.HeightMapOffset + "", 5);
			drawPlayer.bodyFrame.Y = 0;
			drawinfo.HeadOnlySetup(drawPlayer, drawData, new List<int>(), new List<int>(), position.X, position.Y, 1f, Scale);
			drawinfo.playerEffect = SpriteEffects.None;
			drawinfo.colorArmorHead = drawinfo.colorArmorHead.MultiplyRGB(color);
			drawinfo.colorHair = drawinfo.colorHair.MultiplyRGB(color);
			drawinfo.colorHead = drawinfo.colorHead.MultiplyRGB(color);
			drawinfo.colorEyeWhites = drawinfo.colorEyeWhites.MultiplyRGB(color);
			drawinfo.colorEyes = drawinfo.colorEyes.MultiplyRGB(color);
			PlayerLoader.ModifyDrawInfo(ref drawinfo);
			PlayerDrawLayer[] drawLayers = PlayerDrawLayerLoader.GetDrawLayers(drawinfo);
			foreach (PlayerDrawLayer layer in drawLayers) {
				if (layer.IsHeadLayer) {
					layer.DrawWithTransformationAndChildren(ref drawinfo);
				}
			}
			var a = drawPlayer.mount;
			//PlayerDrawLayers.DrawPlayer_TransformDrawData(ref drawinfo);
			if (Scale != 1f) {
				PlayerDrawLayers.DrawPlayer_ScaleDrawData(ref drawinfo, Scale);
			}
			PlayerDrawLayers.DrawPlayer_RenderAllLayers(ref drawinfo);
		}
	}
	public class Reflected : ILoadable {
		public void Load(Mod mod) {
			BoardGameExtensions._messages = typeof(RemadeChatMonitor).GetField("_messages", BindingFlags.NonPublic | BindingFlags.Instance);
			ChatMessageContainerLine._color = typeof(ChatMessageContainer).GetField("_color", BindingFlags.NonPublic | BindingFlags.Instance);
			ChatMessageContainerLine._parsedText = typeof(ChatMessageContainer).GetField("_parsedText", BindingFlags.NonPublic | BindingFlags.Instance);
		}
		public void Unload() {
			BoardGameExtensions._messages = null;
			ChatMessageContainerLine._color = null;
			ChatMessageContainerLine._parsedText = null;
		}
	}
}
