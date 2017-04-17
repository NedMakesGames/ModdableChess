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
using UnityEngine.UI;
using ModdableChess.Logic;
using ModdableChess.Logic.Lobby;
using Baluga3.GameFlowLogic;

namespace ModdableChess.Unity {
    public class LobbyButtonClickMessage : MonoBehaviour {

        [SerializeField]
        public Button button;
        [SerializeField]
        public GUICompKeys messageKey;
        [SerializeField]
        public GUICompKeys enableKey;

        private SubscribableBool enabledState;

        public void Start() {
            if(button == null) {
                button = GetComponent<Button>();
            }
            if(messageKey != 0) {
                Message messenger = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<Message>((int)messageKey);

                button.onClick.AddListener(messenger.Send);
            }
            if(enableKey != 0) {
                enabledState = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<SubscribableBool>((int)enableKey);
                enabledState.Subscribe(new SimpleListener<bool>((e) => RefreshEnabled()));
                RefreshEnabled();
            }
        }

        private void RefreshEnabled() {
            //Debug.Log("Interactable " + enabledState.Value);
            button.interactable = enabledState.Value;
        }
    }
}
