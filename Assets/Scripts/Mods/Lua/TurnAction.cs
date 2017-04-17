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
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Mods.Lua {
    [MoonSharpUserData]
    public class TurnActionComponent {

        [MoonSharpHidden]
        public TurnActionComponent() {

        }

        private string type;
        private int actorX;
        private int actorY;
        private int targetX;
        private int targetY;
        private string promotionPiece;

        public string Type {
            get {
                return type;
            }

            set {
                type = value;
            }
        }

        public string PromotionPiece {
            get {
                return promotionPiece;
            }

            set {
                promotionPiece = value;
            }
        }

        public int ActorX {
            get {
                return actorX;
            }

            set {
                actorX = value;
            }
        }

        public int ActorY {
            get {
                return actorY;
            }

            set {
                actorY = value;
            }
        }

        public int TargetX {
            get {
                return targetX;
            }

            set {
                targetX = value;
            }
        }

        public int TargetY {
            get {
                return targetY;
            }

            set {
                targetY = value;
            }
        }
    }
}
