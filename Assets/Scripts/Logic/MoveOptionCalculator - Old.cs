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

//using Baluga3.Core;
//using Baluga3.GameFlowLogic;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace ModdableChess.Logic {
//    public struct MoveOptionCalcArgs {
//        public int selfPiece;
//        public int behaviorID;
//        public IntVector2 position;
//        public int numPieceMoves;
//        public int numTurnMoves;
//        public PlayerTurnOrder ownerPlayer;
//        public bool checkForChecks;
//        public PlayerTurnOrder activePlayer;

//        public static MoveOptionCalcArgs FromCurrent(int id, Piece piece, int turnMoves, PlayerTurnOrder activePlayer, bool checkForChecks) {
//            return new MoveOptionCalcArgs() {
//                selfPiece = id,
//                behaviorID = piece.BehaviorID,
//                position = piece.BoardPosition,
//                numPieceMoves = piece.NumberOfMoves,
//                numTurnMoves = turnMoves,
//                ownerPlayer = piece.OwnerPlayer,
//                checkForChecks = checkForChecks,
//                activePlayer = activePlayer,
//            };
//        }
//    }

//    public class MoveOptionCalculator {

//        private static readonly int DEPTH = 1000;

//        private Query<bool, CheckCalcArgs> checkQuery;
//        private GameDatabase db;
//        private Board board;

//        private HashSet<IntVector2> visitedSpaces;

//        public MoveOptionCalculator(ChessScene scene) {
//            visitedSpaces = new HashSet<IntVector2>();

//            db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
//            scene.SceneComponents.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
//                .Subscribe(new SimpleListener<Board>((b) => board = b));

//            scene.SceneComponents.GetOrRegister<Query<List<MoveOption>, MoveOptionCalcArgs>>
//                ((int)ComponentKeys.MoveOptionListQuery, Query<List<MoveOption>, MoveOptionCalcArgs>.Create)
//                .Handler = CalculateMoveOptions;
//            checkQuery = scene.SceneComponents.GetOrRegister<Query<bool, CheckCalcArgs>>
//                ((int)ComponentKeys.CheckQuery, Query<bool, CheckCalcArgs>.Create);
//        }

//        private List<MoveOption> CalculateMoveOptions(MoveOptionCalcArgs args) {
//            List<MoveOption> finalOptions = new List<MoveOption>();
//            PieceBehavior behavior = db.Behaviours[args.behaviorID];

//            if(HasMoveOptions(args)) {
//                visitedSpaces.Add(args.position);
//                for(int t = 0; t < behavior.MoveType.Tables.Length; t++) {
//                    RecursiveCalculateFromTables(args, args.position, t, t, 0, finalOptions);
//                }
//                visitedSpaces.Clear();

//                if(args.checkForChecks) {
//                    for(int i = finalOptions.Count - 1; i >= 0; i--) {
//                        IntVector2 space = finalOptions[i].space;
//                        if(checkQuery.Send(new CheckCalcArgs(args.selfPiece, space, args.activePlayer))) {
//                            finalOptions.RemoveAt(i);
//                            //BalugaDebug.Log("Removing space from possible moves " + space);
//                        }
//                    }
//                }
//            }

//            return finalOptions;
//        }

//        private bool HasMoveOptions(MoveOptionCalcArgs args) {
//            if(db.WinLoss == WinLossType.StrictCheckMate) {
//                return args.activePlayer != args.ownerPlayer || db.Behaviours[args.behaviorID].IsKing || !AnyKingInCheck(args.ownerPlayer, args.activePlayer);
//            } else {
//                return true;
//            }
//        }

//        private bool AnyKingInCheck(PlayerTurnOrder player, PlayerTurnOrder activePlayer) {
//            for(int p = 0; p < board.Pieces.Count; p++) {
//                Piece piece = board.Pieces[p];
//                if(db.Behaviours[piece.BehaviorID].IsKing && piece.OwnerPlayer == player && checkQuery.Send(new CheckCalcArgs(p, piece.BoardPosition, activePlayer))) {
//                    return true;
//                }
//            }
//            return false;
//        }

