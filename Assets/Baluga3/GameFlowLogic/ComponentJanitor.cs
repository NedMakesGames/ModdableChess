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
    public class ComponentJanitor : IActivatable {

        private object component;
        private ComponentRegistry reg;
        private int key;
        private bool activated;
        private bool disposed;

        public bool Active {
            get {
                return activated;
            }

            set {
                BalugaDebug.Assert(!disposed);
                if(activated != value) {
                    if(value) {
                        reg.Register(key, component);
                    } else {
                        reg.Remove(key);
                    }
                    this.activated = value;
                }
            }
        }

        public object Component {
            get {
                return component;
            }
        }

        public ComponentRegistry Registry {
            get {
                return reg;
            }
        }

        public int Key {
            get {
                return key;
            }
        }

        public ComponentJanitor(object component, ComponentRegistry registry, int key) {
            UnityEngine.Debug.Assert(component != null, "Component is null");
            UnityEngine.Debug.Assert(registry != null, "Registry is null");
            this.component = component;
            this.reg = registry;
            this.key = key;
        }

        public static ComponentJanitor NewAndActivate(object component, ComponentRegistry coll, int key) {
            ComponentJanitor okd = new ComponentJanitor(component, coll, key);
            okd.Active = true;
            return okd;
        }

        public void Dispose() {
            if(disposed) {
                return;
            }
            this.Active = false;
            this.disposed = true;
            IDisposable d = component as IDisposable;
            if(d != null) {
                d.Dispose();
            }
            component = null;
            reg = null;
            key = -1;
        }
    }
}
