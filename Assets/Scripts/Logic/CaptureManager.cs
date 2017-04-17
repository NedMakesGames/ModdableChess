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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic {
    public struct CaptureCommand {
        public IntVector2 space;
    }

    public class CaptureManager {

        private Board board;
        private Message<int> pieceCapturedEvent;

        public CaptureManager(ChessScene scene) {
            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            scene.Components.GetOrRegister<Command<CaptureCommand>>((int)ComponentKeys.CapturePieceCommand, Command<CaptureCommand>.Create)
                .Handler = DoCapture;
            pieceCapturedEvent = scene.Components.GetOrRegister<Message<int>>((int)ComponentKeys.PieceCapturedEvent, Message<int>.Create);
        }

        private void DoCapture(CaptureCommand command) {
            int pieceOnSpace = board.PieceOnSpace(command.space);
            if(pieceOnSpace >= 0) {
                board.Pieces[pieceOnSpace].IsCaptured = true;
                pieceCapturedEvent.Send(pieceOnSpace);
            } else {
                throw new TurnActionExcecutionException(
                        string.Format("No piece to capture at position ({0}, {1})", command.space.x + 1, command.space.y + 1));
            }
        }
    }
}
