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

    public enum LobbySetupExitState {
        None, Choosing, Ready, Exiting
    }

    public class LobbyExitController {

        private StateMachine state;
        private SubscribableBool readyStatus;
        private LobbyChoices choices;
        private Command<bool> notifyServerReady;
        private ModdableChessGame game;
        private StateMachine screen;
        private StateMachine initStatus;
        private Query<string, string> getModFolder;

        public LobbyExitController(AutoController scene) {
            game = (ModdableChessGame)scene.Game;

            choices = scene.Game.Components.Get<LobbyChoices>((int)ComponentKeys.LobbyChoices);

            state = new StateMachine(0);
            state.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnStateChange()));

            readyStatus = scene.Components.GetOrRegister((int)ComponentKeys.LobbyReadyStatus, SubscribableBool.Create);
            readyStatus.Subscribe(new SimpleListener<bool>((v) => OnReadyStatusChange()));
            readyStatus.Value = false;

            notifyServerReady = scene.Game.Components.Get<Command<bool>>((int)ComponentKeys.LobbyReadyNoticeSendRequest);

            scene.ActivatableList.Add(new ListenerJanitor<IListener<LobbyExitState>>(
                scene.Game.Components.Get<IMessage<LobbyExitState>>((int)ComponentKeys.LobbyExitMessageReceived),
                new SimpleListener<LobbyExitState>(OnAllReadyReceived)));

            scene.Components.GetOrRegister<SubscribableObject<string>>((int)ComponentKeys.LobbyModSelected, SubscribableObject<string>.Create)
                .Subscribe(new SimpleListener<string>(OnModChange));

            initStatus = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.LobbyInitStatus, StateMachine.Create);
            initStatus.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnInitStatusChange()));

            getModFolder = scene.Components.GetOrRegister<Query<string, string>>((int)ComponentKeys.GetCachedModFolder, Query<string, string>.Create);

            screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => {
                state.State = 0;
                readyStatus.Value = false;
                if(s == (int)LobbyScreen.HostGamePrefs || s == (int)LobbyScreen.ClientGamePrefs) {
                    SendReadyStatus();
                }
            }));
        }

        private void OnStateChange() {
            if(state.State != (int)LobbySetupExitState.Ready) {
                if(initStatus.State == (int)LobbyInitializedCheckStatus.Initialized) {
                    notifyServerReady.Send(false);
                }
            }
        }

        private void OnInitStatusChange() {
            switch((LobbySetupExitState)state.State) {
            case LobbySetupExitState.Choosing:
            case LobbySetupExitState.Ready:
                if(initStatus.State == (int)LobbyInitializedCheckStatus.Initialized) {
                    SendReadyStatus();
                }
                break;
            }
        }

        private void OnModChange(string modfolder) {
            if(string.IsNullOrEmpty(modfolder)) {
                readyStatus.Value = false;
            }
        }

        private void OnReadyStatusChange() {
            SendReadyStatus();
        }

        private void SendReadyStatus() {
            if(readyStatus.Value) {
                state.State = (int)LobbySetupExitState.Ready;
                //resendTimer = Server.ServerReadyCheckManager.TIMEOUT_PERIOD;
            } else {
                state.State = (int)LobbySetupExitState.Choosing;
            }
            if(initStatus.State == (int)LobbyInitializedCheckStatus.Initialized) {
                notifyServerReady.Send(readyStatus.Value);
            }
        }

        private void OnAllReadyReceived(LobbyExitState exitState) {
            if(state.State == (int)LobbySetupExitState.Ready) {
                state.State = (int)LobbySetupExitState.Exiting;
                screen.State = (int)LobbyScreen.Disposing;
                choices.OrderChoice = exitState.turnOrderChoice;
                choices.ModFolder = getModFolder.Send(exitState.modName);
                game.Controller = new ModLoadingScene(game);
            }
        }
    }
}
