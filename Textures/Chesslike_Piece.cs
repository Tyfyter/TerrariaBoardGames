using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using BoardGames.UI;

namespace BoardGames.Textures {
    public class Chesslike_Piece : ModItem {
        public override bool CloneNewInstances => true;
        public bool PlayerOne { get; internal set; }
        public bool Vital { get; internal set; }
        public Chesslike_Move[] Moveset { get; private set; }
        public override bool Autoload(ref string name) {
            return false;
        }
        public Chesslike_Piece(Chesslike_Move[] moveset, bool playerOne) : base() {
            Moveset = moveset;
            PlayerOne = playerOne;
        }
        public Chesslike_Piece() : base() {
            Moveset = default;
            PlayerOne = true;
        }
        public Chesslike_Action[] GetMoves(GamePieceItemSlot slot, int YSign) {

            return new Chesslike_Action[0];
        }
    }
    public class Chesslike_Action {
        public Point Move { get; private set; }
        public Point[] Attacks { get; private set; }
        public Chesslike_Action(Point move, params Point[] attacks) {
            Move = move;
            Attacks = attacks;
        }
    }
    public struct Chesslike_Move {
        public readonly int runs;
        public readonly int x;
        public readonly int y;
        public readonly Chesslike_Move_Type type;
        public readonly Chesslike_Move_Restrictions restrictions;
    }
    public enum Chesslike_Move_Type {
        Normal,
        AttackOnly,
        NoAttack,
        AttackNoMove,
        Checker,
        AttackOnlyChecker
    }
    public enum Chesslike_Move_Restrictions {
        None = 0b00000,
        ForwardsOnly = 0b00001,
        BackwardsOnly = 0b00010,
        OnSides = 0x00100,
        NotOnSides = 0x01000,
        Flying = 0x10000
    }
}
