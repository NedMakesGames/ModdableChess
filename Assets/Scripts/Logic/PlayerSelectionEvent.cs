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
using Baluga3.Core;
using Baluga3.GameFlowLogic;
using System;

namespace ModdableChess.Logic {
    public class PlayerSelectionEvent : ISubscribable<IListener> {

        public enum SelectionType {
            Nothing=0, Square, Piece
        }

        private SelectionType selectionType;
        private IntVector2 squarePos;
        private int pieceID;
        private Message messenger;

        public SelectionType CurrentType {
            get {
                return selectionType;
            }
        }

        public IntVector2 SquarePos {
            get {
                return squarePos;
            }
        }

        public int PieceID {
            get {
                return pieceID;
            }
        }

        public PlayerSelectionEvent() {
            selectionType = SelectionType.Nothing;
            messenger = new Message();
        }

        public void Subscribe(IListener listener) {
            messenger.Subscribe(listener);
        }

        public void Unsubscribe(IListener listener) {
            messenger.Unsubscribe(listener);
        }

        public void Dispose() {
            messenger.Dispose();
        }

        public void SetSelectionSquare(IntVector2 pos) {
            if(selectionType != SelectionType.Square || pos != squarePos) {
                this.selectionType = SelectionType.Square;
                this.squarePos = pos;
                messenger.Send();
            }
        }

        internal void Subscribe(object onSelectionChange) {
            throw new NotImplementedException();
        }

        public void SetSelectionPiece(int pieceID) {
            if(selectionType != SelectionType.Piece || pieceID != this.pieceID) {
                this.selectionType = SelectionType.Piece;
                this.pieceID = pieceID;
                messenger.Send();
            }
        }

        public void SetSelectionNothing() {
            if(selectionType != SelectionType.Nothing) {
                this.selectionType = SelectionType.Nothing;
                messenger.Send();
            }
        }

        public static PlayerSelectionEvent Create() {
            return new PlayerSelectionEvent();
        }
    }
}
