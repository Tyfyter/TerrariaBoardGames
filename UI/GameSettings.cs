using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace BoardGames.UI {
	public abstract class GameSettings {
		//[JsonIgnore]
		//public virtual bool? FirstPlayer => null;
		public abstract string Serialize();
		public abstract void Deserialize(string data);
	}
}
