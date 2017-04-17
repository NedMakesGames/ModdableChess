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
using ModdableChess.Logic;
using ModdableChess.Logic.PlayUI;
using UnityEngine.UI;
using Baluga3.GameFlowLogic;

namespace ModdableChess.Unity {
    public class PlayUITextHook : MonoBehaviour {
        [SerializeField]
        public ComponentKeys key;

        private Text text;
        private Logic.PlayUI.TextDisplay logicText;

        public void Start() {
            text = GetComponent<Text>();

            logicText = GameLink.Game.SceneComponents.Get<Logic.PlayUI.TextDisplay>((int)key);

            logicText.Visibility.Subscribe(new SimpleListener<bool>((v) => RefreshVisible()));
            RefreshVisible();

            logicText.Text.Subscribe(new SimpleListener<string>((s) => RefreshText()));
            RefreshText();
        }

        private void RefreshVisible() {
            text.gameObject.SetActive(logicText.Visibility.Value);
        }

        private void RefreshText() {
            text.text = logicText.Text.Value;
        }
    }
}
