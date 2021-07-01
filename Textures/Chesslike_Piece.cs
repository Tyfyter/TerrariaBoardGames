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
        None = 0b0000,
        ForwardsOnly = 0b0001,
        BackwardsOnly = 0b0010,
        OnSides = 0x0100,
        NotOnSides = 0x1000
    }
}
