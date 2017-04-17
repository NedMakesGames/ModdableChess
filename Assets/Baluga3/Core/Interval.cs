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
    public struct Interval : IEquatable<Interval> {
        public int start, count;

        public Interval(int start, int count) {
            this.start = start;
            this.count = count;
        }

        public static Interval FromFirstAndLast(int first, int last) {
            return new Interval(first, last - first);
        }

        public int Start {
            get {
                return start;
            }
        }

        public int End {
            get {
                return start + count - 1;
            }
        }

        public int Count {
            get {
                return count;
            }
        }

        public IEnumerator<int> GetEnumerator() {
            for(int i = start; i < count; i++) {
                yield return i;
            }
        }

        public bool Equals(Interval o) {
            return start == o.start && start == o.start;
        }

        public override bool Equals(object obj) {
            if(obj is Interval) {
                return Equals((Interval)obj);
            } else {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode() {
            int result = (int)(start ^ (start >> 32));
            result = 31 * result + (int)(count ^ (count >> 32));
            return result;
        }

        public string ToString(string format) {
            return string.Format("({0} to {1})", start.ToString(format), count.ToString(format));
        }

        public override string ToString() {
            return ToString("D2");
        }

        public static bool operator ==(Interval a, Interval b) {
            return a.Equals(b);
        }

        public static bool operator !=(Interval a, Interval b) {
            return !a.Equals(b);
        }
    }
}
