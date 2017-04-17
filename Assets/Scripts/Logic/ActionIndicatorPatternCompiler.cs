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

    struct ActionIndicatorPatternCompileArgs {

        public TurnOptions options;
        public int highlightedIndex;
        public List<int> showOnly;
        public bool mouseOverMode;
    }

    class ActionIndicatorPatternCompiler {

        //private Board board;
        private GameDatabase db;

        public ActionIndicatorPatternCompiler(ChessScene scene) {
            //scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
            //    .Subscribe(new SimpleListener<Board>((b) => board = b));
            db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);

            scene.Components.GetOrRegister<Query<ActionIndicatorPattern, ActionIndicatorPatternCompileArgs>>(
                (int)ComponentKeys.GetActionIndicatorPattern, Query<ActionIndicatorPattern, ActionIndicatorPatternCompileArgs>.Create)
                .Handler = Compile;
        }

        private ActionIndicatorPattern Compile(ActionIndicatorPatternCompileArgs args) {
            ActionIndicatorPattern pattern = new ActionIndicatorPattern();
            pattern.indicators = new List<ActionIndicator>();

            List<TurnAction> options = args.options.options;
            for(int o = 0; o < options.Count; o++) {
                if(args.showOnly == null || args.showOnly.Contains(o)) { 
                    TurnAction action = options[o];
                    for(int c = 0; c < action.components.Count; c++) {
                        TurnActionComponent comp = action.components[c];
                        IntVector2 boardPos = GetBoardPosition(comp);
                        ActionIndicatorType type = GetIndicatorType(comp);
                        int decorationModel = GetDecorationModel(comp);
                        int promotionPiece = GetPromotionPiece(comp);
                        ActionIndicatorStrength strength;
                        if(args.mouseOverMode) {
                            strength = ActionIndicatorStrength.Mouseover;
                        } else if(o == args.highlightedIndex) {
                            strength = ActionIndicatorStrength.Selected;
                        } else {
                            strength = ActionIndicatorStrength.Inactive;
                        }

                        if(!CheckForDuplicates(pattern, boardPos, type, decorationModel, promotionPiece, strength)) {
                            pattern.indicators.Add(new ActionIndicator() {
                                boardPosition = boardPos,
                                type = type,
                                decorationModel = decorationModel,
                                strength = strength,
                                promotionPiece = promotionPiece,
                            });
                        }
                    }
                }
            }

            return pattern;
        }

        private bool CheckForDuplicates(ActionIndicatorPattern pattern, IntVector2 pos, ActionIndicatorType type, int decorationModel,
                int promotionPiece, ActionIndicatorStrength strength) {
            for(int i = 0; i < pattern.indicators.Count; i++) {
                ActionIndicator ind = pattern.indicators[i];
                if(ind.boardPosition == pos && ind.type == type && ind.decorationModel == decorationModel && ind.promotionPiece == promotionPiece) {
                    if((int)ind.strength > (int)strength) {
                        ind.strength = strength;
                    }
                    return true;
                }
            }
            return false;
        }

        private IntVector2 GetBoardPosition(TurnActionComponent comp) {
            switch(comp.type) {
            case TurnActionComponentType.MovePiece:
            case TurnActionComponentType.CapturePiece:
                return comp.target;
            case TurnActionComponentType.PromotePiece:
                return comp.actor;
            default:
                return IntVector2.Zero;
            }
        }

        private ActionIndicatorType GetIndicatorType(TurnActionComponent comp) {
            switch(comp.type) {
            case TurnActionComponentType.MovePiece:
                return ActionIndicatorType.Move;
            case TurnActionComponentType.CapturePiece:
                return ActionIndicatorType.Capture;
            case TurnActionComponentType.PromotePiece:
                return ActionIndicatorType.Promote;
            default:
                return 0;
            }
        }

        private int GetDecorationModel(TurnActionComponent comp) {
            switch(comp.type) {
            case TurnActionComponentType.PromotePiece:
                return db.PiecePrototypes[comp.promotionIndex].PromotionIndicatorModelIndex;
            default:
                return -1;
            }
        }

        private int GetPromotionPiece(TurnActionComponent comp) {
            switch(comp.type) {
            case TurnActionComponentType.PromotePiece:
                return comp.promotionIndex;
            default:
                return -1;
            }
        }
    }
}
