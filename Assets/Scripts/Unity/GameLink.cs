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
using ModdableChess.Logic;
using Baluga3.UnityCore;
using Baluga3.GameFlowLogic;
using System.Collections.Generic;
using ModdableChess.Mods;
using ModdableChess.Logic.Server;

namespace ModdableChess.Unity {
    public class GameLink : MonoBehaviour {

        private static bool loaded;
        private static GameLink instance;

        [SerializeField]
        private GameDatabase db;
        [SerializeField]
        private ServerInformation server;
        [SerializeField]
        private LocalPlayerInformation local;
        [SerializeField]
        private LobbyChoices lobby;
        [SerializeField]
        private List<Logic.Lobby.ModListController.ComboModInfo> netMods;
        [SerializeField]
        private List<Mods.LocalModInfo> localMods;
        [SerializeField]
        private List<ModdableChess.Logic.Server.ServerLobbyMessages.Mod> serverHostMods;
        [SerializeField]
        private List<ModdableChess.Logic.Server.ServerLobbyMessages.Mod> serverClientMods;
        [SerializeField]
        private List<ModdableChess.Logic.Server.ServerLobbyMessages.Mod> serverComboMods;

        public static void Load() {
            if(!loaded) {
                instance = Baluga3Object.GetOrAdd<GameLink>();
                instance.CreateGame();
                loaded = true;
            }
        }

        public static ModdableChessGame Game {
            get {
                Load();
                return instance.game;
            }
        }

        public static List<Logic.Lobby.ModListController.ComboModInfo> NetMods {
            get {
                return instance.netMods;
            }

            set {
                instance.netMods = value;
            }
        }

        public static List<LocalModInfo> LocalMods {
            get {
                return instance.localMods;
            }

            set {
                instance.localMods = value;
            }
        }

        public static List<ServerLobbyMessages.Mod> ServerHostMods {
            get {
                return instance.serverHostMods;
            }

            set {
                instance.serverHostMods = value;
            }
        }

        public static List<ServerLobbyMessages.Mod> ServerClientMods {
            get {
                return instance.serverClientMods;
            }

            set {
                instance.serverClientMods = value;
            }
        }

        public static List<ServerLobbyMessages.Mod> ServerComboMods {
            get {
                return instance.serverComboMods;
            }

            set {
                instance.serverComboMods = value;
            }
        }

        private ModdableChessGame game;

        private void CreateGame() {
            game = new ModdableChessGame();
            game.LoadFirstScene();
            this.gameObject.AddComponent<Mods.TOCLoader>();

            db = game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            server = game.Components.Get<ServerInformation>((int)ComponentKeys.ServerInformation);
            local = game.Components.Get<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation);
            lobby = game.Components.Get<LobbyChoices>((int)ComponentKeys.LobbyChoices);
        }

        private void Update() {
            game.Tick(Time.deltaTime);
        }
    }
}
