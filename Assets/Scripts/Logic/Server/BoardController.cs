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
using ModdableChess.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic.Server {
    public class BoardController {

        private ServerInformation info;
        private ServerReadyCheckManager eotReadyCheck;

        public BoardController(ModdableChessGame game) {
            info = game.Components.GetOrRegister<ServerInformation>((int)ComponentKeys.ServerInformation, ServerInformation.Create);
            eotReadyCheck = new ServerReadyCheckManager(game, 10);
            eotReadyCheck.EnforceExtraData = true;

            NetworkTracker.ServerMsgCallbacks.boardActionReceived = OnActionReceived;
            NetworkTracker.ServerMsgCallbacks.boardEndOfTurnReceived = OnEoTReceived;
            NetworkTracker.ServerMsgCallbacks.boardEndOfTurnRefreshed = OnEoTRefreshed;
        }

        private void OnActionReceived(int connectionID, BoardActionMessage msg) {
            if(info.IsConnected(connectionID)) {
                NetworkTracker.ServerSendBoardAction(info.OtherConnectionID(connectionID), msg);
            }
        }

        private void OnEoTReceived(int connection, EndOfTurnStateMessage msg) {
            if(eotReadyCheck.PlayerNotifiedReady(connection, true, msg)) {
                EndOfTurnState hostEoT = eotReadyCheck.GetHostData<EndOfTurnStateMessage>().state;
                EndOfTurnState clientEoT = eotReadyCheck.GetClientData<EndOfTurnStateMessage>().state;
                eotReadyCheck.Clear();
                ServerEndOfTurnState serverResponse = ServerEndOfTurnState.Error;
                if(hostEoT == EndOfTurnState.Error || clientEoT == EndOfTurnState.Error) {
                    serverResponse = ServerEndOfTurnState.Error;
                } else if(hostEoT == EndOfTurnState.Forfeit && clientEoT == EndOfTurnState.Forfeit) {
                    serverResponse = ServerEndOfTurnState.Tie;
                } else if(hostEoT == EndOfTurnState.Forfeit) {
                    serverResponse = ServerEndOfTurnState.ClientWin;
                } else if(clientEoT == EndOfTurnState.Forfeit) {
                    serverResponse = ServerEndOfTurnState.HostWin;
                } else if(hostEoT == EndOfTurnState.Undecided && clientEoT == EndOfTurnState.Undecided) {
                    serverResponse = ServerEndOfTurnState.Undecided;
                } else if(hostEoT == EndOfTurnState.Tie && clientEoT == EndOfTurnState.Tie) {
                    serverResponse = ServerEndOfTurnState.Tie;
                } else if(hostEoT == EndOfTurnState.Win && clientEoT == EndOfTurnState.Loss) {
                    serverResponse = ServerEndOfTurnState.HostWin;
                } else if(hostEoT == EndOfTurnState.Loss && clientEoT == EndOfTurnState.Win) {
                    serverResponse = ServerEndOfTurnState.ClientWin;
                } else {
                    serverResponse = ServerEndOfTurnState.Mismatch;
                }
                NetworkTracker.ServerSendEoT(new ServerEndOfTurnStateMessage() {
                    state = serverResponse
                });
            }
        }

        private void OnEoTRefreshed(int connectionID, NetworkPlayerReadyNotice ready) {
            eotReadyCheck.PlayerNotifiedReady(connectionID, ready.ready);
        }
    }
}
