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

namespace ModdableChess.Logic.Lobby {
    public class ModListButton {
        private string nameString;
        private string statusString;
        private int order;
        private bool choosable;

        public string NameString {
            get {
                return nameString;
            }

            set {
                this.nameString = value;
            }
        }

        public string StatusString {
            get {
                return statusString;
            }

            set {
                this.statusString = value;
            }
        }

        public int Order {
            get {
                return order;
            }

            set {
                this.order = value;
            }
        }

        public bool Choosable {
            get {
                return choosable;
            }

            set {
                this.choosable = value;
            }
        }
    }
}
