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
    public class Checkers_UI : GameUI {
        public override void TryLoadTextures() => LoadTextures();
        public static int[] GamePieceTypes { get; private set; }
        public static Texture2D[] BoardTextures { get; private set; }
        public static Point NoAttack => new Point(-1,-1);
        public bool mandatoryJumps = false;
        public bool hasJumps = false;
        public bool movedOnce = false;
        public static void LoadTextures() {
            BoardTextures = new Texture2D[] {
                ModContent.GetTexture("BoardGames/Textures/Checkers_Tile_Light"),
                ModContent.GetTexture("BoardGames/Textures/Checkers_Tile_Dark")
            };
            GamePieceTypes = new int[] {
                ModContent.ItemType<Textures.Pieces.Black>(),
                ModContent.ItemType<Textures.Pieces.Red>()
            };
            BoardGames.UnloadTextures += UnloadTextures;
        }
        public static void UnloadTextures() {
            BoardTextures = null;
            GamePieceTypes = null;
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
            Point? oldSelected = selectedPiece;
            if(selectedPiece.HasValue) {
                if(movedOnce && !mandatoryJumps && selectedPiece == target) {
                    EndTurn();
                } else {
                    (Point end, Point attacked)[] moves = GetMoves(selectedPiece.Value).Where((v) => v.end == target).ToArray();
                    if(moves.Length > 0 && (!hasJumps||moves[0].attacked!=NoAttack)) {
                        bool jumped = false;
                        int dir = (gameMode == ONLINE ? (currentPlayer == owner) : (currentPlayer == 0)) ? 1 : -1;
                        Item item = gamePieces.Index(selectedPiece.Value).item;
                        gamePieces.Index(selectedPiece.Value).SetItem(null);
                        if(moves[0].attacked != NoAttack) {
                            gamePieces.Index(moves[0].attacked).SetItem(null);
                            jumped = true;
                        }
                        if(target.Y == (3.5f - (dir * 3.5f))) {
                            item.stack = 2;
                        }
                        gamePieces.Index(target).SetItem(item);
                        movedOnce = true;
                        if(!jumped || GetMoves(target).Where((v) => v.attacked != NoAttack).Count() == 0) {
                            EndTurn();
                        }
                        selectedPiece = target;
                    }
                }
            }
            if(!movedOnce)base.SelectPiece(target);
            if(selectedPiece.HasValue) {
                if(SlotEmpty(selectedPiece.Value) ?? true) {
                    selectedPiece = null;
                } else if(gamePieces.Index(selectedPiece.Value)?.item?.type!=GamePieceTypes[currentPlayer]) {
                    selectedPiece = null;
                } else if(GetMoves(selectedPiece.Value).Length==0) {
                    selectedPiece = null;
                }
            }
            if(oldSelected != selectedPiece) {
                SoundEngine.PlaySound(new Terraria.Audio.LegacySoundStyle(21, 0, Terraria.Audio.SoundType.Sound), Main.LocalPlayer.MountedCenter).Pitch = 0.75f;
            }
        }
        public bool HasMoves() {
            bool o = false;
            (Point end, Point attacked)[] moves;
            for(int j = 0; j < 8; j++) {
                for(int i = 0; i < 8; i++) {
                    moves = GetMoves(new Point(i, j));
                    if(moves.Length>0) {
                        if(!mandatoryJumps)return true;
                        o = true;
                        if(moves.Any((v)=>v.attacked!=NoAttack)) {
                            hasJumps = true;
                            return true;
                        }
                    }
                }
            }
            return o;
        }
        public (Point end, Point attacked)[] GetMoves(Point start) {
            if(SlotEmpty(start) ?? true) {
                return new (Point,Point)[0];
            }
            GamePieceItemSlot slot = gamePieces.Index(start);
            if(slot.item.type!=GamePieceTypes[currentPlayer]) {
                return new (Point,Point)[0];
            }
            List<(Point end, Point attacked)> moves = new List<(Point,Point)>{};
            int dir = (gameMode == ONLINE ?(currentPlayer==owner):(currentPlayer==0))?1:-1;
            int tier = slot.item.stack;
            int startX = start.X;
            int startY = start.Y;
            bool hasJump = false;
            switch(SlotEmpty(startX - 1, startY - dir)) {
                case true:
                moves.Add((new Point(startX - 1, startY - dir), NoAttack));
                break;
                case false:
                if(gamePieces[startX - 1, startY - dir]?.item?.type==GamePieceTypes[currentPlayer^1]) {
                    if(SlotEmpty(startX - 2, startY - (dir*2)) ?? false) {
                        moves.Add((new Point(startX - 2, startY - (dir*2)), new Point(startX - 1, startY - dir)));
                        hasJump = true;
                    }
                }
                break;
            }
            switch(SlotEmpty(startX + 1, startY - dir)) {
                case true:
                moves.Add((new Point(startX + 1, startY - dir), NoAttack));
                break;
                case false:
                if(gamePieces[startX + 1, startY - dir]?.item?.type==GamePieceTypes[currentPlayer^1]) {
                    if(SlotEmpty(startX + 2, startY - (dir*2)) ?? false) {
                        moves.Add((new Point(startX + 2, startY - (dir*2)), new Point(startX + 1, startY - dir)));
                        hasJump = true;
                    }
                }
                break;
            }
            if(tier>1) {
                switch(SlotEmpty(startX - 1, startY + dir)) {
                    case true:
                    moves.Add((new Point(startX - 1, startY + dir), NoAttack));
                    break;
                    case false:
                    if(gamePieces[startX - 1, startY + dir]?.item?.type==GamePieceTypes[currentPlayer^1]) {
                        if(SlotEmpty(startX - 2, startY + (dir*2)) ?? false) {
                            moves.Add((new Point(startX - 2, startY + (dir*2)), new Point(startX - 1, startY + dir)));
                            hasJump = true;
                        }
                    }
                    break;
                }
                switch(SlotEmpty(startX + 1, startY + dir)) {
                    case true:
                    moves.Add((new Point(startX + 1, startY + dir), NoAttack));
                    break;
                    case false:
                    if(gamePieces[startX + 1, startY + dir]?.item?.type==GamePieceTypes[currentPlayer^1]) {
                        if(SlotEmpty(startX + 2, startY + (dir*2)) ?? false) {
                            moves.Add((new Point(startX + 2, startY + (dir*2)), new Point(startX + 1, startY + dir)));
                            hasJump = true;
                        }
                    }
                    break;
                }
            }
            if(mandatoryJumps&&hasJump) {
                hasJumps = true;
                moves.RemoveAll((v)=>v.attacked==NoAttack);
            }
            return moves.ToArray();
        }
        public void EndGame(int winner) {
            switch(gameMode) {
                case LOCAL:
                if(winner == 0) {
                    Main.NewText("Black wins", Color.Gray);
                } else {
                    Main.NewText("Red wins", Color.Red);
                }
                break;
                case AI:
                if(winner == 0) {
                    Main.NewText("Player wins", Color.Gray);
                } else {
                    Main.NewText("AI wins", Color.Red);
                }
                break;
                case ONLINE:
                int notOwner = owner^1;
                if(winner==0) {
                    Main.NewText(Main.player[(owner*otherPlayerId)+(notOwner*Main.myPlayer)].name+" wins", Color.Gray);
                } else {
                    Main.NewText(Main.player[(notOwner*otherPlayerId)+(owner*Main.myPlayer)].name+" wins", Color.Red);
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
            movedOnce = false;
            hasJumps = false;
            if(gameMode==LOCAL)owner = currentPlayer;
            if(!HasMoves()) {
                EndGame(currentPlayer ^ 1);
            }
        }
        public void HighlightMoves() {
            if(!selectedPiece.HasValue)return;
            GamePieceItemSlot slot = gamePieces.Index(selectedPiece.Value);
        }
        public override Color GetTileColor(bool glowing) {
            return gameInactive ? new Color(128,128,128,128) : (glowing ? Color.White : new Color(165, 165, 175));
        }
        public override void SetupGame() {
            char[,] pieces = new char[8, 8] {
                {'o','c','o','c','o','c','o','c'},
                {'c','o','c','o','c','o','c','o'},
                {'o','c','o','c','o','c','o','c'},
                {'o','o','o','o','o','o','o','o'},
                {'o','o','o','o','o','o','o','o'},
                {'c','o','c','o','c','o','c','o'},
                {'o','c','o','c','o','c','o','c'},
                {'c','o','c','o','c','o','c','o'}
            };
            int type = -1;
            for(int j = 0; j < 8; j++) {
                for(int i = 0; i < 8; i++) {
                    type = -1;
                    if(pieces[j, i]=='c') {
                        type = owner;
                    }
                    if(type!=-1) {
                        if(j<4) {
                            type ^= 1;
                        }
                        gamePieces[i, j].SetItem(GamePieceTypes[type]);
                    }
                }
            }
        }
    }
}
