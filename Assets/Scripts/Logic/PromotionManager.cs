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
    public struct PromoteCommand {
        public IntVector2 piecePos;
        public int promoteTo;
    }

    public class PromotionManager {

        private Board board;
        private GameDatabase db;
        private Message<int> piecePromotionDone;

        public PromotionManager(AutoController scene) {
            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);

            piecePromotionDone = scene.Components.GetOrRegister<Message<int>>((int)ComponentKeys.PiecePromotionEvent, Message<int>.Create);
            scene.Components.GetOrRegister<Command<PromoteCommand>>
                ((int)ComponentKeys.PromotePieceCommand, Command<PromoteCommand>.Create)
                .Handler = DoPromote;
        }

        private void DoPromote(PromoteCommand command) {
            int pieceOnStart = board.PieceOnSpace(command.piecePos);
            if(pieceOnStart >= 0) {
                Piece piece = board.Pieces[pieceOnStart];
                int promoteTo = command.promoteTo;
                if(promoteTo >= 0 && promoteTo < db.PiecePrototypes.Count && promoteTo != piece.PrototypeID) {
                    BalugaDebug.Log(string.Format("Piece {0} promoted to prototype {1}", pieceOnStart, promoteTo));
                    piece.PrototypeID = promoteTo;
                    piecePromotionDone.Send(pieceOnStart);
                } else {
                    throw new TurnActionExcecutionException(
                        string.Format("Cannot promote piece at ({0}, {1})! It is already the promotion type.", 
                        command.piecePos.x + 1, command.piecePos.y + 1));
                }
            } else {
                throw new TurnActionExcecutionException(
                        string.Format("No piece to promote at position ({0}, {1})", command.piecePos.x + 1, command.piecePos.y + 1));
            }
        }
    }
}
