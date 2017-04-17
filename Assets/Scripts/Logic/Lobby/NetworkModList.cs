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
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace ModdableChess.Logic.Lobby {

    public enum ModNetworkStatus {
        Unknown = 0, Playable, HostOutOfDate, ClientOutOfDate, OnlyHost, OnlyClient
    }

    public class ModListNotifyMessage : MessageBase {
        public ModListNotifyEntry[] entries;
    }

    public class ModListNotifyEntry {
        public string modName;
        public string displayName;
        public string displayVersion;
        public int numericVersion;
        public string author;
    }

    public class ModListServerMessage : MessageBase {
        public ModListServerEntry[] entries;
    }

    public class ModListServerEntry {
        public string modName;
        public string displayName;
        public string displayVersion;
        public int numericVersion;
        public string author;
        public ModNetworkStatus status;
    }

    public class NetworkPickedMod : MessageBase {
        public bool noneSelected;
        public string modName;
    }

    [Serializable]
    public class NetworkModInfo {
        public string modName;
        public string displayName;
        public string displayVersion;
        public int numericVersion;
        public string author;
        public ModNetworkStatus networkStatus;
    }
}
