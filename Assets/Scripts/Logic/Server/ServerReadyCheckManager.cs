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
using ModdableChess.Logic.Lobby;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic.Server {
    public class ServerReadyCheckManager : ITicking {

        public class ConnectionData {
            public int connectionID;
            public float timer;
            public object extraData;
        }

        private ServerInformation serverInfo;
        private List<ConnectionData> connections;

        private float timeOutPeriod;
        private bool enforceExtraData;

        public float TimeOutPeriod {
            get {
                return timeOutPeriod;
            }

            set {
                this.timeOutPeriod = value;
            }
        }

        public bool EnforceExtraData {
            get {
                return enforceExtraData;
            }

            set {
                enforceExtraData = value;
            }
        }

        public ServerReadyCheckManager(ModdableChessGame game, float timeOutPeriod) {
            game.AddTicking(this);
            this.timeOutPeriod = timeOutPeriod;

            connections = new List<ConnectionData>();
            serverInfo = game.Components.Get<ServerInformation>((int)ComponentKeys.ServerInformation);

            game.Components.Get<ServerInformation>((int)ComponentKeys.ServerInformation).Connection.EnterStateMessenger.Subscribe(
                new SimpleListener<int>((s) => CheckServerConnection()));
        }

        public T GetExtraData<T>(int connectionID) where T : class {
            return GetData(connectionID).extraData as T;
        }

        public T GetHostData<T>() where T : class {
            return GetExtraData<T>(0);
        }

        public T GetClientData<T>() where T : class {
            return GetExtraData<T>(serverInfo.ClientConnectionID);
        }

        private ConnectionData GetData(int connectionID) {
            foreach(var c in connections) {
                if(c.connectionID == connectionID) {
                    return c;
                }
            }
            return null;
        }

        private void RemoveData(int connectionID) {
            for(int i = 0; i < connections.Count; i++) {
                if(connections[i].connectionID == connectionID) {
                    connections.RemoveAt(i);
                }
            }
        }

        private void CheckServerConnection() {
            if(serverInfo.Connection.State != (int)ServerConnectionState.Connected) {
                Clear();
            }
        }

        public bool PlayerNotifiedReady(int connectionID, bool isReady, object extraData) {
            if(isReady) {
                bool allReady = CheckNowReady(connectionID, extraData != null);
                ConnectionData connectionData = GetData(connectionID);
                if(connectionData != null) {
                    connectionData.timer = timeOutPeriod;
                    if(extraData != null) {
                        connectionData.extraData = extraData;
                    }
                }
                return allReady;
            } else {
                RemoveData(connectionID);
            }
            return false;
        }

        public bool PlayerNotifiedReady(int connectionID, bool isReady) {
            return PlayerNotifiedReady(connectionID, isReady, null);
        }

        public void Clear() {
            connections.Clear();
        }

        private bool CheckNowReady(int newlyReady, bool hasExtraData) {
            //UnityEngine.Debug.Log("Player notified ready " + newlyReady);

            ConnectionData newConnection = GetData(newlyReady);
            if(newConnection != null) {
                //UnityEngine.Debug.LogError("Player notified ready to exit lobby twice: " + newlyReady);
                return false;
            }
            if(newConnection == null && enforceExtraData && !hasExtraData) {
                return false;
            }
            if(connections.Count >= 2) {
                UnityEngine.Debug.LogError("More than two players notified ready: " + newlyReady);
                //return false;
            }

            newConnection = new ConnectionData() {
                connectionID = newlyReady,
                timer = 0,
                extraData = null,
            };
            connections.Add(newConnection);
            if(connections.Count < 2) {
                return false;
            }

            bool hostReady = false;
            bool clientReady = false;
            foreach(var connectionData in connections) {
                if(serverInfo.IsHost(connectionData.connectionID)) {
                    hostReady = true;
                } else if(serverInfo.ClientConnectionID == connectionData.connectionID) {
                    clientReady = true;
                }
            }

            return hostReady && clientReady;
        }

        public void Tick(float deltaTime) {
            if(connections.Count > 0) {
                for(int i = connections.Count - 1; i >= 0; i--) {
                    ConnectionData connection = connections[i];
                    connection.timer -= deltaTime;
                    if(connection.timer <= 0) {
                        connections.RemoveAt(i);
                    }
                }
            }
        }
    }
}
