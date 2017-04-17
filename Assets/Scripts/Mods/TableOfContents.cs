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

namespace ModdableChess.Mods {
    public class BoardSize {
        public int width;
        public int length;
    }

    public class Piece {
        public string name;
        public string player1Model;
        public string player2Model;
        public string promoteIndicatorModel;
        public string actionOptionFunction;
    }

    public class TableOfContents {
        public string name;
        public string displayName;
        public string author;
        public string displayVersion;
        public int version;

        public BoardSize boardSize;
        public string boardSetupFunction;
        public string winLossFunction;
        public float worldTileSize;
        public string boardModel;
        public ActionIndicator[] actionIndicators;
        public float indicatorStackingHeight;
        public Piece[] pieces;

        public string[] extraLuaFiles;
    }

    public class ActionIndicator {
        public string type;
        public string strength;
        public string model;
    }
}
