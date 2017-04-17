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

using ModdableChess.Logic;
using ModdableChess.Mods.Lua;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModdableChess.Mods {
    public class ScriptController {

        private Script currentEnvironment;

        public ScriptController(ModdableChessGame game) {
            game.Components.Register((int)ComponentKeys.LuaScripts, this);
            UserData.RegisterAssembly();

        }

        public void StartScriptEnvironment() {
            currentEnvironment = new Script(CoreModules.Preset_SoftSandbox);
            currentEnvironment.Options.DebugPrint = (s) => Debug.Log(string.Format("{0}: Lua print: {1}", Time.frameCount, s));
            currentEnvironment.Globals.Set("helper", UserData.Create(new Helper()));
            //currentEnvironment.Globals.Set("actionHelper", UserData.Create(new TurnActionHelper()));
            //currentEnvironment.Globals.Set("winCheckHelper", UserData.Create(new WinLossHelper()));
        }

        public DynValue ReadFile(string file) {
            using(BufferedStream s = new BufferedStream(new FileStream(file, FileMode.Open, FileAccess.Read))) {
                return currentEnvironment.DoStream(s);
            }
        }

        public DynValue GetGlobal(string name) {
            return currentEnvironment.Globals.Get(name);
        }

        public void Clear() {
            currentEnvironment = null;
        }

        public void AddGlobal(string name, DynValue data) {
            currentEnvironment.Globals.Set(name, data);
        }

        public void RemoveGlobal(string name) {
            currentEnvironment.Globals.Remove(name);
        }

        public DynValue CallFunction(Closure func, object arg) {
            ScriptRuntimeException ex;
            DynValue ret = CallFunction(func, arg, out ex);
            if(ex != null) {
                WriteToAuthorLog(string.Format("Lua runtime exception: {0}", ex.DecoratedMessage));
            }
            return ret;
        }

        public DynValue CallFunction(Closure func, object arg, out ScriptRuntimeException errorThrown) {
            DynValue ret;
            errorThrown = null;
            try {
                ret = func.Call(arg);
            } catch(ScriptRuntimeException ex) {
                UnityEngine.Debug.Log(string.Format("{0}: Lua exception\n{1}", UnityEngine.Time.frameCount, ex.DecoratedMessage));
                errorThrown = ex;
                ret = DynValue.NewNil();
            }
            return ret;
        }

        public void WriteToAuthorLog(string message) {
            using(StreamWriter writer = new StreamWriter(new FileStream(ModStrings.GetModErrorLogFilePath(), FileMode.Append, FileAccess.Write))) {
                writer.WriteLine(message);
            }
        }
    }
}
