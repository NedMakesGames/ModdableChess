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

using Baluga3.GameFlowLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic.PlayUI {
    public class TextDisplay {
        private SubscribableBool visible;
        private SubscribableObject<string> text;

        public TextDisplay() {
            visible = new SubscribableBool(true);
            text = new SubscribableObject<string>("");
        }

        public SubscribableObject<string> Text {
            get {
                return text;
            }
        }

        public SubscribableBool Visibility {
            get {
                return visible;
            }
        }

        public static TextDisplay Create() {
            return new TextDisplay();
        }

        public void Dispose() {
            text.Dispose();
            visible.Dispose();
        }
    }
}
