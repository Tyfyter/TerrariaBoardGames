using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.ComponentModel;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using System.Text.RegularExpressions;
using Terraria.ModLoader.Config.UI;

namespace BoardGames {
    [Label("Settings")]
    public class BoardGamesConfig : ModConfig {
        public static BoardGamesConfig Instance;
        public override ConfigScope Mode => ConfigScope.ClientSide;
        [Label("Allow game requests from ")]
        [DrawTicks]
        [DefaultValue(RequestEnum.NotBlocked)]
        [JsonConverter(typeof(StringEnumConverter), true)]
        //[LabelSeparator(" ")]
        //[CustomModConfigItem(typeof(AutospacedEnumElement))]
        public RequestEnum RequestsFrom;
    }
    /*public class AutospacedEnumElement : RangeElement {
        private Func<object> _GetValue;
	    private Func<object> _GetValueString;
	    private Func<int> _GetIndex;
	    private Action<int> _SetValue;
	    private LabelSeparatorAttribute labelSeparatorAttribute;
	    private string labelSeparator;
	    private int max;
	    private string[] valueStrings;
	    public override int NumberTicks => valueStrings.Length;
	    public override float TickIncrement => 1f / (valueStrings.Length - 1f);
	    protected override float Proportion {
		    get {
			    return _GetIndex() / (float)(max - 1);
		    }
		    set {
			    _SetValue((int)Math.Round(value * (max - 1)));
		    }
	    }

	    public override void OnBind() {
		    base.OnBind();
	        labelSeparatorAttribute = ConfigManager.GetCustomAttribute<LabelSeparatorAttribute>(memberInfo, item, list);
            if(labelSeparatorAttribute is null) {
                labelSeparator = ": ";
            } else {
		        labelSeparator = labelSeparatorAttribute.separator;
	        }
		    valueStrings = Enum.GetNames(memberInfo.Type);
		    max = valueStrings.Length;

		    TextDisplayFunction = (() => memberInfo.Name + labelSeparator + _GetValueString()?.ToString());

		    _GetValue = () => memberInfo.GetValue(item);

		    _GetValueString = () => Regex.Replace(valueStrings[_GetIndex()], "(?<!^)([A-Z])", " $1");

		    _GetIndex = () => Array.IndexOf(Enum.GetValues(memberInfo.Type), _GetValue());

            _SetValue = (index) => SetObject(Enum.GetValues(memberInfo.Type).GetValue(index));

		    if (labelAttribute != null) {
			    TextDisplayFunction = (() => labelAttribute.Label + labelSeparator + _GetValueString()?.ToString());
		    }
	    }
    }*/
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class LabelSeparatorAttribute : Attribute {
	    public string separator;
	    public LabelSeparatorAttribute(string separator){
		    this.separator = separator;
	    }
    }
    public enum RequestEnum {
        Everyone,
        NotBlocked,
        FriendsOnly,
        NoOne
    }
}
