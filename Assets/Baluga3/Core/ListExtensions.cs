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

namespace Baluga3.Core {
    public static class ListExtension {
        public static void SafeSet<TValue>(this IList<TValue> list, int index, TValue val) {
            while(index >= list.Count) {
                list.Add(default(TValue));
            }
            list[index] = val;
        }

        public static TValue SafeGet<TValue>(this IList<TValue> list, int index) {
            if(index < list.Count) {
                return list[index];
            } else {
                return default(TValue);
            }
        }

        public static TValue RandomItem<TValue>(this IList<TValue> list, Random r) {
            return list[r.Next(list.Count)];
        }

        public static TValue GetAndRemoveAt<TValue>(this IList<TValue> list, int index) {
            TValue v = list[index];
            list.RemoveAt(index);
            return v;
        }

        public static TValue GetRandomAndRemove<TValue>(this IList<TValue> list, Random r) {
            return list.GetAndRemoveAt(r.Next(list.Count));
        }

        public static void Shuffle<TValue>(this IList<TValue> list, Random r) {
            for(var i = 0; i < list.Count; i++) {
                list.Swap(i, r.Next(i, list.Count));
            }
        }

        public static void Swap<TValue>(this IList<TValue> list, int i, int j) {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
