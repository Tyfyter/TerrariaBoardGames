using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BoardGames.Textures.Chess;
using static BoardGames.UI.GameMode;
using BoardGames.Misc;

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
            if(gameMode!=ONLINE) {
                SetupGame();
                Main.NewText("set up chess game because it was not set to online mode");
            }
        }
        public override void Update(GameTime gameTime) {
            if(endGameTimeout>0){
                if(--endGameTimeout<1) {
                    this.Deactivate();
                    BoardGames.Instance.UI.SetState(null);
                    endGameTimeout = 0;
                    return;
                }
            }
            base.Update(gameTime);
            if(selectedPiece.HasValue) {
                gamePieces.Index(selectedPiece.Value).glowing = true;
            }
            HighlightMoves();
            if(aiMoveTimeout>0) {
                if(++aiMoveTimeout > BoardGames.ai_move_time) {
                    customAI(this);
                }
            }
        }
        public override void SelectPiece(Point target) {
            if(gameMode==ONLINE&&currentPlayer==owner) {
                ModPacket packet = BoardGames.Instance.GetPacket(13);
                packet.Write(PacketType.SelectTile);
                packet.Write(7-target.X);
                packet.Write(7-target.Y);
                packet.Write(otherPlayerId);
                packet.Send();
            } else if(gameMode==AI&&currentPlayer==0) {
                moveMemory.Add(target);
            }
            if(selectedPiece.HasValue) {
                GamePieceItemSlot slot = gamePieces.Index(selectedPiece.Value);
                Chess_Piece piece = slot?.item?.ModItem as Chess_Piece;
                if(!(piece is null)) {
                    int pieceType = piece.Item.type;
                    Point[] moves = new Point[0];
                    int dir = 0;
                    if(gameMode==ONLINE) {
                        dir = (owner==1)^piece.White?1:-1;
                    } else {
                        dir = piece.White ? 1 : -1;
                    }
                    moves = piece.GetMoves(slot, dir);
                    if(moves.Contains(target)) {
                        selectedPiece = target;
                        if(target.Y==(3.5f-(dir*3.5f))&&piece.GetMoves==Chess_Piece.Moves.Pawn) {
                            pieceType = piece.White ? Chess_Piece.White_Queen : Chess_Piece.Black_Queen;
                        }
                        GamePieceItemSlot targetSlot = gamePieces.Index(selectedPiece.Value);
                        if(targetSlot?.item?.type==Chess_Piece.White_King||targetSlot?.item?.type==Chess_Piece.Black_King) {
                            EndGame(currentPlayer);
                        }
                        targetSlot.SetItem(pieceType);
                        slot.SetItem(null);
                        EndTurn();
                        //Main.PlaySound(new Terraria.Audio.LegacySoundStyle(21, 0, Terraria.Audio.SoundType.Sound), Main.LocalPlayer.MountedCenter).Pitch = 1;
                    }
                }
            }
            Point? oldSelected = selectedPiece;
            base.SelectPiece(target);
            if(selectedPiece.HasValue) {
                if(SlotEmpty(selectedPiece.Value) ?? true) {
                    selectedPiece = null;
                } else if((gamePieces.Index(selectedPiece.Value).item.ModItem as Chess_Piece)?.White==(currentPlayer==1)) {
                    selectedPiece = null;
                }
            }
            if(oldSelected != selectedPiece) {
                SoundEngine.PlaySound(new Terraria.Audio.LegacySoundStyle(21, 0, Terraria.Audio.SoundType.Sound), Main.LocalPlayer.MountedCenter).Pitch = 1;
            }
        }
        public void EndGame(int winner) {
            switch(gameMode) {
                case LOCAL:
                if(winner == 0) {
                    Main.NewText("White wins", Color.White);
                } else {
                    Main.NewText("Black wins", Color.Gray);
                }
                break;
                case AI:
                if(winner == 0) {
                    Main.NewText("Player wins", Color.White);
                } else {
                    Main.NewText("AI wins", Color.Gray);
                }
                break;
                case ONLINE:
                int notOwner = owner^1;
                if(winner==0) {
                    Main.NewText(Main.player[(owner*otherPlayerId)+(notOwner*Main.myPlayer)].name+" wins", Color.White);
                } else {
                    Main.NewText(Main.player[(notOwner*otherPlayerId)+(owner*Main.myPlayer)].name+" wins", Color.Gray);
                }
                break;
            }
            endGameTimeout = 180;
            gameInactive = true;
        }
        public void EndTurn() {
            if(gameMode==AI&&currentPlayer==0) {
                aiMoveTimeout = 1;
            }
            currentPlayer ^= 1;
            if(gameMode==LOCAL)owner = currentPlayer;
        }
        public void HighlightMoves() {
            if(!selectedPiece.HasValue)return;
            GamePieceItemSlot slot = gamePieces.Index(selectedPiece.Value);
            Chess_Piece piece = slot?.item?.ModItem as Chess_Piece;
            if(!(piece is null)) {
                Point[] moves = new Point[0];
                if(gameMode==ONLINE) {
                    moves = piece.GetMoves(slot, (owner==1)^piece.White?1:-1);
                } else {
                    moves = piece.GetMoves(slot, piece.White?1:-1);
                }
                for(int i = moves.Length; i-->0;) {
                    gamePieces.Index(moves[i]).glowing = true;
                }
            }
        }
        public override Color GetTileColor(bool glowing) {
            return gameInactive ? new Color(128,128,128,128) : (glowing ? Color.White : new Color(175, 165, 165));
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
                        type = 8 + (owner*3);
                        break;
                        case 'k':
                        type = 10 - owner;
                        break;
                    }
                    if(type!=-1) {
                        BoardGames.Instance.Logger.Info("piece"+pieces[j,i]+":"+owner+":"+j+":"+type);
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
