﻿// Copyright (c) 2017, Timothy Ned Atton.
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
using MoonSharp.Interpreter;
using UnityEngine;

namespace ModdableChess.Logic {
    [Serializable]
    public class PiecePrototype {

        [SerializeField]
        private string luaTag;
        [SerializeField]
        private Closure movementFunction;
        [SerializeField]
        private int firstModelIndex;
        [SerializeField]
        private int secondModelIndex;
        [SerializeField]
        private int promotionModelIndex;

        public int FirstModelIndex {
            get {
                return firstModelIndex;
            }

            set {
                this.firstModelIndex = value;
            }
        }

        public int SecondModelIndex {
            get {
                return secondModelIndex;
            }

            set {
                this.secondModelIndex = value;
            }
        }

        public Closure MovementFunction {
            get {
                return movementFunction;
            }

            set {
                movementFunction = value;
            }
        }

        public string LuaTag {
            get {
                return luaTag;
            }

            set {
                luaTag = value;
            }
        }

        public int PromotionIndicatorModelIndex {
            get {
                return promotionModelIndex;
            }

            set {
                promotionModelIndex = value;
            }
        }
    }
}
