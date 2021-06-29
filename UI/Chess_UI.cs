using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BoardGames.Textures.Chess;

namespace BoardGames.UI {
    public class Chess_UI : GameUI {
        public override void TryLoadTextures() => LoadTextures();
        public static Texture2D[] BoardTextures { get; private set; }
        public static void LoadTextures() {
            BoardTextures = new Texture2D[] { ModContent.GetTexture("BoardGames/Textures/Chess/Tile_White"), ModContent.GetTexture("BoardGames/Textures/Chess/Tile_Black") };
            BoardGames.UnloadTextures += UnloadTextures;
        }
        public static void UnloadTextures() {
            BoardTextures = null;
        }
        protected override void Init(Vector3 scale, Vector2 basePosition, Vector2 slotSize) {
            basePosition = new Vector2((float)(Main.screenWidth * 0.5), (float)(Main.screenHeight * 0.5));
            basePosition -= slotSize * new Vector2(4f, 4);
            gamePieces = new GamePieceItemSlot[8,8];
            try {
                for(int j = 0; j < 8; j++) {
                    for(int i = 0; i < 8; i++) {
                        gamePieces[i, j] = AddSlot(null,
                            basePosition + slotSize * new Vector2(i, j),
                            BoardTextures[(i+j)&1],
                            slotScale: scale.X,
                            HighlightMoves:null
                        );
                    }
                }
            } catch(Exception e) {
                BoardGames.Instance.Logger.Warn(e);
            }
        }
        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            if(selectedPiece.HasValue) {
                gamePieces.Index(selectedPiece.Value).glowing = true;
            }
            HighlightMoves();
        }
        public override void SelectPiece(Point target) {
            if(Main.SmartCursorEnabled) {
                gamePieces.Index(target).SetItem(Chess_Piece.Pieces[(Main.LocalPlayer.selectedItem*2+(Main.LocalPlayer.controlUp?1:0))%12]);
                return;
            }
            if(selectedPiece.HasValue) {
                GamePieceItemSlot slot = gamePieces.Index(selectedPiece.Value);
                Chess_Piece piece = slot?.item?.modItem as Chess_Piece;
                if(!(piece is null)) {
                    Point[] moves = new Point[0];
                    if(gameMode==GameMode.LOCAL) {
                        moves = piece.GetMoves(slot, piece.White?1:-1);
                    } else {
                        moves = piece.GetMoves(slot, (owner==1)^piece.White?1:-1);
                    }
                    if(moves.Contains(target)) {
                        selectedPiece = target;
                        gamePieces.Index(selectedPiece.Value).item = piece.item;
                        slot.SetItem(null);
                        EndTurn();
                    }
                }
            }
            base.SelectPiece(target);
            if(selectedPiece.HasValue) {
                if(SlotEmpty(selectedPiece.Value) ?? true) {
                    selectedPiece = null;
                } else if((gamePieces.Index(selectedPiece.Value).item.modItem as Chess_Piece)?.White==(currentPlayer==1)) {
                    selectedPiece = null;
                }
            }
        }
        public void EndTurn() {
            currentPlayer ^= 1;
            if(gameMode==GameMode.LOCAL)owner = currentPlayer;
        }
        public void HighlightMoves() {
            if(!selectedPiece.HasValue)return;
            GamePieceItemSlot slot = gamePieces.Index(selectedPiece.Value);
            Chess_Piece piece = slot?.item?.modItem as Chess_Piece;
            if(!(piece is null)) {
                Point[] moves = new Point[0];
                if(gameMode==GameMode.LOCAL) {
                    moves = piece.GetMoves(slot, piece.White?1:-1);
                } else {
                    moves = piece.GetMoves(slot, (owner==1)^piece.White?1:-1);
                }
                for(int i = moves.Length; i-->0;) {
                    gamePieces.Index(moves[i]).glowing = true;
                }
            }
        }
        public override Color GetTileColor(bool glowing) {
            return gameInactive ? Color.Gray : (glowing ? Color.White : new Color(175, 175, 175));
        }
        public override void SetupGame() {
            char[,] pieces = new char[8, 8] {
                {'r','n','b','q','k','b','n','r'},
                {'p','p','p','p','p','p','p','p'},
                {'o','o','o','o','o','o','o','o'},
                {'o','o','o','o','o','o','o','o'},
                {'o','o','o','o','o','o','o','o'},
                {'o','o','o','o','o','o','o','o'},
                {'p','p','p','p','p','p','p','p'},
                {'r','n','b','q','k','b','n','r'}
            };
            int type = -1;
            for(int j = 0; j < 8; j++) {
                for(int i = 0; i < 8; i++) {
                    type = -1;
                    switch(pieces[j,i]) {
                        case 'p':
                        type = 0 + owner;
                        break;
                        case 'r':
                        type = 2 + owner;
                        break;
                        case 'b':
                        type = 4 + owner;
                        break;
                        case 'n':
                        type = 6 + owner;
                        break;
                        case 'q':
                        type = 8 + owner;
                        break;
                        case 'k':
                        type = 10 + owner;
                        break;
                    }
                    if(type!=-1) {
                        if(j<4) {
                            type ^= 1;
                        }
                        gamePieces[i, j].SetItem(Chess_Piece.Pieces[type]);
                    }
                }
            }
        }
    }
}
