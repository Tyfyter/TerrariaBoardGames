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
	public interface IChatLine {
		public string OriginalText { get; set; }
		public Color color { get; set; }
		public TextSnippet[] parsedText { get; set; }
	}
	public class ChatMessageContainerLine : IChatLine {
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
        public TextSnippet[] parsedText {
            get => (TextSnippet[])_parsedText.GetValue(chatMessageContainer);
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
    public class Reflected {
        public static void Load() {
            BoardGameExtensions._messages = typeof(RemadeChatMonitor).GetField("_messages", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        public static void Unload() {
            BoardGameExtensions._messages = null;
            ChatMessageContainerLine._color = null;
            ChatMessageContainerLine._parsedText = null;
        }
    }
}
