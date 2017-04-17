// Copyright (c) 2017, Timothy Ned Atton.
// All rights reserved.
// nedmakesgames@gmail.com
// This code was written while streaming on twitch.tv/nedmakesgames
//
// This file is part of Moddable Chess.
//
// Moddable Chess is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Moddable Chess is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Moddable Chess.  If not, see <http://www.gnu.org/licenses/>.

using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Mods.Lua {
    [MoonSharpUserData]
    public class SetupBoard {

        private List<SetupPiece> pieces;

        [MoonSharpHidden]
        public SetupBoard() {
            pieces = new List<SetupPiece>();
        }

        public int Width {
            get;
            [MoonSharpHidden]
            set;
        }

        public int Length {
            get;
            [MoonSharpHidden]
            set;
        }

        public bool IsValidPosition(int x, int y) {
            return x >= 1 && x <= Width && y >= 1 && y <= Length;
        }

        public int NumPieces {
            get {
                return pieces.Count;
            }
        }

        public SetupPiece GetPiece(int index) {
            if(index >= 1 && index <= pieces.Count) {
                return pieces[index - 1];
            } else {
                return null;
            }
        }

        public SetupPiece AddPiece() {
            SetupPiece piece = new SetupPiece();
            pieces.Add(piece);
            return piece;
        }

        public SetupPiece AddPiece(string pieceName, string owner, int x, int y) {
            SetupPiece piece = AddPiece();
            piece.PieceName = pieceName;
            piece.Owner = owner;
            piece.PositionX = x;
            piece.PositionY = y;
            return piece;
        }
    }
}
