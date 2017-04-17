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
using System;
using ModdableChess.Logic;

namespace ModdableChess.Unity {
    [System.Obsolete]
    public class MatchUnityAndLogicScenes : MonoBehaviour {

        private ListenerJanitor<IListener<IGameController>> listener;

        public void Start() {
            listener = ListenerJanitor<IListener<IGameController>>.NewAndActivate(
                GameLink.Game.ControllerEnterMessage,
                new SimpleListener<IGameController>(OnSceneEnter));
        }

        private void OnSceneEnter(IGameController enterScene) {
            if(enterScene is ModLoadingScene) {
                UnityEngine.SceneManagement.SceneManager.LoadScene("ModLoadScene");
            } else if(enterScene is ModdableChess.Logic.Lobby.LobbyScene) {
                UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
            } else if(enterScene is ChessScene) {
                UnityEngine.SceneManagement.SceneManager.LoadScene("PlayScene");
            }
        }

        private void OnDestroy() {
            listener.Dispose();
        }
    }
}
