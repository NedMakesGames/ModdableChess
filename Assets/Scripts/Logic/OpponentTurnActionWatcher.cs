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
    class OpponentTurnActionWatcher {

        private Board board;
        private GameDatabase db;
        private Query<TurnOptions, TurnOptionCalculatorArgs> turnOptionCalculator;
        private Message<TurnAction> choiceMadeEvent;

        public OpponentTurnActionWatcher(ChessScene scene) {
            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            turnOptionCalculator = scene.Components.GetOrRegister<Query<TurnOptions, TurnOptionCalculatorArgs>>
                ((int)ComponentKeys.MoveOptionListQuery, Query<TurnOptions, TurnOptionCalculatorArgs>.Create);
            choiceMadeEvent = scene.Components.GetOrRegister<Message<TurnAction>>((int)ComponentKeys.MoveChoiceMadeEvent, Message<TurnAction>.Create);

            scene.ActivatableList.Add(new ListenerJanitor<IListener<TurnAction>>(
                scene.Game.Components.Get<Message<TurnAction>>((int)ComponentKeys.OpponentTurnActionReceived),
                new SimpleListener<TurnAction>(OnReceivedAction)));
        }

        private void OnReceivedAction(TurnAction action) {
            bool validAction = false;
            if(action.components.Count == 1 && action.components[0].type == TurnActionComponentType.Forfeit) {
                validAction = true;
            } else {
                int pieceIndex = board.PieceOnSpace(action.searchPiece);
                if(pieceIndex >= 0) {
                    TurnOptions currentOptions = turnOptionCalculator.Send(new TurnOptionCalculatorArgs() {
                        pieceIndex = pieceIndex,
                        luaState = LuaTranslator.GetMoveCalcState(pieceIndex, board, db),
                    });
                    foreach(var possible in currentOptions.options) {
                        if(possible.IsEquivalent(action)) {
                            validAction = true;
                            break;
                        }
                    }
                }
            }
            if(!validAction) {
                UnityEngine.Debug.LogError("Cheating detected! Or at least a mod mismatch!");
            }
            choiceMadeEvent.Send(action);
        }
    }
}
