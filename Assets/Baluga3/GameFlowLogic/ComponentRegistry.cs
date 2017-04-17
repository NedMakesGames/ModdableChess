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
using Baluga3.Core;

namespace Baluga3.GameFlowLogic {
    public class ComponentRegistry : IDisposable {
        private Dictionary<int, object> components;
        private Message<int, object> registerChange;

        public Message<int, object> RegisterChange {
            get {
                return registerChange;
            }
        }

        public ComponentRegistry() {
            components = new Dictionary<int, object>();
            registerChange = new Message<int, object>();
        }

        public object Get(int key) {
            object comp = null;
            if(!components.TryGetValue(key, out comp)) {
                throw new System.Exception(string.Format("Component for key {0} not registered", key));
            }
            return comp; 
        }

        public T Get<T>(int key) {
            return (T)Get(key);
        }

        public bool Contains(int key) {
            return components.ContainsKey(key);
        }

        public bool Contains(int key, out object comp) {
            if(components.TryGetValue(key, out comp)) {
                return true;
            } else {
                return false;
            }
        }

        public bool TryGet<T>(int key, out T comp) where T : class {
            object cobj;
            bool inDictionary = Contains(key, out cobj);
            if(inDictionary) {
                comp = cobj as T;
                return comp != null;
            } else {
                comp = null;
                return false;
            }
        }

        public object Register(int key, object comp) {
            return Register<object>(key, comp);
        }

        public T Register<T>(int key, T comp) where T : class {
            BalugaDebug.Assert(!Contains(key));
            components[key] = comp;
            registerChange.Send(key, comp);
            return comp;
        }

        public void Remove(int key) {
            BalugaDebug.Assert(Contains(key));
            components.Remove(key);
            registerChange.Send(key, null);
        }

        public T GetOrRegister<T>(int key, Func<T> creator) where T : class {
            T registered;
            if(TryGet<T>(key, out registered)) {
                return registered;
            } else {
                T newlyCreated = creator();
                Register(key, newlyCreated);
                return newlyCreated;
            }
        }

        public void SafeDo<T>(int key, Action<T> doIfRegistered) where T : class {
            T comp;
            if(TryGet<T>(key, out comp)) {
                doIfRegistered(comp);
            }
        }

        public TReturn SafeDo<T, TReturn>(int key, Func<T, TReturn> doIfRegistered) where T : class {
            T comp;
            if(TryGet<T>(key, out comp)) {
                return doIfRegistered(comp);
            } else {
                return default(TReturn);
            }
        }

        public void DisposeWithComponents() {
            foreach(var c in components.Values) {
                IDisposable d = c as IDisposable;
                if(d != null) {
                    d.Dispose();
                }
            }
            Dispose();
        }

        public void Dispose() {
            components = null;
            registerChange.Dispose();
        }
    }
}
