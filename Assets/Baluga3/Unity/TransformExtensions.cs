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

using UnityEngine;
using System.Collections;

namespace Baluga3.UnityCore {
    public static class TransformExtensions {
        public static void Set2DPosition(this Transform t, Vector2 pos) {
            Vector3 p = t.position;
            t.position = new Vector3(pos.x, pos.y, p.z);
        }

        public static void SetLocal2DPosition(this Transform t, Vector2 pos) {
            Vector3 p = t.localPosition;
            t.localPosition = new Vector3(pos.x, pos.y, p.z);
        }

        public static void SetX(this Transform t, float x) {
            Vector3 p = t.position;
            t.position = new Vector3(x, p.y, p.z);
        }

        public static void SetLocalX(this Transform t, float x) {
            Vector3 p = t.localPosition;
            t.localPosition = new Vector3(x, p.y, p.z);
        }

        public static void SetY(this Transform t, float y) {
            Vector3 p = t.position;
            t.position = new Vector3(p.x, y, p.z);
        }

        public static void SetLocalY(this Transform t, float y) {
            Vector3 p = t.localPosition;
            t.localPosition = new Vector3(p.x, y, p.z);
        }

        public static void SetZ(this Transform t, float z) {
            Vector3 p = t.position;
            t.position = new Vector3(p.x, p.y, z);
        }

        public static void SetLocalZ(this Transform t, float z) {
            Vector3 p = t.localPosition;
            t.localPosition = new Vector3(p.x, p.y, z);
        }

        public static void Set2DRotation(this Transform t, float r) {
            t.rotation = Quaternion.Euler(0, 0, r);
        }

        public static void SetLocal2DRotation(this Transform t, float r) {
            t.localRotation = Quaternion.Euler(0, 0, r);
        }

        public static void Set2DRotation(this Transform t, Vector2 v) {
            t.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);
        }

        public static void SetLocal2DRotation(this Transform t, Vector2 v) {
            t.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);
        }

        public static void Move2D(this Transform t, Vector2 v) {
            t.SetLocal2DPosition((Vector2)t.localPosition + v);
        }
    }
}
