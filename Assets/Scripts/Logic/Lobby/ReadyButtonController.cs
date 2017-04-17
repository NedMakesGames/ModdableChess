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
    public class ReadyButtonController {

        private SubscribableBool buttonEnabled;
        private SubscribableObject<string> buttonLabel;
        private bool isHost;
        private SubscribableObject<string> selectedMod;
        private SubscribableBool readyStatus;

        public ReadyButtonController(AutoController scene) {
            ComponentRegistry guiComps = scene.Components.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages);

            buttonEnabled = new SubscribableBool(true);
            guiComps.Register((int)GUICompKeys.ReadyBtnEnabled, buttonEnabled);
            buttonLabel = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.ReadyBtnLabel, buttonLabel);
            Message buttonClick = new Message();
            guiComps.Register((int)GUICompKeys.ReadyBtnClicked, buttonClick);
            buttonClick.Subscribe(new SimpleListener(OnButtonClick));

            selectedMod = scene.Components.GetOrRegister<SubscribableObject<string>>
                ((int)ComponentKeys.LobbyModSelected, SubscribableObject<string>.Create);
            selectedMod.Subscribe(new SimpleListener<string>((m) => {
                RefreshButton();
            }));
            readyStatus = scene.Components.GetOrRegister<SubscribableBool>((int)ComponentKeys.LobbyReadyStatus, SubscribableBool.Create);
            readyStatus.Subscribe(new SimpleListener<bool>((v) => RefreshButton()));

            StateMachine screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => {
                isHost = s == (int)LobbyScreen.HostGamePrefs;
                if(s == (int)LobbyScreen.HostGamePrefs || s == (int)LobbyScreen.ClientGamePrefs) {
                    ScreenEnter();
                }
            }));
        }

        private void OnButtonClick() {
            readyStatus.Value = !readyStatus.Value;
            //RefreshButton();
        }

        private void ScreenEnter() {
            RefreshButton();
        }

        private void RefreshButton() {
            if(isHost) {
                if(!string.IsNullOrEmpty(selectedMod.Value)) {
                    buttonEnabled.Value = true;
                    SetButtonReadyText();
                } else {
                    buttonEnabled.Value = false;
                    buttonLabel.Value = "Select a playable mod!";
                }
            } else {
                buttonEnabled.Value = !isHost;
                SetButtonReadyText();
            }
        }

        private void SetButtonReadyText() {
            if(readyStatus.Value) {
                buttonLabel.Value = "Ready! Cancel?";
            } else {
                buttonLabel.Value = "Ready?";
            }
        }

        
    }
}
