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
    public class AssetLoader {

        public enum State {
            Loading, Success, Error, 
        }
        public enum ErrorType {
            None = 0, NullAssetName, BundleError, NullAsset, NotContained
        }

        private string assetName;
        private string tocName;
        private AssetBundleLoader bundleLoader;
        private State state;
        private ErrorType errorType;
        private UnityEngine.Object asset;
        private bool disposed;

        public State CurrentState {
            get {
                return state;
            }
        }

        public UnityEngine.Object Asset {
            get {
                return asset;
            }
        }

        public ErrorType Error {
            get {
                return errorType;
            }
        }

        public AssetBundleLoader BundleLoader {
            get {
                return bundleLoader;
            }
        }

        public AssetLoader(AssetBundleLoader bundle, string assetName, string tocName) {
            this.tocName = tocName;
            this.bundleLoader = bundle;
            if(string.IsNullOrEmpty(assetName)) {
                SetError(ErrorType.NullAssetName);
            } else {
                this.assetName = assetName;
                this.state = State.Loading;
                Baluga3.UnityCore.Baluga3Object.Instance.StartCoroutine(LoadCoroutine());
            }
        }

        public void Dispose(bool disposeAsset) {
            disposed = true;
            if(disposeAsset && asset != null) {
                UnityEngine.Object.Destroy(asset);
            }
        }

        private void SetError(ErrorType type) {
            this.state = State.Error;
            this.errorType = type;
        }

        private IEnumerator LoadCoroutine() {
            while(bundleLoader.CurrentState == AssetBundleLoader.State.Loading) {
                yield return 1;
            }

            if(bundleLoader.CurrentState == AssetBundleLoader.State.Error) {
                SetError(ErrorType.BundleError);
            } else {
                if(bundleLoader.Bundle.Contains(assetName)) {
                    AssetBundleRequest request = bundleLoader.Bundle.LoadAssetAsync(assetName);
                
                    while(!request.isDone) {
                        if(disposed) {
                            break;
                        }
                        yield return 1;
                    }

                    if(!disposed) {
                        asset = request.asset;
                        if(asset == null) {
                            SetError(ErrorType.NullAsset);
                        } else {
                            state = State.Success;
                        }
                    }
                } else {
                    SetError(ErrorType.NotContained);
                }
            }

            Debug.Log(string.Format("Asset \"{0}\" = {2}, finished loading with {1}-{3}", tocName, state, asset, errorType));
        }
    }
}
