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
using ModdableChess.Logic.Lobby;
using UnityEngine.UI;
using Baluga3.GameFlowLogic;
using ModdableChess.Logic;

namespace ModdableChess.Unity {
    public class LobbyToggleGroup : MonoBehaviour {

        [SerializeField]
        private ToggleGroup group;
        [SerializeField]
        private GUICompKeys allowAllOffKey;

        private SubscribableBool allowAllOff;

        private void Start() {
            if(group == null) {
                group = GetComponent<ToggleGroup>();
            }
            if(allowAllOffKey != 0) {
                allowAllOff = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<SubscribableBool>((int)allowAllOffKey);
                allowAllOff.Subscribe(new SimpleListener<bool>((e) => RefreshAllOff()));
                RefreshAllOff();
            }
        }

        private void RefreshAllOff() {
            group.allowSwitchOff = allowAllOff.Value;
        }
    }
}
