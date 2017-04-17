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
    public class SimpleListener : IDisposable, IListener {
        private Action callback;
        private bool disposed;

        public Action Callback {
            get {
                return callback;
            }

            set {
                BalugaDebug.Assert(!disposed);
                this.callback = value;
            }
        }

        public SimpleListener(Action callback) {
            this.callback = callback;
        }

        public void Notify() {
            BalugaDebug.Assert(!disposed);
            callback();
        }

        public void Dispose() {
            if(disposed) {
                return;
            }
            this.disposed = true;
            callback = null;
        }
    }

    public class SimpleListener<T> : IDisposable, IListener<T> {
        private Action<T> callback;
        private bool disposed;

        public Action<T> Callback {
            get {
                return callback;
            }

            set {
                BalugaDebug.Assert(!disposed);
                this.callback = value;
            }
        }

        public SimpleListener(Action<T> callback) {
            this.callback = callback;
        }

        public void Notify(T arg) {
            BalugaDebug.Assert(!disposed);
            callback(arg);
        }

        public void Dispose() {
            if(disposed) {
                return;
            }
            this.disposed = true;
            callback = null;
        }
    }

    public class SimpleListener<T1, T2> : IDisposable, IListener<T1, T2> {
        private Action<T1, T2> callback;
        private bool disposed;

        public Action<T1, T2> Callback {
            get {
                return callback;
            }

            set {
                BalugaDebug.Assert(!disposed);
                this.callback = value;
            }
        }

        public SimpleListener(Action<T1, T2> callback) {
            this.callback = callback;
        }

        public void Notify(T1 arg1, T2 arg2) {
            BalugaDebug.Assert(!disposed);
            callback(arg1, arg2);
        }

        public void Dispose() {
            if(disposed) {
                return;
            }
            this.disposed = true;
            callback = null;
        }
    }
}
