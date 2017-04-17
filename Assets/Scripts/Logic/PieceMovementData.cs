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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic {
    [System.Obsolete]
    public class PieceMovementData {

        public class Table {
            public int columns;
            public Option[] options;
            public int[] repeatWithTables;
            public int beforePieceMoves;
            public int beforeTurnMoves;
            public bool allowMultiTurn;
        }

        [Flags]
        public enum SpaceFilled {
            Open=1, Enemy=2, Ally=4
        }

        public class Option {
            public bool moveTo;
            public bool isSelf;
            public SpaceFilled filledReqs;
        }

        private Table[] tables;

        public Table[] Tables {
            get {
                return tables;
            }

            set {
                this.tables = value;
            }
        }

        // 0 1 0 repeating on
        // 1 X 1
        // 0 1 0

        // 0 1 0 
        // 0 X 0 
        // 0 0 0 

        // 0 0 1 0 0
        // 0 0 E 0 0
        // 0 0 X 0 0
        // 0 0 0 0 0
        // 0 0 0 0 0

        // 0 0 0 0 1
        // 0 0 0 E 0
        // 0 0 X 0 0
        // 0 0 0 0 0
        // 0 0 0 0 0

        // 0 0 0 0 0
        // 0 0 0 0 0
        // 0 0 X E 1
        // 0 0 0 0 0
        // 0 0 0 0 0

        // Rook:
        // Table 0:
        // X 1
        // Repeat with [0, 1]

        // Table 1:
        // X 2
        // Repeat with []

        // Table 2:
        // 1 X
        // Repeat with [2, 3]

        // Table 3:
        // 2 X
        // Repeat with []

        // Table 4:
        // 1
        // X
        // Repeat with [4, 5]

        // Table 5:
        // 2
        // X
        // Repeat with []

        // Table 6:
        // X
        // 1
        // Repeat with [6, 7]

        // Table 7:
        // X
        // 2
        // Repeat with []
    }
}
