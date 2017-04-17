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
using ModdableChess.Unity;
using Baluga3.GameFlowLogic;
using ModdableChess.Logic;
using System.Collections.Generic;
using System.IO;
using System;
using MoonSharp.Interpreter;

namespace ModdableChess.Mods {

    public class ModDeepLoader : MonoBehaviour {

        private class ModLoaderException : Exception {
            public ModLoaderException(string msg) : base(msg) {

            }
        }

        private enum State {
            Idle=0, Loading, Error
        }

        private Dictionary<string, AssetBundleLoader> bundleCache;
        private Dictionary<string, AssetLoader> assetCache;
        private Dictionary<string, XMLLoader> xmlCache;
        private Dictionary<string, LuaLoader> luaCache;
        private TableOfContents toc;
        private State state;
        private ScriptController scripts;

        private void Start() {
            assetCache = new Dictionary<string, AssetLoader>();
            bundleCache = new Dictionary<string, AssetBundleLoader>();
            xmlCache = new Dictionary<string, XMLLoader>();
            luaCache = new Dictionary<string, LuaLoader>();
            state = State.Idle;

            scripts = GameLink.Game.Components.Get<ScriptController>((int)ComponentKeys.LuaScripts);

            GameLink.Game.SceneComponents.GetOrRegister<Command<string>>((int)ComponentKeys.DeepLoadMod, Command<string>.Create).Handler = LoadMod;
        }

        private void OnDisable() {
            foreach(var loader in bundleCache.Values) {
                loader.Dispose();
            }
            foreach(var loader in assetCache.Values) {
                loader.Dispose(state == State.Error);
            }
            foreach(var loader in xmlCache.Values) {
                loader.Dispose();
            }
            foreach(var loader in luaCache.Values) {
                loader.Dispose();
            }
        }

        public void LoadMod(string folder) {
            try {
                scripts.StartScriptEnvironment();

                state = State.Loading;
                TOCLoadResult tocLoadResult = GameLink.Game.Components.Get<Query<TOCLoadResult, string>>((int)ComponentKeys.TableOfContentsLoadRequest).Send(folder);
                if(tocLoadResult.error != TOCLoadError.None) {
                    throw new ModLoaderException("Cannot load table of contents, error: " + tocLoadResult.error);
                }
                toc = tocLoadResult.toc;
                Debug.Log("Start loading " + toc.name);

                if(toc.extraLuaFiles != null) {
                    for(int i = 0; i < toc.extraLuaFiles.Length; i++) {
                        StartLoadLua(folder, toc.extraLuaFiles[i], string.Format("Extra lua file {0}", i));
                    }
                }

                StartLoadAsset(folder, toc.boardModel, "boardModel");
                StartLoadLua(folder, toc.boardSetupFunction, "boardSetupFunction");
                StartLoadLua(folder, toc.winLossFunction, "winLossFunction");

                if(toc.pieces == null || toc.pieces.Length == 0) {
                    throw new ModLoaderException("Mod requires at least one piece.");
                }

                for(int p = 0; p < toc.pieces.Length; p++) {
                    Piece piece = toc.pieces[p];
                    if(piece == null) {
                        throw new ModLoaderException("Null piece given at position " + p);
                    } else if(string.IsNullOrEmpty(piece.name)) {
                        throw new ModLoaderException("Piece not given a name at position " + p);
                    }
                    string baseExcName = string.Format("piece {0} ", piece.name);
                    StartLoadAsset(folder, piece.player1Model, baseExcName + "player1Model");
                    StartLoadAsset(folder, piece.player2Model, baseExcName + "player2Model");
                    if(!string.IsNullOrEmpty(piece.promoteIndicatorModel)) {
                        StartLoadAsset(folder, piece.promoteIndicatorModel, baseExcName + "player2Model");
                    }
                    StartLoadLua(folder, piece.actionOptionFunction, baseExcName + "actionOptionFunction");
                }

                if(toc.actionIndicators == null || toc.actionIndicators.Length == 0) {
                    throw new ModLoaderException("Mod requires action indicator models.");
                }

                for(int i = 0; i < toc.actionIndicators.Length; i++) {
                    ActionIndicator indicator = toc.actionIndicators[i];
                    if(indicator == null) {
                        throw new ModLoaderException("Null indicator given at position " + i);
                    } else if(string.IsNullOrEmpty(indicator.type)) {
                        throw new ModLoaderException("Indicator not given a type at position " + i);
                    }
                    string testName;
                    if(string.IsNullOrEmpty(indicator.strength)) {
                        testName = string.Format("indicator {0}", indicator.type);
                    } else {
                        testName = string.Format("indicator {0}-{1}", indicator.type, indicator.strength);
                    }
                    StartLoadAsset(folder, indicator.model, testName);
                }
            } catch (ModLoaderException mlx) {
                GameLink.Game.SceneComponents.Get<Message<string>>((int)ComponentKeys.ModLoadError).Send(mlx.Message);
            }
        }

