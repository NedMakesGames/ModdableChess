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

using Baluga3.Core;
using Baluga3.GameFlowLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace ModdableChess.Logic.Server {
    public class ServerController : IServerCallbacks, ITicking {

        private ServerInformation info;
        private IServerCommandable commandable;
        private Queue<int> disconnectNextFrame;

        public ServerController(ModdableChessGame game) {
            game.AddTicking(this);

            info = game.Components.GetOrRegister<ServerInformation>((int)ComponentKeys.ServerInformation, ServerInformation.Create);
            ClearServerInfo();
            game.Components.Register((int)ComponentKeys.Server_Callbacks, this);
            game.Components.GetOrRegister<Query<bool, StartHostCommand>>((int)ComponentKeys.StartHostCommand, Query<bool, StartHostCommand>.Create)
                .Handler = StartHost;
            game.Components.GetOrRegister<Command>((int)ComponentKeys.StopHostCommand, Command.Create)
                .Handler = StopHost;

            info.Connection.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) =>
                UnityEngine.Debug.LogError(string.Format("{0}: Server connection change {1}", game.TickCount, (ServerConnectionState)s))));
            disconnectNextFrame = new Queue<int>();

            //new ReconnectionServerManager(game);
            //new ServerReadyCheckManager(game);
            //new ServerLoadingErrorManager(game);
        }

        public void RegisterCommandable(IServerCommandable commandable) {
            BalugaDebug.Assert(this.commandable == null);
            this.commandable = commandable;
        }

        private bool StartHost(StartHostCommand cmd) {
            info.Port = cmd.port;
            info.Password = cmd.password;
            info.WantsInProgressGame = cmd.wantsInProgressGame;

            return commandable.StartServer(cmd.port);
        }

        private void StopHost() {
            if(info.Connection.State != (int)ServerConnectionState.Stopped) {
                commandable.ForceStopServer();
            }
            ClearServerInfo();
        }

        public void OnStart() {
            info.Connection.State = (int)ServerConnectionState.Open;
        }

        public void OnStop() {
            if(info.Connection.State != (int)ServerConnectionState.None) {
                info.Connection.State = (int)ServerConnectionState.Stopped;
            }
        }

        public void OnDisconnect(int connectionID) {
            if(connectionID == info.ClientConnectionID) {
                info.ClientConnectionID = -1;
                info.Connection.State = (int)ServerConnectionState.Open;
            }
        }

        public void OnError(int connectionID, UnityEngine.Networking.NetworkError errorCode) {
            BalugaDebug.Log("Server connection error " + errorCode);
        }

        private void ClearServerInfo() {
            info.ClientConnectionID = -1;
            info.Port = -1;
            info.Password = "";
            info.WantsInProgressGame = false;
            info.Connection.State = (int)ServerConnectionState.None;
        }

        public void OnValidationRequest(int connectionID, ValidationInfo val) {
            ValidationResponse response = null;

            // if connectionID is zero, just accept, it's the host local client
            if(connectionID == 0) {
                response = new ValidationResponse() {
                    isHost = true,
                    accepted = true,
                };
            } else {
                if(info.ClientConnectionID >= 0) {
                    BalugaDebug.Log(string.Format("Third player tried to connect! {0} \"{1}\"", connectionID, val.password));
                    response = new ValidationResponse() {
                        accepted = false,
                    };
                } else {
                    BalugaDebug.Log(string.Format("New try connect, mine: \"{0}\" {3}, theirs \"{1}\" {2}",
                        info.Password, val.password, val.inProgressGame, info.WantsInProgressGame));
                    bool validPassword = connectionID == 0 || string.IsNullOrEmpty(info.Password) || info.Password == val.password;
                    bool validGameState = connectionID == 0 || info.WantsInProgressGame == val.inProgressGame;

                    if(validPassword && validGameState) {
                        info.ClientConnectionID = connectionID;
                        BalugaDebug.Log(string.Format("Newly connected client {0} \"{1}\" {2}", connectionID, val.password, val.inProgressGame));
                        info.Connection.State = (int)ServerConnectionState.Connected;
                        response = new ValidationResponse() {
                            isHost = false,
                            accepted = true,
                        };
                    } else {
                        BalugaDebug.Log(string.Format("Player tried to connect with bad password/state! {0} \"{1}\" {2}",
                            connectionID, val.password, val.inProgressGame));
                        response = new ValidationResponse() {
                            accepted = false,
                        };
                    }
                }
            }

            if(response != null) {
                commandable.SendValidationResponse(connectionID, response);
                if(!response.accepted) {
                    disconnectNextFrame.Enqueue(connectionID);
                }
            }
        }

        public void Tick(float deltaTime) {
            while(disconnectNextFrame.Count > 0) {
                commandable.ForceDisconnectConnection(disconnectNextFrame.Dequeue());
            }
        }
    }
}
