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

using Baluga3.Core;
using Baluga3.GameFlowLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic {
    public enum ReconnectionState {
        None=0,
        FirstCheck,
        Connected,
        Disconnected, // Currently not connected to other player, server/client is open
        TryRestartMyConnection, // Currently not connected and failed to open server/client, waiting to retry
        WaitingForServerReply, // Waiting for server to send game state
        SignaledReady, // I signaled I am ready to play
        Exiting,
    }

    public class ReconnectionManager : ITicking {

        private const float RECONNECT_FAIL_RETRY_PERIOD = 5;

        private StateMachine state;
        private AutoController scene;
        private ConnectionHelper connHelper;
        private float reconnectTimer;
        private Command<bool> sendReady;
        private Command<NetworkGameState> sendGameState;
        private Command stopRefreshGameState;
        private Query<NetworkGameState> compileGameState;
        private Query<Board, NetworkGameState> createBoard;
        private Message<Board> newBoardMessage;
        private TickingJanitor tickHelper;
        private Command exitToLobby;

        public ReconnectionManager(AutoController scene) {
            this.scene = scene;
            tickHelper = new TickingJanitor(scene.Game, this);
            scene.ActivatableList.Add(tickHelper);

            state = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.ReconnectionState, StateMachine.Create);
            state.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => UnityEngine.Debug.LogError(
                string.Format("{1}: Reconnection state: {0}", (ReconnectionState)s, scene.Game.TickCount))));

            connHelper = scene.Game.Components.Get<ConnectionHelper>((int)ComponentKeys.ConnectionHelper);
            sendGameState = scene.Game.Components.Get<Command<NetworkGameState>>((int)ComponentKeys.LoadingGameStateSend);
            stopRefreshGameState = scene.Game.Components.Get<Command>((int)ComponentKeys.LoadingGameStateStopRefreshing);
            sendReady = scene.Game.Components.Get<Command<bool>>((int)ComponentKeys.LoadingReadySend);
            newBoardMessage = scene.Components.GetOrRegister((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create);
            compileGameState = scene.Components.GetOrRegister<Query<NetworkGameState>>
                ((int)ComponentKeys.CompileCurrentGameState, Query<NetworkGameState>.Create);
            createBoard = scene.Components.GetOrRegister<Query<Board, NetworkGameState>>
                ((int)ComponentKeys.CreateBoardCommand, Query<Board, NetworkGameState>.Create);
            exitToLobby = scene.Components.GetOrRegister<Command>((int)ComponentKeys.EndScreenExitPressed, Command.Create);

            scene.ActivatableList.Add(new ListenerJanitor<IListener<NetworkGameState>>(
                scene.Game.Components.Get<IMessage<NetworkGameState>>((int)ComponentKeys.LoadingGameStateReceived),
                new SimpleListener<NetworkGameState>(OnReconnectionReplyReceived)));
            scene.ActivatableList.Add(new ListenerJanitor<IListener>(
                scene.Game.Components.Get<IMessage>((int)ComponentKeys.LoadingExitReceived),
                new SimpleListener(OnAllReadyReceived)));
            scene.ActivatableList.Add(new ListenerJanitor<IListener>(
                scene.Game.Components.Get<IMessage>((int)ComponentKeys.LoadingErrorReceived),
                new SimpleListener(OnErrorReceived)));

            GameDatabase db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            connHelper.SetGameInProgress(true);
            connHelper.WriteRejoinFile(db.ModFolder);
            scene.ActivatableList.Add(new ListenerJanitor<IListener<int>>(
                connHelper.Connection.EnterStateMessenger,
                new SimpleListener<int>((s) => CheckConnection())));

            state.State = (int)ReconnectionState.FirstCheck;
            state.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnStateChange()));

            scene.Components.GetOrRegister<Message<GameEndType>>((int)ComponentKeys.GameEnd, Message<GameEndType>.Create)
                .Subscribe(new SimpleListener<GameEndType>(OnGameEnd));
        }

        private void OnGameEnd(GameEndType type) {
            connHelper.SetGameInProgress(false);
        }

        private void OnStateChange() {
            ReconnectionState rs = (ReconnectionState)state.State;
            if(rs != ReconnectionState.WaitingForServerReply) {
                stopRefreshGameState.Send();
            }
            if(rs != ReconnectionState.SignaledReady) {
                sendReady.Send(false);
            }
        }

        public void Tick(float deltaTime) {
            switch(state.State) {
            case (int)ReconnectionState.FirstCheck:
                CheckConnection();
                if(state.State == (int)ReconnectionState.FirstCheck) {
                    state.State = (int)ReconnectionState.Connected;
                }
                newBoardMessage.Send(scene.Game.Components.Get<Board>((int)ComponentKeys.GameBoard));
                break;
            case (int)ReconnectionState.TryRestartMyConnection:
                reconnectTimer -= deltaTime;
                if(reconnectTimer <= 0) {
                    TryToReconnect();
                }
                break;
            }
        }

        private void CheckConnection() {
            switch((ConnectionHelper.State)connHelper.Connection.State) {
            case ConnectionHelper.State.Unconnected:
                if(state.State != (int)ReconnectionState.Disconnected) {
                    state.State = (int)ReconnectionState.Disconnected;
                }
                break;
            case ConnectionHelper.State.Stopped:
                TryToReconnect();
                break;
            case ConnectionHelper.State.None:
                Baluga3.Core.BalugaDebug.Log("Client rejected");
                ExitToLobby();
                break;
            case ConnectionHelper.State.Connected:
                if(state.State == (int)ReconnectionState.Disconnected) {
                    state.State = (int)ReconnectionState.WaitingForServerReply;
                    SendGameState();
                }
                break;
            }
        }

        private void TryToReconnect() {
            state.State = (int)ReconnectionState.Disconnected;
            if(!connHelper.RestartConnection()) {
                state.State = (int)ReconnectionState.TryRestartMyConnection;
                reconnectTimer = RECONNECT_FAIL_RETRY_PERIOD;
            }
        }

        private void SendGameState() {
            NetworkGameState state = compileGameState.Send();
            sendGameState.Send(state);
        }

        private void OnReconnectionReplyReceived(NetworkGameState gameState) {
            if(state.State == (int)ReconnectionState.WaitingForServerReply) {
                state.State = (int)ReconnectionState.SignaledReady;
                if(gameState == null) {
                    BalugaDebug.Log("Server replied no game state");
                    ExitToLobby();
                } else {
                    Board board = createBoard.Send(gameState);
                    scene.Game.Components.Remove((int)ComponentKeys.GameBoard);
                    scene.Game.Components.Register((int)ComponentKeys.GameBoard, board);
                    newBoardMessage.Send(board);
                    sendReady.Send(true);
                }
            }
        }

        private void OnAllReadyReceived() {
            if(state.State == (int)ReconnectionState.SignaledReady) {
                state.State = (int)ReconnectionState.Connected;
            }
        }

        private void OnErrorReceived() {
            ExitToLobby();
        }

        private void ExitToLobby() {
            state.State = (int)ReconnectionState.Exiting;
            exitToLobby.Send();
        }
    }
}
