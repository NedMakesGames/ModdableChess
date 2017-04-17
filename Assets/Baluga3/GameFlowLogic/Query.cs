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
    public class Query<ReturnT> : IDisposable, IQueryable<ReturnT> {

        private Func<ReturnT> func;
        private bool disposed;

        public Func<ReturnT> Handler {
            get {
                return func;
            }

            set {
                BalugaDebug.Assert(!disposed && func == null);
                this.func = value;
            }
        }

        public Query() {

        }

        public Query(Func<ReturnT> func) {
            this.func = func;
        }

        public static Query<ReturnT> Create() {
            return new Query<ReturnT>();
        }

        public void Dispose() {
            disposed = true;
            func = null;
        }

        public ReturnT Send() {
            BalugaDebug.Assert(!disposed);
            BalugaDebug.Assert(func != null);
            return func();
        }
    }

    public class Query<ReturnT, ArgT> : IDisposable, IQueryable<ReturnT, ArgT> {

        private Func<ArgT, ReturnT> func;
        private bool disposed;

        public Func<ArgT, ReturnT> Handler {
            get {
                return func;
            }

            set {
                BalugaDebug.Assert(!disposed && func == null);
                this.func = value;
            }
        }

        public Query() {

        }

        public Query(Func<ArgT, ReturnT> func) {
            this.func = func;
        }

        public static Query<ReturnT, ArgT> Create() {
            return new Query<ReturnT, ArgT>();
        }

        public void Dispose() {
            disposed = true;
            func = null;
        }

        public ReturnT Send(ArgT arg) {
            BalugaDebug.Assert(!disposed);
            BalugaDebug.Assert(func != null);
            return func(arg);
        }
    }

    public class Query<ReturnT, Arg1T, Arg2T> : IDisposable, IQueryable<ReturnT, Arg1T, Arg2T> {

        private Func<Arg1T, Arg2T, ReturnT> func;
        private bool disposed;

        public Func<Arg1T, Arg2T, ReturnT> Handler {
            get {
                return func;
            }

            set {
                BalugaDebug.Assert(!disposed && func == null);
                this.func = value;
            }
        }

        public Query() {

        }

        public Query(Func<Arg1T, Arg2T, ReturnT> func) {
            this.func = func;
        }

        public static Query<ReturnT, Arg1T, Arg2T> Create() {
            return new Query<ReturnT, Arg1T, Arg2T>();
        }

        public void Dispose() {
            disposed = true;
            func = null;
        }

        public ReturnT Send(Arg1T arg1, Arg2T arg2) {
            BalugaDebug.Assert(!disposed);
            BalugaDebug.Assert(func != null);
            return func(arg1, arg2);
        }
    }
}
