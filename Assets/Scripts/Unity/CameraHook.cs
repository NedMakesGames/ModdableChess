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

namespace ModdableChess.Unity {
    public class CameraHook : MonoBehaviour {

        private SubscribableQuaternion rotation;
        private SubscribableFloat zoom;
        private Transform child;

        private void Start() {
            child = GetComponentInChildren<Camera>().transform;

            rotation = GameLink.Game.SceneComponents.GetOrRegister<SubscribableQuaternion>
                ((int)ComponentKeys.GameCameraRotation, SubscribableQuaternion.Create);
            rotation.Subscribe(new SimpleListener<Quaternion>((q) => RefreshRotation()));
            RefreshRotation();

            zoom = GameLink.Game.SceneComponents.GetOrRegister<SubscribableFloat>
                ((int)ComponentKeys.GameCameraZoom, SubscribableFloat.Create);
            zoom.Subscribe(new SimpleListener<float>((z) => RefreshZoom()));
            RefreshZoom();
        }

        private void RefreshRotation() {
            transform.rotation = rotation.Value;
        }

        private void RefreshZoom() {
            child.localPosition = new Vector3(0, 0, -zoom.Value);
        }
    }
}
