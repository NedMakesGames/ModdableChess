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
using ModdableChess.Mods;

namespace ModdableChess.Logic {
    public struct NextTurnInfo {
        public PlayerTurnOrder player;
        public int number;
    }

    public class TurnActionExcecutionException : Exception {
        public TurnActionExcecutionException(string m) : base(m) {

        }
    }

    public enum TurnActionExecutionResult {
        None = 0, Success, Error, Forfeit
    }

    public class TurnManager {

        private Command startTurnChoosingCommand;
        private Command stopTurnChoosingCommand;
        private Command<MoveCommand> moveCommand;
        private Command<CaptureCommand> captureCommand;
        private Command<PromoteCommand> promoteCommand;
        private ScriptController scripts;
        private Command<TurnActionExecutionResult> handleEndOfTurn;
        private StateMachine connectionState;
        private Message turnChange;

        //private GameDatabase db;
        private Board board;

        public TurnManager(ChessScene scene) {
            //db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            startTurnChoosingCommand = scene.Components.GetOrRegister<Command>
                ((int)ComponentKeys.StartTurnChoosingCommand, Command.Create);
            stopTurnChoosingCommand = scene.Components.GetOrRegister<Command>
                ((int)ComponentKeys.StopTurnChoosingCommand, Command.Create);
            scene.Components.GetOrRegister<Query<NextTurnInfo>>((int)ComponentKeys.NextTurnInfoQuery, Query<NextTurnInfo>.Create)
                .Handler = FigureNextTurn;
            moveCommand = scene.Components.GetOrRegister<Command<MoveCommand>>
                ((int)ComponentKeys.MovePieceCommand, Command<MoveCommand>.Create);
            captureCommand = scene.Components.GetOrRegister<Command<CaptureCommand>>
                ((int)ComponentKeys.CapturePieceCommand, Command<CaptureCommand>.Create);
            promoteCommand = scene.Components.GetOrRegister<Command<PromoteCommand>>
                ((int)ComponentKeys.PromotePieceCommand, Command<PromoteCommand>.Create);
            scene.Components.GetOrRegister<Message<TurnAction>>((int)ComponentKeys.MoveChoiceMadeEvent, Message<TurnAction>.Create)
                .Subscribe(new SimpleListener<TurnAction>(HandleChoiceMade));
            scene.Components.GetOrRegister<Command<bool>>((int)ComponentKeys.NextTurnCommand, Command<bool>.Create).Handler = NextTurnCommand;
            scripts = scene.Game.Components.Get<ScriptController>((int)ComponentKeys.LuaScripts);
            handleEndOfTurn = scene.Components.GetOrRegister<Command<TurnActionExecutionResult>>
                ((int)ComponentKeys.HandleEndOfTurn, Command<TurnActionExecutionResult>.Create);
            turnChange = scene.Components.GetOrRegister<Message>((int)ComponentKeys.TurnChange, Message.Create);

            scene.Components.GetOrRegister<Message>((int)ComponentKeys.TurnState, Message.Create)
                .Subscribe(new SimpleListener(OnTurnStateChange));
            connectionState = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.ReconnectionState, StateMachine.Create);
            connectionState.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnConnectionStateChange()));
        }

        private void OnConnectionStateChange() {
            if(board != null && board.TurnState == TurnState.Choosing && IsLocalPlayerTurn()) {
                if(connectionState.State == (int)ReconnectionState.Connected) {
                    startTurnChoosingCommand.Send();
                } else {
                    stopTurnChoosingCommand.Send();
                }
            }
        }

        private void OnTurnStateChange() {
            if(board.TurnState != TurnState.Choosing) {
                stopTurnChoosingCommand.Send();
            }
        }

        private void NextTurnCommand(bool increment) {
            if(increment) {
                NextTurnInfo next = FigureNextTurn();
                board.TurnNumber++;
                board.ActivePlayer = next.player;
                turnChange.Send();
            }
            if(IsLocalPlayerTurn()) {
                startTurnChoosingCommand.Send();
            }
        }

        private bool IsLocalPlayerTurn() {
            //UnityEngine.Debug.LogError(
            //    string.Format("Local active check: local {0}, active {1}, number {2}",
            //    board.LocalPlayerOrder, board.ActivePlayer, board.TurnNumber));
            return board.ActivePlayer == board.LocalPlayerOrder;
        }

        private NextTurnInfo FigureNextTurn() {
            PlayerTurnOrder nextPlayer;
            switch(board.ActivePlayer) {
            case PlayerTurnOrder.Undecided:
            case PlayerTurnOrder.Second:
                nextPlayer = PlayerTurnOrder.First;
                break;
            default:
                nextPlayer = PlayerTurnOrder.Second;
                break;
            }
            return new NextTurnInfo() {
                player = nextPlayer,
                number = board.TurnNumber + 1,
            };
        }

        private void HandleChoiceMade(TurnAction action) {
            stopTurnChoosingCommand.Send();
            TurnActionExecutionResult result = TurnActionExecutionResult.Success;
            for(int c = 0; c < action.components.Count; c++) {
                TurnActionComponent comp = action.components[c];
                try {
                    switch(comp.type) {
                    case TurnActionComponentType.MovePiece:
                        moveCommand.Send(new MoveCommand() {
                            startPosition = comp.actor,
                            moveTo = comp.target,
                        });
                        break;
                    case TurnActionComponentType.CapturePiece:
                        captureCommand.Send(new CaptureCommand() {
                            space = comp.target,
                        });
                        break;
                    case TurnActionComponentType.PromotePiece:
                        promoteCommand.Send(new PromoteCommand() {
                            piecePos = comp.actor,
                            promoteTo = comp.promotionIndex,
                        });
                        break;
                    case TurnActionComponentType.Forfeit:
                        if(board.ActivePlayer == board.LocalPlayerOrder) {
                            result = TurnActionExecutionResult.Forfeit;
                        }
                        break;
                    default:
                        throw new TurnActionExcecutionException("Bad component type");
                    }
                } catch(TurnActionExcecutionException ex) {
                    scripts.WriteToAuthorLog(string.Format("Turn action execution error on component {0}: {1}", c, ex.Message));
                    result = TurnActionExecutionResult.Error;
                    break;
                }
            }
            handleEndOfTurn.Send(result);
        }
    }
}
