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
    public class LobbyLabelControl : MonoBehaviour {

        [SerializeField]
        public Text text;
        [SerializeField]
        public GUICompKeys setTextKey;

        private SubscribableObject<string> displayText;

        public void Start() {
            if(text == null) {
                text = GetComponent<Text>();
            }
            if(setTextKey != 0) {
                displayText = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<SubscribableObject<string>>((int)setTextKey);
                displayText.Subscribe(new SimpleListener<string>((e) => RefreshDisplayText()));
                RefreshDisplayText();
            }
        }

        private void RefreshDisplayText() {
            text.text = displayText.Value;
        }
    }
}
