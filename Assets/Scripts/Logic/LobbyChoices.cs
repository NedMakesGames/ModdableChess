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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic {
    public enum LobbyTurnOrderChoice {
        None=0, HostIsFirst, ClientIsFirst, Random
    }

    public enum LobbyMatchState {
        None,
        FreshLobby, // Sets default values in the lobby, asks server to generate starting game board
        Rejoining, // Sets default values in the lobby, asks server for in progress game board
        RepeatGame // Sets last values in the lobby, asks server to generate starting game board
    }

    [Serializable]
    public class LobbyChoices {
        [UnityEngine.SerializeField]
        private string modFolder;
        [UnityEngine.SerializeField]
        private LobbyTurnOrderChoice orderChoice;
        [UnityEngine.SerializeField]
        private LobbyMatchState matchState;

        public string ModFolder {
            get {
                return modFolder;
            }

            set {
                this.modFolder = value;
            }
        }

        public LobbyTurnOrderChoice OrderChoice {
            get {
                return orderChoice;
            }

            set {
                this.orderChoice = value;
            }
        }

        public LobbyMatchState MatchState {
            get {
                return matchState;
            }

            set {
                this.matchState = value;
            }
        }
    }
}
