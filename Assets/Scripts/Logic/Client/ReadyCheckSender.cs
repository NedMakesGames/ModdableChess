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

namespace ModdableChess.Logic.Client {
    public class ReadyCheckSender : ITicking, IActivatable {

        private float initialWaitPeriod = 0;
        private float resendPeriod;
        private TickingJanitor tickHelper;
        private Action<bool> callback;
        private float timer;
        private bool active;

        public ReadyCheckSender(Game game, float resendPeriod, Action<bool> callback) {
            this.resendPeriod = resendPeriod;
            this.callback = callback;
            tickHelper = new TickingJanitor(game, this);
        }

        public bool Active {
            get {
                return active;
            }

            set {
                if(active != value) {
                    bool wasActive = active;
                    this.active = value;
                    tickHelper.Active = active;
                    if(active) {
                        if(initialWaitPeriod > 0) {
                            timer = initialWaitPeriod;
                        } else {
                            Send();
                        }
                    } else if(wasActive) {
                        callback(false);
                    }
                }
            }
        }

        public float InitialWaitPeriod {
            get {
                return initialWaitPeriod;
            }

            set {
                initialWaitPeriod = value;
            }
        }

        public float ResendPeriod {
            get {
                return resendPeriod;
            }

            set {
                resendPeriod = value;
            }
        }

        private void Send() {
            timer = resendPeriod;
            callback(true);
        }

        public void Dispose() {
            tickHelper.Dispose();
            callback = null;
        }

        public void Tick(float deltaTime) {
            timer -= deltaTime;
            if(timer <= 0) {
                Send();
            }
        }
    }
}
