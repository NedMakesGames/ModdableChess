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
    public class Baluga3Object : MonoBehaviour {

        private static Baluga3Object instance;
        private static bool loaded;

        public static Baluga3Object Instance {
            get {
                Load();
                return instance;
            }
        }

        public static void Load() {
            if(!loaded) {
                GameObject obj = new GameObject("Baluga3");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<Baluga3Object>();
                loaded = true;
            }
        }

        public static Type GetOrAdd<Type>() where Type : MonoBehaviour {
            Load();
            if(instance != null) {
                Type c = instance.GetComponent<Type>();
                if(c != null) {
                    return c;
                } else {
                    return instance.gameObject.AddComponent<Type>();
                }
            } else {
                return null;
            }
        }
    }
}
