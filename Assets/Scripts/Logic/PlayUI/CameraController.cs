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
using Baluga3.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModdableChess.Logic.PlayUI {

    public class CameraController : ITicking {
        const float DEFAULT_X_ROTATION = 30;
        const float MOUSE_SENSE = 0.5f;
        const float ZOOM_INCREMENT = 0.5f;
        const float ZOOM_MIN = 10;
        const float ZOOM_MAX = 50;
        const float ZOOM_DEFAULT = 25;

        private SubscribableQuaternion rotation;
        private SubscribableBool trackMouseBtn;
        private SubscribableVector2 mouseChange;
        private SubscribableFloat zoom;
        private SubscribableInt mouseWheel;
        private Board board;

        public CameraController(ChessScene scene) {
            scene.ActivatableList.Add(new TickingJanitor(scene.Game, this));

            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>(ReceivedNewBoard));

            rotation = scene.Components.GetOrRegister<SubscribableQuaternion>((int)ComponentKeys.GameCameraRotation, SubscribableQuaternion.Create);
            rotation.Value = Quaternion.identity;
            zoom = scene.Components.GetOrRegister<SubscribableFloat>((int)ComponentKeys.GameCameraZoom, SubscribableFloat.Create);
            zoom.Value = ZOOM_DEFAULT;

            trackMouseBtn = scene.Components.GetOrRegister<SubscribableBool>((int)ComponentKeys.CameraTrackMouseBtn, SubscribableBool.Create);
            mouseChange = scene.Components.GetOrRegister<SubscribableVector2>((int)ComponentKeys.MouseChange, SubscribableVector2.Create);
            mouseWheel = scene.Components.GetOrRegister<SubscribableInt>((int)ComponentKeys.ScrollWheel, SubscribableInt.Create);
            scene.Components.GetOrRegister<SubscribableBool>((int)ComponentKeys.CameraRecenterBtn, SubscribableBool.Create)
                .Subscribe(new ReleaseListener(RecenterCamera));
        }

        private void ReceivedNewBoard(Board newBoard) {
            bool firstBoard = board == null;
            this.board = newBoard;
            if(firstBoard) {
                RecenterCamera();
            }
        }

        private void RecenterCamera() {
            float yRotation = board.LocalPlayerOrder == PlayerTurnOrder.First ? 0 : 180;
            rotation.Value = Quaternion.Euler(DEFAULT_X_ROTATION, yRotation, 0);
            zoom.Value = ZOOM_DEFAULT;
        }

        public void Tick(float deltaTime) {
            Cursor.visible = !trackMouseBtn.Value;
            if(trackMouseBtn.Value) {
                Vector2 rotateAngle = mouseChange.Value * MOUSE_SENSE;
                Quaternion startRotation = rotation.Value;
                rotation.Value =
                    Quaternion.AngleAxis(rotateAngle.x, startRotation * Vector3.up) *
                    Quaternion.AngleAxis(rotateAngle.y, startRotation * Vector3.right) *
                    startRotation;
                zoom.Value = Mathf.Clamp(zoom.Value + -mouseWheel.Value * ZOOM_INCREMENT, ZOOM_MIN, ZOOM_MAX);
            }
        }
    }
}
