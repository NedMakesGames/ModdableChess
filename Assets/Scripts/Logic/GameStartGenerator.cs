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
using Baluga3.Core;
using System.Collections.Generic;
using UnityEngine.Networking;
using ModdableChess.Mods;
using MoonSharp.Interpreter;
using ModdableChess.Mods.Lua;

namespace ModdableChess.Logic {
    public class BoardSetupException : Exception {
        public BoardSetupException(string m) : base(m) {

        }
    }

    public class GameStartGenerator {

        private LobbyChoices lobby;
        private GameDatabase db;
        private ScriptController scripts;

        public GameStartGenerator(ModLoadingScene scene) {

            lobby = scene.Game.Components.Get<LobbyChoices>((int)ComponentKeys.LobbyChoices);
            db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            scripts = scene.Game.Components.Get<ScriptController>((int)ComponentKeys.LuaScripts);

            scene.Components.GetOrRegister((int)ComponentKeys.CalculateStartConditions, Query<NetworkGameState>.Create)
                .Handler = CalculateStartingConditions;
        }

        private NetworkGameState CalculateStartingConditions() {
            Debug.LogError("Calculating start");

            PlayerTurnOrder hostOrder = PlayerTurnOrder.Undecided;
            switch(lobby.OrderChoice) {
            case LobbyTurnOrderChoice.HostIsFirst:
                hostOrder = PlayerTurnOrder.First;
                break;
            case LobbyTurnOrderChoice.ClientIsFirst:
                hostOrder = PlayerTurnOrder.Second;
                break;
            default: {
                    bool hostFirst = UnityEngine.Random.value < 0.5f;
                    hostOrder = hostFirst ? PlayerTurnOrder.First : PlayerTurnOrder.Second;
                }
                break;
            }

            NetworkGameState gsc = new NetworkGameState() {
                hostPlayerOrder = hostOrder,
                turnNumber = 0,
                activePlayer = PlayerTurnOrder.First,
                turnState = TurnState.None,
            };

            SetupBoard lBoard = new SetupBoard();
            lBoard.Width = db.BoardDimensions.x;
            lBoard.Length = db.BoardDimensions.y;
            ScriptRuntimeException luaException;
            scripts.CallFunction(db.SetupFunction, lBoard, out luaException);
            if(luaException != null) {
                throw new BoardSetupException(string.Format("Board setup exception: exception in Lua code: {0}", luaException.DecoratedMessage));
            }

            if(lBoard.NumPieces == 0) {
                throw new BoardSetupException("Board setup exception: No piece created");
            }
            HashSet<IntVector2> takenPositions = new HashSet<IntVector2>();
            bool anyPlayer1 = false;
            bool anyPlayer2 = false;

            List<NetworkGameStatePiece> pieceList = new List<NetworkGameStatePiece>();
            for(int p = 1; p <= lBoard.NumPieces; p++) {
                SetupPiece lPiece = lBoard.GetPiece(p);
                IntVector2 pos = TranslatePosition(lPiece, p, takenPositions);
                PlayerTurnOrder player = TranslatePlayer(lPiece, p);
                switch(player) {
                case PlayerTurnOrder.First:
                    anyPlayer1 = true;
                    break;
                case PlayerTurnOrder.Second:
                    anyPlayer2 = true;
                    break;
                }
                int prototype = TranslatePrototype(lPiece, p);
                NetworkGameStatePiece piece = new NetworkGameStatePiece() {
                    prototypeID = prototype,
                    owner = player,
                    position = pos,
                    isCaptured = false,
                    numberMovesTaken = 0,
                };
                pieceList.Add(piece);
            }

            if(!anyPlayer1) {
                throw new BoardSetupException("Board setup exception: First player given no pieces");
            } else if(!anyPlayer2) {
                throw new BoardSetupException("Board setup exception: Second player given no pieces");
            }
            
            gsc.pieces = pieceList.ToArray();

            return gsc;
        }

        private IntVector2 TranslatePosition(SetupPiece lPiece, int index, HashSet<IntVector2> takenPositions) {
            IntVector2 pos = new IntVector2(lPiece.PositionX - 1, lPiece.PositionY - 1);
            if(pos.x >= 0 && pos.x < db.BoardDimensions.x && pos.y >= 0 && pos.y < db.BoardDimensions.y) {
                if(!takenPositions.Contains(pos)) {
                    return pos;
                } else {
                    throw new BoardSetupException(string.Format("Board setup exception: Piece {0} given already filled position", index));
                }
            } else {
                throw new BoardSetupException(string.Format("Board setup exception: Piece {0} given invalid position", index));
            }
        }

        private PlayerTurnOrder TranslatePlayer(SetupPiece lPiece, int index) {
            switch(lPiece.Owner.ToUpperInvariant()) {
            case "FIRST":
            case "WHITE":
                return PlayerTurnOrder.First;
            case "SECOND":
            case "LAST":
            case "BLACK":
                return PlayerTurnOrder.Second;
            default:
                throw new BoardSetupException(string.Format("Board setup exception: Piece {0} given invalid owner \"{1}\"", index, lPiece.Owner));
            }
        }

        private int TranslatePrototype(SetupPiece lPiece, int index) {
            int protoID = db.PieceNameToIndex(lPiece.PieceName);
            if(protoID < 0) {
                throw new BoardSetupException(string.Format("Board setup exception: Piece {0} given invalid name \"{1}\"", index, lPiece.PieceName));
            }
            return protoID;
        }
    }
}
