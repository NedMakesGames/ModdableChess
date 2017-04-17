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
    public static class Vector3Utils {

        public static bool ApproximatelyEqual(this Vector3 v, Vector3 other) {
            return Mathf.Approximately(v.x, other.x) && Mathf.Approximately(v.y, other.y) && Mathf.Approximately(v.z, other.z);
        }

        public static Vector3 MemberwiseMult(this Vector3 v, Vector3 other) {
            return new Vector3(v.x * other.x, v.y * other.y, v.z * other.z);
        }

    }
}
