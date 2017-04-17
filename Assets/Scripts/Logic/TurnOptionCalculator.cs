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
using MoonSharp.Interpreter;
using ModdableChess.Mods;

namespace ModdableChess.Logic {

    public struct TurnOptionCalculatorArgs {
        public int pieceIndex;
        public Mods.Lua.MoveCalcState luaState;
    }

    public class TurnOptions {
        public List<TurnAction> options;
    }

    public class TurnOptionCalculator {

        private class BadActionException : Exception {
            public BadActionException(string m) : base(m) {

            }
        }

        private GameDatabase db;
        private Board board;
        private ScriptController scripts;
        private DynValue luaHelper;

        public TurnOptionCalculator(ChessScene scene) {
            scripts = scene.Game.Components.Get<ScriptController>((int)ComponentKeys.LuaScripts);
            db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            luaHelper = UserData.Create(new Mods.Lua.TurnActionHelper());

            scene.Components.GetOrRegister<Query<TurnOptions, TurnOptionCalculatorArgs>>
                ((int)ComponentKeys.MoveOptionListQuery, Query<TurnOptions, TurnOptionCalculatorArgs>.Create)
                .Handler = CalculateMoveOptions;
        }

        private TurnOptions CalculateMoveOptions(TurnOptionCalculatorArgs args) {
            TurnOptions finalOptions = new TurnOptions();
            finalOptions.options = new List<TurnAction>();
            Piece piece = board.Pieces[args.pieceIndex];
            PiecePrototype proto = db.PiecePrototypes[piece.PrototypeID];

            scripts.AddGlobal("actionHelper", luaHelper);
            DynValue ret = scripts.CallFunction(proto.MovementFunction, args.luaState);
            scripts.RemoveGlobal("actionHelper");
            try {
                Table optionList = SafeGetTable(ret, string.Format("Piece {0} action function error: Expected table to be returned", proto.LuaTag));
                int numOptions = optionList.Length;
                for(int i = 1; i <= numOptions; i++) {
                    Table actionList = SafeGetTable(optionList.Get(i),
                        string.Format("Piece {0} action function error: Expected table at position {1} in returned list", proto.LuaTag, i));
                    TurnAction action = TranslateActionTable(args.pieceIndex, actionList, i);
                    finalOptions.options.Add(action);
                }
            } catch (BadActionException ex) {
                scripts.WriteToAuthorLog(ex.Message);
                finalOptions.options.Clear();
            }

            return finalOptions;
        }

        public TurnAction TranslateActionTable(int searchPiece, Table actionList, int optionIndex) {
            TurnAction action = new TurnAction();
            Piece piece = board.Pieces[searchPiece];
            action.searchPiece = piece.BoardPosition;
            string pieceName = db.PiecePrototypes[piece.PrototypeID].LuaTag;
            int numActions = actionList.Length;
            //BalugaDebug.Log(numActions);
            action.components = new List<TurnActionComponent>(numActions);
            
            for(int i = 1; i <= numActions; i++) {
                string exMsgHeader = string.Format("Piece {0} action function error at Option {1} Index {2}:", pieceName, optionIndex, i);
                TurnActionComponent comp = SafeGetAction(actionList.Get(i), exMsgHeader);
                action.components.Add(comp);
            }
            return action;
        }

        private int SafeGetInt(DynValue dynValue, string exceptionMessage) {
            if(dynValue.Type == DataType.Number) {
                return (int)Math.Round(dynValue.Number);
            } else {
                throw new BadActionException(exceptionMessage);
            }
        }

        private Table SafeGetTable(DynValue tab, string exceptionMessage) {
            if(tab.Type == DataType.Table) {
                return tab.Table;
            } else {
                throw new BadActionException(exceptionMessage);
            }
        }

        private TurnActionComponent SafeGetAction(DynValue value, string excMsgHeader) {
            if(value.Type == DataType.UserData) {
                Mods.Lua.TurnActionComponent luaComp = value.UserData.Object as Mods.Lua.TurnActionComponent;
                if(luaComp != null) {
                    return TranslateTurnActionComp(luaComp, excMsgHeader);
                } else {
                    throw new BadActionException(excMsgHeader + " UserData cannot be coerced into TurnActionComponent");
                }
            } else {
                throw new BadActionException(excMsgHeader + " Expected UserData type");
            }
        }

        private TurnActionComponent TranslateTurnActionComp(Mods.Lua.TurnActionComponent luaComp, string excMsgHeader) {
            TurnActionComponent logicComp = new TurnActionComponent();
            switch(luaComp.Type.ToUpperInvariant()) {
            case "MOVE":
                logicComp.type = TurnActionComponentType.MovePiece;
                logicComp.actor = ToBoardPosition(luaComp.ActorX, luaComp.ActorY, excMsgHeader, "start");
                logicComp.target = ToBoardPosition(luaComp.TargetX, luaComp.TargetY, excMsgHeader, "target");
                if(logicComp.actor == logicComp.target) {
                    throw new BadActionException(excMsgHeader + " Start and target positions are equal");
                }
                return logicComp;
            case "CAPTURE":
                logicComp.type = TurnActionComponentType.CapturePiece;
                logicComp.target = ToBoardPosition(luaComp.TargetX, luaComp.TargetY, excMsgHeader, "target");
                return logicComp;
            case "PROMOTION":
            case "PROMOTE":
                logicComp.type = TurnActionComponentType.PromotePiece;
                logicComp.actor = ToBoardPosition(luaComp.ActorX, luaComp.ActorY, excMsgHeader, "promoted");
                logicComp.promotionIndex = db.PieceNameToIndex(luaComp.PromotionPiece);
                if(logicComp.promotionIndex < 0) {
                    throw new BadActionException(excMsgHeader + " Nonexistent piece given for promotion");
                }
                return logicComp;
            default:
                throw new BadActionException(string.Format("{0} Invalid action type \"{1}\"", excMsgHeader, luaComp.Type));
            }
        }

        private IntVector2 ToBoardPosition(int luaX, int luaY, string excMsgHeader, string positionName) {
            IntVector2 p = new IntVector2(luaX - 1, luaY - 1);
            if(board.InBounds(db.BoardDimensions, p)) {
                return p;
            } else {
                throw new BadActionException(string.Format("{0} Invalid position for {1}", excMsgHeader, positionName));
            }
        }

        public static void FilterOptionsForPosition(List<int> actionList, TurnOptions allOptions, IntVector2 boardPos) {
            actionList.Clear();
            for(int a = 0; a < allOptions.options.Count; a++) {
                TurnAction action = allOptions.options[a];
                bool addAction = false;
                // Add if final move action ends at boardPos
                int lastMoveIndex = -1;
                for(int i = action.components.Count - 1; i >= 0; i--) {
                    if(action.components[i].type == TurnActionComponentType.MovePiece) {
                        lastMoveIndex = i;
                        break;
                    }
                }
                if(lastMoveIndex >= 0 && action.components[lastMoveIndex].target == boardPos) {
                    addAction = true;
                } else {
                    // Add actions that capture or promote a piece on this position
                    foreach(var component in action.components) {
                        switch(component.type) {
                        case TurnActionComponentType.CapturePiece:
                            if(component.target == boardPos) {
                                addAction = true;
                            }
                            break;
                        case TurnActionComponentType.PromotePiece:
                            if(component.actor == boardPos) {
                                addAction = true;
                            }
                            break;
                        }
                        if(addAction) {
                            break;
                        }
                    }
                }
                if(addAction) {
                    actionList.Add(a);
                }
            }
        }
    }
}
