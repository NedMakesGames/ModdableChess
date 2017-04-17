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

using UnityEngine;
using System.Collections;
using Baluga3.Core;
using Baluga3.GameFlowLogic;
using System;

namespace Baluga3.UnityCore {
    public class ScreenWatcher : MonoBehaviour {

        public static ScreenWatcher Load() {
            return Baluga3Object.GetOrAdd<ScreenWatcher>();
        }

        private IntVector2 lastScreenSize;
        private bool lastFullscreen;
        private SubscribableVar<IntVector2> resolution;
        private SimpleListener<IntVector2> changeListener;

        public SubscribableVar<IntVector2> ResolutionMessage {
            get {
                return resolution;
            }

            set {
                if(resolution != null) {
                    resolution.Unsubscribe(changeListener);
                }
                this.resolution = value;
                if(resolution != null) {
                    resolution.Value = lastScreenSize;
                    resolution.Subscribe(changeListener);
                }
            }
        }

        private void Awake() {
            changeListener = new SimpleListener<IntVector2>(OnChangeCommand);
        }

        void Start() {
            lastScreenSize = new IntVector2(-1, -1);
            Refresh();
        }

        private void Refresh() {
            IntVector2 size = new IntVector2(UnityEngine.Screen.width, UnityEngine.Screen.height);
            if(lastScreenSize != size || lastFullscreen != UnityEngine.Screen.fullScreen) {
                lastScreenSize = size;
                lastFullscreen = UnityEngine.Screen.fullScreen;
                if(resolution != null) {
                    resolution.Value = lastScreenSize;
                }
            }
        }

        void Update() {
            Refresh();
        }

        private void OnChangeCommand(IntVector2 resolution) {
            if(lastScreenSize != resolution) {
                throw new NotImplementedException();
            }
        }

#if DEBUG
        public void DebugTimer() {
            lastScreenSize = new IntVector2(-1, -1);
        }
#endif
    }
}
