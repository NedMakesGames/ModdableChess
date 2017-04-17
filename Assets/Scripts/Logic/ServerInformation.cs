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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace ModdableChess.Logic {
    public enum ServerConnectionState {
        None=0,
        Open, // waiting for connection to another user
        Connected, // connected to another user
        Stopped // server closed, but attempting to reopen
    }

    [Serializable]
    public class ServerInformation : ITicking {
        public static ServerInformation Create() {
            return new ServerInformation();
        }

        [UnityEngine.SerializeField]
        private int clientConnectionID;
        [UnityEngine.SerializeField]
        private StateMachine connection;
        [UnityEngine.SerializeField]
        private int port;
        [UnityEngine.SerializeField]
        private string password;
        [UnityEngine.SerializeField]
        private bool wantsInProgressGame;

        private SafeMessage<int, int> exitConnectionMsgr;
        private SafeMessage<int> enterConnectionMsgr;

        public int ClientConnectionID {
            get {
                return clientConnectionID;
            }

            set {
                this.clientConnectionID = value;
            }
        }

        public StateMachine Connection {
            get {
                return connection;
            }

            set {
                this.connection = value;
            }
        }

        public string Password {
            get {
                return password;
            }

            set {
                this.password = value;
            }
        }

        public bool WantsInProgressGame {
            get {
                return wantsInProgressGame;
            }

            set {
                this.wantsInProgressGame = value;
            }
        }

        public int Port {
            get {
                return port;
            }

            set {
                this.port = value;
            }
        }

        public ServerInformation() {
            exitConnectionMsgr = new SafeMessage<int, int>();
            enterConnectionMsgr = new SafeMessage<int>();
            connection = new StateMachine(0, exitConnectionMsgr, enterConnectionMsgr);
        }

        public void Tick(float deltaTime) {
            exitConnectionMsgr.Tick(deltaTime);
            enterConnectionMsgr.Tick(deltaTime);
        }

        public bool IsHost(int cid) {
            return cid == 0;
        }

        public bool IsConnected(int cid) {
            return cid >= 0 && IsHost(cid) || cid == clientConnectionID;
        }

        public int OtherConnectionID(int cid) {
            if(IsHost(cid)) {
                return clientConnectionID;
            } else if(cid == clientConnectionID) {
                return 0;
            } else {
                return -1;
            }
        }
    }
}
