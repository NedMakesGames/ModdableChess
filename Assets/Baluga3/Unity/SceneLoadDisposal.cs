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
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

namespace Baluga3.UnityCore {
    public class SceneLoadDisposal : MonoBehaviour {

        private static SceneLoadDisposal instance;
        public static void Load() {
            if(instance == null) {
                instance = Baluga3Object.GetOrAdd<SceneLoadDisposal>();
            }
        }

        private Dictionary<Scene, List<IDisposable>> disposables;

        void Awake() {
            disposables = new Dictionary<Scene, List<IDisposable>>();
            SceneManager.sceneUnloaded += DisposeScene;
        }

        private static List<IDisposable> GetList() {
            Load();
            List<IDisposable> l;
            Scene scene = SceneManager.GetActiveScene();
            if(!instance.disposables.TryGetValue(scene, out l)) {
                l = new List<IDisposable>();
                instance.disposables[scene] = l;
            }
            return l;
        }

        public static void Add(IDisposable d) {
            GetList().Add(d);
        }

        public static void Remove(IDisposable d) {
            GetList().Remove(d);
        }

        private void DisposeScene(Scene scene) {
            List<IDisposable> l;
            if(disposables.TryGetValue(scene, out l)) {
                foreach(var d in l) {
                    d.Dispose();
                }
                l.Clear();
            }
        }
    }
}
