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
using System.Linq;
using System.Text;

namespace ModdableChess.Logic.Server {
    class LobbyInitReadyCheck {
        public const float TIMEOUT_PERIOD = 10;

        private ServerReadyCheckManager readyCheck;
        private Message onLobbyInitialized; 

        public LobbyInitReadyCheck(ModdableChessGame game) {
            readyCheck = new ServerReadyCheckManager(game, TIMEOUT_PERIOD);

            onLobbyInitialized = game.Components.GetOrRegister<Message>((int)ComponentKeys.Server_LobbyInitialized, Message.Create);

            NetworkTracker.ServerMsgCallbacks.lobbyInitNotifyReady = OnReceivedNotice;
        }

        private void OnReceivedNotice(int connectionID, NetworkPlayerReadyNotice notice) {
            bool allReady = readyCheck.PlayerNotifiedReady(connectionID, notice.ready);
            if(allReady) {
                readyCheck.Clear();
                onLobbyInitialized.Send();
                NetworkTracker.ServerSendLobbyInitAllReady();
            }
        }
    }
}
