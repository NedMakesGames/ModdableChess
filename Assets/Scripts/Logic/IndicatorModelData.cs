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
    public class IndicatorModelData {
        private int[] full;
        private int[] small;
        private int[] mouseover;
        private float stackingHeight;

        public int[] Full {
            get {
                return full;
            }

            set {
                full = value;
            }
        }

        public int[] Small {
            get {
                return small;
            }

            set {
                small = value;
            }
        }

        public int[] Mouseover {
            get {
                return mouseover;
            }

            set {
                mouseover = value;
            }
        }

        public float StackingHeight {
            get {
                return stackingHeight;
            }

            set {
                stackingHeight = value;
            }
        }

        public int GetIndexFor(ActionIndicatorType type, ActionIndicatorStrength strength) {
            int typeI = (int)type - 1;
            switch(strength) {
            case ActionIndicatorStrength.Inactive:
                if(small[typeI] < 0) {
                    return full[typeI];
                } else {
                    return small[typeI];
                }
            case ActionIndicatorStrength.Mouseover:
                if(mouseover[typeI] < 0) {
                    return full[typeI];
                } else {
                    return mouseover[typeI];
                }
            default:
                return full[typeI];
            }
        }
    }
}
