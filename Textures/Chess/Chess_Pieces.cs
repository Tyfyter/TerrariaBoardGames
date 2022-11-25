using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using BoardGames.UI;
using System.Text.RegularExpressions;
using BoardGames.Misc;
using Microsoft.Xna.Framework.Graphics;

namespace BoardGames.Textures.Chess {
    [Autoload(false)]
    public class Chess_Piece : ModItem {
        protected override bool CloneNewInstances => true;
        string name;
		public override string Name => name;
		public static string[] PieceNames => new string[]{"Pawn","Rook","Bishop","Knight","Queen","King"};
        public static int White_Pawn   => Pieces[0];
        public static int Black_Pawn   => Pieces[1];
        public static int White_Rook   => Pieces[2];
        public static int Black_Rook   => Pieces[3];
        public static int White_Bishop => Pieces[4];
        public static int Black_Bishop => Pieces[5];
        public static int White_Knight => Pieces[6];
        public static int Black_Knight => Pieces[7];
        public static int White_Queen  => Pieces[8];
        public static int Black_Queen  => Pieces[9];
        public static int White_King   => Pieces[10];
        public static int Black_King   => Pieces[11];
        public static int[] Pieces { get; internal set; }
        public bool White { get; internal set; }
        public Func<GamePieceItemSlot, int, Point[]> GetMoves { get; private set; }
		public override void AutoStaticDefaults() {
            try {
                TextureAssets.Item[Item.type] = ModContent.Request<Texture2D>("BoardGames/Textures/Chess/" + Name);
            } catch(Exception) {
                TextureAssets.Item[Item.type] = ModContent.Request<Texture2D>("BoardGames/Textures/Chess/" + (White?"White":"Black") + "_Pawn");
            }
			if (DisplayName.IsDefault())DisplayName.SetDefault(Name.Replace('_',' ').Trim());
		}
        public override void SetStaticDefaults() {
            for(int i = 0; i < 12; i++) {
                if(Pieces[i]==0) {
                    Pieces[i] = Item.type;
                    break;
                }
            }
        }
        public Chess_Piece(string name, Func<GamePieceItemSlot, int, Point[]> getMoves, bool white) : base() {
            this.name = name;
            GetMoves = getMoves;
            this.White = white;
        }
        public Chess_Piece() : base() {
            name = "White_Pawn";
            GetMoves = Moves.Pawn;
            White = true;
        }
        public static class Moves {
            public static Func<GamePieceItemSlot, int, Point[]> FromName(string name) {
                switch(name) {
                    case "Pawn":
                    return Pawn;
                    case "Rook":
                    return Rook;
                    case "Bishop":
                    return Bishop;
                    case "Knight":
                    return Knight;
                    case "Queen":
                    return Queen;
                    case "King":
                    return King;
                }
                return null;
            }
            public static Point[] Pawn(GamePieceItemSlot slot, int YSign) {
                Chess_UI ui = slot.ParentUI as Chess_UI;
                Point pos = slot.index;
                List<Point> moves = new List<Point>{};
                Chess_Piece self = (Chess_Piece)slot.item.ModItem;
                Point currentPos = pos;
                currentPos.Y -= YSign;
                if(ui.SlotEmpty(currentPos)??false) {
                    moves.Add(currentPos);
                }
                if(!(ui.SlotEmpty(currentPos.X-1,currentPos.Y)??true)) {
                    if((ui.gamePieces[currentPos.X-1,currentPos.Y].item.ModItem as Chess_Piece)?.White != self.White)moves.Add(new Point(currentPos.X-1,currentPos.Y));
                }
                if(!(ui.SlotEmpty(currentPos.X+1,currentPos.Y)??true)) {
                    if((ui.gamePieces[currentPos.X+1,currentPos.Y].item.ModItem as Chess_Piece)?.White != self.White)moves.Add(new Point(currentPos.X+1,currentPos.Y));
                }
                if(pos.Y==(YSign>0?6:1)) {
                    currentPos.Y -= YSign;
                    if(ui.SlotEmpty(currentPos)??false) {
                        moves.Add(currentPos);
                    }
                }
                return moves.ToArray();
            }
            public static Point[] Rook(GamePieceItemSlot slot, int YSign) {
                Chess_UI ui = slot.ParentUI as Chess_UI;
                Point pos = slot.index;
                List<Point> moves = new List<Point>{};
                Chess_Piece self = (Chess_Piece)slot.item.ModItem;
                #region up
                bool valid = true;
                Point currentPos = pos;
                while(valid) {
                    currentPos.Y -= YSign;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region down
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.Y += YSign;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region left
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.X -= 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region right
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.X += 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                return moves.ToArray();
            }
            public static Point[] Bishop(GamePieceItemSlot slot, int YSign) {
                Chess_UI ui = slot.ParentUI as Chess_UI;
                Point pos = slot.index;
                List<Point> moves = new List<Point>{};
                Chess_Piece self = (Chess_Piece)slot.item.ModItem;
                #region up/right
                bool valid = true;
                Point currentPos = pos;
                while(valid) {
                    currentPos.Y -= YSign;
                    currentPos.X += 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region up/left
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.Y -= YSign;
                    currentPos.X -= 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region down/left
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.Y += YSign;
                    currentPos.X -= 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region down/right
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.Y += YSign;
                    currentPos.X += 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                return moves.ToArray();
            }
            public static Point[] Knight(GamePieceItemSlot slot, int YSign) {
                Chess_UI ui = slot.ParentUI as Chess_UI;
                Point pos = slot.index;
                List<Point> moves = new List<Point>{};
                Chess_Piece self = (Chess_Piece)slot.item.ModItem;
                int X = pos.X;
                int Y = pos.Y;
                for(int y = -1; y < 2; y++) {
                    if(y==0)continue;
                    for(int x = -1; x < 2; x++) {
                        if(x==0)continue;
                        switch(ui.SlotEmpty(X + x*2, Y + y)) {
                            case true:
                            moves.Add(new Point(X + x*2, Y + y));
                            break;
                            case false:
                            if(ui.gamePieces[X + x*2, Y + y].item.ModItem is Chess_Piece target && target.White != self.White) {
                                moves.Add(new Point(X + x*2, Y + y));
                            }
                            break;
                        }
                        switch(ui.SlotEmpty(X + x, Y + y*2)) {
                            case true:
                            moves.Add(new Point(X + x, Y + y*2));
                            break;
                            case false:
                            if(ui.gamePieces[X + x, Y + y*2].item.ModItem is Chess_Piece target && target.White != self.White) {
                                moves.Add(new Point(X + x, Y + y*2));
                            }
                            break;
                        }
                    }
                }
                return moves.ToArray();
            }
            public static Point[] Queen(GamePieceItemSlot slot, int YSign) {
                Chess_UI ui = slot.ParentUI as Chess_UI;
                Point pos = slot.index;
                List<Point> moves = new List<Point>{};
                Chess_Piece self = (Chess_Piece)slot.item.ModItem;
                #region up
                bool valid = true;
                Point currentPos = pos;
                while(valid) {
                    currentPos.Y -= YSign;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region down
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.Y += YSign;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region left
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.X -= 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region right
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.X += 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region up/right
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.Y -= YSign;
                    currentPos.X += 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region up/left
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.Y -= YSign;
                    currentPos.X -= 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region down/left
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.Y += YSign;
                    currentPos.X -= 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                #region down/right
                valid = true;
                currentPos = pos;
                while(valid) {
                    currentPos.Y += YSign;
                    currentPos.X += 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(currentPos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.ModItem is Chess_Piece target && target.White != self.White) {
                            moves.Add(currentPos);
                        }
                        break;
                        case null:
                        valid = false;
                        break;
                    }
                }
                #endregion
                return moves.ToArray();
            }
            public static Point[] King(GamePieceItemSlot slot, int YSign) {
                Chess_UI ui = slot.ParentUI as Chess_UI;
                Point pos = slot.index;
                List<Point> moves = new List<Point>{};
                Chess_Piece self = (Chess_Piece)slot.item.ModItem;
                int X = pos.X;
                int Y = pos.Y;
                for(int y = -1; y < 2; y++) {
                    for(int x = -1; x < 2; x++) {
                        switch(ui.SlotEmpty(X + x, Y + y)) {
                            case true:
                            moves.Add(new Point(X + x, Y + y));
                            break;
                            case false:
                            if(ui.gamePieces[X + x, Y + y].item.ModItem is Chess_Piece target && target.White != self.White) {
                                moves.Add(new Point(X + x, Y + y));
                            }
                            break;
                        }
                    }
                }
                return moves.ToArray();
            }
        }
    }
}
