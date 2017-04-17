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
using UnityEngine;

namespace Baluga3.Core {
    [Serializable]
    public struct IntVector2 : IEquatable<IntVector2> {
        public static readonly IntVector2 Zero = new IntVector2(0, 0);

        public int x;
        public int y;

        public int X {
            get {
                return x;
            }
        }

        public int Y {
            get {
                return y;
            }
        }

        public Vector2 Vector2 {
            get {
                return new Vector2(x, y);
            }
        }

        public IntVector2(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public bool Equals(IntVector2 o) {
            return x == o.x && y == o.y;
        }

        public override bool Equals(object obj) {
            if(obj is IntVector2) {
                return Equals((IntVector2)obj);
            } else {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode() {
            int result = (int)(x ^ (x >> 32));
            result = 31 * result + (int)(y ^ (y >> 32));
            return result;
        }

        public string ToString(string format) {
            return string.Format("({0}, {1})", x.ToString(format), y.ToString(format));
        }

        public override string ToString() {
            return ToString("D2");
        }

        public static bool operator ==(IntVector2 a, IntVector2 b) {
            return a.Equals(b);
        }

        public static bool operator !=(IntVector2 a, IntVector2 b) {
            return !a.Equals(b);
        }

        public static IntVector2 operator +(IntVector2 a, IntVector2 b) {
            return new IntVector2(a.x + b.x, a.y + b.y);
        }

        public static IntVector2 operator -(IntVector2 a, IntVector2 b) {
            return new IntVector2(a.x - b.x, a.y - b.y);
        }

        public static IntVector2 operator -(IntVector2 a) {
            return new IntVector2(-a.x, -a.y);
        }

        public static IntVector2 operator *(IntVector2 a, IntVector2 b) {
            return new IntVector2(a.x * b.x, a.y * b.y);
        }

        public static IntVector2 operator /(IntVector2 a, IntVector2 b) {
            return new IntVector2(a.x / b.x, a.y / b.y);
        }

        public static IntVector2 operator *(IntVector2 a, int s) {
            return new IntVector2(a.x * s, a.y * s);
        }

        public static Vector2 operator *(IntVector2 a, float s) {
            return new Vector2(a.x * s, a.y * s);
        }

        public static IntVector2 operator /(IntVector2 a, int s) {
            return new IntVector2(a.x / s, a.y / s);
        }

        public static Vector2 operator /(IntVector2 a, float s) {
            return new Vector2(a.x / s, a.y / s);
        }
    }
}
