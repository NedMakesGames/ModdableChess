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

namespace ModdableChess.Logic.PlayUI {
    public class ActiveTurnDisplay {

        private TextDisplay text;
        private Board board;
        private StateMachine matchState;

        public ActiveTurnDisplay(ChessScene scene) {
            text = scene.Components.GetOrRegister<TextDisplay>((int)ComponentKeys.ActiveTurnText, TextDisplay.Create);
            text.Text.Value = "";
            text.Visibility.Value = false;

            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => {
                    board = b;
                    RefreshText();
                }));

            scene.Components.GetOrRegister<Message>((int)ComponentKeys.TurnChange, Message.Create)
                .Subscribe(new SimpleListener(RefreshText));

            matchState = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.MatchState, StateMachine.Create);
            matchState.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnMatchStateChange()));
            OnMatchStateChange();
        }

        private void OnMatchStateChange() {
            text.Visibility.Value = matchState.State == (int)MatchState.Play;
        }

        private void RefreshText() {
            if(board != null) {
                if(board.ActivePlayer == board.LocalPlayerOrder) {
                    text.Text.Value = string.Format("Turn #{0} - your turn!", board.TurnNumber + 1);
                } else {
                    text.Text.Value = string.Format("Turn #{0} - opponent's turn!", board.TurnNumber + 1);
                }
            }
        }
    }
}
