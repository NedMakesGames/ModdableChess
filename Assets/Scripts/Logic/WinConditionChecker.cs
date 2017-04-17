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
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModdableChess.Mods.Lua;

namespace ModdableChess.Logic {

    public enum EoTScriptState {
        Undecided=0, FirstPlayerWins, SecondPlayerWins, Tie, Error, Forfeit
    }

    class WinConditionChecker {

        private Board board;
        private GameDatabase db;
        private ScriptController scripts;
        private WinLossHelper luaHelper;
        private DynValue luaHelperValue;
        private Dictionary<int, Mods.Lua.PieceTurnOptions> optionCache;
        private MoveCalcState lastCalcState;
        private Query<TurnOptions, TurnOptionCalculatorArgs> actionCalculator;
        private Query<NextTurnInfo> nextTurnInfo;

        private Query<bool, PieceTurnOptions> turnOptionsHasOptions;
        private Query<bool, PieceTurnOptions, IntVector2> turnOptionsCanMove;
        private Query<bool, PieceTurnOptions, IntVector2> turnOptionsCanCapture;
        private Query<bool, PieceTurnOptions, IntVector2> turnOptionsCanPromote;

        public WinConditionChecker(AutoController scene) {
            scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            scripts = scene.Game.Components.Get<ScriptController>((int)ComponentKeys.LuaScripts);
            actionCalculator = scene.Components.GetOrRegister<Query<TurnOptions, TurnOptionCalculatorArgs>>
                ((int)ComponentKeys.MoveOptionListQuery, Query<TurnOptions, TurnOptionCalculatorArgs>.Create);
            nextTurnInfo = scene.Components.GetOrRegister<Query<NextTurnInfo>>((int)ComponentKeys.NextTurnInfoQuery, Query<NextTurnInfo>.Create);

            lastCalcState = new MoveCalcState();
            optionCache = new Dictionary<int, Mods.Lua.PieceTurnOptions>();
            luaHelper = new Mods.Lua.WinLossHelper();
            luaHelper.QPlayerHasPieces = new Query<bool, string>(CheckHasPieces);
            luaHelper.QGetTurnOptions = new Query<Mods.Lua.PieceTurnOptions, Mods.Lua.Piece>(GetTurnOptions);
            luaHelper.QPlayerCanMove = new Query<bool, string>(CheckCanMove);
            luaHelperValue = UserData.Create(luaHelper);

            turnOptionsHasOptions = new Query<bool, PieceTurnOptions>(CheckTurnOptionsHasAny);
            turnOptionsCanMove = new Query<bool, PieceTurnOptions, IntVector2>(CheckTurnOptionsCanMove);
            turnOptionsCanCapture = new Query<bool, PieceTurnOptions, IntVector2>(CheckTurnOptionsCanCapture);
            turnOptionsCanPromote = new Query<bool, PieceTurnOptions, IntVector2>(CheckTurnOptionsCanPromote);

            scene.Components.GetOrRegister<Query<EoTScriptState>>
                ((int)ComponentKeys.GameOverQuery, Query<EoTScriptState>.Create).Handler = CheckGameOver;
        }

        private EoTScriptState CheckGameOver() {
            EoTScriptState type = EoTScriptState.Undecided;

            NextTurnInfo nextTurn = nextTurnInfo.Send();
            lastCalcState.TurnInfo = LuaTranslator.GetTurnInfo(nextTurn);
            lastCalcState.Board = LuaTranslator.GetBoard(board, db);

            luaHelper.Ready = true;
            scripts.AddGlobal("winCheckHelper", luaHelperValue);
            DynValue ret = scripts.CallFunction(db.WinLossCheck, lastCalcState.Board);
            scripts.RemoveGlobal("winCheckHelper");
            luaHelper.Ready = false;

            foreach(var options in optionCache.Values) {
                options.Dispose();
            }
            optionCache.Clear();

            if(ret.Type != DataType.String) {
                type = EoTScriptState.Error;
            } else {
                switch(ret.String.ToUpperInvariant()) {
                case Mods.Lua.LuaConstants.GameOverFirstWins:
                    type = EoTScriptState.FirstPlayerWins;
                    break;
                case Mods.Lua.LuaConstants.GameOverSecondWins:
                    type = EoTScriptState.SecondPlayerWins;
                    break;
                case Mods.Lua.LuaConstants.GameOverTie:
                    type = EoTScriptState.Tie;
                    break;
                case Mods.Lua.LuaConstants.GameOverUndecided:
                    type = EoTScriptState.Undecided;
                    break;
                default:
                    type = EoTScriptState.Error;
                    break;
                }
            }

            return type;
        }

