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
using ModdableChess.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModdableChess.Logic {

    public class ModLoadingScene : AutoController {

        private const float MAX_LOADING_TIME = 2 * 60;

        private enum Phase {
            Startup, ModLoading, StartCondition, ReadyToPlay, Exiting, ModLoadError
        }

        private Command sendError;
        private Command<bool> sendReady;
        private Command<NetworkGameState> sendGameState;
        private Command<string> reqModLoad;
        private Command stopRefreshGameState;
        private LobbyChoices lobbyChoices;
        private ConnectionHelper connHelper;
        private Query<NetworkGameState> startGenerator;
        private float timer;
        private StateMachine phase;
        private Message<string> loadingError;

        public ModLoadingScene(ModdableChessGame game) : base(game) {
            connHelper = game.Components.Get<ConnectionHelper>((int)ComponentKeys.ConnectionHelper);
            ActivatableList.Add(new ListenerJanitor<IListener<int>>(
                connHelper.Connection.EnterStateMessenger,
                new SimpleListener<int>((s) => CheckDisconnect())));

            phase = new StateMachine(0);
            phase.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnStateChange()));
            timer = 1;
            lobbyChoices = game.Components.Get<LobbyChoices>((int)ComponentKeys.LobbyChoices);
            startGenerator = Components.GetOrRegister<Query<NetworkGameState>>((int)ComponentKeys.CalculateStartConditions,
                Query<NetworkGameState>.Create);

            reqModLoad = Components.GetOrRegister((int)ComponentKeys.DeepLoadMod, Command<string>.Create);
            sendGameState = Game.Components.Get<Command<NetworkGameState>>((int)ComponentKeys.LoadingGameStateSend);
            stopRefreshGameState = Game.Components.Get<Command>((int)ComponentKeys.LoadingGameStateStopRefreshing);
            sendReady = Game.Components.Get<Command<bool>>((int)ComponentKeys.LoadingReadySend);
            sendError = Game.Components.Get<Command>((int)ComponentKeys.LoadingErrorSend);

            loadingError = Components.GetOrRegister((int)ComponentKeys.ModLoadError, Message<string>.Create);
            loadingError.Subscribe(new SimpleListener<string>(OnModLoadError));

            Components.GetOrRegister((int)ComponentKeys.ModTranslationComplete, Message.Create).Subscribe(new SimpleListener(OnModTranslated));

            ActivatableList.Add(new ListenerJanitor<IListener<NetworkGameState>>(
                Game.Components.Get<IMessage<NetworkGameState>>((int)ComponentKeys.LoadingGameStateReceived),
                new SimpleListener<NetworkGameState>(OnReceiveConditions)));
            ActivatableList.Add(new ListenerJanitor<IListener>(
                Game.Components.Get<IMessage>((int)ComponentKeys.LoadingExitReceived),
                new SimpleListener(ReadyToPlay)));
            ActivatableList.Add(new ListenerJanitor<IListener>(
                Game.Components.Get<IMessage>((int)ComponentKeys.LoadingErrorReceived),
                new SimpleListener(ExitToLobby)));

            new GameStartGenerator(this);
            new BoardCreator(this);
            new ModErrorLog(this);

            CheckDisconnect();
        }

        public override void Enter() {
            base.Enter();
            UnityEngine.SceneManagement.SceneManager.LoadScene("ModLoadScene");
        }

        private void OnStateChange() {
            Phase ps = (Phase)phase.State;
            if(ps != Phase.StartCondition) {
                stopRefreshGameState.Send();
            }
            if(ps != Phase.ReadyToPlay) {
                sendReady.Send(false);
            }
        }

        public override void Tick(float deltaTime) {
            base.Tick(deltaTime);
            switch((Phase)phase.State) {
            case Phase.Startup:
                timer -= Time.deltaTime;
                if(timer <= 0) {
                    phase.State = (int)Phase.ModLoading;
                    timer = MAX_LOADING_TIME;
                    reqModLoad.Send(lobbyChoices.ModFolder);
                }
                break;
            case Phase.ModLoading:
            case Phase.ModLoadError:
            case Phase.ReadyToPlay:
            case Phase.StartCondition:
                timer -= Time.deltaTime;
                if(timer <= 0) {
                    ExitAndDisconnect();
                }
                break;
            }
        }

        private void CheckDisconnect() {
            if(connHelper.Connection.State != (int)ServerConnectionState.Connected) {
                ExitAndDisconnect();
            }
        }

        private void ExitAndDisconnect() {
            BalugaDebug.Log("Exit loading from network error");
            connHelper.StopConnection();

            Game.Controller = new Lobby.LobbyScene((ModdableChessGame)Game);
        }

        private void OnModLoadError(string msg) {
            Debug.LogError("Mod error: " + msg);
            phase.State = (int)Phase.ModLoadError;
            sendError.Send();
        }

        private void OnModTranslated() {
            if(phase.State == (int)Phase.ModLoading) {
                phase.State = (int)Phase.StartCondition;
                NetworkGameState gameState = null;
                switch(lobbyChoices.MatchState) {
                case LobbyMatchState.FreshLobby:
                case LobbyMatchState.RepeatGame:
                    if(connHelper.CurrentMode == ConnectionHelper.Mode.Host) {
                        try {
                            gameState = startGenerator.Send();
                        } catch (BoardSetupException ex) {
                            loadingError.Send(ex.Message);
                        }
                    }
                    break;
                }
                if(phase.State == (int)Phase.StartCondition) {
                    sendGameState.Send(gameState);
                }
            }
        }

        private void OnReceiveConditions(NetworkGameState state) {
            if(phase.State == (int)Phase.StartCondition) {
                if(state == null) {
                    ExitToLobby();
                } else {
                    phase.State = (int)Phase.ReadyToPlay;
                    Board board = Components.Get<Query<Board, NetworkGameState>>
                        ((int)ComponentKeys.CreateBoardCommand).Send(state);
                    if(Game.Components.Contains((int)ComponentKeys.GameBoard)) {
                        Game.Components.Remove((int)ComponentKeys.GameBoard);
                    }
                    Game.Components.Register((int)ComponentKeys.GameBoard, board);
                    sendReady.Send(true);
                }
            }
        }

        private void ReadyToPlay() {
            if(phase.State == (int)Phase.ReadyToPlay) {
                phase.State = (int)Phase.Exiting;
                UnityEngine.Debug.Log("Game completely loaded and ready");
                Game.Controller = new ChessScene((ModdableChessGame)Game);
            }
        }

        private void ExitToLobby() {
            if(phase.State != (int)Phase.Exiting) {
                phase.State = (int)Phase.Exiting;
                UnityEngine.Debug.Log("Error received, exiting to lobby");
                Game.Components.Get<ScriptController>((int)ComponentKeys.LuaScripts).Clear();
                Game.Controller = new Lobby.LobbyScene((ModdableChessGame)Game);
            }
        }
    }
}
