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
using UnityEngine.Networking;

namespace ModdableChess.Logic.Lobby {
    public class SetupHostScreenManager {

        private enum State {
            Hidden, Choosing, Connecting
        }

        private State state;
        private StateMachine screen;
        private SubscribableBool startBtnEnabled;
        private SubscribableObject<string> setPortField;
        private SubscribableObject<string> setAddressField;
        private SubscribableBool setInputInteractable;
        private int port = 1000;
        private bool hasPort = true;
        private string address = "";
        private string password = "";
        private ConnectionHelper netHelper;
        private SubscribableObject<string> statusText;
        private SubscribableObject<string> titleText;
        private SubscribableObject<string> startButtonText;
        private SubscribableBool setAddressInteractable;
        private LobbyChoices choices;

        private SubscribableObject<string> setPasswordField;

        //private int frame;

        public SetupHostScreenManager(AutoController scene) {
            state = State.Hidden;

            ComponentRegistry guiComps = scene.Components.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages);
            guiComps.Register((int)GUICompKeys.BtnHostCancelPress, new Message());
            guiComps.Register((int)GUICompKeys.BtnStartHostPress, new Message());
            guiComps.Register((int)GUICompKeys.HostPortChange, new Message<string>());
            guiComps.Register((int)GUICompKeys.ClientAddressChange, new Message<string>());

            startBtnEnabled = new SubscribableBool(hasPort);
            guiComps.Register((int)GUICompKeys.BtnHostStartEnabledCommand, startBtnEnabled);

            setInputInteractable = new SubscribableBool(state != State.Connecting);
            guiComps.Register((int)GUICompKeys.HostStartInputInteractable, setInputInteractable);

            setPortField = new SubscribableObject<string>(port.ToString());
            guiComps.Register((int)GUICompKeys.HostPortSetCommand, setPortField);

            setAddressField = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.ClientAddressTextField, setAddressField);
            setAddressInteractable = new SubscribableBool(false);
            guiComps.Register((int)GUICompKeys.HostAddressInteractable, setAddressInteractable);

            statusText = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.HostConnectionStatusLabel, statusText);
            titleText = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.StartGameTitleText, titleText);
            startButtonText = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.StartGameStartButtonText, startButtonText);

            guiComps.Get<Message<string>>((int)GUICompKeys.HostPortChange).Subscribe(new SimpleListener<string>(OnPortChange));
            guiComps.Get<Message<string>>((int)GUICompKeys.ClientAddressChange).Subscribe(new SimpleListener<string>(OnAddressChange));
            guiComps.Get<Message>((int)GUICompKeys.BtnHostCancelPress).Subscribe(new SimpleListener(OnCancelPress));
            guiComps.Get<Message>((int)GUICompKeys.BtnStartHostPress).Subscribe(new SimpleListener(OnStartPress));

            setPasswordField = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.PasswordTextSet, setPasswordField);
            Message<string> passwordChange = new Message<string>();
            guiComps.Register((int)GUICompKeys.PasswordChange, passwordChange);
            passwordChange.Subscribe(new SimpleListener<string>(OnPasswordChange));

            choices = scene.Game.Components.Get<LobbyChoices>((int)ComponentKeys.LobbyChoices);
            screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            netHelper = scene.Game.Components.Get<ConnectionHelper>((int)ComponentKeys.ConnectionHelper);
            scene.ActivatableList.Add(new ListenerJanitor<IListener<int>>(
                netHelper.Connection.EnterStateMessenger,
                new SimpleListener<int>(OnConnectionChange)));

            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => {
                //BalugaDebug.Log("Entering screen " + (LobbyScreen)s);
                if(s == (int)LobbyScreen.SetupHost || s == (int)LobbyScreen.SetupClient) {
                    state = State.Choosing;
                    EnterScreen();
                } else {
                    state = State.Hidden;
                }
            }));
        }

        private void EnterScreen() {
            BalugaDebug.Log("Set defaults");
            hasPort = true;
            port = 1000;
            password = "";
            setPortField.Value = port.ToString();
            setPasswordField.Value = "";
            statusText.Value = "";
            setInputInteractable.Value = true;

            if(screen.State == (int)LobbyScreen.SetupHost) {
                setAddressInteractable.Value = false;
                setAddressField.Value = "localhost";
                titleText.Value = "Host Game";
                startButtonText.Value = "Start host";
            } else if(screen.State == (int)LobbyScreen.SetupClient) {
                setAddressInteractable.Value = true;
                address = "localhost";
                setAddressField.Value = address;
                titleText.Value = "Join Game";
                startButtonText.Value = "Join";
            }

            RefreshStartButtonInteractable();
        }

        private void RefreshStartButtonInteractable() {
            startBtnEnabled.Value = state != State.Connecting && hasPort && (screen.State == (int)LobbyScreen.SetupHost || !string.IsNullOrEmpty(address));
        }

        private void OnPortChange(string input) {
            hasPort = !string.IsNullOrEmpty(input) && int.TryParse(input, out port);
            setPortField.Value = port.ToString();
            RefreshStartButtonInteractable();
            BalugaDebug.Log("Port " + port);
        }

        private void OnAddressChange(string input) {
            address = input;
            setAddressField.Value = address;
            RefreshStartButtonInteractable();
            BalugaDebug.Log("Address " + address);
        }

        private void OnPasswordChange(string input) {
            password = input;
            setPasswordField.Value = input;
            BalugaDebug.Log("Password " + password);
        }

        private void OnCancelPress() {
            BalugaDebug.Log(state);
            if(state == State.Connecting) {
                BalugaDebug.Log("Stop host command");
                netHelper.StopConnection();
            }
            screen.State = (int)LobbyScreen.PickHostOrClient;
        }

        private void OnStartPress() {
            if(hasPort) {
                if(screen.State == (int)LobbyScreen.SetupHost) {
                    BalugaDebug.Log("Start host command");
                    bool success = netHelper.StartHost(port, password, false);

                    if(success) {
                        statusText.Value = "Waiting for connection...";
                        state = State.Connecting;
                        setInputInteractable.Value = false;
                    } else {
                        statusText.Value = "Error creating host. Port already in use?";
                    }
                } else if(screen.State == (int)LobbyScreen.SetupClient) {
                    state = State.Connecting;
                    netHelper.StartClient(address, port, password, false);
                    statusText.Value = "Waiting for connection...";
                    setInputInteractable.Value = false;
                    setAddressInteractable.Value = false;
                }
            }
            RefreshStartButtonInteractable();
        }

        private void OnConnectionChange(int connectionState) {
            if(state == State.Connecting) {
                switch((ConnectionHelper.State)netHelper.Connection.State) {
                case ConnectionHelper.State.Connected:
                    choices.MatchState = LobbyMatchState.FreshLobby;
                    if(screen.State == (int)LobbyScreen.SetupHost) {
                        screen.State = (int)LobbyScreen.HostGamePrefs;
                    } else if(screen.State == (int)LobbyScreen.SetupClient) {
                        screen.State = (int)LobbyScreen.ClientGamePrefs;
                    }
                    break;
                case ConnectionHelper.State.Stopped:
                case ConnectionHelper.State.None:
                    state = State.Choosing;
                    statusText.Value = "Server rejected us. Two players already or wrong password.";
                    setInputInteractable.Value = true;
                    setAddressInteractable.Value = true;
                    break;
                }
                RefreshStartButtonInteractable();
            }
        }
    }
}