        private bool CheckHasPieces(string player) {
            PlayerTurnOrder turnOrder = LuaTranslator.GetPlayerFromLua(player);
            for(int p = 0; p < board.Pieces.Count; p++) {
                Piece piece = board.Pieces[p];
                if(!piece.IsCaptured && piece.OwnerPlayer == turnOrder) {
                    return true;
                }
            }
            return false;
        }

        private bool CheckCanMove(string player) {
            PlayerTurnOrder turnOrder = LuaTranslator.GetPlayerFromLua(player);
            for(int p = 0; p < board.Pieces.Count; p++) {
                Piece piece = board.Pieces[p];
                if(!piece.IsCaptured && piece.OwnerPlayer == turnOrder) {
                    PieceTurnOptions luaOptions = CacheTurnOptions(p);
                    if(CheckTurnOptionsHasAny(luaOptions)) {
                        return true;
                    }
                }
            }
            return false;
        }

        private PieceTurnOptions CacheTurnOptions(int logicPieceIndex) {
            PieceTurnOptions luaOptions;
            if(!optionCache.TryGetValue(logicPieceIndex, out luaOptions)) {
                luaOptions = new PieceTurnOptions();
                optionCache[logicPieceIndex] = luaOptions;
                luaOptions.Piece = lastCalcState.Board.Pieces[logicPieceIndex];
                luaOptions.QHasOptions = turnOptionsHasOptions;
                luaOptions.QCanMove = turnOptionsCanMove;
                luaOptions.QCanCapture = turnOptionsCanCapture;
                luaOptions.QCanPromote = turnOptionsCanPromote;

                lastCalcState.MovingPiece = luaOptions.Piece.Index;
                luaOptions.Options = actionCalculator.Send(new TurnOptionCalculatorArgs() {
                    luaState = lastCalcState,
                    pieceIndex = logicPieceIndex,
                });
            }
            return luaOptions;
        }

        private PieceTurnOptions GetTurnOptions(Mods.Lua.Piece piece) {
            return CacheTurnOptions(piece.Index - 1);
        }

        private bool CheckTurnOptionsHasAny(PieceTurnOptions opt) {
            foreach(var option in opt.Options.options) {
                if(option.components.Count > 0) {
                    return true;
                }
            }
            return false;
        }

        private bool CheckTurnOptionsCanMove(PieceTurnOptions opt, IntVector2 p) {
            foreach(var option in opt.Options.options) {
                foreach(var comp in option.components) {
                    if(comp.type == TurnActionComponentType.MovePiece && comp.target.x == p.x - 1 && comp.target.y == p.y - 1) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckTurnOptionsCanCapture(PieceTurnOptions opt, IntVector2 p) {
            foreach(var option in opt.Options.options) {
                foreach(var comp in option.components) {
                    if(comp.type == TurnActionComponentType.CapturePiece && comp.target.x == p.x - 1 && comp.target.y == p.y - 1) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckTurnOptionsCanPromote(PieceTurnOptions opt, IntVector2 p) {
            foreach(var option in opt.Options.options) {
                foreach(var comp in option.components) {
                    if(comp.type == TurnActionComponentType.PromotePiece && comp.actor.x == p.x - 1 && comp.actor.y == p.y - 1) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
