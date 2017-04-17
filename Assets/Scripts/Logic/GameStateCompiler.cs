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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic {
    public class GameStateCompiler {

        private Board board;
        private LocalPlayerInformation localPlayer;

        public GameStateCompiler(AutoController scene) {
            localPlayer = scene.Game.Components.Get<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation);
            scene.Components.GetOrRegister((int)ComponentKeys.CompileCurrentGameState,
                Query<NetworkGameState>.Create).Handler = CompileState;
            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
        }

        private NetworkGameState CompileState() {
            NetworkGameState state = new NetworkGameState();
            state.activePlayer = board.ActivePlayer;
            if(localPlayer.IsHost) {
                state.hostPlayerOrder = board.LocalPlayerOrder;
            } else {
                if(board.LocalPlayerOrder == PlayerTurnOrder.First) {
                    state.hostPlayerOrder = PlayerTurnOrder.Second;
                } else {
                    state.hostPlayerOrder = PlayerTurnOrder.First;
                }
            }
            state.turnNumber = board.TurnNumber;
            state.turnState = board.TurnState;

            // Pieces
            state.pieces = new NetworkGameStatePiece[board.Pieces.Count];
            for(int p = 0; p < state.pieces.Length; p++) {
                NetworkGameStatePiece piece = new NetworkGameStatePiece();
                state.pieces[p] = piece;
                Piece livePiece = board.Pieces[p];
                piece.prototypeID = livePiece.PrototypeID;
                piece.position = livePiece.BoardPosition;
                piece.owner = livePiece.OwnerPlayer;
                piece.isCaptured = livePiece.IsCaptured;
                piece.numberMovesTaken = livePiece.NumberOfMoves;
            }

            return state;
        }

        
    }
}
