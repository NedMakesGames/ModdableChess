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
using ModdableChess.Unity;
using UnityEngine;

namespace ModdableChess.Unity {
    public class BoardModelCreator : MonoBehaviour {

        private BoardModel boardModel;
        private GameDatabase db;
        private int boardLayer;

        private void Start() {
            db = GameLink.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            boardLayer = LayerMask.NameToLayer("Board");
            CreateBoard();
        }

        private void CreateBoard() {
            GameObject gobj = (GameObject)Instantiate(db.ModelPrefabs[db.BoardModelIndex]);
            gobj.layer = boardLayer;
            foreach(var child in gobj.GetComponentsInChildren<Transform>()) {
                child.gameObject.layer = boardLayer;
            }
            boardModel = gobj.AddComponent<BoardModel>();
        }
    }
}
