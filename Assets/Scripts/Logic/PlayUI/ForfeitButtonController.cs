﻿// Copyright (c) 2017, Timothy Ned Atton.
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
    class ForfeitButtonController {

        private Button button;
        private StateMachine chooserState;
        private Command forfeitMessage;

        public ForfeitButtonController(ChessScene scene) {
            button = scene.Components.GetOrRegister<Button>((int)ComponentKeys.PlayForfeitButton, Button.Create);
            button.Text.Value = "Forfeit";
            button.Interactable.Value = true;
            button.Visibility.Value = false;
            button.PressMessage.Subscribe(new SimpleListener(OnPress));

            chooserState = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.TurnChooserState, StateMachine.Create);
            chooserState.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnChooserStateChange()));
            OnChooserStateChange();

            forfeitMessage = scene.Components.GetOrRegister<Command>((int)ComponentKeys.ForfeitGame, Command.Create);
        }

        private void OnChooserStateChange() {
            switch((TurnChooser.State)chooserState.State) {
            case TurnChooser.State.ChoosePiece:
            case TurnChooser.State.ChooseAction:
            case TurnChooser.State.PieceChosenWait:
                button.Visibility.Value = true;
                break;
            default:
                button.Visibility.Value = false;
                break;
            }
        }

        private void OnPress() {
            forfeitMessage.Send();
        }
    }
}
