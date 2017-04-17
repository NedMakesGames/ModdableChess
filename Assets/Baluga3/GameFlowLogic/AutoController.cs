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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Baluga3.GameFlowLogic {
    public class AutoController : IGameController {

        private Game game;
        private ComponentRegistry sceneComps;
        private ActivatableList aList;
        private DisposableList dList;

        public Game Game {
            get {
                return game;
            }
        }

        public ComponentRegistry Components {
            get {
                return sceneComps;
            }
        }

        public ActivatableList ActivatableList {
            get {
                return aList;
            }
        }

        public DisposableList DisposableList {
            get {
                return dList;
            }
        }

        public AutoController(Game game) {
            this.game = game;
            this.aList = new ActivatableList();
            this.dList = new DisposableList();
            this.sceneComps = new ComponentRegistry();
        }

        public virtual void Dispose() {
            aList.Clear();
            dList.Clear();
            sceneComps.DisposeWithComponents();
        }

        public virtual void Enter() {
            aList.Active = true;
        }

        public virtual void Exit() {
            aList.Active = false;
        }

        public virtual void Tick(float deltaTime) {

        }
    }
}
