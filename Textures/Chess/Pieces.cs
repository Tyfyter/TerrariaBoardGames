using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using BoardGames.UI;

namespace BoardGames.Textures.Chess {
    public class Chess_Piece : ModItem {
        bool white;
        public Func<GamePieceItemSlot, Point[]> GetMoves { get; private set; }
        public override bool Autoload(ref string name) {
            return false;
        }
        public Chess_Piece(Func<GamePieceItemSlot, Point[]> getMoves, bool white) : base() {
            GetMoves = getMoves;
            this.white = white;
        }
        public static class Moves {
            public static Point[] Pawn(GamePieceItemSlot slot, int YSign) {
                Chess_UI ui = slot.ParentUI as Chess_UI;
                Point pos = slot.index;
                List<Point> moves = new List<Point>{};
                pos.Y -= YSign;
                if(ui.SlotEmpty(pos)??false) {
                    moves.Add(pos);
                }
                if(!(ui.SlotEmpty(pos.X-1,pos.Y)??true)) {
                    moves.Add(new Point(pos.X-1,pos.Y));
                }
                if(!(ui.SlotEmpty(pos.X+1,pos.Y)??true)) {
                    moves.Add(new Point(pos.X+1,pos.Y));
                }
                return moves.ToArray();
            }
            public static Point[] Rook(GamePieceItemSlot slot, int YSign) {
                Chess_UI ui = slot.ParentUI as Chess_UI;
                Point pos = slot.index;
                List<Point> moves = new List<Point>{};
                Chess_Piece self = (Chess_Piece)slot.item.modItem;
                #region up
                bool valid = true;
                Point currentPos = pos;
                while(valid) {
                    currentPos.Y -= YSign;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                Chess_Piece self = (Chess_Piece)slot.item.modItem;
                #region up/right
                bool valid = true;
                Point currentPos = pos;
                while(valid) {
                    currentPos.Y -= YSign;
                    currentPos.X += 1;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
            public static Point[] Queen(GamePieceItemSlot slot, int YSign) {
                Chess_UI ui = slot.ParentUI as Chess_UI;
                Point pos = slot.index;
                List<Point> moves = new List<Point>{};
                Chess_Piece self = (Chess_Piece)slot.item.modItem;
                #region up
                bool valid = true;
                Point currentPos = pos;
                while(valid) {
                    currentPos.Y -= YSign;
                    switch(ui.SlotEmpty(currentPos)) {
                        case true:
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                        moves.Add(pos);
                        break;
                        case false:
                        valid = false;
                        if(ui.gamePieces.Index(currentPos).item.modItem is Chess_Piece target && target.white != self.white) {
                            moves.Add(pos);
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
                Chess_Piece self = (Chess_Piece)slot.item.modItem;
                int X = pos.X;
                int Y = pos.Y;
                for(int y = -1; y < 2; y++) {
                    for(int x = -1; x < 2; x++) {
                        switch(ui.SlotEmpty(X + x, Y + y)) {
                            case true:
                            moves.Add(pos);
                            break;
                            case false:
                            if(ui.gamePieces[X + x, Y + y].item.modItem is Chess_Piece target && target.white != self.white) {
                                moves.Add(pos);
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
