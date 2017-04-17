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
using Baluga3.GameFlowLogic;
using System;
using Baluga3.Core;

namespace ModdableChess.Logic {
    public enum MatchState {
        Uninitialized=0, Intro, Play, End
    }

    public enum TurnState {
        None, Choosing, WaitServerEoT,
    }

    public enum GameEndType {
        None, Win, Loss, Tie, Error,
    }

    public class MatchManager : ITicking {
        private StateMachine matchState;
        private Message turnStateChange;
        private Command<bool> startTurnCommand;
        private Query<EoTScriptState> checkGameOver;
        private Message<GameEndType> gameOverMessage;
        private Command<EndOfTurnState> sendEoTState;
        private Command stopRefreshEoTState;
        private float timer = 0;
        private bool startTurnNextFrame;
        private GameDatabase db;
        private Board board;
        private LocalPlayerInformation localInfo;
        private TurnActionExecutionResult lastActionResult;
        private bool resetTurnState;

        public MatchManager(AutoController scene) {
            scene.Game.AddTicking(this);

            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>(ReceivedNewBoard));

            localInfo = scene.Game.Components.GetOrRegister<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation, LocalPlayerInformation.Create);
            matchState = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.MatchState, StateMachine.Create);
            startTurnCommand = scene.Components.GetOrRegister<Command<bool>>((int)ComponentKeys.NextTurnCommand, Command<bool>.Create);
            gameOverMessage = scene.Components.GetOrRegister<Message<GameEndType>>((int)ComponentKeys.GameEnd, Message<GameEndType>.Create);
            checkGameOver = scene.Components.GetOrRegister<Query<EoTScriptState>>((int)ComponentKeys.GameOverQuery, Query<EoTScriptState>.Create);
            sendEoTState = scene.Game.Components.GetOrRegister<Command<EndOfTurnState>>((int)ComponentKeys.SendEndOfTurnState, Command<EndOfTurnState>.Create);
            stopRefreshEoTState = scene.Game.Components.GetOrRegister<Command>((int)ComponentKeys.StopRefreshEndOfTurnState, Command.Create);

            turnStateChange = scene.Components.GetOrRegister<Message>((int)ComponentKeys.TurnState, Message.Create);
            turnStateChange.Subscribe(new SimpleListener(OnTurnStateChange));

            scene.Components.GetOrRegister<Command<TurnActionExecutionResult>>
                ((int)ComponentKeys.HandleEndOfTurn, Command<TurnActionExecutionResult>.Create).Handler = DoEndOfTurn;
            scene.ActivatableList.Add(new ListenerJanitor<IListener<ServerEndOfTurnState>>(
                scene.Game.Components.GetOrRegister<Message<ServerEndOfTurnState>>((int)ComponentKeys.EndOfTurnStateReceived, Message<ServerEndOfTurnState>.Create),
                new SimpleListener<ServerEndOfTurnState>(OnReceivedEndOfTurnState)));
        }

        private void ReceivedNewBoard(Board newBoard) {
            TurnState oldTurnState = board == null ? TurnState.None : board.TurnState;
            this.board = newBoard;
            if(oldTurnState != board.TurnState) {
                resetTurnState = true;
            }
        }

        private void SetTurnState(TurnState state) {
            board.TurnState = state;
            turnStateChange.Send();
            switch(board.TurnState) {
            case TurnState.Choosing:
                startTurnCommand.Send(false);
                break;
            case TurnState.WaitServerEoT:
                SendEndOfTurnState();
                break;
            }
        }

        private void OnTurnStateChange() {
            if(board.TurnState != TurnState.WaitServerEoT) {
                stopRefreshEoTState.Send();
            }
        }

        public void Tick(float deltaTime) {
            switch((MatchState)matchState.State) {
            case 0:
                matchState.State = (int)MatchState.Intro;
                timer = 1;
                break;
            case MatchState.Intro:
                timer -= deltaTime;
                if(timer <= 0) {
                    matchState.State = (int)MatchState.Play;
                    SetTurnState(TurnState.Choosing);
                }
                break;
            }
            if(resetTurnState) {
                resetTurnState = false;
                SetTurnState(board.TurnState);
            }
        }

        private void DoEndOfTurn(TurnActionExecutionResult result) {
            lastActionResult = result;
            SetTurnState(TurnState.WaitServerEoT);
        }

        private void SendEndOfTurnState() { 
            EoTScriptState overType;
            switch(lastActionResult) {
            case TurnActionExecutionResult.Success:
                overType = checkGameOver.Send();
                break;
            case TurnActionExecutionResult.Forfeit:
                overType = EoTScriptState.Forfeit;
                break;
            default:
                overType = EoTScriptState.Error;
                break;
            }

            BalugaDebug.Log("Game over type: " + overType);

            // Send win/loss/not-decided/error to server, and wait for reply
            EndOfTurnState eotState = EndOfTurnState.Error;
            switch(overType) {
            case EoTScriptState.FirstPlayerWins:
                if(board.LocalPlayerOrder == PlayerTurnOrder.First) {
                    eotState = EndOfTurnState.Win;
                } else {
                    eotState = EndOfTurnState.Loss;
                }
                break;
            case EoTScriptState.SecondPlayerWins:
                if(board.LocalPlayerOrder == PlayerTurnOrder.First) {
                    eotState = EndOfTurnState.Loss;
                } else {
                    eotState = EndOfTurnState.Win;
                }
                break;
            case EoTScriptState.Tie:
                eotState = EndOfTurnState.Tie;
                break;
            case EoTScriptState.Undecided:
                eotState = EndOfTurnState.Undecided;
                break;
            case EoTScriptState.Forfeit:
                eotState = EndOfTurnState.Forfeit;
                break;
            default:
                eotState = EndOfTurnState.Error;
                break;
            }

            BalugaDebug.Log("End of turn state: " + eotState);
            sendEoTState.Send(eotState);
        }

        private void OnReceivedEndOfTurnState(ServerEndOfTurnState serverState) {

            board.TurnState = TurnState.Choosing;
            turnStateChange.Send();

            BalugaDebug.Log("Server end of turn reply: " + serverState);
            if(serverState == ServerEndOfTurnState.Undecided) {
                startTurnCommand.Send(true);
            } else {
                GameEndType endType;
                switch(serverState) {
                case ServerEndOfTurnState.HostWin:
                    endType = localInfo.IsHost ? GameEndType.Win : GameEndType.Loss;
                    break;
                case ServerEndOfTurnState.ClientWin:
                    endType = localInfo.IsHost ? GameEndType.Loss : GameEndType.Win;
                    break;
                case ServerEndOfTurnState.Tie:
                    endType = GameEndType.Tie;
                    break;
                default:
                    endType = GameEndType.Error;
                    break;
                }
                BalugaDebug.Log("End of game type: " + endType);
                gameOverMessage.Send(endType);
                matchState.State = (int)MatchState.End;
            }
            
        }
    }
}
