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

using Baluga3.GameFlowLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Baluga3.GameFlowLogic {
    public interface ISubscribableVar<Type> : ISubscribable<IListener<Type>> {
        Type Value {
            get;
            set;
        }
    }

    public class SubscribableVar<Type> : ISubscribableVar<Type> {
        private Message<Type> m;
        private Type last;
        private Func<Type, Type, bool> testEquality;

        public Type Value {
            get {
                return last;
            }

            set {
                if(!testEquality(last, value)) {
                    this.last = value;
                    m.Send(value);
                }
            }
        }

        public Func<Type, Type, bool> EqualityTester {
            get {
                return testEquality;
            }

            set {
                testEquality = value;
            }
        }

        public SubscribableVar(Type initialValue, Func<Type, Type, bool> testEquality) {
            this.last = initialValue;
            this.m = new Message<Type>();
            this.testEquality = testEquality;
        }

        public void Dispose() {
            m.Dispose();
            testEquality = null;
        }

        public void Subscribe(IListener<Type> listener) {
            m.Subscribe(listener);
        }

        public void Unsubscribe(IListener<Type> listener) {
            m.Unsubscribe(listener);
        }

        public static SubscribableVar<Type> Create() {
            return new SubscribableVar<Type>(default(Type), null);
        }
    }

    public class SubscribableValue<Type> : SubscribableVar<Type> where Type : IEquatable<Type> {
        private static bool TestEquality(Type a, Type b) {
            return a.Equals(b);
        }

        public SubscribableValue(Type initialValue) : base(initialValue, TestEquality) {

        }

        public static new SubscribableValue<Type> Create() {
            return new SubscribableValue<Type>(default(Type));
        }
    }

    public class SubscribableBool : SubscribableValue<bool> {
        public SubscribableBool(bool initialValue) : base(initialValue) {
        }

        public static new SubscribableBool Create() {
            return new SubscribableBool(false);
        }
    }

    public class SubscribableFloat : SubscribableValue<float> {
        public SubscribableFloat(float initialValue) : base(initialValue) {
        }

        public static new SubscribableFloat Create() {
            return new SubscribableFloat(0);
        }
    }

    public class SubscribableInt : SubscribableValue<int> {
        public SubscribableInt(int initialValue) : base(initialValue) {
        }

        public static new SubscribableInt Create() {
            return new SubscribableInt(0);
        }
    }

    public class SubscribableObject<Type> : SubscribableVar<Type> where Type : class {
        private static bool TestEquality(Type a, Type b) {
            if(a == null && b == null) {
                return true;
            } else if(a == null || b == null) {
                return false;
            } else {
                return a.Equals(b);
            }
        }

        public SubscribableObject(Type initialValue) : base(initialValue, TestEquality) {

        }

        public static new SubscribableObject<Type> Create() {
            return new SubscribableObject<Type>(null);
        }
    }

    public class SubscribableVector2 : SubscribableVar<Vector2> {
        private static bool TestEquality(Vector2 a, Vector2 b) {
            return a.x == b.x && a.y == b.y;
        }

        public SubscribableVector2(Vector2 initialValue) : base(initialValue, TestEquality) {

        }

        public static new SubscribableVector2 Create() {
            return new SubscribableVector2(Vector2.zero);
        }
    }

    public class SubscribableQuaternion : SubscribableVar<Quaternion> {
        private static bool TestEquality(Quaternion a, Quaternion b) {
            return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        }

        public SubscribableQuaternion(Quaternion initialValue) : base(initialValue, TestEquality) {

        }

        public static new SubscribableQuaternion Create() {
            return new SubscribableQuaternion(Quaternion.identity);
        }
    }
}