        private void StartLoadAsset(string modPath, string assetPath, string exceptionName) {
            if(string.IsNullOrEmpty(assetPath)) {
                throw new ModLoaderException("No value given for expected asset: " + exceptionName);
            }

            int lastSlashIndex = assetPath.LastIndexOf('/');
            string assetBundleName = assetPath.Substring(0, lastSlashIndex);
            string assetName = assetPath.Substring(lastSlashIndex + 1, assetPath.Length - lastSlashIndex - 1);

            if(!assetCache.ContainsKey(assetPath)) {

                AssetBundleLoader bundleLoader;
                if(!bundleCache.TryGetValue(assetBundleName, out bundleLoader)) {
                    bundleLoader = new AssetBundleLoader(assetBundleName, Path.Combine(modPath, assetBundleName));
                    bundleCache[assetBundleName] = bundleLoader;
                    Debug.Log(string.Format("Loading bundle {0}", assetBundleName));
                }

                AssetLoader assetLoader = new AssetLoader(bundleLoader, assetName, assetPath);
                assetCache[assetPath] = assetLoader;

                Debug.Log(string.Format("Loading asset {0} in bundle {1}", assetName, assetBundleName));
            }
        }

        private void StartLoadXML(string modPath, string assetPath, System.Type assetType, string exceptionName) {
            if(string.IsNullOrEmpty(assetPath)) {
                throw new ModLoaderException("No value given for expected asset: " + exceptionName);
            }

            if(!xmlCache.ContainsKey(assetPath)) {
                XMLLoader pbLoader = new XMLLoader(modPath, assetPath, assetType);
                xmlCache[assetPath] = pbLoader;

                Debug.Log(string.Format("Loading XML {0}", assetPath));
            }
        }

        private void StartLoadLua(string modPath, string assetPath, string exceptionName) {
            if(string.IsNullOrEmpty(assetPath)) {
                throw new ModLoaderException("No value given for expected lua file: " + exceptionName);
            }

            int lastDotIndex = assetPath.LastIndexOf('.');
            string scriptName, functionName;
            if(lastDotIndex < 0) {
                scriptName = assetPath;
                functionName = ".";
            } else {
                scriptName = assetPath.Substring(0, lastDotIndex);
                functionName = assetPath.Substring(lastDotIndex + 1, assetPath.Length - lastDotIndex - 1);
            }

            LuaLoader loader;
            if(!luaCache.TryGetValue(scriptName, out loader)) {
                loader = new LuaLoader(scripts, modPath, scriptName);
                luaCache[scriptName] = loader;

                Debug.Log(string.Format("Loading lua {0}", scriptName));
            }
            loader.Functions.Add(functionName);
        }

