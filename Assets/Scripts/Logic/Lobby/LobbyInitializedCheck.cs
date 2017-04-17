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

namespace ModdableChess.Logic.Lobby {

    public enum LobbyInitializedCheckStatus {
        None, Sent, Initialized
    }

    public class LobbyInitializedCheck {

        private StateMachine state;
        private StateMachine screen;
        private Command<bool> sendReady;

        public LobbyInitializedCheck(AutoController scene) {
            state = new StateMachine(0);
            scene.Components.Register((int)ComponentKeys.LobbyInitStatus, state);

            sendReady = scene.Game.Components.Get<Command<bool>>((int)ComponentKeys.LobbyInitSendReady);

            scene.ActivatableList.Add(new ListenerJanitor<IListener>(
                scene.Game.Components.Get<IMessage>((int)ComponentKeys.LobbyInitAllReadyReceived),
                new SimpleListener(OnAllReadyReceived)));

            screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnScreenChange()));
        }

        private void OnScreenChange() {
            if(screen.State == (int)LobbyScreen.HostGamePrefs || screen.State == (int)LobbyScreen.ClientGamePrefs) {
                SendReadyCheck(true);
            } else {
                if(state.State == (int)LobbyInitializedCheckStatus.Sent) {
                    SendReadyCheck(false);
                }
                state.State = (int)LobbyInitializedCheckStatus.None;
            }
        }

        private void SendReadyCheck(bool ready) {
            if(ready) {
                state.State = (int)LobbyInitializedCheckStatus.Sent;
            } else {
                state.State = (int)LobbyInitializedCheckStatus.None;
            }
            sendReady.Send(ready);
        }

        private void OnAllReadyReceived() {
            if(state.State == (int)LobbyInitializedCheckStatus.Sent) {
                state.State = (int)LobbyInitializedCheckStatus.Initialized;
                sendReady.Send(false);
            }
        }
    }
}
