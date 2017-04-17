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

using UnityEngine;
using System.Collections;
using Baluga3.GameFlowLogic;
using System;

namespace ModdableChess.Logic.Lobby {
    public class LobbyBackButton {

        private Message onClick;
        private SubscribableBool buttonInteractable;
        private SubscribableBool readyStatus;
        private StateMachine screen;
        private ConnectionHelper connHelper;

        public LobbyBackButton(AutoController scene) {
            ComponentRegistry guiComps = scene.Components.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages);

            onClick = new Message();
            guiComps.Register((int)GUICompKeys.BackButtonClicked, onClick);
            onClick.Subscribe(new SimpleListener(OnButtonClicked));

            buttonInteractable = new SubscribableBool(false);
            guiComps.Register((int)GUICompKeys.BackButtonInteractable, buttonInteractable);

            connHelper = scene.Game.Components.Get<ConnectionHelper>((int)ComponentKeys.ConnectionHelper);
            scene.ActivatableList.Add(new ListenerJanitor<IListener<int>>(
                connHelper.Connection.EnterStateMessenger,
                new SimpleListener<int>((s) => CheckDisconnect())));

            readyStatus = scene.Components.GetOrRegister<SubscribableBool>((int)ComponentKeys.LobbyReadyStatus, SubscribableBool.Create);
            readyStatus.Subscribe(new SimpleListener<bool>(OnReadyStatusChange));

            screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>(OnEnterScreen));
        }

        private void CheckDisconnect() {
            switch((LobbyScreen)screen.State) {
            case LobbyScreen.ClientGamePrefs:
            case LobbyScreen.HostGamePrefs:
                if(connHelper.Connection.State != (int)ConnectionHelper.State.Connected) {
                    connHelper.StopConnection();
                    DoBack();
                }
                break;
            }
        }

        private void OnEnterScreen(int screen) {
            LobbyScreen screenState = (LobbyScreen)screen;
            if(screenState == LobbyScreen.ClientGamePrefs || screenState == LobbyScreen.HostGamePrefs) {
                RefreshButtonInteractable();
            }
            CheckDisconnect();
        }

        private void OnReadyStatusChange(bool ready) {
            RefreshButtonInteractable();
        }

        private void RefreshButtonInteractable() {
            buttonInteractable.Value = !readyStatus.Value;
        }

        private void OnButtonClicked() {
            switch((LobbyScreen)screen.State) {
            case LobbyScreen.ClientGamePrefs:
            case LobbyScreen.HostGamePrefs:
                connHelper.StopConnection();
                break;
            }
            DoBack();
        }

        private void DoBack() {
            RejoinFile.Clear();
            screen.State = (int)LobbyScreen.PickHostOrClient;
        }
    }
}
