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
using Baluga3.Core;
using UnityEngine;

namespace Baluga3.Independent {
    public static class Convert {
        public static Vector2 Vector2(UnityEngine.Vector2 u) {
            return new Vector2(u.x, u.y);
        }

        public static UnityEngine.Vector2 Vector2(Vector2 u) {
            return new UnityEngine.Vector2(u.X, u.Y);
        }

        public static Vector2 Vector2(UnityEngine.Vector3 u) {
            return new Vector2(u.x, u.y);
        }

        public static UnityEngine.Vector3 Vector3(Vector3 b) {
            return new UnityEngine.Vector3(b.X, b.Y, b.Z);
        }

        public static UnityEngine.Quaternion Quaternion(Quaternion b) {
            return new UnityEngine.Quaternion(b.I, b.J, b.K, b.Real);
        }

        public static Core.Range Range(UnityEngine.Vector2 r) {
            return new Core.Range(r.x, r.y);
        }
    }
}
