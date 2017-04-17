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

using UnityEngine;
using System.Collections;
using Baluga3.GameFlowLogic;
using Baluga3.Core;
using System;

namespace ModdableChess.Logic {
    public struct MoveCommand {
        public IntVector2 startPosition;
        public IntVector2 moveTo;
    }

    public class PieceMover {
        
        private Message<int> pieceChangePos;
        
        private Board board;

        public PieceMover(ChessScene scene) {
            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));

            scene.Components.GetOrRegister<Command<MoveCommand>>((int)ComponentKeys.MovePieceCommand, Command<MoveCommand>.Create)
                .Handler = DoMovePiece;

            pieceChangePos = scene.Components.GetOrRegister<Message<int>>((int)ComponentKeys.PieceMovedEvent, Message<int>.Create);
            
        }

        private void DoMovePiece(MoveCommand command) {
            int pieceOnTarget = board.PieceOnSpace(command.moveTo);
            if(pieceOnTarget < 0) {
                int pieceOnStart = board.PieceOnSpace(command.startPosition);
                if(pieceOnStart >= 0) {
                    Piece movedPiece = board.Pieces[pieceOnStart];
                    movedPiece.BoardPosition = command.moveTo;
                    movedPiece.NumberOfMoves++;
                    BalugaDebug.Log(string.Format("Moved piece {0} to {1}", pieceOnStart, command.moveTo));
                    pieceChangePos.Send(pieceOnStart);
                } else {
                    throw new TurnActionExcecutionException(
                        string.Format("No piece to move at position ({0}, {1})", command.moveTo.x + 1, command.moveTo.y + 1));
                }
            } else {
                throw new TurnActionExcecutionException(
                    string.Format("Cannot move! Piece exists on ({0}, {1}) already. Capture first?", command.moveTo.x + 1, command.moveTo.y + 1));
            }
        }
    }
}
