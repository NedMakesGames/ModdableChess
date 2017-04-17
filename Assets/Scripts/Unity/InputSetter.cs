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
using Baluga3.GameFlowLogic;
using ModdableChess.Logic;

namespace ModdableChess.Unity {
    public class InputSetter : MonoBehaviour {

        private SubscribableBool selAcceptBtn;
        private SubscribableBool selBackBtn;
        private SubscribableBool selCycleBtn;
        private SubscribableInt scrollWheel;
        private SubscribableBool cameraMouseTrack;
        private SubscribableVector2 mouseChange;
        private SubscribableBool recenterCamera;
        private Vector2 lastMousePosition;

        private void Start() {
            selAcceptBtn = GameLink.Game.SceneComponents.Get<SubscribableBool>((int)ComponentKeys.PlayerSelectionAcceptBtn);
            selBackBtn = GameLink.Game.SceneComponents.Get<SubscribableBool>((int)ComponentKeys.PlayerSelectionCancelBtn);
            selCycleBtn = GameLink.Game.SceneComponents.Get<SubscribableBool>((int)ComponentKeys.PlayerSelectionCycleBtn);
            scrollWheel = GameLink.Game.SceneComponents.Get<SubscribableInt>((int)ComponentKeys.ScrollWheel);
            cameraMouseTrack = GameLink.Game.SceneComponents.Get<SubscribableBool>((int)ComponentKeys.CameraTrackMouseBtn);
            mouseChange = GameLink.Game.SceneComponents.Get<SubscribableVector2>((int)ComponentKeys.MouseChange);
            recenterCamera = GameLink.Game.SceneComponents.Get<SubscribableBool>((int)ComponentKeys.CameraRecenterBtn);
        }

        private void Update() {
            selAcceptBtn.Value = Input.GetKey(KeyCode.Mouse0);
            selBackBtn.Value = Input.GetKey(KeyCode.Escape);
            selCycleBtn.Value = Input.GetKey(KeyCode.Tab);
            float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
            if(Mathf.Approximately(0, mouseWheel)) {
                scrollWheel.Value = 0;
            } else if(mouseWheel > 0) {
                scrollWheel.Value = 1;
            } else {
                scrollWheel.Value = -1;
            }
            cameraMouseTrack.Value = Input.GetKey(KeyCode.Mouse1);
            recenterCamera.Value = Input.GetKey(KeyCode.Space);

            Vector2 mousePosition = Input.mousePosition;
            mouseChange.Value = mousePosition - lastMousePosition;
            lastMousePosition = mousePosition;
            
        }
    }
}
