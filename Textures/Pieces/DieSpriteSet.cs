using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace BoardGames.Textures.Pieces {
    public class DieSpriteSet {
        public Texture2D BaseTexture { get; private set; }
        public Texture2D[] LayerTextures { get; private set; }
        int _endIndex;
        int _startIndex;
        public DieSpriteSet(string baseTexture, int endIndex, int startIndex = 0) {
            BaseTexture = ModContent.GetTexture(baseTexture);
            endIndex++;
            LayerTextures = new Texture2D[endIndex-startIndex];
            for(int i = startIndex; i < endIndex; i++) {
                LayerTextures[i - startIndex] = ModContent.GetTexture(baseTexture+"_Tip_"+i);
            }
            _startIndex = startIndex;
            _endIndex = endIndex;
        }
        public int[] GetRolls(int count = 1, bool allowDuplicates = false, UnifiedRandom random = null) {
            if(random is null) {
                random = Main.rand;
            }
            List<int> rolls = new List<int>{};
            List<int> validRolls = new List<int>{};
            for(int i = 0; i < _endIndex-_startIndex; i++) {
                validRolls.Add(i);
            }
            int roll = -1;
            for(int i = 0; i < count; i++) {
                roll = random.Next(validRolls);
                rolls.Add(roll);
                if(!allowDuplicates) {
                    validRolls.Remove(roll);
                }
            }
            return rolls.ToArray();
        }
    }
}
