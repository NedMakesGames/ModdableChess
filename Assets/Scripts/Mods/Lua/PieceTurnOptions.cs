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

using Baluga3.Core;
using Baluga3.GameFlowLogic;
using ModdableChess.Logic;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Mods.Lua {
    [MoonSharpUserData]
    public class PieceTurnOptions {

        private Piece piece;
        private TurnOptions options;
        private bool disposed;
        private Query<bool, PieceTurnOptions> qHasOptions;
        private Query<bool, PieceTurnOptions, IntVector2> qCanMove;
        private Query<bool, PieceTurnOptions, IntVector2> qCanCapture;
        private Query<bool, PieceTurnOptions, IntVector2> qCanPromote;

        [MoonSharpHidden]
        public PieceTurnOptions() {

        }

        public Piece Piece {
            get {
                return piece;
            }
            [MoonSharpHidden]
            set {
                this.piece = value;
            }
        }

        public bool Disposed {
            get {
                return true;
            }
        }

        [MoonSharpHidden]
        public TurnOptions Options {
            get {
                return options;
            }

            set {
                options = value;
            }
        }

        [MoonSharpHidden]
        public Query<bool, PieceTurnOptions> QHasOptions {
            get {
                return qHasOptions;
            }

            set {
                qHasOptions = value;
            }
        }

        [MoonSharpHidden]
        public Query<bool, PieceTurnOptions, IntVector2> QCanMove {
            get {
                return qCanMove;
            }

            set {
                qCanMove = value;
            }
        }

        [MoonSharpHidden]
        public Query<bool, PieceTurnOptions, IntVector2> QCanCapture {
            get {
                return qCanCapture;
            }

            set {
                qCanCapture = value;
            }
        }

        [MoonSharpHidden]
        public Query<bool, PieceTurnOptions, IntVector2> QCanPromote {
            get {
                return qCanPromote;
            }

            set {
                qCanPromote = value;
            }
        }

        [MoonSharpHidden]
        public void Dispose() {
            disposed = true;
            piece = null;
        }

        public bool HasAnyOptions() {
            if(disposed) {
                return false;
            } else {
                return qHasOptions.Send(this);
            }
        }

        public bool CanMoveTo(int x, int y) {
            if(disposed) {
                return false;
            } else {
                return qCanMove.Send(this, new IntVector2(x, y));
            }
        }

        public bool CanCapture(int x, int y) {
            if(disposed) {
                return false;
            } else {
                return qCanCapture.Send(this, new IntVector2(x, y));
            }
        }

        public bool CanCapture(Piece piece) {
            return CanCapture(piece.PositionX, piece.PositionY);
        }

        public bool CanPromote(int x, int y) {
            if(disposed) {
                return false;
            } else {
                return qCanPromote.Send(this, new IntVector2(x, y));
            }
        }
    }
}
