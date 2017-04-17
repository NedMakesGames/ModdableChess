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

namespace Baluga3.Independent {
    public static class BMath {
        public const float EPSILON = 0.001f;

        public static bool ApproximatelyEqual(float a, float b, float threshold=EPSILON) {
            return ((a < b) ? (b - a) : (a - b)) <= threshold;
        }

        public static int RoundToInt(float v) {
            return (int)Math.Round(v);
        }

        public static int Min(int a, int b) {
            return a < b ? a : b;
        }

        public static int Max(int a, int b) {
            return a > b ? a : b;
        }

        public static int Abs(int a) {
            return a < 0 ? -a : a;
        }

        public static float Min(float a, float b) {
            return a < b ? a : b;
        }

        public static float Max(float a, float b) {
            return a > b ? a : b;
        }

        public static float Abs(float a) {
            return a < 0 ? -a : a;
        }

        public static float Clamp(float value, float min, float max) {
            if(value > min) {
                return value < max ? value : max;
            } else {
                return min;
            }
        }

        public static float MoveTowards(float current, float target, float maxDelta) {
            float d = target - current;
            if(Abs(d) <= maxDelta) {
                return target;
            }
            return current + (d > 0 ? 1f : -1f) * maxDelta;
        }
    }
}
