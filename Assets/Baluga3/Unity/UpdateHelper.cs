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
using System;
using Baluga3.Core;

namespace Baluga3.UnityCore {
    public interface IUpdating : System.IDisposable {
        void Update();
    }

    public class OneShotUpdate : IUpdating {
        private int onFrame;
        private Action callback;

        public OneShotUpdate(int f, Action c) {
            this.onFrame = f;
            this.callback = c;
        }

        public void Update() {
            if(onFrame <= Time.frameCount) {
                UpdateHelper.Unregister(this);
                callback();
            }
        }

        public void Dispose() {
            UpdateHelper.Unregister(this);
            callback = null;
        }
    }

    public class UpdateHelper : MonoBehaviour {

        private static UpdateHelper instance;
        public static void Load() {
            if(instance == null) {
                instance = Baluga3Object.GetOrAdd<UpdateHelper>();
            }
        }

        public static void Register(IUpdating t, bool autoDispose=false) {
            Load();
            instance.clients.Add(t);
            if(autoDispose) {
                SceneLoadDisposal.Add(t);
            }
        }

        public static void Unregister(IUpdating t) {
            Load();
            instance.clients.Remove(t);
        }

        public static OneShotUpdate RunNextUpdate(Action callback) {
            return RunInFrames(0, callback);
        }

        public static OneShotUpdate RunInFrames(int num, Action callback) {
            OneShotUpdate u = new OneShotUpdate(Time.frameCount + num, callback);
            Register(u);
            return u;
        }

        //public List<InputSubscriber> debugList;
        public RemovalSafeList<IUpdating> clients;

        void Awake() {
            DontDestroyOnLoad(gameObject);
            //debugList = new List<InputSubscriber>();
            clients = new RemovalSafeList<IUpdating>();
        }

        void Update() {
            //Debug.Log(subs.Count);
            foreach(var c in clients) {
                //Debug.Log(sub);
                c.Update();
            }
        }
    }
}
