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

using Baluga3.GameFlowLogic;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Mods.Lua {
    [MoonSharpUserData]
    public class WinLossHelper {

        private Query<bool, string> qPlayerHasPieces;
        private Query<bool, string> qPlayerCanMove;
        private Query<PieceTurnOptions, Piece> qGetTurnOptions;
        private bool ready;

        [MoonSharpHidden]
        public Query<bool, string> QPlayerHasPieces {
            get {
                return qPlayerHasPieces;
            }

            set {
                qPlayerHasPieces = value;
            }
        }

        [MoonSharpHidden]
        public Query<bool, string> QPlayerCanMove {
            get {
                return qPlayerCanMove;
            }

            set {
                qPlayerCanMove = value;
            }
        }

        [MoonSharpHidden]
        public Query<PieceTurnOptions, Piece> QGetTurnOptions {
            get {
                return qGetTurnOptions;
            }

            set {
                qGetTurnOptions = value;
            }
        }

        [MoonSharpHidden]
        public bool Ready {
            get {
                return ready;
            }

            set {
                ready = value;
            }
        }

        [MoonSharpHidden]
        public WinLossHelper() {

        }

        public string GetWinState(string player) {
            switch(player.ToUpperInvariant()) {
            case LuaConstants.FirstPlayer:
                return LuaConstants.GameOverFirstWins;
            case LuaConstants.SecondPlayer:
                return LuaConstants.GameOverSecondWins;
            default:
                return "";
            }
        }

        public string GetTieState() {
            return LuaConstants.GameOverTie;
        }

        public string GetUndecidedState() {
            return LuaConstants.GameOverUndecided;
        }

        public bool DoesPlayerHaveAnyPieces(string player) {
            if(ready) {
                return qPlayerHasPieces.Send(player);
            } else {
                return false;
            }
        }

        public bool CanPlayerMove(string player) {
            if(ready) {
                return qPlayerCanMove.Send(player);
            } else {
                return false;
            }
        }

        public PieceTurnOptions GetPieceTurnOptions(Piece piece) {
            if(ready) {
                return qGetTurnOptions.Send(piece);
            } else {
                return null;
            }
        }
    }
}
