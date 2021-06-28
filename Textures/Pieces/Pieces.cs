using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace BoardGames.Textures.Pieces {
    public class Black : ModItem {}
    public class Blue : ModItem {
        public override bool OnPickup(Player player) {
            if(Main.myPlayer==player.whoAmI) {
                BoardGames.TestUr();
            }
            return true;
        }
    }
    public class Green : ModItem {}
    public class Grey : ModItem {}
    public class Purple : ModItem {}
    public class Red : ModItem {}
    public class White : ModItem {}
    public class Yellow : ModItem {}
}
