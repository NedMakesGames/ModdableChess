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

using Baluga3.GameFlowLogic;
using ModdableChess.Logic;
using ModdableChess.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ModdableChess.Mods {
    [Serializable]
    public class LocalModInfo {
        public string filePath;
        public string modName;
        public string displayName;
        public string displayVersion;
        public int numericVersion;
        public string author;
    }

    public class ShallowModLoader {

        private Query<TOCLoadResult, string> tocLoader;
        private AutoController scene;

        public ShallowModLoader(AutoController scene) {
            this.scene = scene;
            scene.Components.Register((int)ComponentKeys.SearchModFolderRequest, new Query<List<LocalModInfo>, string>(SearchForMods));
        }

        public List<LocalModInfo> SearchForMods(string parentFolder) {
            if(tocLoader == null) {
                tocLoader = scene.Game.Components.Get<Query<TOCLoadResult, string>>((int)ComponentKeys.TableOfContentsLoadRequest);
            }

            List<LocalModInfo> mods = new List<LocalModInfo>();

            if(Directory.Exists(parentFolder)) {
                foreach(var folder in Directory.GetDirectories(parentFolder)) {
                    string filePath = Path.Combine(parentFolder, folder);
                    TOCLoadResult tocResult = tocLoader.Send(filePath);
                    if(tocResult.error == TOCLoadError.None) {
                        TableOfContents toc = tocResult.toc;
                        LocalModInfo info = new LocalModInfo() {
                            filePath = filePath,
                            modName = toc.name,
                            displayName = toc.displayName,
                            displayVersion = toc.displayVersion,
                            numericVersion = toc.version,
                            author = toc.author,
                        };
                        mods.Add(info);
                    } else {
                        UnityEngine.Debug.Log(string.Format("Unable to load mod TOC at {0}: {1}", folder, tocResult.error));
                    }
                }
            } else {
                UnityEngine.Debug.Log("Mod folder does not exist");
            }

            return mods;
        }
    }
}
