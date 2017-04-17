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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModdableChess.Unity {
    public class PlayUIButtonHook : MonoBehaviour {
        [SerializeField]
        public ComponentKeys key;

        private Button unityButton;
        private Text text;
        private Logic.PlayUI.Button logicButton;

        public void Start() {
            unityButton = GetComponent<Button>();
            text = unityButton.GetComponentInChildren<Text>();

            logicButton = GameLink.Game.SceneComponents.Get<Logic.PlayUI.Button>((int)key);

            logicButton.Interactable.Subscribe(new SimpleListener<bool>((e) => RefreshInteractable()));
            RefreshInteractable();

            logicButton.Visibility.Subscribe(new SimpleListener<bool>((v) => RefreshVisible()));
            RefreshVisible();

            logicButton.Text.Subscribe(new SimpleListener<string>((s) => RefreshText()));
            RefreshText();

            unityButton.onClick.AddListener(logicButton.PressMessage.Send);
        }

        private void RefreshInteractable() {
            unityButton.interactable = logicButton.Interactable.Value;
        }

        private void RefreshVisible() {
            unityButton.gameObject.SetActive(logicButton.Visibility.Value);
        }

        private void RefreshText() {
            text.text = logicButton.Text.Value;
        }
    }
}
