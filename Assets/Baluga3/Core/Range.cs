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
using UnityEngine;

namespace Baluga3.Core {
    [Serializable]
    public struct Range : IEquatable<Range> {
        private float a, b;

        public Range(float a, float b) {
            this.a = a;
            this.b = b;
        }

        public float A {
            get {
                return a;
            }
        }

        public float B {
            get {
                return b;
            }
        }

        public float Min {
            get {
                return a > b ? b : a;
            }
        }

        public float Max {
            get {
                return a > b ? a : b;
            }
        }

        public float Delta {
            get {
                return b - a;
            }
        }

        public float Lerp(float v) {
            if(v <= 0) {
                return a;
            } else if(v >= 1) {
                return b;
            } else {
                return LerpUnclamped(v);
            }
        }

        public float LerpUnclamped(float v) {
            return a + (b - a) * v;
        }

        public float InverseLerp(float v) {
            if(Mathf.Approximately(a, b)) {
                return 1;
            } else {
                return (v - a) / (b - a);
            }
        }

        public float Clamp(float v) {
            if(v > Max) {
                return Max;
            } else if(v < Min) {
                return Min;
            } else {
                return v;
            }
        }

        public float Random(System.Random r) {
            return Lerp((float)r.NextDouble());
        }

        public int RandomInt(System.Random r) {
            int intMin = Mathf.RoundToInt(Min);
            int intMax = Mathf.RoundToInt(Max);
            return r.Next(intMin, intMax + 1);
        }

        public bool Equals(Range o) {
            return a == o.a && b == o.b;
        }

        public override bool Equals(object obj) {
            if(obj is Range) {
                return Equals((Range)obj);
            } else {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode() {
            return a.GetHashCode() ^ b.GetHashCode();
        }

        public string ToString(string format) {
            return string.Format("({0} to {1})", a.ToString(format), b.ToString(format));
        }

        public override string ToString() {
            return ToString("N2");
        }

        public static bool operator ==(Range a, Range b) {
            return a.Equals(b);
        }

        public static bool operator !=(Range a, Range b) {
            return !a.Equals(b);
        }

        public static Range operator +(Range a, Range b) {
            return new Range(a.a + b.a, a.b + b.b);
        }

        public static Range operator -(Range a, Range b) {
            return new Range(a.a - b.a, a.b - b.b);
        }

        public static Range operator -(Range a) {
            return new Range(-a.a, -a.b);
        }

        public static Range operator *(Range a, float s) {
            return new Range(a.a * s, a.b * s);
        }

        public static Range operator /(Range a, float s) {
            return new Range(a.a / s, a.b / s);
        }
    }
}
