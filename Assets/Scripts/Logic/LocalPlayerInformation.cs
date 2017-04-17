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
    public enum PlayerTurnOrder {
        Undecided=0, First, Second
    }

    public enum ClientConnectionState {
        None=0, Seeking, Connected, Validated, Stopped
    }

    [Serializable]
    public class LocalPlayerInformation : ITicking {
        [UnityEngine.SerializeField]
        private bool isHost;
        [UnityEngine.SerializeField]
        private StateMachine connection;
        [UnityEngine.SerializeField]
        private string serverAddress;
        [UnityEngine.SerializeField]
        private int serverPort;
        [UnityEngine.SerializeField]
        private string password;
        [UnityEngine.SerializeField]
        private bool inProgressGame;

        private SafeMessage<int, int> exitConnectionMsgr;
        private SafeMessage<int> enterConnectionMsgr;

        public LocalPlayerInformation() {
            exitConnectionMsgr = new SafeMessage<int, int>();
            enterConnectionMsgr = new SafeMessage<int>();
            connection = new StateMachine(0, exitConnectionMsgr, enterConnectionMsgr);
        }

        public static LocalPlayerInformation Create() {
            return new LocalPlayerInformation();
        }

        public StateMachine Connection {
            get {
                return connection;
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

        public bool InProgressGame {
            get {
                return inProgressGame;
            }

            set {
                this.inProgressGame = value;
            }
        }

        public string ServerAddress {
            get {
                return serverAddress;
            }

            set {
                this.serverAddress = value;
            }
        }

        public int ServerPort {
            get {
                return serverPort;
            }

            set {
                this.serverPort = value;
            }
        }

        public bool IsHost {
            get {
                return isHost;
            }

            set {
                this.isHost = value;
            }
        }

        public void Tick(float deltaTime) {
            exitConnectionMsgr.Tick(deltaTime);
            enterConnectionMsgr.Tick(deltaTime);
        }
    }
}
