using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGames {
    public static class Extensions {
        public static T Index<T>(this T[,] array, Point index) {
            return array[index.X, index.Y];
        }
    }
}
