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
using Baluga3.GameFlowLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Baluga3.Input {
    public class WASDDirectionController : ISubscribable<IListener<IntVector2>> {

        private IMessage<IntVector2> changeMsg;
        private IntVector2 dir;
        private bool allowDiagonal;

        public WASDDirectionController() {
            changeMsg = new Message<IntVector2>();
        }

        public static WASDDirectionController Create() {
            return new WASDDirectionController();
        }

        public IntVector2 Direction {
            get {
                return dir;
            }
        }

        public bool AllowDiagonal {
            get {
                return allowDiagonal;
            }

            set {
                allowDiagonal = value;
            }
        }

        public void SetButtons(bool up, bool left, bool down, bool right) {
            IntVector2 p = IntVector2.Zero;
            if(right && !left) {
                p.x = 1;
            } else if(left && !right) {
                p.x = -1;
            }
            if(allowDiagonal || p.x == 0) {
                if(up && !down) {
                    p.y = 1;
                } else if(down && !up) {
                    p.y = -1;
                }
            }
            if(dir != p) {
                dir = p;
                changeMsg.Send(p);
            }
        }

        public void Dispose() {
            changeMsg.Dispose();
        }

        public void Subscribe(IListener<IntVector2> listener) {
            changeMsg.Subscribe(listener);
        }

        public void Unsubscribe(IListener<IntVector2> listener) {
            changeMsg.Unsubscribe(listener);
        }
    }
}
