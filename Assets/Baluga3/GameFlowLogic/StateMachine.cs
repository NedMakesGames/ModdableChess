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

namespace Baluga3.GameFlowLogic {
    [Serializable]
    public class StateMachine : IDisposable {

        [UnityEngine.SerializeField]
        private int state;

        private IMessage<int, int> leaveStateMsg;
        private IMessage<int> enterStateMsg;

        public int State {
            get {
                return state;
            }

            set {
                if(state != value) {
                    leaveStateMsg.Send(state, value);
                    this.state = value;
                    enterStateMsg.Send(state);
                }
            }
        }

        public IMessage<int, int> ExitStateMessenger {
            get {
                return leaveStateMsg;
            }
        }

        public IMessage<int> EnterStateMessenger {
            get {
                return enterStateMsg;
            }
        }

        public StateMachine(int initState) : this(initState, new Message<int, int>(), new Message<int>()) {

        }

        public StateMachine(int initState, IMessage<int, int> leaveStateMsg, IMessage<int> enterStateMsg) {
            this.leaveStateMsg = leaveStateMsg;
            this.enterStateMsg = enterStateMsg;
            this.state = initState;
        }

        public static StateMachine Create() {
            return new StateMachine(0);
        }

        public void Dispose() {
            leaveStateMsg.Dispose();
            enterStateMsg.Dispose();
            leaveStateMsg = null;
            enterStateMsg = null;
        }
    }
}
