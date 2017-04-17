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
    public class LobbyInputFieldMessages : MonoBehaviour {

        [SerializeField]
        public InputField field;
        [SerializeField]
        public GUICompKeys messageKey;
        [SerializeField]
        public GUICompKeys setKey;
        [SerializeField]
        public GUICompKeys interactableKey;

        private SubscribableObject<string> fieldText;
        private SubscribableBool interactable;

        public void Start() {
            if(field == null) {
                field = GetComponent<InputField>();
            }
            if(messageKey != 0) {
                Message<string> valueChange = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<Message<string>>((int)messageKey);

                field.onValueChanged.AddListener(valueChange.Send);
            }
            if(setKey != 0) {
                fieldText = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<SubscribableObject<string>>((int)setKey);
                fieldText.Subscribe(new SimpleListener<string>((t) => RefreshText()));
                RefreshText();
            }
            if(interactableKey != 0) {
                interactable = GameLink.Game.SceneComponents.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages).Get<SubscribableBool>((int)interactableKey);
                interactable.Subscribe(new SimpleListener<bool>((t) => RefreshInteractable()));
                RefreshInteractable();
            }
        }

        private void RefreshText() {
            //Debug.Log("Field set: " + fieldText.Value);
            field.text = fieldText.Value;
        }

        private void RefreshInteractable() {
            field.interactable = interactable.Value;
        }
    }
}
