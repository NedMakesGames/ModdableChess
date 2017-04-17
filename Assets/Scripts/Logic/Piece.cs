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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModdableChess.Logic {
    [Serializable]
    public class Piece {
        [SerializeField]
        private PlayerTurnOrder ownerPlayer;
        [SerializeField]
        private int prototypeID;
        [SerializeField]
        private IntVector2 boardPosition;
        [SerializeField]
        private bool isCaptured;
        [SerializeField]
        private int numberOfMoves;

        public PlayerTurnOrder OwnerPlayer {
            get {
                return ownerPlayer;
            }

            set {
                this.ownerPlayer = value;
            }
        }

        public int PrototypeID {
            get {
                return prototypeID;
            }

            set {
                this.prototypeID = value;
            }
        }

        public IntVector2 BoardPosition {
            get {
                return boardPosition;
            }

            set {
                this.boardPosition = value;
            }
        }

        public bool IsCaptured {
            get {
                return isCaptured;
            }

            set {
                this.isCaptured = value;
            }
        }

        public int NumberOfMoves {
            get {
                return numberOfMoves;
            }

            set {
                this.numberOfMoves = value;
            }
        }
    }
}
