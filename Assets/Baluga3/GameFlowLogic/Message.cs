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
    public class Message : IMessage {
        private RemovalSafeList<IListener> subs;
        private bool disposed;

        public Message() {
            subs = new RemovalSafeList<IListener>();
        }

        public static Message Create() {
            return new Message();
        }

        public void Subscribe(IListener listener) {
            BalugaDebug.Assert(!disposed);
            subs.Add(listener);
        }

        public void Unsubscribe(IListener listener) {
            if(!disposed) {
                subs.Remove(listener);
            }
        }

        public void Send() {
            BalugaDebug.Assert(!subs.IsIterationLocked, "Callback iteration locked");
            if(!subs.IsIterationLocked) {
                foreach(var ob in subs) {
                    if(disposed) {
                        break;
                    }
                    ob.Notify();
                }
            }
        }

        public void Dispose() {
            if(disposed) {
                return;
            }
            disposed = true;
            subs = null;
        }
    }

    public class Message<T> : IMessage<T> {
        private RemovalSafeList<IListener<T>> subs;
        private bool disposed;
        private List<T> waiting;

        public Message() {
            subs = new RemovalSafeList<IListener<T>>();
        }

        public static Message<T> Create() {
            return new Message<T>();
        }

        public void Subscribe(IListener<T> listener) {
            BalugaDebug.Assert(!disposed);
            subs.Add(listener);
        }

        public void Unsubscribe(IListener<T> listener) {
            if(!disposed) {
                subs.Remove(listener);
            }
        }

        public void Send(T arg) {
            BalugaDebug.Assert(!subs.IsIterationLocked, "Callback iteration locked");
            if(!subs.IsIterationLocked) {
                foreach(var ob in subs) {
                    if(disposed) {
                        break;
                    }
                    ob.Notify(arg);
                }
            }
        }

        public void Dispose() {
            if(disposed) {
                return;
            }
            disposed = true;
            subs = null;
        }
    }

    public class Message<T1, T2> : IMessage<T1, T2> {
        private RemovalSafeList<IListener<T1, T2>> subs;
        private bool disposed;

        public Message() {
            subs = new RemovalSafeList<IListener<T1, T2>>();
        }

        public static Message<T1, T2> Create() {
            return new Message<T1, T2>();
        }

        public void Subscribe(IListener<T1, T2> listener) {
            BalugaDebug.Assert(!disposed);
            subs.Add(listener);
        }

        public void Unsubscribe(IListener<T1, T2> listener) {
            if(!disposed) {
                subs.Remove(listener);
            }
        }

        public void Send(T1 arg1, T2 arg2) {
            BalugaDebug.Assert(!subs.IsIterationLocked, "Callback iteration locked");
            if(!subs.IsIterationLocked) {
                foreach(var ob in subs) {
                    if(disposed) {
                        break;
                    }
                    ob.Notify(arg1, arg2);
                }
            }
        }

        public void Dispose() {
            if(disposed) {
                return;
            }
            disposed = true;
            subs = null;
        }
    }
}
