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
    class BoardCreator {

        private LocalPlayerInformation localPlayer;

        public BoardCreator(AutoController scene) {
            localPlayer = scene.Game.Components.Get<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation);
            scene.Components.GetOrRegister<Query<Board, NetworkGameState>>
                ((int)ComponentKeys.CreateBoardCommand, Query<Board, NetworkGameState>.Create).Handler = CreateBoard;
        }

        private Board CreateBoard(NetworkGameState state) {

            UnityEngine.Debug.Log("Board creation command");

            Board board = new Board();
            if(localPlayer.IsHost) {
                board.LocalPlayerOrder = state.hostPlayerOrder;
            } else {
                if(state.hostPlayerOrder == PlayerTurnOrder.First) {
                    board.LocalPlayerOrder = PlayerTurnOrder.Second;
                } else {
                    board.LocalPlayerOrder = PlayerTurnOrder.First;
                }
            }
            board.TurnNumber = state.turnNumber;
            board.ActivePlayer = state.activePlayer;
            board.TurnState = state.turnState;
            board.Pieces = new List<Piece>();
            foreach(var piece in state.pieces) {
                board.Pieces.Add(new Piece() {
                    OwnerPlayer = piece.owner,
                    PrototypeID = piece.prototypeID,
                    BoardPosition = piece.position,
                    IsCaptured = piece.isCaptured,
                    NumberOfMoves = piece.numberMovesTaken,
                });
            }
            UnityEngine.Debug.LogError(
                string.Format("New board, turn info: local {0}, active {1}, number {2}",
                board.LocalPlayerOrder, board.ActivePlayer, board.TurnNumber));
            return board;
        }
    }
}
