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

    public class StateEnterWatcher : IActivatable {
        private StateMachine stateMachine;
        private SimpleListener<int> enterL;
        private SimpleListener<int, int> exitL;
        private bool active;
        private bool inState;
        private List<int> activeStates;
        private Action enterCallback;
        private Action exitCallback;

        public bool InState {
            get {
                return inState;
            }
        }

        public bool Active {
            get {
                return active;
            }

            set {
                if(value != active) {
                    active = value;
                    if(active) {
                        stateMachine.EnterStateMessenger.Subscribe(enterL);
                        stateMachine.ExitStateMessenger.Subscribe(exitL);
                    } else {
                        stateMachine.EnterStateMessenger.Unsubscribe(enterL);
                        stateMachine.ExitStateMessenger.Unsubscribe(exitL);
                    }
                }
            }
        }

        public StateMachine StateMachine {
            get {
                return stateMachine;
            }
        }

        public List<int> ActiveStates {
            get {
                return activeStates;
            }

            set {
                this.activeStates = value;
                NotifyActiveStatesChanged();
            }
        }

        public StateEnterWatcher(StateMachine stateMachine, List<int> activeStates, Action enterState, Action exitState) {
            this.stateMachine = stateMachine;
            this.activeStates = activeStates;
            this.enterCallback = enterState;
            this.exitCallback = exitState;
            enterL = new SimpleListener<int>((state) => {
                if(!inState && activeStates.Contains(state)) {
                    inState = true;
                    enterCallback();
                }
            });
            exitL = new SimpleListener<int, int>((exit, enter) => {
                if(inState && !activeStates.Contains(enter)) {
                    inState = false;
                    exitCallback();
                }
            });
            NotifyActiveStatesChanged();
        }

        public void NotifyActiveStatesChanged() {
            bool nowIn = activeStates.Contains(stateMachine.State);
            if(nowIn != inState) {
                inState = nowIn;
                if(inState) {
                    enterCallback();
                } else {
                    exitCallback();
                }
            }
        }

        public void Dispose() {
            this.Active = false;
            activeStates = null;
            stateMachine = null;
            enterL.Dispose();
            enterL = null;
            exitL.Dispose();
            exitL = null;
            enterCallback = null;
            exitCallback = null;
        }
    }
}