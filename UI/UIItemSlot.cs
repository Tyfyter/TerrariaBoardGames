using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace BoardGames.UI {
	// This class wraps the vanilla ItemSlot class into a UIElement. The ItemSlot class was made before the UI system was made, so it can't be used normally with UIState.
	// By wrapping the vanilla ItemSlot class, we can easily use ItemSlot.
	// ItemSlot isn't very modder friendly and operates based on a "Context" number that dictates how the slot behaves when left, right, or shift clicked and the background used when drawn.
	// If you want more control, you might need to write your own UIElement.
	// See ExamplePersonUI for usage and use the Awesomify chat option of Example Person to see in action.
	public class GamePieceItemSlot : UIElement {
		internal Item item;
		internal Texture2D texture;
		private readonly float _scale;
        protected internal Point index = new Point(-1,-1);
        public bool glowing = false;
        public Action<Point> HighlightMoves = null;
        public GameUI ParentUI => Parent as GameUI;
		public GamePieceItemSlot(Texture2D texture, float scale = 1f, Item _item = null) {
			this.texture = texture;
			_scale = scale;
            SetItem(_item);
			Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
		}
        public void SetItem(Item _item) {
			if(_item == null) {
				item = new Item();
				item.SetDefaults(0);
			} else if(_item.IsAir) {
				item = new Item();
				item.SetDefaults(0);
			} else {
				item = _item;
			}
        }
        public void SetItem(int type) {
			item = new Item();
			item.SetDefaults(type);
        }
        public override void Update(GameTime gameTime) {
            if(ParentUI.currentPlayer!=ParentUI.owner) {
                return;
            }
            if(ContainsPoint(Main.MouseScreen)) {
                if(!(HighlightMoves is null)) {
                    HighlightMoves(index);
                }
                if(!PlayerInput.IgnoreMouseInterface&&ParentUI.JustClicked) {
                    ParentUI.SelectPiece(index);
                }
            }
        }
        protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = _scale;
			Rectangle rectangle = GetDimensions().ToRectangle();

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
			}
            spriteBatch.Draw(texture, rectangle, glowing?Color.White:Color.LightGray);
			// Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
			ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.MouseItem, rectangle.TopLeft());
			Main.inventoryScale = oldScale;
		}
	}
    public class RefItemSlot : UIElement {
		internal Ref<Item> item;
		internal readonly int _context;
		internal readonly int color;
		private readonly float _scale;
		internal Func<Item, bool> ValidItemFunc;
        protected internal int index = -1;
		public RefItemSlot(int colorContext = ItemSlot.Context.CraftingMaterial, int context = ItemSlot.Context.InventoryItem, float scale = 1f, Ref<Item> _item = null) {
			color = colorContext;
            _context = context;
			_scale = scale;
			item = _item;
			Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = _scale;
			Rectangle rectangle = GetDimensions().ToRectangle();

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem)) {
                    ItemSlot.Handle(ref item.Value, _context);
				}
			}
			// Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
			ItemSlot.Draw(spriteBatch, ref item.Value, color, rectangle.TopLeft());
			Main.inventoryScale = oldScale;
		}
	}
}