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
    public class SafeMessage : IMessage, ITicking {
        private RemovalSafeList<IListener> subs;
        private bool callAgain;
        private bool disposed;

        public SafeMessage() {
            subs = new RemovalSafeList<IListener>();
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
            if(subs.IsIterationLocked) {
                callAgain = true;
            } else { 
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
            callAgain = false;
        }

        public void Tick(float deltaTime) {
            BalugaDebug.Assert(!disposed, "Disposed LockSafeMessenger still in tick list");
            if(callAgain) {
                Send();
            }
        }
    }

    public class SafeMessage<T> : IMessage<T>, ITicking {
        private RemovalSafeList<IListener<T>> subs;
        private Queue<T> waitingArgs;
        private bool disposed;

        public SafeMessage() {
            subs = new RemovalSafeList<IListener<T>>();
            waitingArgs = new Queue<T>();
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
            if(subs.IsIterationLocked) {
                waitingArgs.Enqueue(arg);
            } else {
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
            waitingArgs = null;
        }

        public void Tick(float deltaTime) {
            BalugaDebug.Assert(!disposed, "Disposed LockSafeMessenger still in tick list");
            if(waitingArgs.Count > 0) {
                int startCount = waitingArgs.Count;
                for(int i = 0; i < startCount; i++) {
                    Send(waitingArgs.Dequeue());
                }
            }
        }
    }

    public class SafeMessage<T1, T2> : IMessage<T1, T2>, ITicking {

        public struct Waiting {
            public T1 arg1;
            public T2 arg2;
        }

        private RemovalSafeList<IListener<T1, T2>> subs;
        private Queue<Waiting> waitingArgs;
        private bool disposed;

        public SafeMessage() {
            subs = new RemovalSafeList<IListener<T1, T2>>();
            waitingArgs = new Queue<Waiting>();
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
            if(subs.IsIterationLocked) {
                waitingArgs.Enqueue(new Waiting() {
                    arg1 = arg1,
                    arg2 = arg2,
                });
            } else {
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
            waitingArgs = null;
        }

        public void Tick(float deltaTime) {
            BalugaDebug.Assert(!disposed, "Disposed LockSafeMessenger still in tick list");
            if(waitingArgs.Count > 0) {
                int startCount = waitingArgs.Count;
                for(int i = 0; i < startCount; i++) {
                    Waiting w = waitingArgs.Dequeue();
                    Send(w.arg1, w.arg2);
                }
            }
        }
    }
}
