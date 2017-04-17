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
    public class OtherPlayerReadyTextController {

        private SubscribableObject<string> label;
        private bool isModSelected;

        public OtherPlayerReadyTextController(AutoController scene) {

            ComponentRegistry guiComps = scene.Components.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages);

            label = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.OtherPlayerReadyLabel, label);

            scene.ActivatableList.Add(new ListenerJanitor<IListener<bool>>(
                scene.Game.Components.Get<IMessage<bool>>((int)ComponentKeys.LobbyOtherPlayerReadyReceived),
                new SimpleListener<bool>(OnNetworkReadyNoticeReceived)));

            StateMachine screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => {
                if(s == (int)LobbyScreen.HostGamePrefs || s == (int)LobbyScreen.ClientGamePrefs) {
                    ScreenEnter();
                }
            }));
        }

        private void ScreenEnter() {
            label.Value = "Other player not ready";
        }

        private void OnNetworkReadyNoticeReceived(bool ready) {
            if(ready) {
                label.Value = "Other player ready!";
            } else {
                label.Value = "Other player not ready";
            }
        }
    }
}