//        private void RecursiveCalculateFromTables(MoveOptionCalcArgs args, IntVector2 usingPos, int tableIndex, int originalTable, int depth, List<MoveOption> finalOptions) {
//            if(depth >= DEPTH) {
//                return;
//            }
//            PieceMovementData.Table table = db.Behaviours[args.behaviorID].MoveType.Tables[tableIndex];
//            List<IntVector2> possibleFromTable = new List<IntVector2>();
//            if(CalculateFromTable(possibleFromTable, args, table, usingPos)) {
//                foreach(var space in possibleFromTable) {
//                    //BalugaDebug.Log(string.Format("{0} - {1}", depth, space));
//                    if(!visitedSpaces.Contains(space)) {
//                        visitedSpaces.Add(space);
//                        finalOptions.Add(new MoveOption() {
//                            space = space,
//                            behaviorMoveTable = originalTable,
//                        });
//                        foreach(var t in table.repeatWithTables) {
//                            RecursiveCalculateFromTables(args, space, t, originalTable, depth + 1, finalOptions);
//                        }
//                    }
//                }
//            }
//        }

//        private bool CalculateFromTable(List<IntVector2> possible, MoveOptionCalcArgs args, PieceMovementData.Table table, IntVector2 usingPos) {
//            IntVector2 pieceOffset = IntVector2.Zero;
//            for(int i = 0; i < table.options.Length; i++) {
//                if(table.options[i].isSelf) {
//                    pieceOffset = new IntVector2(i % table.columns, i / table.columns);
//                    break;
//                }
//            }

//            if(table.beforePieceMoves > 0 && args.numPieceMoves >= table.beforePieceMoves) {
//                return false;
//            } else if(table.beforeTurnMoves > 0 && args.numTurnMoves >= table.beforeTurnMoves) {
//                return false;
//            }

//            //BalugaDebug.Log(string.Format("Start search table position {0} offset {1}", usingPos, pieceOffset));

//            bool isTablePossible = true;
//            for(int i = 0; i < table.options.Length; i++) {
//                IntVector2 optSpace = new IntVector2(i % table.columns, i / table.columns) - pieceOffset;
//                if(args.ownerPlayer == PlayerTurnOrder.Second) {
//                    optSpace = -optSpace;
//                }
//                IntVector2 space = optSpace + usingPos;
//                PieceMovementData.Option opt = table.options[i];
//                if(!opt.isSelf && board.InBounds(db.BoardDimensions, space)) {
//                    int pieceOnSpace = board.PieceOnSpace(space);
//                    if(pieceOnSpace == args.selfPiece) {
//                        pieceOnSpace = -1;
//                    }

//                    bool isOptionSatified = false;
//                    if((opt.filledReqs & PieceMovementData.SpaceFilled.Open) != 0 && pieceOnSpace < 0) {
//                        isOptionSatified = true;
//                    } else if((opt.filledReqs & PieceMovementData.SpaceFilled.Enemy) != 0 && pieceOnSpace >= 0 && board.Pieces[pieceOnSpace].OwnerPlayer != args.ownerPlayer) {
//                        isOptionSatified = true;
//                    } else if((opt.filledReqs & PieceMovementData.SpaceFilled.Ally) != 0 && pieceOnSpace >= 0 && board.Pieces[pieceOnSpace].OwnerPlayer == args.ownerPlayer) {
//                        isOptionSatified = true;
//                    }

//                    //BalugaDebug.Log(string.Format("Option {0}, position {1} possible {2} reqs {3}", i, space, isOptionSatified, opt.filledReqs));

//                    if(isOptionSatified) {
//                        if(opt.moveTo) {
//                            possible.Add(space);
//                        }
//                    } else {
//                        isTablePossible = false;
//                        break;
//                    }
//                }
//            }

//            if(possible.Count == 0) {
//                isTablePossible = false;
//            }

//            if(!isTablePossible) {
//                possible.Clear();
//            }
//            return isTablePossible;
//        }
//    }
//}
