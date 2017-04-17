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

namespace ModdableChess.Logic.Client {
    public class ClientController : IClientCallbacks, ITicking {

        private enum ConnectWaitState {
            None, JustDisconnected
        }

        private LocalPlayerInformation info;
        private IClientCommandable commandable;
        private ConnectWaitState connectWait;

        public ClientController(Game game) {
            game.AddTicking(this);

            info = game.Components.GetOrRegister<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation, LocalPlayerInformation.Create);
            ClearLocalInfo();
            game.Components.Register((int)ComponentKeys.Client_Callbacks, this);
            game.Components.GetOrRegister<Query<bool, StartClientCommand>>((int)ComponentKeys.StartClientCommand, Query<bool, StartClientCommand>.Create)
                .Handler = StartClient;
            game.Components.GetOrRegister<Command>((int)ComponentKeys.StopClientCommand, Command.Create)
                .Handler = StopClient;

            info.Connection.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) =>
                UnityEngine.Debug.LogError(string.Format("{0}: Client connection change {1}", game.TickCount, (ClientConnectionState)s))));
        }

        private void ClearLocalInfo() {
            info.ServerAddress = null;
            info.ServerPort = 0;
            info.Password = null;
            info.InProgressGame = false;
            info.Connection.State = (int)ClientConnectionState.None;
        }

        private bool StartClient(StartClientCommand cmd) {
            info.ServerAddress = cmd.address;
            info.ServerPort = cmd.port;
            info.Password = cmd.password;
            info.InProgressGame = cmd.gameInProgress;

            if(connectWait == ConnectWaitState.JustDisconnected) {
                return false;
            } else {
                return commandable.StartClient(cmd.address, cmd.port);
            }
        }

        private void StopClient() {
            if(info.Connection.State != (int)ClientConnectionState.Stopped) {
                commandable.ForceStopClient();
            }
            ClearLocalInfo();
        }

        public void OnConnect() {
            info.Connection.State = (int)ClientConnectionState.Connected;
            commandable.SendValidationRequest(new ValidationInfo() {
                password = info.Password,
                inProgressGame = info.InProgressGame,
            });
        }

        public void OnDisconnect() {
            // Usually the client stops when disconnected, and OnStop is called before
        }

        public void OnError(NetworkError error) {
            BalugaDebug.Log("Client connection error " + error);
        }

        public void OnStart() {
            info.Connection.State = (int)ClientConnectionState.Seeking;
        }

        public void OnStop() {
            if(info.Connection.State != (int)ClientConnectionState.None) {
                connectWait = ConnectWaitState.JustDisconnected;
                info.Connection.State = (int)ClientConnectionState.Stopped;
            }
        }

        public void OnValidationResponse(ValidationResponse response) {
            if(response.accepted) {
                info.IsHost = response.isHost;
                info.Connection.State = (int)ClientConnectionState.Validated;
            } else {
                StopClient();
            }
        }

        public void RegisterCommandable(IClientCommandable commandable) {
            this.commandable = commandable;
        }

        public void Tick(float deltaTime) {
            if(connectWait == ConnectWaitState.JustDisconnected) {
                connectWait = ConnectWaitState.None;
            }
        }
    }
}
