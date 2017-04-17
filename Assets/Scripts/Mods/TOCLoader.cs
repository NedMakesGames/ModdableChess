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
using System.Xml.Serialization;
using System.IO;
using Baluga3.GameFlowLogic;
using ModdableChess.Unity;
using ModdableChess.Logic;
using System.Xml;

namespace ModdableChess.Mods {

    public enum TOCLoadError {
        None = 0, MissingFolder, MissingTOCFile, XMLException, IOException, NoName,
    }

    public class TOCLoadResult {
        public TableOfContents toc;
        public TOCLoadError error;
    }

    public class TOCLoader : MonoBehaviour {

        private ComponentJanitor cleanup;
        private Query<TOCLoadResult, string> func;

        private void Start() {
            func = new Query<TOCLoadResult, string>(LoadTOC);
            cleanup = ComponentJanitor.NewAndActivate(func, GameLink.Game.Components, (int)ComponentKeys.TableOfContentsLoadRequest);
        }

        private void OnDisable() {
            cleanup.Dispose();
        }

        public TOCLoadResult LoadTOC(string folder) {
            TOCLoadResult result = new TOCLoadResult();
            if(!Directory.Exists(folder)) {
                result.error = TOCLoadError.MissingFolder;
                return result;
            }
            string finalFolder = Path.GetFileName(folder);
            string filename = Path.Combine(folder, finalFolder) + ".toc";
            if(!File.Exists(filename)) {
                result.error = TOCLoadError.MissingTOCFile;
                return result;
            }
            try {
                XmlSerializer xml = new XmlSerializer(typeof(TableOfContents));
                using(BufferedStream s = new BufferedStream(new FileStream(filename, FileMode.Open, FileAccess.Read))) {
                    result.toc = (TableOfContents)xml.Deserialize(s);
                }
            } catch (XmlException) {
                result.error = TOCLoadError.XMLException;
                return result;
            } catch (IOException) {
                result.error = TOCLoadError.IOException;
                return result;
            }

            if(string.IsNullOrEmpty(result.toc.displayName)) {
                result.toc.displayName = result.toc.name;
            }
            if(string.IsNullOrEmpty(result.toc.displayVersion)) {
                result.toc.displayVersion = result.toc.version.ToString();
            }
            if(string.IsNullOrEmpty(result.toc.author)) {
                result.toc.author = "Anonymous";
            }

            if(string.IsNullOrEmpty(result.toc.name)) {
                result.error = TOCLoadError.NoName;
            } 

            return result;
        }
    }
}
