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
using UnityEngine.UI;

namespace ModdableChess.Unity {
    public class ToggleBoxMessages : MonoBehaviour {
        [SerializeField]
        public Toggle toggle;
        [SerializeField]
        public GUICompKeys changeKey;
        [SerializeField]
        public GUICompKeys setKey;
        [SerializeField]
        public GUICompKeys enableKey;

        private SubscribableBool enabledState;
        private SubscribableBool selectedState;

        public void Start() {
            if(toggle == null) {
                toggle = GetComponent<Toggle>();
            }
            if(changeKey != 0) {
                Message<bool> messenger = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<Message<bool>>((int)changeKey);
                toggle.onValueChanged.AddListener(messenger.Send);
            }
            if(enableKey != 0) {
                enabledState = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<SubscribableBool>((int)enableKey);
                enabledState.Subscribe(new SimpleListener<bool>((e) => RefreshEnabled()));
                RefreshEnabled();
            }
            if(setKey != 0) {
                selectedState = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<SubscribableBool>((int)setKey);
                selectedState.Subscribe(new SimpleListener<bool>((e) => RefreshSelected()));
                RefreshSelected();
            }
        }

        private void RefreshEnabled() {
            //Debug.Log("Interactable " + enabledState.Value);
            toggle.interactable = enabledState.Value;
        }

        private void RefreshSelected() {
            toggle.isOn = selectedState.Value;
        }
    }
}
