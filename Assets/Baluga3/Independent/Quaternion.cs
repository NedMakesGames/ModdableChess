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

namespace Baluga3.Independent {
    public struct Quaternion : IEquatable<Quaternion> {

        public static readonly Quaternion Identity = new Quaternion(1, 0, 0, 0);

        private float real, i, j, k;

        public float I {
            get {
                return i;
            }
        }

        public float J {
            get {
                return j;
            }
        }

        public float K {
            get {
                return k;
            }
        }

        public float Real {
            get {
                return real;
            }
        }

        public Vector3 Vector {
            get {
                return new Vector3(i, j, k);
            }
        }

        public Quaternion(float real, float i, float j, float k) {
            this.real = real;
            this.i = i;
            this.j = j;
            this.k = k;
        }

        public Quaternion(float real, Vector3 v) {
            this.real = real;
            this.i = v.X;
            this.j = v.Y;
            this.k = v.Z;
        }

        public static Quaternion FromEuler(float zAngle, float yAngle, float xAngle) {
            float cosZ = (float)Math.Cos(0.5f * zAngle);
            float cosY = (float)Math.Cos(0.5f * yAngle);
            float cosX = (float)Math.Cos(0.5f * xAngle);

            float sinZ = (float)Math.Sin(0.5f * zAngle);
            float sinY = (float)Math.Sin(0.5f * yAngle);
            float sinX = (float)Math.Sin(0.5f * xAngle);

            return new Quaternion(
                cosZ * cosY * cosX + sinZ * sinY * sinX,
                cosZ * cosY * sinX - sinZ * sinY * cosX,
                cosZ * sinY * cosX + sinZ * cosY * sinX,
                sinZ * cosY * cosX - cosZ * sinY * sinX);
        }

        public Quaternion Conjugate() {
            return new Quaternion(real, -i, -j, -k);
        }

        public float NormSquared() {
            return real * real + i * i + j * j + k * k;
        }

        public float Norm() {
            return (float)Math.Sqrt(NormSquared());
        }

        public bool IsUnit() { 
            return BMath.ApproximatelyEqual(1, real * real + i * i + j * j + k * k);
        }

        public Quaternion Invert() {
            if(IsUnit()) {
                return Conjugate();
            } else {
                return Conjugate() / NormSquared();
            }
        }

        public Quaternion Normalize() {
            float n = Norm();
            return new Quaternion(real / n, i / n, j / n, k / n);
        }

        public static Quaternion Lerp(Quaternion from, Quaternion to, float t) {
            BalugaDebug.Assert(from.IsUnit());
            BalugaDebug.Assert(to.IsUnit());
            return (from * (1 - t) + to * t).Normalize();
        }

        public static float Dot(Quaternion a, Quaternion b) {
            return a.Real * b.Real + Vector3.Dot(a.Vector, b.Vector);
        }

        public static Quaternion Slerp(Quaternion from, Quaternion to, float t) {
            BalugaDebug.Assert(from.IsUnit());
            BalugaDebug.Assert(to.IsUnit());
            float dot = Quaternion.Dot(from, to);
            /*	dot = cos(theta)
			if (dot < 0), q1 and q2 are more than 90 degrees apart,
			so we can invert one to reduce spinning	*/
            if(dot < 0) {
                dot = -dot;
                to = -to;
            }

            if(dot < 0.95f) {
                float angle = (float)Math.Acos(dot);
                return SlerpNoChecks(from, to, t, angle);
            } else { // if the angle is small, use linear interpolation								
                return Quaternion.Lerp(from, to, t);
            }
        }

        private static Quaternion SlerpNoChecks(Quaternion from, Quaternion to, float t, float angle) {
            return ((from * (float)Math.Sin(angle * (1 - t)) + to * (float)Math.Sin(angle * t)) / (float)Math.Sin(angle)).Normalize();
        }

        public Vector3 Rotate(Vector3 v) {
            BalugaDebug.Assert(IsUnit());
            //https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Vector_Rotation
            Vector3 qv = Vector;
            Vector3 t = 2 * Vector3.Cross(qv, v);
            /*UnityEngine.Debug.Log(qv);
            UnityEngine.Debug.Log(t);
            UnityEngine.Debug.Log(v);
            UnityEngine.Debug.Log(q.real * t);
            UnityEngine.Debug.Log(Vector3.Cross(qv, t));*/
            return v + real * t + Vector3.Cross(qv, t);
            /*Quaternion p = new Quaternion(0, v);
            return (q * (p * q.Conjugate())).Vector;*/
        }

        public static Quaternion RotationOnAxis(Vector3 axis, float angle) {
            BalugaDebug.Assert(axis.IsUnit());
            return new Quaternion((float)Math.Cos(angle / 2f), axis * (float)Math.Sin(angle / 2f)); // Is normalized
        }

        public static Quaternion FromToRotation(Vector3 from, Vector3 to) {
            return Quaternion.FromToRotation(from, to, Vector3.Zero);
        }