        private void Update() {
            if(state == State.Loading) {
                bool allLoaded = true;
                foreach(var loader in assetCache.Values) {
                    if(loader.CurrentState == AssetLoader.State.Loading) {
                        allLoaded = false;
                        break;
                    }
                }
                foreach(var loader in xmlCache.Values) {
                    if(loader.CurrentState == XMLLoader.State.Loading) {
                        allLoaded = false;
                        break;
                    }
                }
                foreach(var loader in luaCache.Values) {
                    if(loader.CurrentState == LuaLoader.State.Loading) {
                        allLoaded = false;
                        break;
                    }
                }
                if(allLoaded) {
                    state = State.Idle;
                    try {
                        LoadedMod mod = CompileAssets();
                        GameLink.Game.SceneComponents.Get<ICallable<LoadedMod>>((int)ComponentKeys.ModAssetsLoaded).Send(mod);
                    } catch (ModLoaderException mlx) {
                        GameLink.Game.SceneComponents.Get<Message<string>>((int)ComponentKeys.ModLoadError).Send(mlx.Message);
                    }
                }
            }
        }

        private LoadedMod CompileAssets() {
            LoadedMod mod = new LoadedMod();
            mod.toc = toc;
            mod.assets = new Dictionary<string, object>();
            mod.xmlObjects = new Dictionary<string, object>();
            mod.luaFunctions = new Dictionary<string, Closure>();
            foreach(var cached in assetCache) {
                string assetName = cached.Key;
                AssetLoader loader = cached.Value;
                if(loader.CurrentState == AssetLoader.State.Success) {
                    mod.assets[assetName] = loader.Asset;
                } else {
                    if(loader.Error == AssetLoader.ErrorType.BundleError) {
                        if(loader.BundleLoader.Error == AssetBundleLoader.ErrorType.WWWError) {
                            throw new ModLoaderException(string.Format("Unable to load asset \"{0}\" due to error loading asset bundle: {1}", assetName, loader.BundleLoader.WWWError));
                        } else {
                            throw new ModLoaderException(string.Format("Unable to load asset \"{0}\" due to error loading asset bundle: {1}", assetName, loader.BundleLoader.Error));
                        }
                    } else {
                        throw new ModLoaderException(string.Format("Unable to load asset \"{0}\": {1}", assetName, loader.Error));
                    }
                }
            }
            foreach(var cached in xmlCache) {
                string assetName = cached.Key;
                XMLLoader loader = cached.Value;
                if(loader.CurrentState == XMLLoader.State.Success) {
                    mod.xmlObjects[assetName] = loader.LoadedObject;
                } else {
                    throw new ModLoaderException(string.Format("Unable to load XML file \"{0}\": {1}", assetName, loader.Error));
                }
            }
            foreach(var cached in luaCache) {
                string scriptName = cached.Key;
                LuaLoader loader = cached.Value;
                if(loader.CurrentState == LuaLoader.State.Success) {
                    foreach(var functionName in loader.Functions) {
                        DynValue dvalue;
                        string cachedName;
                        if(functionName == ".") {
                            cachedName = scriptName;
                            dvalue = loader.LoadedObject;
                        } else {
                            cachedName = string.Format("{0}.{1}", scriptName, functionName);
                            dvalue = scripts.GetGlobal(functionName);
                        }
                        if(dvalue.Type == DataType.Function) {
                            Debug.Log("Set function " + cachedName);
                            mod.luaFunctions[cachedName] = dvalue.Function;
                        } else {
                            throw new ModLoaderException(string.Format("Expected Lua function given {0}", cachedName));
                        }
                    }
                } else {
                    switch(loader.Error) {
                    case LuaLoader.ErrorType.RuntimeException:
                        throw new ModLoaderException(string.Format("Runtime exception in Lua file \"{0}\": {1}", scriptName, loader.LuaError));
                    case LuaLoader.ErrorType.SyntaxException:
                        throw new ModLoaderException(string.Format("Syntax exception in Lua file \"{0}\": {1}", scriptName, loader.LuaError));
                    default:
                        throw new ModLoaderException(string.Format("Unable to load Lua file \"{0}\": {1}", scriptName, loader.Error));
                    }
                }
            }

            return mod;
        }
    }
}
