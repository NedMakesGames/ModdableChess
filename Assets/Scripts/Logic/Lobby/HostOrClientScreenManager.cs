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
    public class HostOrClientScreenManager {

        private StateMachine screen;
        private SubscribableBool rejoinButtonVisible;

        public HostOrClientScreenManager(AutoController scene) {
            ComponentRegistry guiComps = scene.Components.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages);
            guiComps.Register((int)GUICompKeys.ChooseToHostBtnPress, new Message());
            guiComps.Register((int)GUICompKeys.ChooseToJoinBtnPress, new Message());
            guiComps.Register((int)GUICompKeys.ChooseToRejoinBtnPress, new Message());

            rejoinButtonVisible = new SubscribableBool(false);
            guiComps.Register((int)GUICompKeys.ChooseToRejoinBtnVisible, rejoinButtonVisible);

            guiComps.Get<Message>((int)GUICompKeys.ChooseToHostBtnPress).Subscribe(new SimpleListener(OnHostBtn));
            guiComps.Get<Message>((int)GUICompKeys.ChooseToJoinBtnPress).Subscribe(new SimpleListener(OnJoinBtn));
            guiComps.Get<Message>((int)GUICompKeys.ChooseToRejoinBtnPress).Subscribe(new SimpleListener(OnRejoinBtn));

            screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnEnterScreen()));
        }

        private void OnEnterScreen() {
            if(screen.State == (int)LobbyScreen.PickHostOrClient) {
                rejoinButtonVisible.Value = RejoinFile.Exists();
            }
        }

        private void OnHostBtn() {
            screen.State = (int)LobbyScreen.SetupHost;
        }

        private void OnJoinBtn() {
            screen.State = (int)LobbyScreen.SetupClient;
        }

        private void OnRejoinBtn() {
            screen.State = (int)LobbyScreen.Rejoin;
        }
    }
}
