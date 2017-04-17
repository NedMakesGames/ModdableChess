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
using Baluga3.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic {
    class PieceChooser {

        private Board board;
        private StateMachine chooserState;
        private PlayerSelectionEvent selEvent;
        private SubscribableBool acceptBtn;
        private SubscribableInt chosenPiece;

        public PieceChooser(ChessScene scene) {
            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            chooserState = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.TurnChooserState, StateMachine.Create);
            chooserState.EnterStateMessenger.Subscribe(new SimpleListener<int>(OnEnterState));
            selEvent = scene.Components.GetOrRegister<PlayerSelectionEvent>((int)ComponentKeys.PlayerSelectionChange, PlayerSelectionEvent.Create);
            //selEvent.Subscribe(new SimpleListener(OnSelectionChange));
            acceptBtn = scene.Components.GetOrRegister<SubscribableBool>((int)ComponentKeys.PlayerSelectionAcceptBtn, SubscribableBool.Create);
            acceptBtn.Subscribe(new TriggerListener(OnSelectionBtnTrigger));
            chosenPiece = scene.Components.GetOrRegister<SubscribableInt>((int)ComponentKeys.TurnChooserChosenPiece, SubscribableInt.Create);
        }

        private void OnEnterState(int state) {
            if(chooserState.State == (int)TurnChooser.State.ChoosePiece) {
                chosenPiece.Value = -1;
            }
        }

        private void OnSelectionBtnTrigger() {
            if(chooserState.State == (int)TurnChooser.State.ChoosePiece) {
                int pieceID = -1;
                switch(selEvent.CurrentType) {
                case PlayerSelectionEvent.SelectionType.Piece:
                    pieceID = selEvent.PieceID;
                    break;
                case PlayerSelectionEvent.SelectionType.Square:
                    pieceID = board.PieceOnSpace(selEvent.SquarePos);
                    break;
                }
                if(pieceID >= 0) {
                    Piece piece = board.Pieces[pieceID];
                    if(!piece.IsCaptured && piece.OwnerPlayer == board.ActivePlayer) {
                        chosenPiece.Value = pieceID;
                    }
                }
            }
        }
    }
}
