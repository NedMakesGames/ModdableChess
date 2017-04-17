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

namespace Baluga3.GameFlowLogic {
    public class ListenerJanitor<T> : IActivatable {

        private ISubscribable<T> subscribable;
        private T subscriber;
        private bool active;
        private bool disposed;

        public bool Active {
            get {
                return active;
            }

            set {
                BalugaDebug.Assert(!disposed);
                if(active != value) {
                    if(value) {
                        subscribable.Subscribe(subscriber);
                    } else {
                        subscribable.Unsubscribe(subscriber);
                    }
                    this.active = value;
                }
            }
        }

        public ISubscribable<T> Subscribable {
            get {
                return subscribable;
            }
        }

        public T Subscriber {
            get {
                return subscriber;
            }
        }

        public ListenerJanitor(ISubscribable<T> subscribable, T subscriber) {
            UnityEngine.Debug.Assert(subscribable != null, "Subscribable is null");
            UnityEngine.Debug.Assert(subscriber != null, "Subscriber is null");
            this.subscribable = subscribable;
            this.subscriber = subscriber;
        }

        public static ListenerJanitor<T> NewAndActivate(ISubscribable<T> subscribable, T subscriber) {
            ListenerJanitor<T> okd = new ListenerJanitor<T>(subscribable, subscriber);
            okd.Active = true;
            return okd;
        }

        public void Dispose() {
            if(disposed) {
                return;
            }
            this.Active = false;
            this.disposed = true;
            IDisposable d = subscriber as IDisposable;
            if(d != null) {
                d.Dispose();
            }
            subscriber = default(T);
            subscribable = null;
        }
    }
}
