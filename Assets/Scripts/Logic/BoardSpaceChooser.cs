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
using Baluga3.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic {
    public struct TurnChooserHighlight {
        public TurnOptions options;
        public List<int> indexesOnSpace;
        public int highlightedIndex;
    }

    class BoardSpaceChooser {
        private Board board;
        private GameDatabase db;
        private StateMachine chooserState;
        private PlayerSelectionEvent selEvent;
        private SubscribableBool acceptBtn;
        private SubscribableObject<TurnAction> chosenAction;
        private Message<TurnChooserHighlight> highlightMessage;
        private Query<TurnOptions, TurnOptionCalculatorArgs> turnOptionCalculator;
        private SubscribableInt chosenPiece;
        private TurnOptions currentOptions;

        private List<int> actionList;
        private Dictionary<IntVector2, int> inputOffsets;

        public BoardSpaceChooser(ChessScene scene) {
            actionList = new List<int>();
            inputOffsets = new Dictionary<IntVector2, int>();
            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            chooserState = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.TurnChooserState, StateMachine.Create);
            chooserState.EnterStateMessenger.Subscribe(new SimpleListener<int>(OnEnterState));
            selEvent = scene.Components.GetOrRegister<PlayerSelectionEvent>((int)ComponentKeys.PlayerSelectionChange, PlayerSelectionEvent.Create);
            selEvent.Subscribe(new SimpleListener(OnSelectionChange));
            acceptBtn = scene.Components.GetOrRegister<SubscribableBool>((int)ComponentKeys.PlayerSelectionAcceptBtn, SubscribableBool.Create);
            acceptBtn.Subscribe(new TriggerListener(OnSelectionBtnTrigger));
            chosenAction = scene.Components.GetOrRegister<SubscribableObject<TurnAction>>
                ((int)ComponentKeys.TurnChooserChosenAction, SubscribableObject<TurnAction>.Create);
            highlightMessage = scene.Components.GetOrRegister<Message<TurnChooserHighlight>>
                ((int)ComponentKeys.TurnChooserHighlightChange, Message<TurnChooserHighlight>.Create);
            chosenPiece = scene.Components.GetOrRegister<SubscribableInt>((int)ComponentKeys.TurnChooserChosenPiece, SubscribableInt.Create);
            turnOptionCalculator = scene.Components.GetOrRegister<Query<TurnOptions, TurnOptionCalculatorArgs>>
                ((int)ComponentKeys.MoveOptionListQuery, Query<TurnOptions, TurnOptionCalculatorArgs>.Create);
            scene.Components.GetOrRegister<SubscribableBool>((int)ComponentKeys.PlayerSelectionCycleBtn, SubscribableBool.Create)
                .Subscribe(new TriggerListener(OnCycleButtonPress));
            scene.Components.GetOrRegister<SubscribableInt>((int)ComponentKeys.ScrollWheel, SubscribableInt.Create)
                .Subscribe(new SimpleListener<int>((s) => {
                    if(s != 0) {
                        OnScrollWheel(s);
                    }
                }));
        }

        private void OnEnterState(int state) {
            if(chooserState.State == (int)TurnChooser.State.ChooseAction) {
                chosenAction.Value = null;
                inputOffsets.Clear();
                currentOptions = turnOptionCalculator.Send(new TurnOptionCalculatorArgs() {
                    pieceIndex = chosenPiece.Value,
                    luaState = LuaTranslator.GetMoveCalcState(chosenPiece.Value, board, db),
                });
                RefreshHighlightedValue();
            } else {
                highlightMessage.Send(new TurnChooserHighlight() {
                    options = null,
                    indexesOnSpace = null,
                    highlightedIndex = -1,
                });
                currentOptions = null;
            }
        }

        private void OnCycleButtonPress() {
            if(chooserState.State == (int)TurnChooser.State.ChooseAction) {
                SetOffsetValue(1);
            }
        }

        private void OnScrollWheel(int s) {
            if(chooserState.State == (int)TurnChooser.State.ChooseAction) {
                SetOffsetValue(s > 0 ? 1 : -1);
            }
        }

        private void SetOffsetValue(int change) {
            IntVector2 boardPos = GetSelectedPosition();
            if(boardPos.x >= 0) {
                int oldValue;
                if(!inputOffsets.TryGetValue(boardPos, out oldValue)) {
                    oldValue = 0;
                }
                inputOffsets[boardPos] = oldValue + change;
                RefreshHighlightedValue();
            }
        }

        private void OnSelectionChange() {
            if(chooserState.State == (int)TurnChooser.State.ChooseAction) {
                RefreshHighlightedValue();
            }
        }

        private void RefreshHighlightedValue() {
            int highlighted = GetSelectedAction();
            highlightMessage.Send(new TurnChooserHighlight() {
                options = currentOptions,
                indexesOnSpace = highlighted < 0 ? null : actionList,
                highlightedIndex = highlighted,
            });
        }

        private void OnSelectionBtnTrigger() {
            if(chooserState.State == (int)TurnChooser.State.ChooseAction) {
                int selectedIndex = GetSelectedAction();
                if(selectedIndex >= 0) {
                    chosenAction.Value = currentOptions.options[selectedIndex];
                } else {
                    chosenAction.Value = null;
                }
            }
        }

        private IntVector2 GetSelectedPosition() {
            switch(selEvent.CurrentType) {
            case PlayerSelectionEvent.SelectionType.Piece:
                return board.Pieces[selEvent.PieceID].BoardPosition;
            case PlayerSelectionEvent.SelectionType.Square:
                return selEvent.SquarePos;
            default:
                return new IntVector2(-1, -1);
            }
        }

        private int GetSelectedAction() {
            IntVector2 boardPos = GetSelectedPosition();
            if(boardPos.x < 0) {
                return -1;
            }
            TurnOptionCalculator.FilterOptionsForPosition(actionList, currentOptions, boardPos);
            if(actionList.Count > 0) {
                int index = inputOffsets.GetValueOrDefault(boardPos, 0);
                if(index < 0) {
                    while(index < 0) {
                        index += actionList.Count;
                    }
                } else {
                    index = index % actionList.Count;
                }
                return actionList[index];
            } else {
                return -1;
            }
        }
    }
}
