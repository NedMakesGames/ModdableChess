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
    class MouseOverMoveOptionIndicators {

        private Board board;
        private GameDatabase db;
        private Query<TurnOptions, TurnOptionCalculatorArgs> moveOptionCalculator;
        private Query<ActionIndicatorPattern, ActionIndicatorPatternCompileArgs> compiler;
        private PlayerSelectionEvent selEvent;
        private SubscribableInt chosenPiece;

        private Message<ActionIndicatorPattern> displayMessage;

        public MouseOverMoveOptionIndicators(AutoController scene) {

            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);

            displayMessage = scene.Components.GetOrRegister<Message<ActionIndicatorPattern>>
                ((int)ComponentKeys.MouseOverIndicatorPattern, Message<ActionIndicatorPattern>.Create);
            moveOptionCalculator = scene.Components.GetOrRegister<Query<TurnOptions, TurnOptionCalculatorArgs>>
                ((int)ComponentKeys.MoveOptionListQuery, Query<TurnOptions, TurnOptionCalculatorArgs>.Create);
            compiler = scene.Components.GetOrRegister<Query<ActionIndicatorPattern, ActionIndicatorPatternCompileArgs>>(
                (int)ComponentKeys.GetActionIndicatorPattern, Query<ActionIndicatorPattern, ActionIndicatorPatternCompileArgs>.Create);
            chosenPiece = scene.Components.GetOrRegister<SubscribableInt>((int)ComponentKeys.TurnChooserChosenPiece, SubscribableInt.Create);
            chosenPiece.Subscribe(new SimpleListener<int>((p) => RefreshMoveOptions()));
            selEvent = scene.Components.GetOrRegister<PlayerSelectionEvent>((int)ComponentKeys.PlayerSelectionChange, PlayerSelectionEvent.Create);
            selEvent.Subscribe(new SimpleListener(RefreshMoveOptions));
        }

        private void RefreshMoveOptions() {
            int pieceID = -1;
            switch(selEvent.CurrentType) {
            case PlayerSelectionEvent.SelectionType.Piece:
                pieceID = selEvent.PieceID;
                break;
            case PlayerSelectionEvent.SelectionType.Square:
                pieceID = board.PieceOnSpace(selEvent.SquarePos);
                break;
            }
            if(pieceID >= 0 && pieceID != chosenPiece.Value) {
                Piece piece = board.Pieces[pieceID];
                TurnOptions calculatedOptions = moveOptionCalculator.Send(new TurnOptionCalculatorArgs() {
                    pieceIndex = pieceID,
                    luaState = LuaTranslator.GetMoveCalcState(pieceID, board, db),
                });

                displayMessage.Send(compiler.Send(new ActionIndicatorPatternCompileArgs() {
                    options = calculatedOptions,
                    highlightedIndex = -1,
                    mouseOverMode = true,
                }));
            } else {
                displayMessage.Send(null);
            }
        }
    }
}
