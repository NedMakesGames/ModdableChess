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

namespace ModdableChess.Logic.Client {
    class ClientLobbyInit {
        private ReadyCheckSender sender;
        private Message allReadyMessage;

        public ClientLobbyInit(ModdableChessGame game) {
            sender = new ReadyCheckSender(game, 5, (r) => SendMessage(r));

            NetworkTracker.ClientMsgCallbacks.lobbyInitAllReady = OnAllReady;

            game.Components.GetOrRegister<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation, LocalPlayerInformation.Create)
                .Connection.EnterStateMessenger.Subscribe(new SimpleListener<int>(OnConnectionChange));
            game.Components.GetOrRegister<Command<bool>>((int)ComponentKeys.LobbyInitSendReady, Command<bool>.Create).Handler = OnSendReadyCommand;
            allReadyMessage = game.Components.GetOrRegister<Message>((int)ComponentKeys.LobbyInitAllReadyReceived, Message.Create);
        }

        private void OnConnectionChange(int state) {
            if(state != (int)ClientConnectionState.Validated) {
                sender.Active = false;
            }
        }

        private void OnSendReadyCommand(bool ready) {
            sender.Active = ready;
        }

        private void OnAllReady() {
            allReadyMessage.Send();
        }

        private static void SendMessage(bool ready) {
            NetworkTracker.ClientSendLobbyInitReady(new NetworkPlayerReadyNotice() {
                ready = ready,
            });
        }
    }
}
