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
using Baluga3.Core;

namespace ModdableChess.Unity {
    public class PlayerSelectionRaycaster : MonoBehaviour {

        [SerializeField]
        private float maxDistance;
        [SerializeField]
        private LayerMask mask;

        private Camera mainCamera;
        private PlayerSelectionEvent selEvent;
        private int boardLayer;

        void Start() {
            selEvent = GameLink.Game.SceneComponents.Get<PlayerSelectionEvent>((int)ComponentKeys.PlayerSelectionChange);
            mainCamera = Camera.main;
            boardLayer = LayerMask.NameToLayer("Board");
        }

        private void Update() {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, maxDistance, mask)) {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
                if(hit.collider.gameObject.layer == boardLayer) {
                    selEvent.SetSelectionSquare(FigureBoardSquare(hit.point, (BoxCollider)hit.collider));
                } else {
                    PieceModel model = hit.collider.GetComponentInParent<PieceModel>();
                    if(model != null) {
                        selEvent.SetSelectionPiece(model.PieceID);
                    } else {
                        selEvent.SetSelectionNothing();
                    }
                }
            } else {
                Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.cyan);
                selEvent.SetSelectionNothing();
            }
        }

        private IntVector2 FigureBoardSquare(Vector3 worldPoint, BoxCollider collider) {
            Vector3 localPoint = collider.transform.InverseTransformPoint(worldPoint);
            Vector3 scaledPoint = new Vector3(localPoint.x / collider.size.x + 0.5f, 0, localPoint.z / collider.size.z + 0.5f);
            IntVector2 p = new IntVector2(
                Mathf.Clamp(Mathf.FloorToInt(scaledPoint.x * 8), 0, 7),
                Mathf.Clamp(Mathf.FloorToInt(scaledPoint.z * 8), 0, 7));
            //Debug.Log(string.Format("World {0}, local {1}, scaled {2}, square {3}", worldPoint, localPoint, scaledPoint, p));
            return p;
        }
    }
}
