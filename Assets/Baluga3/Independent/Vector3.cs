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
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Baluga3.Independent {
    [Serializable]
    public struct Vector3 : IEquatable<Vector3>, IXmlSerializable {
        public static readonly Vector3 Zero = new Vector3(0, 0, 0);
        public static readonly Vector3 Right = new Vector3(1, 0, 0);
        public static readonly Vector3 Up = new Vector3(0, 1, 0);
        public static readonly Vector3 Forward = new Vector3(0, 0, 1);
        public static readonly Vector3 Left = new Vector3(-1, 0, 0);
        public static readonly Vector3 Down = new Vector3(0, -1, 0);
        public static readonly Vector3 Backward = new Vector3(0, 0, -1);

        private float x, y, z;

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

        public float Z {
            get {
                return z;
            }
        }

        public Vector3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float SquaredMagnitude() {
            return x * x + y * y + z * z;
        }

        public bool IsUnit() {
            return BMath.ApproximatelyEqual(1, x * x + y * y + z * z);
        }

        public float Magnitude() {
            return (float)Math.Sqrt(SquaredMagnitude());
        }

        public Vector3 Normalize() {
            return this / Magnitude();
        }

        public static float Dot(Vector3 a, Vector3 b) {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vector3 Cross(Vector3 a, Vector3 b) {
            return new Vector3(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x); 
        }

        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta) {
            Vector3 a = target - current;
            float magnitude = a.Magnitude();
            if(magnitude <= maxDistanceDelta || BMath.ApproximatelyEqual(magnitude, 0f)) {
                return target;
            } else {
                return current + a / magnitude * maxDistanceDelta;
            }
        }

        public bool ApproximatelyEquals(Vector3 o) {
            return BMath.ApproximatelyEqual(X, o.X) && BMath.ApproximatelyEqual(Y, o.Y) && BMath.ApproximatelyEqual(Z, o.Z);
        }

        public bool Equals(Vector3 o) {
            return x == o.x && y == o.y && z == o.z;
        }

        public override bool Equals(object obj) {
            if(obj is Vector3) {
                return Equals((Vector3)obj);
            } else {
                return base.Equals(obj);
            }
        }

        public string ToString(string format) {
            return string.Format("({0}, {1}, {2})", x.ToString(format), y.ToString(format), z.ToString(format));
        }

        public override string ToString() {
            return ToString("N2");
        }

        public override int GetHashCode() {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }

        public XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader) {
            x = 0;
            y = 0;
            z = 0;
            bool success = false;
            string line = reader.ReadString();
            string[] numbers = line.Split(' ');
            if(numbers.Length >= 3) {
                try {
                    x = float.Parse(numbers[0]);
                    y = float.Parse(numbers[1]);
                    z = float.Parse(numbers[2]);
                    success = true;
                } catch (FormatException) {

                }
            }
            if(!success) {
                UnityEngine.Debug.LogError("Bad Vector3 XML " + line);
            }
            //UnityEngine.Debug.Log(this);
        }

        public void WriteXml(XmlWriter writer) {
            writer.WriteString(string.Format("{0} {1} {2}", x, y, z));
        }

        public static bool operator==(Vector3 a, Vector3 b) {
            return a.Equals(b);
        }

        public static bool operator!=(Vector3 a, Vector3 b) {
            return !a.Equals(b);
        }

        public static Vector3 operator+(Vector3 a, Vector3 b) {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b) {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3 operator -(Vector3 a) {
            return new Vector3(-a.x, -a.y, -a.z);
        }

        public static Vector3 operator *(Vector3 a, Vector3 b) {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 operator /(Vector3 a, Vector3 b) {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static Vector3 operator *(Vector3 a, float s) {
            return new Vector3(a.x * s, a.y * s, a.z * s);
        }

        public static Vector3 operator *(float s, Vector3 a) {
            return new Vector3(a.x * s, a.y * s, a.z * s);
        }

        public static Vector3 operator /(Vector3 a, float s) {
            return new Vector3(a.x / s, a.y / s, a.z / s);
        }
    }
}
