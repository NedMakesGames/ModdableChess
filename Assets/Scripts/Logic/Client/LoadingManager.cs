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
    class LoadingManager {

        private Message<NetworkGameState> onGameStateReceived;
        private Message onErrorReceived;
        private Message onExitReceived;
        private ReadyCheckSender gameStateRefresher;
        private ReadyCheckSender readySender;

        public LoadingManager(ModdableChessGame game) {

            game.Components.GetOrRegister<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation, LocalPlayerInformation.Create)
                .Connection.EnterStateMessenger.Subscribe(new SimpleListener<int>(OnConnectionChange));

            gameStateRefresher = new ReadyCheckSender(game, 5, (r) => SendGameStateRefresh(r));
            gameStateRefresher.InitialWaitPeriod = gameStateRefresher.ResendPeriod;
            onGameStateReceived = game.Components.GetOrRegister<Message<NetworkGameState>>
                ((int)ComponentKeys.LoadingGameStateReceived, Message<NetworkGameState>.Create);
            game.Components.GetOrRegister<Command<NetworkGameState>>
                ((int)ComponentKeys.LoadingGameStateSend, Command<NetworkGameState>.Create).Handler = SendGameState;
            game.Components.GetOrRegister<Command>
                ((int)ComponentKeys.LoadingGameStateStopRefreshing, Command.Create).Handler = StopRefreshGameState;

            onErrorReceived = game.Components.GetOrRegister<Message>
                ((int)ComponentKeys.LoadingErrorReceived, Message.Create);
            game.Components.GetOrRegister<Command>
                ((int)ComponentKeys.LoadingErrorSend, Command.Create).Handler = SendError;

            onExitReceived = game.Components.GetOrRegister<Message>
                ((int)ComponentKeys.LoadingExitReceived, Message.Create);
            readySender = new ReadyCheckSender(game, 5, (r) => SendReadyState(r));
            game.Components.GetOrRegister<Command<bool>>
                ((int)ComponentKeys.LoadingReadySend, Command<bool>.Create).Handler = ReadyStateChange;

            NetworkTracker.ClientMsgCallbacks.loadingGameStateReply = OnReceivedGameState;
            NetworkTracker.ClientMsgCallbacks.loadingErrorRelay = OnReceivedError;
            NetworkTracker.ClientMsgCallbacks.loadingExitReceived = OnReceivedExit;

        }

        private void OnConnectionChange(int state) {
            if(state != (int)ClientConnectionState.Validated) {
                readySender.Active = false;
                gameStateRefresher.Active = false;
            }
        }

        private void SendGameState(NetworkGameState state) {
            bool hasState = state != null;
            if(state == null) {
                state = new NetworkGameState() {
                    pieces = new NetworkGameStatePiece[0],
                };
            }
            NetworkTracker.ClientSendLoadingGameState(new LoadingGameStateMessage() {
                hasState = hasState,
                gameState = state,
            });
            gameStateRefresher.Active = true;
        }

        private void SendGameStateRefresh(bool ready) {
            NetworkTracker.ClientSendLoadingGameStateRefresh(new NetworkPlayerReadyNotice() {
                ready = ready,
            });
        }

        private void StopRefreshGameState() {
            gameStateRefresher.Active = false;
        }

        private void SendError() {
            NetworkTracker.ClientSendLoadingError();
        }

        private void SendReadyState(bool ready) {
            NetworkTracker.ClientSendLoadingReadyState(new NetworkPlayerReadyNotice() {
                ready = ready,
            });
        }

        private void ReadyStateChange(bool ready) {
            readySender.Active = ready;
        }

        private void OnReceivedGameState(LoadingGameStateMessage msg) {
            onGameStateReceived.Send(msg.hasState ? msg.gameState : null);
        }

        private void OnReceivedError() {
            onErrorReceived.Send();
        }

        private void OnReceivedExit() {
            onExitReceived.Send();
        }
    }
}
