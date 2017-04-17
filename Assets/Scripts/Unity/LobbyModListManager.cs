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
using ModdableChess.Logic;
using Baluga3.GameFlowLogic;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ModdableChess.Unity {
    public class LobbyModListManager : MonoBehaviour {

        [SerializeField]
        private GameObject modBtnPrefab;
        [SerializeField]
        private Vector2 buttonOffset;
        [SerializeField]
        private float buttonYIncrement;

        private ModList modList;
        private List<LobbyModButton> buttons;

        private void Start() {
            buttons = new List<LobbyModButton>();
            modList = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<ModList>((int)GUICompKeys.ModList);
            modList.OnListUpdate.Subscribe(new SimpleListener(MakeButtons));
            MakeButtons();
        }

        private void MakeButtons() {
            foreach(var btn in buttons) {
                GameObject.Destroy(btn.gameObject);
            }
            buttons.Clear();

            for(int i = 0; i < modList.Buttons.Count; i++) {
                ModListButton btnInfo = modList.Buttons[i];
                LobbyModButton btn = Instantiate<GameObject>(modBtnPrefab, transform).GetComponent<LobbyModButton>();
                btn.transform.localPosition = buttonOffset + new Vector2(0, buttonYIncrement * btnInfo.Order);
                btn.manager = this;
                btn.nameText.text = btnInfo.NameString;
                btn.statusText.text = btnInfo.StatusString;
                btn.listIndex = i;
                btn.button.interactable = btnInfo.Choosable;
                buttons.Add(btn);
            }
        }

        public void OnButtonClick(int listIndex) {
            modList.OnButtonClick.Send(listIndex);
        }
    }
}