        public static Quaternion FromToRotation(Vector3 from, Vector3 to, Vector3 fallbackAxis) {
            BalugaDebug.Assert(from.IsUnit());
            BalugaDebug.Assert(to.IsUnit());
            // from https://bitbucket.org/sinbad/ogre/src/9db75e3ba05c/OgreMain/include/OgreVector3.h?fileviewer=file-view-default#cl-651
            // Based on Stan Melax's article in Game Programming Gems

            float d = Vector3.Dot(from, to);
            // If dot == 1, vectors are the same
            if(d >= 1.0f) {
                return Quaternion.Identity;
            } else if(d < 1e-6f - 1.0f) {
                if(fallbackAxis.ApproximatelyEquals(Vector3.Zero)) {
                    // Generate an axis
                    fallbackAxis = Vector3.Cross(Vector3.Right, from);
                    if(fallbackAxis.ApproximatelyEquals(Vector3.Zero)) { // pick another if colinear
                        fallbackAxis = Vector3.Cross(Vector3.Up, from);
                    }
                }
                return Quaternion.RotationOnAxis(fallbackAxis, (float)Math.PI);
            } else {
                float s = (float)Math.Sqrt((1 + d) * 2);
                Vector3 c = Vector3.Cross(from, to);
                return new Quaternion(s * 0.5f, c / s).Normalize();
            }
        }

        public Quaternion FromToRotationForceUp(Vector3 from, Vector3 to, Vector3 up) {
            Quaternion rot1 = FromToRotation(from, to);

            // Because of the 1rst rotation, the up is probably completely screwed up.
            // Find the rotation between the "up" of the rotated object, and the desired up
            Vector3 newUp = rot1.Rotate(Vector3.Up);
            Quaternion rot2 = FromToRotation(newUp, up);

            return rot2 * rot1;
        }

        public Quaternion LookAtRotation(Vector3 direction, Vector3 up) {
            return FromToRotationForceUp(Vector3.Forward, direction, up);
        }

        public static Quaternion RotateTowards(Quaternion current, Quaternion target, float maxAngle) {
            BalugaDebug.Assert(current.IsUnit());
            BalugaDebug.Assert(target.IsUnit());

            if(maxAngle < 0.001f) {
                return current;
            }

            float cosTheta = Quaternion.Dot(current, target);

            // q1 and q2 are already equal.
            // Force q2 just to be sure
            if(cosTheta > 0.9999f) {
                return target;
            }

            // Avoid taking the long path around the sphere
            if(cosTheta < 0) {
                current = current * -1.0f;
                cosTheta *= -1.0f;
            }

            float angle = (float)Math.Acos(cosTheta);

            // If there is only a 2&deg; difference, and we are allowed 5&deg;,
            // then we arrived.
            if(angle <= maxAngle) {
                return target;
            }

            float fT = maxAngle / angle;
            return SlerpNoChecks(current, target, fT, maxAngle);
        }

        public string ToString(string format) {
            return string.Format("({0}, {1}, {2}, {3})", real.ToString(format), i.ToString(format), j.ToString(format), k.ToString(format));
        }

        public override string ToString() {
            return ToString("N2");
        }

        public bool Equals(Quaternion o) {
            return real == o.Real && i == o.I && j == o.J && k == o.K;
        }

        public override bool Equals(object obj) {
            if(obj is Quaternion) {
                return Equals((Quaternion)obj);
            } else {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode() {
            return real.GetHashCode() ^ i.GetHashCode() ^ j.GetHashCode() ^ k.GetHashCode();
        }

        public static Quaternion operator *(Quaternion q, float s) {
            return new Quaternion(q.real * s, q.i * s, q.j * s, q.k * s);
        }

        public static Quaternion operator *(float s, Quaternion q) {
            return new Quaternion(q.real * s, q.i * s, q.j * s, q.k * s);
        }

        public static Quaternion operator /(Quaternion q, float s) {
            return new Quaternion(q.real / s, q.i / s, q.j / s, q.k / s);
        }

        public static Quaternion operator +(Quaternion a, Quaternion b) {
            return new Quaternion(
                a.real + b.real,
                a.i + b.i,
                a.j + b.j,
                a.k + b.k);
        }

        public static Quaternion operator -(Quaternion a, Quaternion b) {
            return new Quaternion(
                a.real - b.real,
                a.i - b.i,
                a.j - b.j,
                a.k - b.k);
        }

        public static Quaternion operator -(Quaternion a) {
            return new Quaternion(
                -a.real,
                -a.i,
                -a.j,
                -a.k);
        }

        public static Quaternion operator *(Quaternion a, Quaternion b) {
            /*return new Quaternion(
                a.real * b.real - a.i * b.i - a.j * b.j - a.k * b.k,
                a.real * b.i + a.i * b.real + a.j * b.k - a.k * b.j,
                a.real * b.j - a.i * b.k + a.j * b.real + a.k * b.i,
                a.real * b.k + a.i * b.j - a.j * b.i + a.k * b.real
                );*/
            Vector3 av = a.Vector;
            Vector3 bv = b.Vector;
            return new Quaternion(
                a.real * b.real - Vector3.Dot(av, bv),
                a.real * bv + b.real * av + Vector3.Cross(av, bv));
        }

        
    }
}
