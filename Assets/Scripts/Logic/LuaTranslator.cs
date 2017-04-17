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
    public class LuaTranslator {
        public static string GetPlayerTag(PlayerTurnOrder order) {
            switch(order) {
            case PlayerTurnOrder.First:
                return Mods.Lua.LuaConstants.FirstPlayer;
            case PlayerTurnOrder.Second:
                return Mods.Lua.LuaConstants.SecondPlayer;
            default:
                return "";
            }
        }

        public static PlayerTurnOrder GetPlayerFromLua(string player) {
            switch(player) {
            case Mods.Lua.LuaConstants.FirstPlayer:
                return PlayerTurnOrder.First;
            case Mods.Lua.LuaConstants.SecondPlayer:
                return PlayerTurnOrder.Second;
            default:
                return PlayerTurnOrder.Undecided;
            }
        }

        public static Mods.Lua.Board GetBoard(Board gameBoard, GameDatabase db) {
            Mods.Lua.Board luaBoard = new Mods.Lua.Board() {
                Width = db.BoardDimensions.x,
                Length = db.BoardDimensions.y,
                Pieces = new Mods.Lua.Piece[gameBoard.Pieces.Count],
            };

            for(int i = 0; i < luaBoard.NumPieces; i++) {
                luaBoard.Pieces[i] = GetPiece(i, gameBoard, db);
            }

            return luaBoard;
        }

        public static Mods.Lua.Piece GetPiece(int pieceIndex, Board gameBoard, GameDatabase db) {
            Piece gamePiece = gameBoard.Pieces[pieceIndex];
            PiecePrototype behavior = db.PiecePrototypes[gamePiece.PrototypeID];
            bool isCaptured = gamePiece.IsCaptured;
            return new Mods.Lua.Piece() {
                Index = pieceIndex + 1,
                IsCaptured = isCaptured,
                Owner = GetPlayerTag(gamePiece.OwnerPlayer),
                PieceName = behavior.LuaTag,
                PositionX = isCaptured ? 0 : gamePiece.BoardPosition.x + 1,
                PositionY = isCaptured ? 0 : gamePiece.BoardPosition.y + 1,
                NumberOfMoves = gamePiece.NumberOfMoves,
            };
        }

        public static Mods.Lua.TurnInfo GetTurnInfo(Board gameBoard) {
            return new Mods.Lua.TurnInfo() {
                ActivePlayer = GetPlayerTag(gameBoard.ActivePlayer),
                TurnNumber = gameBoard.TurnNumber,
            };
        }

        public static Mods.Lua.TurnInfo GetTurnInfo(NextTurnInfo nextTurn) {
            return new Mods.Lua.TurnInfo() {
                ActivePlayer = GetPlayerTag(nextTurn.player),
                TurnNumber = nextTurn.number,
            };
        }

        public static Mods.Lua.MoveCalcState GetMoveCalcState(int movingPiece, Board gameBoard, GameDatabase db) {
            return new Mods.Lua.MoveCalcState() {
                MovingPiece = movingPiece + 1,
                Board = GetBoard(gameBoard, db),
                TurnInfo = GetTurnInfo(gameBoard),
            };
        }
    }
}
