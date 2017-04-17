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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace ModdableChess.Mods {
    class XMLLoader {
        public enum State {
            Loading, Success, Error,
        }
        public enum ErrorType {
            None=0, NullFilePath, MissingFile, XMLException, IOException
        }

        private string assetName;
        private State state;
        private ErrorType errorType;
        private object loadedObject;
        private Type loadedType;

        public State CurrentState {
            get {
                return state;
            }
        }

        public object LoadedObject {
            get {
                return loadedObject;
            }
        }

        public ErrorType Error {
            get {
                return errorType;
            }
        }

        public XMLLoader(string modPath, string assetPath, Type loadedType) {
            if(string.IsNullOrEmpty(assetPath)) {
                SetError(ErrorType.NullFilePath);
            } else {
                this.assetName = Path.Combine(modPath, assetPath) + ".xml";
                this.state = State.Loading;
                this.loadedType = loadedType;
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
                XmlSerializer xml = new XmlSerializer(loadedType);
                loadedObject = null;
                try {
                    using(BufferedStream s = new BufferedStream(new FileStream(assetName, FileMode.Open, FileAccess.Read))) {
                        loadedObject = xml.Deserialize(s);
                        state = State.Success;
                    }
                } catch(XmlException) {
                    SetError(ErrorType.XMLException);
                } catch(IOException) {
                    SetError(ErrorType.IOException);
                }
            } else {
                SetError(ErrorType.MissingFile);
            }
            Debug.Log(string.Format("Loaded xml {0} with {1}-{2}", Path.GetFileNameWithoutExtension(assetName), state, errorType));
        }
    }
}
