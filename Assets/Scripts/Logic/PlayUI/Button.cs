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

namespace ModdableChess.Logic.PlayUI {
    public class Button : IDisposable {
        private SubscribableBool interactable;
        private SubscribableBool visible;
        private SubscribableObject<string> text;
        private Message pressMessage;

        public Button() {
            interactable = new SubscribableBool(true);
            visible = new SubscribableBool(true);
            text = new SubscribableObject<string>("");
            pressMessage = new Message();
        }

        public SubscribableBool Interactable {
            get {
                return interactable;
            }
        }

        public SubscribableObject<string> Text {
            get {
                return text;
            }
        }

        public Message PressMessage {
            get {
                return pressMessage;
            }
        }

        public SubscribableBool Visibility {
            get {
                return visible;
            }
        }

        public static Button Create() {
            return new Button();
        }

        public void Dispose() {
            interactable.Dispose();
            text.Dispose();
            pressMessage.Dispose();
            visible.Dispose();
        }
    }
}
