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
using ModdableChess.Logic.Client;
using ModdableChess.Logic.Lobby;
using ModdableChess.Logic.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic {
    public class ModdableChessGame : Game {

        public ComponentRegistry SceneComponents {
            get {
                return ((AutoController)base.Controller).Components;
            }
        }

        public ModdableChessGame() {
            this.Components.Register((int)ComponentKeys.GameDatabase, new GameDatabase());

            ServerInformation si = new ServerInformation();
            this.Components.Register((int)ComponentKeys.ServerInformation, si);
            AddTicking(si);

            LocalPlayerInformation lpi = new LocalPlayerInformation();
            this.Components.Register((int)ComponentKeys.LocalPlayerInformation, lpi);
            AddTicking(lpi);

            this.Components.Register((int)ComponentKeys.LobbyChoices, new LobbyChoices());

            new ServerController(this);
            new Server.LobbyInitReadyCheck(this);
            new Server.ServerLobbyMessages(this);
            new Server.LoadingManager(this);
            new Server.BoardController(this);

            new ClientController(this);
            new Client.ClientLobbyInit(this);
            new Client.LobbyMessages(this);
            new Client.LoadingManager(this);
            new Client.BoardController(this);

            new ConnectionHelper(this);
            new Mods.ScriptController(this);
        }

        public void LoadFirstScene() {
            this.Controller = new LobbyScene(this);
        }
    }
}
