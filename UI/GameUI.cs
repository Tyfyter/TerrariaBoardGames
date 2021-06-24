using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using System;
using System.Linq;

namespace BoardGames.UI {
	public abstract class GameUI : UIState {
        public int owner = 0;
        public int currentPlayer = 0;
        public GamePieceItemSlot[,] gamePieces;
        public Point? selectedPiece;
        bool oldMouseLeft;
        public bool JustClicked => Main.mouseLeft && !oldMouseLeft;
        public override void OnInitialize() {
            Main.UIScaleMatrix.Decompose(out Vector3 scale, out Quaternion ignore, out Vector3 ignore2);
            Vector2 basePosition = new Vector2((float)(Main.screenWidth * 0.05), (float)(Main.screenHeight * 0.4));
            Vector2 slotSize = new Vector2(50 * scale.X, 50 * scale.Y);
            Init(scale, basePosition, slotSize);
        }
        protected GamePieceItemSlot AddSlot(Item item, Vector2 position, Texture2D texture, bool usePercent = false, float slotScale = 1f, Action<Point> HighlightMoves = null) {
            GamePieceItemSlot itemSlot = new GamePieceItemSlot(texture, slotScale, item) {
                HighlightMoves = HighlightMoves
            };
            if(usePercent) {
                itemSlot.Left = new StyleDimension { Percent = position.X };
                itemSlot.Top = new StyleDimension { Percent = position.Y };
            } else {
                itemSlot.Left = new StyleDimension { Pixels = position.X };
                itemSlot.Top = new StyleDimension { Pixels = position.Y };
            }
            Append(itemSlot);
            return itemSlot;
        }
        public override void Update(GameTime gameTime) {
            Point boardSize = new Point(gamePieces.GetLength(0),gamePieces.GetLength(1));
            for(int i = 0; i < gamePieces.Length; i++) {
                gamePieces[i % boardSize.X, i / boardSize.X].glowing = false;
                gamePieces[i % boardSize.X, i / boardSize.X].index = new Point(i % boardSize.X, i / boardSize.X);
            }
            for(int i = 0; i < gamePieces.Length; i++) {
                gamePieces[i % boardSize.X, i / boardSize.X].Update(gameTime);
            }
            oldMouseLeft = Main.mouseLeft;
        }
        protected abstract void Init(Vector3 scale, Vector2 basePosition, Vector2 slotSize);
        public abstract void TryLoadTextures();
        public virtual void SelectPiece(Point target) {
            if(selectedPiece==target) {
                selectedPiece = null;
            } else {
                selectedPiece = target;
            }
        }
    }
    public class UrUI : GameUI {
        public override void TryLoadTextures() => LoadTextures();
        public static int[] GamePieceTypes { get; private set; }
        int roll = 0;
        static char[,] Grid => new char[8, 2] {
                {'r','n'},
                {'n','5'},
                {'5','n'},
                {'n','r'},
                {'o','5'},
                {'o','n'},
                {'r','n'},
                {'n','n'}
            };
        public static void LoadTextures() {
            if(!(RosetteTexture is null)) return;
            RosetteTexture = ModContent.GetTexture("BoardGames/Textures/Ur_Rosette");
            OtherTextures = new Texture2D[] { ModContent.GetTexture("BoardGames/Textures/Ur_Base"), ModContent.GetTexture("BoardGames/Textures/Ur_5") };
            GamePieceTypes = new int[] {
                ModContent.ItemType<Textures.Pieces.Red>(),
                ModContent.ItemType<Textures.Pieces.Blue>()
            };
            BoardGames.UnloadTextures += UnloadTextures;
        }
        public static void UnloadTextures() {
            RosetteTexture = null;
            OtherTextures = null;
            GamePieceTypes = null;
        }
        public static Texture2D RosetteTexture { get; private set; }
        public static Texture2D[] OtherTextures { get; private set; }
        protected override void Init(Vector3 scale, Vector2 basePosition, Vector2 slotSize) {
            basePosition = new Vector2((float)(Main.screenWidth * 0.5), (float)(Main.screenHeight * 0.5));
            basePosition -= slotSize * new Vector2(1.5f, 4);
            gamePieces = new GamePieceItemSlot[3,8];
            try {
                for(int j = 0; j < 8; j++) {
                    for(int i = 0; i < 3; i++) {
                        gamePieces[i, j] = AddSlot(null,
                            basePosition + slotSize * new Vector2(i, j),
                            GetTexture(Grid[j, i % 2]),
                            slotScale: scale.X,
                            HighlightMoves:HighlightMoves
                        );
                    }
                }
            } catch(Exception e) {
                BoardGames.Instance.Logger.Warn(e);
            }
        }
        public override void Update(GameTime gameTime) {
            if(PlayerInput.Triggers.JustPressed.MouseRight) {
                roll++;
                if(roll>4) {
                    roll = 4;
                }
            }else if(PlayerInput.Triggers.JustPressed.MouseMiddle) {
                roll--;
                if(roll<0) {
                    roll = 0;
                }
            }
            base.Update(gameTime);
        }
        public void HighlightMoves(Point target) {
            Point[] move;
            if(CanMoveFrom(target)) {
                move = GetMoveFrom(target);
                if(move.Length>0 && CanMoveTo(move[0])) {
                    gamePieces[target.X, target.Y].glowing = true;
                    gamePieces[move[0].X, move[0].Y].glowing = true;
                }
            }else if(CanMoveTo(target)) {
                move = GetMoveTo(target);
                if(move.Length>0 && CanMoveFrom(move[0])) {
                    gamePieces[target.X, target.Y].glowing = true;
                    gamePieces[move[0].X, move[0].Y].glowing = true;
                }
            }
        }
        public override void SelectPiece(Point target) {
            Point slotA = new Point(-1,-1);
            Point slotB = new Point(-1,-1);
            Point[] move = GetMoveFrom(target);
            if(move.Length>0 && CanMoveFrom(target) && CanMoveTo(move[0])) {
                slotA = new Point(target.X, target.Y);
                slotB = new Point(move[0].X, move[0].Y);
            } else {
                move = GetMoveTo(target);
                if(move.Length>0) {
                    slotA = new Point(move[0].X, move[0].Y);
                    slotB = new Point(target.X, target.Y);
                }
            }
            if(slotA!=new Point(-1,-1) && CanMoveFrom(slotA) && CanMoveTo(slotB)) {
                if(Grid[slotA.Y, slotA.X % 2] == 'o') {
                    gamePieces[slotB.X, slotB.Y].SetItem(GamePieceTypes[owner]);
                } else if(Grid[slotB.Y, slotB.X % 2] == 'o') {
                    gamePieces[slotA.X, slotA.Y].SetItem(null);
                } else {
                    gamePieces[slotB.X, slotB.Y].SetItem(gamePieces[slotA.X, slotA.Y].item);
                    gamePieces[slotA.X, slotA.Y].SetItem(null);
                }
            }
        }
        public void ShowMovesTo(Point target) {
            Point[] move = GetMoveTo(target);
            if(move.Length>0 && CanMoveFrom(move[0])) {
                gamePieces[move[0].X, move[0].Y].glowing = true;
            }
        }
        public bool CanMoveFrom(Point value) {
            if(Grid[value.Y,value.X%2]=='o') {
                return true;
            }
            GamePieceItemSlot slot = gamePieces[value.X,value.Y];
            if(!(slot.item is null)&&slot.item.type==GamePieceTypes[owner]) {
                return true;
            }
            return false;
        }
        public bool CanMoveTo(Point value) {
            GamePieceItemSlot slot = gamePieces[value.X,value.Y];
            if(slot.item is null||slot.item.IsAir||(slot.item.type!=GamePieceTypes[owner]&&Grid[value.Y,value.X%2]!='r')) {
                return true;
            }
            return false;
        }
        public Point[] GetMoveFrom(Point current) {
            try {
                for(int i = roll; i-->0;) {
                    current = GetNextSquare(current);
                    if(Grid[current.Y, current.X % 2] == 'o' && i > 0) {
                        return new Point[0];
                    }
                }
            } catch(Exception) {
                return new Point[0];
            }
            return new Point[]{current};
        }
        public Point[] GetMoveTo(Point current) {
            Point target = current;
            for(int i = roll; i-->0;) {
                current = GetLastSquare(current);
                if(Grid[current.Y,current.X%2]=='o'&&i>0) {
                    return new Point[0];
                }
            }
            return new Point[]{current};
        }
        public Point GetNextSquare(Point current) {
            if(current.Y==0&&current.X!=1) {
                return new Point(1,0);
            }
            if(current.Y==7&&current.X==1) {
                return new Point(owner*2,7);
            }
            if(current.X==1) {
                Item targetItem = gamePieces[current.X, current.Y + 1].item;
                if(!(targetItem is null)&&!targetItem.IsAir&&targetItem.type!=GamePieceTypes[owner]&&Grid[current.Y+1,current.X%2]=='r'){
                    return new Point(1,current.Y+2);
                }
                return new Point(1,current.Y+1);
            } else {
                return new Point(current.X,current.Y-1);
            }
        }
        public Point GetLastSquare(Point current) {
            if(current.Y==0&&current.X==1) {
                return new Point(owner*2,0);
            }
            if(current.Y==7&&current.X!=1) {
                return new Point(1,7);
            }
            if(current.X==1) {
                return new Point(1,current.Y-1);
            } else {
                return new Point(current.X,current.Y+1);
            }
        }
        public static Texture2D GetTexture(char type) {
            switch(type) {
                case 'r':
                return RosetteTexture;
                case '5':
                return OtherTextures[1];
                case 'o':
                return BoardGames.EmptySlotTexture;
                default:
                return OtherTextures[0];
            }
        }
    }
}