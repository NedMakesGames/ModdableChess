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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic.Lobby {
    public enum LobbyScreen {
        None=0, PickHostOrClient, SetupHost, SetupClient, HostGamePrefs, ClientGamePrefs, Disposing,
        Rejoin
    }

    class LobbyScene : AutoController {

        private StateMachine screen;

        public LobbyScene(ModdableChessGame game) : base(game) {

            ComponentRegistry guiMessages = new ComponentRegistry();
            Components.Register((int)ComponentKeys.LobbyGUIMessages, guiMessages);

            screen = new StateMachine(0);
            Components.Register((int)ComponentKeys.LobbyScreen, screen);

            new HostOrClientScreenManager(this);
            new SetupHostScreenManager(this);
            new LobbyInitializedCheck(this);
            new TurnOrderSelectionController(this);
            new Mods.ShallowModLoader(this);
            new ModListController(this);
            new ReadyButtonController(this);
            new OtherPlayerReadyTextController(this);
            new LobbyExitController(this);
            new LobbyBackButton(this);
            new RejoinScreenController(this);

            ServerInformation server = Game.Components.Get<ServerInformation>((int)ComponentKeys.ServerInformation);
            server.Connection.EnterStateMessenger.Subscribe(new SimpleListener<int>(OnServerConnectionChange));
        }

        public override void Enter() {
            base.Enter();
            UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
        }

        private void OnServerConnectionChange(int state) {
            if(state == (int)ServerConnectionState.None) {
                LobbyChoices choices = Game.Components.Get<LobbyChoices>((int)ComponentKeys.LobbyChoices);
                choices.ModFolder = null;
                choices.OrderChoice = LobbyTurnOrderChoice.None;
            }
        }

        public override void Tick(float deltaTime) {
            if(screen.State == 0) {
                InitializeScreenState();
            }
        }

        private void InitializeScreenState() {
            ConnectionHelper netHelper = Game.Components.Get<ConnectionHelper>((int)ComponentKeys.ConnectionHelper);
            switch((ConnectionHelper.State)netHelper.Connection.State) {
            case ConnectionHelper.State.Stopped:
            case ConnectionHelper.State.Unconnected:
                netHelper.StopConnection();
                screen.State = (int)LobbyScreen.PickHostOrClient;
                break;
            case ConnectionHelper.State.Connected:
                netHelper.SetGameInProgress(false);
                screen.State = netHelper.CurrentMode == ConnectionHelper.Mode.Host ? (int)LobbyScreen.HostGamePrefs : (int)LobbyScreen.ClientGamePrefs;
                break;
            default:
                screen.State = (int)LobbyScreen.PickHostOrClient;
                break;
            }
        }
    }
}
