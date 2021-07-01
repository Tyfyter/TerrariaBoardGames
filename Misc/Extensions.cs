using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGames.Misc {
    public static class Extensions {
        public static T Index<T>(this T[,] array, Point index) {
            return array[index.X, index.Y];
        }
        public static string ReplaceLast(this string source, string oldValue, string newValue){
            int place = source.LastIndexOf(oldValue);
            if(place == -1)return source;
            string result = source.Remove(place, oldValue.Length).Insert(place, newValue);
            return result;
        }
    }
}
