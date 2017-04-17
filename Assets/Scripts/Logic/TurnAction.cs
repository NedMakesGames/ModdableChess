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

namespace ModdableChess.Logic {
    public enum TurnActionComponentType {
        None, MovePiece, CapturePiece, PromotePiece, Forfeit
    }

    public class TurnAction {
        public IntVector2 searchPiece;
        public List<TurnActionComponent> components;

        public bool IsEquivalent(TurnAction other) {
            if(searchPiece == other.searchPiece && components.Count == other.components.Count) {
                for(int i = 0; i < components.Count; i++) {
                    if(!components[i].IsEquivalent(other.components[i])) {
                        return false;
                    }
                }
                return true;
            } else {
                return false;
            }
        }
    }

    public class TurnActionComponent {
        public TurnActionComponentType type;
        public IntVector2 actor;
        public IntVector2 target;
        public int promotionIndex;

        internal bool IsEquivalent(TurnActionComponent other) {
            if(type != other.type) {
                return false;
            } else {
                switch(type) {
                case TurnActionComponentType.MovePiece:
                    return actor == other.actor && target == other.target;
                case TurnActionComponentType.CapturePiece:
                    return target == other.target;
                case TurnActionComponentType.PromotePiece:
                    return actor == other.actor && promotionIndex == other.promotionIndex;
                default:
                    return true;
                }
            }
        }
    }
}
