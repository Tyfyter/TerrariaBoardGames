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
using BoardGames.Textures.Pieces;

namespace BoardGames.UI {
	public abstract class GameUI : UIState {
        public int owner = 0;
        public int currentPlayer = 0;
        public GamePieceItemSlot[,] gamePieces;
        public Point? selectedPiece;
        bool oldMouseLeft;
        public bool solitaire;
        public int aiMoveTimeout = 0;
        public bool local;
        public bool gameInactive;
        public bool JustClicked => Main.mouseLeft && !oldMouseLeft;
        public override void OnInitialize() {
            Main.UIScaleMatrix.Decompose(out Vector3 scale, out Quaternion ignore, out Vector3 ignore2);
            Vector2 basePosition = new Vector2((float)(Main.screenWidth * 0.05), (float)(Main.screenHeight * 0.4));
            Vector2 slotSize = new Vector2(50 * scale.X, 50 * scale.Y);
            //solitaire = Main.netMode == NetmodeID.SinglePlayer;
            local = Main.netMode == NetmodeID.SinglePlayer;
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
        public static DieSpriteSet[] DieSet { get; private set; }
        int roll = 0;
        bool rolled = false;
        int[][] allRolls;
        public int[] remainingPieces;
        public int[] activePieces;
        int endTurnTimeout = 0;
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
            DieSet = new DieSpriteSet[] {
                new DieSpriteSet("BoardGames/Textures/Pieces/Ur_Die_1", startIndex:1, endIndex:4),
                new DieSpriteSet("BoardGames/Textures/Pieces/Ur_Die_2", startIndex:1, endIndex:4)
            };
            BoardGames.UnloadTextures += UnloadTextures;
        }
        public static void UnloadTextures() {
            RosetteTexture = null;
            OtherTextures = null;
            GamePieceTypes = null;
            DieSet = null;
        }
        public static Texture2D RosetteTexture { get; private set; }
        public static Texture2D[] OtherTextures { get; private set; }
        protected override void Init(Vector3 scale, Vector2 basePosition, Vector2 slotSize) {
            basePosition = new Vector2((float)(Main.screenWidth * 0.5), (float)(Main.screenHeight * 0.5));
            basePosition -= slotSize * new Vector2(1.5f, 4);
            gamePieces = new GamePieceItemSlot[3,8];
            remainingPieces = new int[] { 7,7 };
            activePieces = new int[] { 0,0 };
            allRolls = new int[4][];
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
            if(gameInactive||remainingPieces[0]==0||remainingPieces[1]==0) {
                if(!gameInactive) {
                    if(solitaire) {
                        if(remainingPieces[0]==0) {
                            Main.NewText("Player wins", Color.Firebrick);
                        } else {
                            Main.NewText("AI wins", Color.DodgerBlue);
                        }
                    } else if(local) {
                        if(remainingPieces[0]==0) {
                            Main.NewText("Player 1 wins", Color.Firebrick);
                        } else {
                            Main.NewText("Player 2 wins", Color.DodgerBlue);
                        }
                    } else {
                        if(remainingPieces[owner]==0) {
                            Main.NewText(Main.LocalPlayer.name+" wins", Color.Firebrick);
                        }
                    }
                }
                gameInactive = true;
                return;
            }
            if(endTurnTimeout>0) {
                if(++endTurnTimeout>45) {
                    EndTurn();
                    endTurnTimeout = 0;
                }
            }
            if(aiMoveTimeout>0) {
                if(++aiMoveTimeout > 30) {
                    Point AiMove = new Point();
                    aiMoveTimeout = 0;
                    List<Point> allMoves = new List<Point> { };
                    List<(int priority, Point target)> captureMoves = new List<(int,Point)> { };
                    Point? offMove = null;
                    if(!rolled) {
                        SelectPiece(new Point(2, 5));
                        if(endTurnTimeout<1)
                            aiMoveTimeout = 1;
                    } else {
                        Point[] move;
                        Item targetItem;
                        for(int j = 0; j < 8; j++) {
                            for(int i = 0; i < 3; i++) {
                                if((i ^ 2) == (currentPlayer*2)) {
                                    continue;
                                }
                                if(i == 2&&j == 5) {
                                    move = null;
                                }
                                if(CanMoveTo(new Point(i, j))) {
                                    move = GetMoveTo(new Point(i, j));
                                    if(move.Length != 0 && CanMoveFrom(move[0])) {
                                        if(Grid[j, i%2] == 'o') {
                                            if(Grid[move[0].Y, i%2] != 'o') {
                                                offMove = move[0];
                                                allMoves.Add(offMove.Value);
                                            }
                                        } else {
                                            allMoves.Add(new Point(i,j));
                                            targetItem = gamePieces[i,j].item;
                                            if(!(targetItem is null)&&!targetItem.IsAir) {
                                                captureMoves.Add((j, new Point(i,j)));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if(offMove.HasValue) {
                            AiMove = offMove.Value;
                        }else if(captureMoves.Count==0) {
                            AiMove = Main.rand.Next(allMoves);
                        } else {
                            captureMoves = captureMoves.OrderBy(v=>v.priority).ToList();
                            AiMove = captureMoves[0].target;
                        }
                        SelectPiece(AiMove);
                    }
                    if(endTurnTimeout==0&&aiMoveTimeout==0&&currentPlayer!=0) {
                        string warning = $"ai attempted invalid move {AiMove} with roll {roll}";
                        if(offMove.HasValue) {
                            warning += " believing it would move a piece off the board";
                        }else if(captureMoves.Count==0) {
                            warning += " believing it would capture an opponent's piece";
                        }
                        BoardGames.Instance.Logger.Warn(warning);
                        Main.NewText(warning, Color.OrangeRed);
                        endTurnTimeout = 1;
                    }
                }
            }
            base.Update(gameTime);
        }
        public void HighlightMoves(Point target) {
            if(!rolled||(target.X^2)==(currentPlayer*2)||Grid[target.Y, target.X%2] == 'o') {
                return;
            }
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
            if((target.X^2)==(currentPlayer*2)) {
                return;
            }
            bool noAction = true;
            if(Grid[target.Y, target.X%2] == 'o') {
                if(!rolled) {
                    roll = 0;
                    int[] rolls;
                    for(int i = 0; i < 4; i++) {
                        rolls = DieSet[Main.rand.Next(2)].GetRolls(2);
                        allRolls[i] = rolls;
                        if(rolls[0]==0||rolls[1]==0) {
                            roll++;
                        }
                    }
                    if(roll==0) {
                        endTurnTimeout = 1;
                    } else {
                        Point[] move;
                        int totalMoves = 0;
                        for(int j = 0; j < 8; j++) {
                            for(int i = 0; i < 3; i++) {
                                if(i==(currentPlayer^2)) {
                                    continue;
                                }
                                if(CanMoveFrom(new Point(i,j))) {
                                    move = GetMoveFrom(new Point(i,j));
                                    if(move.Length != 0 && CanMoveTo(move[0])) {
                                        totalMoves++;
                                    }
                                }
                            }
                        }
                        if(totalMoves==0) {
                            endTurnTimeout = 1;
                        }
                    }
                    rolled = true;
                    noAction = false;
                }
            } else if (rolled){
                Point slotA = new Point(-1,-1);
                Point slotB = new Point(-1,-1);
                Point[] move = GetMoveFrom(target);
                if(move.Length!=0 && CanMoveFrom(target) && CanMoveTo(move[0])) {
                    slotA = new Point(target.X, target.Y);
                    slotB = new Point(move[0].X, move[0].Y);
                } else {
                    move = GetMoveTo(target);
                    if(move.Length!=0) {
                        slotA = new Point(move[0].X, move[0].Y);
                        slotB = new Point(target.X, target.Y);
                    }
                }
                if(slotA!=new Point(-1,-1) && CanMoveFrom(slotA) && CanMoveTo(slotB)) {
                    noAction = false;
                    if(Grid[slotA.Y, slotA.X % 2] == 'o') {
                        gamePieces[slotB.X, slotB.Y].SetItem(GamePieceTypes[currentPlayer]);
                        activePieces[currentPlayer]++;
                    } else if(Grid[slotB.Y, slotB.X % 2] == 'o') {
                        gamePieces[slotA.X, slotA.Y].SetItem(null);
                        remainingPieces[currentPlayer]--;
                        activePieces[currentPlayer]--;
                    } else {
                        if(gamePieces[slotB.X, slotB.Y].item?.type==GamePieceTypes[currentPlayer^1]) {
                            activePieces[currentPlayer^1]--;
                        }
                        gamePieces[slotB.X, slotB.Y].SetItem(gamePieces[slotA.X, slotA.Y].item);
                        gamePieces[slotA.X, slotA.Y].SetItem(null);
                    }
                    if(Grid[slotB.Y, slotB.X % 2] == 'r') {
                        rolled = false;
                        if(solitaire&&currentPlayer!=0) {
                            aiMoveTimeout = 1;
                        }
                    } else {
                        EndTurn();
                    }
                }
            }
            if(noAction&&solitaire&&endTurnTimeout==0&&aiMoveTimeout==0&&currentPlayer!=0) {
                return;
            }
        }
        public void EndTurn() {
            rolled = false;
            if(solitaire&&currentPlayer==0) {
                aiMoveTimeout = 1;
            }
            currentPlayer ^= 1;
            if(local)owner = currentPlayer;
        }
        public bool CanMoveFrom(Point value) {
            if(Grid[value.Y,value.X%2]=='o') {
                return remainingPieces[currentPlayer]>activePieces[currentPlayer];
            }
            GamePieceItemSlot slot = gamePieces[value.X,value.Y];
            if(!(slot.item is null)&&slot.item.type==GamePieceTypes[currentPlayer]) {
                return true;
            }
            return false;
        }
        public bool CanMoveTo(Point value) {
            GamePieceItemSlot slot = gamePieces[value.X,value.Y];
            if(slot.item is null||slot.item.IsAir||(slot.item.type!=GamePieceTypes[currentPlayer]&&Grid[value.Y,value.X%2]!='r')) {
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
                    }else if(i == 0 && Grid[current.Y, current.X % 2] == 'r' && gamePieces[current.X,current.Y].item?.type==GamePieceTypes[currentPlayer^1]) {
                        current = GetNextSquare(current);
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
                }else if(i == roll && Grid[current.Y, current.X % 2] == 'r' && gamePieces[current.X,current.Y].item?.type==GamePieceTypes[currentPlayer^1]) {
                    current = GetLastSquare(current);
                }
            }
            return new Point[]{current};
        }
        public Point GetNextSquare(Point current) {
            if(current.Y==0&&current.X!=1) {
                return new Point(1,0);
            }
            if(current.Y==7&&current.X==1) {
                return new Point(currentPlayer*2,7);
            }
            if(current.X==1) {
                return new Point(1,current.Y+1);
            } else {
                return new Point(current.X,current.Y-1);
            }
        }
        public Point GetLastSquare(Point current) {
            if(current.Y==0&&current.X==1) {
                return new Point(currentPlayer*2,0);
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
        public override void Draw(SpriteBatch spriteBatch) {
            base.Draw(spriteBatch);
            if(gameInactive) return;
            Vector2 rollPos = gamePieces[currentPlayer * 2, 5].GetDimensions().ToRectangle().Center();
            var font = Main.fontCombatText[1];
            string text = "roll";
            if(rolled){
                text = "" + roll;
            }
            rollPos -= font.MeasureString(text)/2;
			Utils.DrawBorderStringFourWay(spriteBatch, font, text, rollPos.X, rollPos.Y, Color.White, Color.Black, Vector2.Zero, 1);
            Rectangle pieceSquare = gamePieces[1, 7].GetDimensions().ToRectangle();
            Vector2 centerPos = pieceSquare.Center();
            for(int i = remainingPieces[0]; i-->0;)
                spriteBatch.Draw(Main.itemTexture[GamePieceTypes[0]],
                    centerPos+new Vector2(pieceSquare.Width*-3, pieceSquare.Height*(remainingPieces[0]-i)*-0.5f),
                    (i < activePieces[0])?new Color(150,150,150,150):Color.White);
            for(int i = remainingPieces[1]; i-->0;)
                spriteBatch.Draw(Main.itemTexture[GamePieceTypes[1]],
                    centerPos+new Vector2(pieceSquare.Width*3, pieceSquare.Height*(remainingPieces[1]-i)*-0.5f),
                    (i < activePieces[1])?new Color(150,150,150,150):Color.White);
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