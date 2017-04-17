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
    class GameOverScreen {

        private Button exitButton;
        private TextDisplay headerText;
        private TextDisplay resultText;
        private Command exitToLobby;

        private StateMachine matchState;

        public GameOverScreen(ChessScene scene) {

            exitButton = scene.Components.GetOrRegister<Button>((int)ComponentKeys.EndScreenExitBtn, Button.Create);
            exitButton.Text.Value = "Exit";
            exitButton.Visibility.Value = false;
            exitButton.PressMessage.Subscribe(new SimpleListener(OnExitPressed));

            headerText = scene.Components.GetOrRegister<TextDisplay>((int)ComponentKeys.EndScreenHeaderText, TextDisplay.Create);
            headerText.Text.Value = "Match End";
            headerText.Visibility.Value = false;

            resultText = scene.Components.GetOrRegister<TextDisplay>((int)ComponentKeys.EndScreenResultText, TextDisplay.Create);
            resultText.Text.Value = "";
            resultText.Visibility.Value = false;

            matchState = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.MatchState, StateMachine.Create);
            matchState.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnMatchStateChange()));

            exitToLobby = scene.Components.GetOrRegister<Command>((int)ComponentKeys.EndScreenExitPressed, Command.Create);

            scene.Components.GetOrRegister<Message<GameEndType>>((int)ComponentKeys.GameEnd, Message<GameEndType>.Create)
                .Subscribe(new SimpleListener<GameEndType>(OnGameEndMessage));
        }

        private void OnGameEndMessage(GameEndType type) {
            switch(type) {
            case GameEndType.Win:
                resultText.Text.Value = "Win!";
                break;
            case GameEndType.Loss:
                resultText.Text.Value = "Loss!";
                break;
            case GameEndType.Tie:
                resultText.Text.Value = "Tie!";
                break;
            default:
                resultText.Text.Value = "Error";
                break;
            }
        }

        private void OnMatchStateChange() {
            bool inEndState = matchState.State == (int)MatchState.End;
            exitButton.Visibility.Value = inEndState;
            headerText.Visibility.Value = inEndState;
            resultText.Visibility.Value = inEndState;
        }

        private void OnExitPressed() {
            exitToLobby.Send();
        }
    }
}
