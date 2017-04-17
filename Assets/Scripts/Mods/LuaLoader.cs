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

using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModdableChess.Mods {
    class LuaLoader {

        public enum State {
            Loading, Success, Error,
        }
        public enum ErrorType {
            None = 0, NullFilePath, MissingFile, IOException, SyntaxException, RuntimeException
        }

        private string assetName;
        private State state;
        private ErrorType errorType;
        private DynValue loadedObject;
        private ScriptController scripts;
        private HashSet<string> functions;
        private string luaError;

        public State CurrentState {
            get {
                return state;
            }
        }

        public DynValue LoadedObject {
            get {
                return loadedObject;
            }
        }

        public ErrorType Error {
            get {
                return errorType;
            }
        }

        public HashSet<string> Functions {
            get {
                return functions;
            }
        }

        public string LuaError {
            get {
                return luaError;
            }
        }

        public LuaLoader(ScriptController scripts, string modPath, string assetPath) {
            this.functions = new HashSet<string>();
            this.scripts = scripts;
            if(string.IsNullOrEmpty(assetPath)) {
                SetError(ErrorType.NullFilePath);
            } else {
                this.assetName = Path.Combine(modPath, assetPath) + ".lua";
                this.state = State.Loading;
                Load();
            }
        }

        public void Dispose() {
            loadedObject = null;
        }

        private void SetError(ErrorType type) {
            this.state = State.Error;
            this.errorType = type;
        }

        private void Load() {
            if(File.Exists(assetName)) {
                loadedObject = null;
                try {
                    loadedObject = scripts.ReadFile(assetName);
                    state = State.Success;
                } catch(SyntaxErrorException ex) {
                    SetError(ErrorType.SyntaxException);
                    luaError = ex.DecoratedMessage;
                } catch(ScriptRuntimeException ex) {
                    SetError(ErrorType.RuntimeException);
                    luaError = ex.DecoratedMessage;
                } catch(IOException) {
                    SetError(ErrorType.IOException);
                }
            } else {
                SetError(ErrorType.MissingFile);
            }
            Debug.Log(string.Format("Loaded lua {0} with {1}-{2}", Path.GetFileNameWithoutExtension(assetName), state, errorType));
        }
    }
}
