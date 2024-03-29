using BoardGames.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace BoardGames.UI {
	public class GamePieceItemSlot : UIElement {
		internal Item item;
		internal AutoCastingAsset<Texture2D> texture;
		private readonly float _scale;
		protected internal Point index = new Point(-1, -1);
		public bool glowing = false;
		public Action<Point> HighlightMoves = null;
		public GameUI ParentUI => Parent as GameUI;
		public GamePieceItemSlot(AutoCastingAsset<Texture2D> texture, float scale = 1f, Item _item = null) {
			this.texture = texture;
			_scale = scale;
			SetItem(_item);
			Width.Set(TextureAssets.InventoryBack9.Value.Width * scale, 0f);
			Height.Set(TextureAssets.InventoryBack9.Value.Height * scale, 0f);
		}
		public void SetItem(Item _item) {
			if (_item == null) {
				item = new Item();
				item.SetDefaults(0);
			} else if (_item.IsAir) {
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
			if (ParentUI.currentPlayer != ParentUI.owner || ParentUI.gameInactive) {
				return;
			}
			if (ContainsPoint(Main.MouseScreen)) {
				if (!(HighlightMoves is null)) {
					HighlightMoves(index);
				}
				if (!PlayerInput.IgnoreMouseInterface && ParentUI.JustClicked) {
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
			spriteBatch.Draw(texture, rectangle, ParentUI.GetTileColor(glowing));
			// Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
			int stack = item.stack;
			item.stack = 1;
			Vector2 itemPos = rectangle.TopLeft();
			if (!ParentUI.gameInactive) {
				for (int i = stack; i-- > 0;) {
					ItemSlot.Draw(spriteBatch, ref item, ItemSlot.Context.MouseItem, itemPos);
					itemPos.Y -= rectangle.Height * 0.0625f;
				}
			}
			item.stack = stack;
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
			Width.Set(TextureAssets.InventoryBack9.Value.Width * scale, 0f);
			Height.Set(TextureAssets.InventoryBack9.Value.Height * scale, 0f);
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