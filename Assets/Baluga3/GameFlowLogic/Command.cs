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
    public class Command : IDisposable, ICallable {

        private Action handler;
        private bool disposed;

        public Action Handler {
            get {
                return handler;
            }

            set {
                BalugaDebug.Assert(!disposed && handler == null);
                this.handler = value;
            }
        }

        public Command() {

        }

        public Command(Action handler) {
            this.handler = handler;
        }

        public static Command Create() {
            return new Command();
        }

        public void Dispose() {
            disposed = true;
            handler = null;
        }

        public void Send() {
            handler();
        }
    }

    public class Command<T> : IDisposable, ICallable<T> {

        private Action<T> handler;
        private bool disposed;

        public Action<T> Handler {
            get {
                return handler;
            }

            set {
                BalugaDebug.Assert(!disposed && handler == null);
                this.handler = value;
            }
        }

        public Command() {

        }

        public Command(Action<T> handler) {
            this.handler = handler;
        }

        public static Command<T> Create() {
            return new Command<T>();
        }

        public void Dispose() {
            disposed = true;
            handler = null;
        }

        public void Send(T arg) {
            handler(arg);
        }
    }

    public class Command<T1, T2> : IDisposable, ICallable<T1, T2> {

        private Action<T1, T2> handler;
        private bool disposed;

        public Action<T1, T2> Handler {
            get {
                return handler;
            }

            set {
                BalugaDebug.Assert(!disposed && handler == null);
                this.handler = value;
            }
        }

        public Command() {

        }

        public Command(Action<T1, T2> handler) {
            this.handler = handler;
        }

        public static Command<T1, T2> Create() {
            return new Command<T1, T2>();
        }

        public void Dispose() {
            disposed = true;
            handler = null;
        }

        public void Send(T1 arg1, T2 arg2) {
            handler(arg1, arg2);
        }
    }
}
