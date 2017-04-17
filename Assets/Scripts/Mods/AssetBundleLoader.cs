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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModdableChess.Mods {
    public class AssetBundleLoader {

        public enum State {
            Loading, Success, Error
        }
        public enum ErrorType {
            None = 0, NullFilePath, MissingFile, WWWError
        }

        private string name;
        private string path;
        private AssetBundle bundle;
        private State state;
        private ErrorType errorType;
        private string wwwError;
        private bool disposed;

        public AssetBundle Bundle {
            get {
                return bundle;
            }
        }

        public State CurrentState {
            get {
                return state;
            }
        }

        public ErrorType Error {
            get {
                return errorType;
            }
        }

        public string WWWError {
            get {
                return wwwError;
            }
        }

        public AssetBundleLoader(string bundleName, string bundlePath) {
            this.name = bundleName;
            if(string.IsNullOrEmpty(bundlePath)) {
                SetError(ErrorType.NullFilePath);
            } else { 
                this.path = bundlePath;
                this.state = State.Loading;
                Baluga3.UnityCore.Baluga3Object.Instance.StartCoroutine(LoadCoroutine());
            }
        }

        public void Dispose() {
            disposed = true;
            if(bundle != null) {
                bundle.Unload(false);
            }
        }

        private void SetError(ErrorType type) {
            this.state = State.Error;
            this.errorType = type;
        }

        private IEnumerator LoadCoroutine() {
            if(System.IO.File.Exists(path)) {

                WWW www = new WWW(@"file://" + path);
                while(!www.isDone) {
                    if(disposed) { 
                        break;
                    }
                    yield return 1;
                }
                if(!disposed) {
                    if(!string.IsNullOrEmpty(www.error)) {
                        SetError(ErrorType.WWWError);
                        wwwError = www.error;
                    } else {
                        bundle = www.assetBundle;
                        state = State.Success;
                    }
                }
                www.Dispose();
            } else {
                SetError(ErrorType.MissingFile);
            }

            Debug.Log(string.Format("Asset bundle {0} finished loading with {1}-{2}-{3}", name, state, errorType, wwwError));
        }
    }
}
