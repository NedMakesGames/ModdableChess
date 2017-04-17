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
using ModdableChess.Logic;
using ModdableChess.Logic.Lobby;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModdableChess.Unity {
    public class LobbyScreenSwitcher : MonoBehaviour {

        [SerializeField]
        private GameObject hostOrJoinScreen;
        [SerializeField]
        private GameObject hostStartScreen;
        [SerializeField]
        private GameObject preferenceScreen;
        [SerializeField]
        private GameObject rejoinScreen;

        private StateMachine screen;

        private void Start() {
            screen = GameLink.Game.SceneComponents.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => Refresh()));
            Refresh();
        }

        private void Refresh() {
            LobbyScreen current = (LobbyScreen)screen.State;
            hostOrJoinScreen.SetActive(current == LobbyScreen.PickHostOrClient);
            hostStartScreen.SetActive(current == LobbyScreen.SetupHost || current == LobbyScreen.SetupClient);
            preferenceScreen.SetActive(current == LobbyScreen.ClientGamePrefs || current == LobbyScreen.HostGamePrefs);
            rejoinScreen.SetActive(current == LobbyScreen.Rejoin);
        }
    }
}
