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
    public class RejoinScreenController {

        public enum State {
            None, StartError, StartHost, StartClient, ClientRejected
        }

        private SubscribableObject<string> messageText;
        private StateMachine screen;
        private StateMachine state;
        private ConnectionHelper connHelper;
        private AutoController scene;
        private LobbyChoices choices;
        
        public RejoinScreenController(AutoController scene) {
            this.scene = scene;
            ComponentRegistry guiComps = scene.Components.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages);

            messageText = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.RejoinText, messageText);

            Message backButtonPressed = new Message();
            guiComps.Register((int)GUICompKeys.RejoinCancelButton, backButtonPressed);
            backButtonPressed.Subscribe(new SimpleListener(OnBackButtonPressed));

            choices = scene.Game.Components.Get<LobbyChoices>((int)ComponentKeys.LobbyChoices);

            screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnScreenEnter()));

            state = new StateMachine(0);
            state.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => RefreshText()));

            connHelper = scene.Game.Components.Get<ConnectionHelper>((int)ComponentKeys.ConnectionHelper);
            scene.ActivatableList.Add(new ListenerJanitor<IListener<int>>(
                connHelper.Connection.EnterStateMessenger,
                new SimpleListener<int>((s) => OnConnectionChange())));
        }

        private void RefreshText() {
            switch((State)state.State) {
            case State.None:
                messageText.Value = "";
                break;
            case State.StartError:
                messageText.Value = "There was an error trying to establish connection.";
                break;
            case State.StartClient:
                messageText.Value = "Starting client. Searching for host.";
                break;
            case State.StartHost:
                messageText.Value = "Starting host. Searching for client.";
                break;
            case State.ClientRejected:
                messageText.Value = "Host rejected our connection!";
                break;
            }
        }

        private void OnScreenEnter() {
            if(screen.State == (int)LobbyScreen.Rejoin) {
                StartRejoin();
            } else {
                state.State = (int)State.None;
            }
        }

        private void StartRejoin() {
            RejoinFile.Data rejoinInfo;
            ConnectionHelper.RejoinFileRead rejoinResult = connHelper.RejoinFromFile(true, out rejoinInfo);
            switch(rejoinResult) {
            case ConnectionHelper.RejoinFileRead.Success:
                choices.ModFolder = rejoinInfo.modFolder;
                choices.OrderChoice = 0;
                choices.MatchState = LobbyMatchState.Rejoining;
                if(connHelper.CurrentMode == ConnectionHelper.Mode.Host) {
                    state.State = (int)State.StartHost;
                } else {
                    state.State = (int)State.StartClient;
                }
                break;
            default:
                state.State = (int)State.StartError;
                break;
            }
        }

        private void OnConnectionChange() {
            switch((State)state.State) {
            case State.StartClient:
            case State.StartHost:
                switch((ConnectionHelper.State)connHelper.Connection.State) {
                case ConnectionHelper.State.Connected:
                    LoadNextScene();
                    break;
                case ConnectionHelper.State.None:
                    state.State = (int)State.ClientRejected;
                    break;
                default:
                    state.State = (int)State.StartError;
                    break;
                }
                break;
            }
        }

        private void OnBackButtonPressed() {
            RejoinFile.Clear();
            connHelper.StopConnection();
            state.State = (int)State.None;
            screen.State = (int)LobbyScreen.PickHostOrClient;
        }

        private void LoadNextScene() {
            scene.Game.Controller = new ModLoadingScene((ModdableChessGame)scene.Game);
        }
    }
}
