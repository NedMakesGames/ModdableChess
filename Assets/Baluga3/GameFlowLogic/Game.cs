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
using System.Collections.Generic;

namespace Baluga3.GameFlowLogic {
    public abstract class Game {
        private ComponentRegistry globalComps;
        private IGameController controller;
        private RemovalSafeList<ITicking> tickings;
        private int tickCount;
        private Message<IGameController> controllerEnterMessage, controllerExitMessage;

        public Game() {
            globalComps = new ComponentRegistry();
            tickings = new RemovalSafeList<ITicking>();
            controllerEnterMessage = new Message<IGameController>();
            controllerExitMessage = new Message<IGameController>();
        }

        public ComponentRegistry Components {
            get {
                return globalComps;
            }
        }

        public IGameController Controller {
            get {
                return controller;
            }

            set {
                if(controller != null) {
                    controller.Exit();
                    if(controllerExitMessage != null) {
                        controllerExitMessage.Send(controller);
                    }
                }
                this.controller = value;
                if(controller != null) {
                    controller.Enter();
                    if(controllerEnterMessage != null) {
                        controllerEnterMessage.Send(controller);
                    }
                }
            }
        }

        public int TickCount {
            get {
                return tickCount;
            }
        }

        public Message<IGameController> ControllerEnterMessage {
            get {
                return controllerEnterMessage;
            }
        }

        public Message<IGameController> ControllerExitMessage {
            get {
                return controllerExitMessage;
            }
        }

        public void AddTicking(ITicking ticking) {
            tickings.Add(ticking);
        }

        public void RemoveTicking(ITicking ticking) {
            tickings.Remove(ticking);
        }

        public void Tick(float deltaTime) {
            tickCount++;
            BalugaDebug.tick = tickCount;
            if(controller != null) {
                controller.Tick(deltaTime);
            }
            foreach(var t in tickings) {
                t.Tick(deltaTime);
            }
        }
    }
}
