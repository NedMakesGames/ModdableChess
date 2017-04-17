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
    [System.Obsolete]
    public class SetupClientScreenManager {

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
        private Message<string, int> startClientCommand;
        private Message stopClientCommand;
        private SubscribableObject<string> statusText;

        public SetupClientScreenManager(AutoController scene) {
            ComponentRegistry guiComps = scene.Components.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages);
            guiComps.Register((int)GUICompKeys.BtnClientCancelPress, new Message());
            guiComps.Register((int)GUICompKeys.BtnStartClientPress, new Message());
            guiComps.Register((int)GUICompKeys.ClientPortChange, new Message<string>());
            guiComps.Register((int)GUICompKeys.ClientAddressChange, new Message<string>());

            startBtnEnabled = new SubscribableBool(hasPort);
            guiComps.Register((int)GUICompKeys.BtnClientStartEnabledCommand, startBtnEnabled);

            setInputInteractable = new SubscribableBool(state != State.Connecting);
            guiComps.Register((int)GUICompKeys.ClientStartInputInteractable, setInputInteractable);

            setPortField = new SubscribableObject<string>(port.ToString());
            guiComps.Register((int)GUICompKeys.ClientPortSetCommand, setPortField);

            setAddressField = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.ClientAddressTextField, setAddressField);

            statusText = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.ClientConnectionStatusLabel, statusText);

            guiComps.Get<Message<string>>((int)GUICompKeys.ClientPortChange).Subscribe(new SimpleListener<string>(OnPortChange));
            guiComps.Get<Message<string>>((int)GUICompKeys.ClientAddressChange).Subscribe(new SimpleListener<string>(OnAddressChange));
            guiComps.Get<Message>((int)GUICompKeys.BtnClientCancelPress).Subscribe(new SimpleListener(OnCancelPress));
            guiComps.Get<Message>((int)GUICompKeys.BtnStartClientPress).Subscribe(new SimpleListener(OnStartPress));

            screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            startClientCommand = scene.Game.Components.Get<Message<string, int>>((int)ComponentKeys.StartClientCommand);
            stopClientCommand = scene.Game.Components.Get<Message>((int)ComponentKeys.StopClientCommand);

            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => { 
                if(s == (int)LobbyScreen.SetupClient) {
                    state = State.Choosing;
                    SetDefaults();
                } else {
                    state = State.Hidden;
                }
            }));

            scene.ActivatableList.Add(new ListenerJanitor<IListener<int>>(
                scene.Game.Components.Get<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation).Connection.EnterStateMessenger,
                new SimpleListener<int>(OnConnectionEstablished)));
        }

        private void SetDefaults() {
            hasPort = true;
            port = 1000;
            address = "localhost";
            startBtnEnabled.Value = true;
            setPortField.Value = port.ToString();
            setAddressField.Value = address;
            statusText.Value = "";
            setInputInteractable.Value = true;
        }

        private void OnPortChange(string input) {
            hasPort = !string.IsNullOrEmpty(input) && int.TryParse(input, out port);
            startBtnEnabled.Value = hasPort;
            setPortField.Value = port.ToString();

            BalugaDebug.Log("Port " + port);
        }

        private void OnAddressChange(string input) {
            address = input;
            setAddressField.Value = address;
            BalugaDebug.Log("Address " + address);
        }

        private void OnCancelPress() {
            if(state == State.Connecting) {
                stopClientCommand.Send();
            }
            screen.State = (int)LobbyScreen.PickHostOrClient;
        }

        private void OnStartPress() {
            BalugaDebug.Log("Start pressed");
            if(hasPort) {
                state = State.Connecting;
                startClientCommand.Send(address, port);
                statusText.Value = "Waiting for connection...";
                setInputInteractable.Value = false;
                startBtnEnabled.Value = false;
            }
        }

        private void OnConnectionEstablished(int connectionState) {
            if(screen.State == (int)LobbyScreen.SetupClient) {
                if(connectionState == (int)ClientConnectionState.Connected) {
                    screen.State = (int)LobbyScreen.ClientGamePrefs;
                //} else if(connectionState == (int)ClientConnectionState.Rejected) {
                //    statusText.Value = "Server rejected us. Two players already.";
                //    setInputInteractable.Value = true;
                //    startBtnEnabled.Value = true;
                //    stopClientCommand.Send();
                }
            }
        }
    }
}
