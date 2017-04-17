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
    public class TickingJanitor : IActivatable {

        private ITicking ticking;
        private Game game;
        private bool active;
        private bool forceInactive;
        private bool tickingAdded;

        public TickingJanitor(Game game, ITicking ticking) {
            this.game = game;
            this.ticking = ticking;
        }

        public static TickingJanitor NewAndActivate(Game game, ITicking ticking) {
            TickingJanitor j = new TickingJanitor(game, ticking);
            j.Active = true;
            return j;
        }

        public bool Active {
            get {
                return active;
            }

            set {
                this.active = value;
                Refresh();
            }
        }

        private void Refresh() {
            bool wantsAdded = active && !forceInactive;
            if(wantsAdded != tickingAdded) {
                if(wantsAdded) {
                    game.AddTicking(ticking);
                } else {
                    game.RemoveTicking(ticking);
                }
                tickingAdded = wantsAdded;
            }
        }

        public bool ForceInactive {
            get {
                return forceInactive;
            }

            set {
                this.forceInactive = value;
                Refresh();
            }
        }

        public void Dispose() {
            this.Active = false;
            this.game = null;
            this.ticking = null;
        }
    }
}
