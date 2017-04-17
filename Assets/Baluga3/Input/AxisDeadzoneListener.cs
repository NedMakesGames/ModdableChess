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
    public class AxisDeadzoneListener : IListener<float> {

        private Action<float> callback;
        private float deadzone;
        private float lastAxis;

        public AxisDeadzoneListener(float deadzone, Action<float> callback) {
            this.deadzone = deadzone;
            this.callback = callback;
        }

        public void Notify(float axis) {
            float effective = 0;
            if(Math.Abs(axis) >= deadzone) {
                effective = axis;
            }
            if(!Mathf.Approximately(effective, lastAxis)) {
                lastAxis = effective;
                callback(effective);
            }
        }
    }
}
