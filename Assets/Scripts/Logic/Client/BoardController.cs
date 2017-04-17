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
    public class BoardController {

        private Message<TurnAction> opponentActionReceived;
        private Message<ServerEndOfTurnState> eotStateReceived;
        private GameDatabase db;
        private ReadyCheckSender eotReadyCheck;

        public BoardController(Game game) {

            db = game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);

            opponentActionReceived = game.Components.GetOrRegister<Message<TurnAction>>
                ((int)ComponentKeys.OpponentTurnActionReceived, Message<TurnAction>.Create);
            game.Components.GetOrRegister<Command<TurnAction>>
                ((int)ComponentKeys.SendTurnAction, Command<TurnAction>.Create).Handler = SendTurnAction;

            eotReadyCheck = new ReadyCheckSender(game, 5, (r) => SendEoTReadyRefresh(r));
            eotReadyCheck.InitialWaitPeriod = eotReadyCheck.ResendPeriod;
            eotStateReceived = game.Components.GetOrRegister<Message<ServerEndOfTurnState>>
                ((int)ComponentKeys.EndOfTurnStateReceived, Message<ServerEndOfTurnState>.Create);
            game.Components.GetOrRegister<Command<EndOfTurnState>>
                ((int)ComponentKeys.SendEndOfTurnState, Command<EndOfTurnState>.Create).Handler = SendEndOfTurnState;
            game.Components.GetOrRegister<Command>
                ((int)ComponentKeys.StopRefreshEndOfTurnState, Command.Create).Handler = StopSendingEoTRefresh;

            NetworkTracker.ClientMsgCallbacks.boardActionReceived = OnOpponentActionReceived;
            NetworkTracker.ClientMsgCallbacks.boardEndOfTurnReceived = OnEoTStateReceived;
        }

        private void SendTurnAction(TurnAction action) {
            BoardActionMessage msg = new BoardActionMessage();
            msg.searchPiece = action.searchPiece;
            msg.components = new BoardActionMessageComponent[action.components.Count];
            for(int i = 0; i < msg.components.Length; i++) {
                TurnActionComponent c = action.components[i];
                msg.components[i] = new BoardActionMessageComponent() {
                    type = c.type,
                    actor = c.actor,
                    target = c.target,
                    promotionPieceName = db.PiecePrototypes[c.promotionIndex].LuaTag,
                };
            }
            NetworkTracker.ClientSendBoardAction(msg);
        }

        private void OnOpponentActionReceived(BoardActionMessage msg) {
            TurnAction action = new TurnAction();
            action.searchPiece = msg.searchPiece;
            action.components = new List<TurnActionComponent>();
            for(int i = 0; i < msg.components.Length; i++) {
                BoardActionMessageComponent c = msg.components[i];
                action.components.Add(new TurnActionComponent() {
                    type = c.type,
                    actor = c.actor,
                    target = c.target,
                    promotionIndex = db.PieceNameToIndex(c.promotionPieceName),
                });
            }
            opponentActionReceived.Send(action);
        }

        private void SendEndOfTurnState(EndOfTurnState state) {
            NetworkTracker.ClientSendEoT(new EndOfTurnStateMessage() {
                state = state,
            });
            eotReadyCheck.Active = true;
        }

        private void SendEoTReadyRefresh(bool ready) {
            NetworkTracker.ClientSendEoTRefresh(new NetworkPlayerReadyNotice() {
                ready = ready,
            });
        }

        private void StopSendingEoTRefresh() {
            eotReadyCheck.Active = false;
        }

        private void OnEoTStateReceived(ServerEndOfTurnStateMessage msg) {
            eotStateReceived.Send(msg.state);
        }
    }
}
