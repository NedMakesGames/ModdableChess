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
using ModdableChess.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic.Server {
    public class LoadingManager {

        private ServerInformation info;
        private ServerReadyCheckManager gameStateRefresh;
        private ServerReadyCheckManager readyToPlay;

        public LoadingManager(ModdableChessGame game) {
            info = game.Components.GetOrRegister<ServerInformation>((int)ComponentKeys.ServerInformation, ServerInformation.Create);
            //info.Connection.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => CheckServerConnection()));
            readyToPlay = new ServerReadyCheckManager(game, 10);
            gameStateRefresh = new ServerReadyCheckManager(game, 10);
            gameStateRefresh.EnforceExtraData = true;

            NetworkTracker.ServerMsgCallbacks.loadingGameStateReceived = OnReceiveGameState;
            NetworkTracker.ServerMsgCallbacks.loadingGameStateRefresh = OnGameStateRefresh;
            NetworkTracker.ServerMsgCallbacks.loadingErrorReceived = OnReceiveError;
            NetworkTracker.ServerMsgCallbacks.loadingReadyReceived = OnReceiveReady;
        }

        //private void CheckServerConnection() {
        //    if(info.Connection.State != (int)ServerConnectionState.Connected) {
        //        gameStates.Clear();
        //    }
        //}

        private void OnReceiveGameState(int connectionID, LoadingGameStateMessage msg) {
            if(info.IsConnected(connectionID)) {
                if(gameStateRefresh.PlayerNotifiedReady(connectionID, true, msg)) {
                    LoadingGameStateMessage hostGS = gameStateRefresh.GetHostData<LoadingGameStateMessage>();
                    LoadingGameStateMessage clientGS = gameStateRefresh.GetClientData<LoadingGameStateMessage>();
                    LoadingGameStateMessage sendBackGS;
                    if(hostGS.hasState) {
                        sendBackGS = hostGS;
                    } else {
                        sendBackGS = clientGS;
                    }
                    gameStateRefresh.Clear();
                    NetworkTracker.ServerSendLoadingGameState(sendBackGS);
                }
            }
        }

        private void OnGameStateRefresh(int connectionID, NetworkPlayerReadyNotice msg) {
            if(info.IsConnected(connectionID)) {
                gameStateRefresh.PlayerNotifiedReady(connectionID, msg.ready);
            }
        }

        private void OnReceiveError(int connectionID) {
            if(info.IsConnected(connectionID)) {
                NetworkTracker.ServerSendLoadingError();
            }
        }

        private void OnReceiveReady(int connectionID, NetworkPlayerReadyNotice ready) {
            if(info.IsConnected(connectionID)) {
                if(readyToPlay.PlayerNotifiedReady(connectionID, ready.ready)) {
                    readyToPlay.Clear();
                    NetworkTracker.ServerSendLoadingExit();
                }
            }
        }
    }
}
