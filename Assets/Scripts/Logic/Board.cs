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
    public class Board {
        [SerializeField]
        private List<Piece> pieces;
        [SerializeField]
        private PlayerTurnOrder localPlayerOrder;
        [SerializeField]
        private int turnNumber;
        [SerializeField]
        private PlayerTurnOrder activePlayer;
        [SerializeField]
        private TurnState turnState;

        public List<Piece> Pieces {
            get {
                return pieces;
            }

            set {
                this.pieces = value;
            }
        }

        public PlayerTurnOrder LocalPlayerOrder {
            get {
                return localPlayerOrder;
            }

            set {
                this.localPlayerOrder = value;
            }
        }

        public int TurnNumber {
            get {
                return turnNumber;
            }

            set {
                this.turnNumber = value;
            }
        }

        public PlayerTurnOrder ActivePlayer {
            get {
                return activePlayer;
            }

            set {
                this.activePlayer = value;
            }
        }

        public TurnState TurnState {
            get {
                return turnState;
            }

            set {
                turnState = value;
            }
        }

        public Board() {

        }

        public int PieceOnSpace(IntVector2 space) {
            for(int i = 0; i < pieces.Count; i++) {
                if(pieces[i].BoardPosition == space && !pieces[i].IsCaptured) {
                    return i;
                }
            }
            return -1;
        }

        public bool InBounds(IntVector2 dimensions, IntVector2 space) {
            return space.X >= 0 && space.Y >= 0 && space.X < dimensions.x && space.Y < dimensions.y;
        }
    }
}
