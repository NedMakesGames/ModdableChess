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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Baluga3.Independent {
    [Serializable]
    public struct Vector2 : IEquatable<Vector2>, IXmlSerializable {
        public static readonly Vector2 Zero = new Vector2(0, 0);

        private float x, y;

        public float X {
            get {
                return x;
            }
        }

        public float Y {
            get {
                return y;
            }
        }

        public Vector3 Vector3XY0 {
            get {
                return new Vector3(x, y, 0);
            }
        }

        public Vector3 Vector3X0Y {
            get {
                return new Vector3(x, 0, y);
            }
        }

        public Vector2(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public float SquaredMagnitude() {
            return x * x + y * y;
        }

        public float Magnitude() {
            return (float)Math.Sqrt(SquaredMagnitude());
        }

        public bool IsUnit() {
            return BMath.ApproximatelyEqual(1, x * x + y * y);
        }

        public Vector2 Normalize() {
            return this / Magnitude();
        }

        public static float Dot(Vector2 a, Vector2 b) {
            return a.x * b.x + a.y * b.y;
        }

        public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta) {
            Vector2 a = target - current;
            float magnitude = a.Magnitude();
            if(magnitude <= maxDistanceDelta || BMath.ApproximatelyEqual(magnitude, 0f)) {
                return target;
            } else {
                return current + a / magnitude * maxDistanceDelta;
            }
        }

        public bool ApproximatelyEqual(Vector2 o) {
            return BMath.ApproximatelyEqual(x, o.x) && BMath.ApproximatelyEqual(y, o.y);
        }

        public bool Equals(Vector2 o) {
            return x == o.x && y == o.y;
        }

        public override bool Equals(object obj) {
            if(obj is Vector2) {
                return Equals((Vector2)obj);
            } else {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode() {
            return x.GetHashCode() ^ y.GetHashCode();
        }

        public string ToString(string format) {
            return string.Format("({0}, {1})", x.ToString(format), y.ToString(format));
        }

        public override string ToString() {
            return ToString("N2");
        }

        public XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader) {
            x = 0;
            y = 0;
            bool success = false;
            string line = reader.ReadString();
            string[] numbers = line.Split(' ');
            if(numbers.Length >= 2) {
                try {
                    x = float.Parse(numbers[0]);
                    y = float.Parse(numbers[1]);
                    success = true;
                } catch(FormatException) {

                }
            }
            if(!success) {
                UnityEngine.Debug.LogError("Bad Vector2 XML " + line);
            }
            UnityEngine.Debug.Log(this);
        }

        public void WriteXml(XmlWriter writer) {
            writer.WriteString(string.Format("{0} {1}", x, y));
        }

        public static bool operator ==(Vector2 a, Vector2 b) {
            return a.Equals(b);
        }

        public static bool operator !=(Vector2 a, Vector2 b) {
            return !a.Equals(b);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b) {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator -(Vector2 a) {
            return new Vector2(-a.x, -a.y);
        }

        public static Vector2 operator *(Vector2 a, Vector2 b) {
            return new Vector2(a.x * b.x, a.y * b.y);
        }

        public static Vector2 operator /(Vector2 a, Vector2 b) {
            return new Vector2(a.x / b.x, a.y / b.y);
        }

        public static Vector2 operator *(Vector2 a, float s) {
            return new Vector2(a.x * s, a.y * s);
        }

        public static Vector2 operator /(Vector2 a, float s) {
            return new Vector2(a.x / s, a.y / s);
        }
    }
}
