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
    [System.Obsolete]
    public struct CheckCalcArgs {
        public int kingIndex;
        public IntVector2 testPosition;
        public PlayerTurnOrder activePlayer;

        public CheckCalcArgs(int kingIndex, IntVector2 testPosition, PlayerTurnOrder activePlayer) {
            this.kingIndex = kingIndex;
            this.testPosition = testPosition;
            this.activePlayer = activePlayer;
        }
    }

    [System.Obsolete]
    public class CheckCalculator {

        private Board board;
        private GameDatabase db;
        private Query<TurnOptions, TurnOptionCalculatorArgs> moveOptionQuery;

        public CheckCalculator(ChessScene scene) {
            scene.Components.GetOrRegister<Query<bool, CheckCalcArgs>>((int)ComponentKeys.CheckQuery, Query<bool, CheckCalcArgs>.Create)
                .Handler = Calculate;

            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);

            moveOptionQuery = scene.Components.GetOrRegister<Query<TurnOptions, TurnOptionCalculatorArgs>>
                ((int)ComponentKeys.MoveOptionListQuery, Query<TurnOptions, TurnOptionCalculatorArgs>.Create);
        }

        private bool Calculate(CheckCalcArgs args) {
            Piece king = board.Pieces[args.kingIndex];
            for(int p = 0; p < board.Pieces.Count; p++) {
                Piece enemy = board.Pieces[p];
                if(enemy.OwnerPlayer != king.OwnerPlayer) {
                    TurnOptions moveOptions = moveOptionQuery.Send(new TurnOptionCalculatorArgs() {
                        pieceIndex = p,
                        luaState = LuaTranslator.GetMoveCalcState(p, board, db),
                    });
                    foreach(var option in moveOptions.options) {
                        if(option.components[0].target == args.testPosition) {
                            BalugaDebug.Log(string.Format("King {0} in check from {1} at {2}", args.kingIndex, p, enemy.BoardPosition));
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        
    }
}
