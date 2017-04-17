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
using ModdableChess.Logic;
using ModdableChess.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModdableChess.Mods {
    public class ModTranslator : MonoBehaviour {

        private class ModException : Exception {
            public ModException(string msg) : base(msg) {

            }
        }

        private void Start() {
            GameLink.Game.SceneComponents.GetOrRegister<Command<LoadedMod>>((int)ComponentKeys.ModAssetsLoaded, Command<LoadedMod>.Create)
                .Handler = Translate;
        }

        private void Translate(LoadedMod loaded) {
            try {
                Debug.Log("Mod started translation");
                GameDatabase db = GameLink.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
                db.ModFolder = GameLink.Game.Components.Get<LobbyChoices>((int)ComponentKeys.LobbyChoices).ModFolder;

                TranslateBoardSize(loaded, db);

                Dictionary<string, int> behaviorIndexMap = new Dictionary<string, int>();
                if(db.PiecePrototypes == null) {
                    db.PiecePrototypes = new List<Logic.PiecePrototype>();
                }
                db.PiecePrototypes.Clear();

                Dictionary<string, int> modelNameMap = new Dictionary<string, int>();
                db.ModelPrefabs = new List<GameObject>();

                db.BoardModelIndex = SetAndMapModel(modelNameMap, db, loaded, loaded.toc.boardModel);
                TranslateActionIndicators(modelNameMap, db, loaded);

                for(int i = 0; i < loaded.toc.pieces.Length; i++) {
                    TranslatePiece(loaded, db, behaviorIndexMap, modelNameMap, i);
                }

                TranslateBoardSetup(loaded, db, behaviorIndexMap);
                TranslateWinCondition(loaded, db);

                Debug.Log("Mod finished translation");
                GameLink.Game.SceneComponents.Get<Message>((int)ComponentKeys.ModTranslationComplete).Send();
            } catch (ModException mx) {
                GameLink.Game.SceneComponents.Get<Message<string>>((int)ComponentKeys.ModLoadError).Send(mx.Message);
            }
        }

        private static void TranslateBoardSize(LoadedMod loaded, GameDatabase db) {
            if(loaded.toc.boardSize.width <= 1) {
                throw new ModException("Invalid board width");
            }
            if(loaded.toc.boardSize.length <= 1) {
                throw new ModException("Invalid board length");
            }
            db.BoardDimensions = new Baluga3.Core.IntVector2(loaded.toc.boardSize.width, loaded.toc.boardSize.length);
            if(loaded.toc.worldTileSize <= 0) {
                throw new ModException("Invalid world tile size");
            }
            db.WorldTileSize = loaded.toc.worldTileSize;
        }

        private void TranslatePiece(LoadedMod loaded, GameDatabase db, Dictionary<string, int> behaviorIndexMap, Dictionary<string, int> modelNameMap, int pieceIndex) {
            Piece p = loaded.toc.pieces[pieceIndex];
            Logic.PiecePrototype lpb = new Logic.PiecePrototype();
            Mods.Piece mpb = loaded.toc.pieces[pieceIndex];
            db.PiecePrototypes.Add(lpb);
            behaviorIndexMap[p.name] = db.PiecePrototypes.Count - 1;
            lpb.LuaTag = p.name;
            lpb.FirstModelIndex = SetAndMapModel(modelNameMap, db, loaded, p.player1Model);
            lpb.SecondModelIndex = SetAndMapModel(modelNameMap, db, loaded, p.player2Model);
            if(string.IsNullOrEmpty(p.promoteIndicatorModel)) {
                lpb.PromotionIndicatorModelIndex = -1;
            } else {
                lpb.PromotionIndicatorModelIndex = SetAndMapModel(modelNameMap, db, loaded, p.promoteIndicatorModel);
            }
            Debug.Log("Want function " + mpb.actionOptionFunction);
            lpb.MovementFunction = loaded.luaFunctions[mpb.actionOptionFunction];
        }

        private int SetAndMapModel(Dictionary<string, int> map, GameDatabase db, LoadedMod loaded, string name) {
            int index;
            if(!map.TryGetValue(name, out index)) {
                GameObject model = loaded.assets[name] as GameObject;
                if(model == null) {
                    throw new ModException(string.Format("Cannot cast \"{0}\" into a GameObject", name));
                }
                db.ModelPrefabs.Add(model);
                index = db.ModelPrefabs.Count - 1;
                map[name] = index;
            }
            return index;
        }

        //private static void TranslatePromotion(LoadedMod loaded, GameDatabase db, Dictionary<string, int> behaviorIndexMap, int pieceIndex) {
        //    Piece p = loaded.toc.pieces[pieceIndex];
        //    Logic.PiecePrototype lpb = db.PiecePrototypes[behaviorIndexMap[p.name]];
        //    if(string.IsNullOrEmpty(p.canPromoteTo)) {
        //        lpb.PromotionID = -1;
        //    } else {
        //        int promotionID;
        //        if(behaviorIndexMap.TryGetValue(p.canPromoteTo, out promotionID)) {
        //            lpb.PromotionID = promotionID;
        //        } else {
        //            throw new ModException(string.Format("Piece promotion \"{0}\" refers to non-existant piece \"{1}\"", p.name, p.canPromoteTo));
        //        }
        //    }
        //}

        private static void TranslateBoardSetup(LoadedMod loaded, GameDatabase db, Dictionary<string, int> behaviorIndexMap) {
            //Logic.BoardSetup lsetup = new Logic.BoardSetup();
            //Mods.BoardSetup msetup = (Mods.BoardSetup)loaded.xmlObjects[loaded.toc.boardSetup];
            //switch(msetup.setupType.ToUpperInvariant()) {
            //case "DETERMINED":
            //    lsetup.setupType = BoardSetupType.Determined;
            //    break;
            //default:
            //    throw new ModException(string.Format("Unknown board setup type \"{0}\"", msetup.setupType));
            //}

            //if(msetup.pieces == null || msetup.pieces.Length == 0) {
            //    throw new ModException(string.Format("Board setup does not include any pieces"));
            //}
            //lsetup.spaces = new BoardSpaceSetup[msetup.pieces.Length];
            //bool noPlayer1 = true, noPlayer2 = true;
            //for(int i = 0; i < msetup.pieces.Length; i++) {
            //    BoardSpaceSetup lpiece = new BoardSpaceSetup();
            //    BoardSetupPiece mpiece = msetup.pieces[i];
            //    if(mpiece == null) {
            //        throw new ModException(string.Format("Board setup includes null piece at index {0}", i));
            //    }
            //    lsetup.spaces[i] = lpiece;
            //    int behaviorIndex;
            //    if(behaviorIndexMap.TryGetValue(mpiece.pieceName, out behaviorIndex)) {
            //        lpiece.behavior = behaviorIndex;
            //    } else {
            //        throw new ModException(string.Format("Board setup piece {0} references unknown piece \"{1}\"", i, mpiece.pieceName));
            //    }

            //    switch(mpiece.player.ToUpperInvariant()) {
            //    case "1":
            //    case "WHITE":
            //    case "FIRST":
            //        lpiece.owner = PlayerTurnOrder.First;
            //        noPlayer1 = false;
            //        break;
            //    case "2":
            //    case "BLACK":
            //    case "SECOND":
            //        lpiece.owner = PlayerTurnOrder.Second;
            //        noPlayer2 = false;
            //        break;
            //    default:
            //        throw new ModException(string.Format("Board setup piece {0} references unknown player owner type \"{1}\"", i, mpiece.player));
            //    }

            //    if(mpiece.xPosition < 0 || mpiece.xPosition >= db.BoardDimensions.x || mpiece.yPosition < 0 || mpiece.yPosition >= db.BoardDimensions.y) {
            //        throw new ModException(string.Format("Board setup piece {0} has invalid board position", i));
            //    }
            //    lpiece.position = new Baluga3.Core.IntVector2(mpiece.xPosition, mpiece.yPosition);
            //}

            //if(noPlayer1 || noPlayer2) {
            //    throw new ModException(string.Format("Board setup does not have at least one piece for each player"));
            //}

            //db.BoardSetup = lsetup;

            db.SetupFunction = loaded.luaFunctions[loaded.toc.boardSetupFunction];
        }

        private static void TranslateWinCondition(LoadedMod loaded, GameDatabase db) {
            //switch(loaded.toc.winLossType.ToUpperInvariant()) {
            //case "CHESS":
            //case "CHECKMATE":
            //case "CAPTURETHEKING":
            //    db.WinLoss = WinLossType.CaptureTheKing;
            //    break;
            //case "CHECKERS":
            //case "ELIMINATION":
            //    db.WinLoss = WinLossType.Elimination;
            //    break;
            //default:
            //    throw new ModException(string.Format("Unknown win/loss type \"{0}\"", loaded.toc.winLossType));
            //}

            //if(db.WinLoss == WinLossType.CaptureTheKing) {
            //    bool noKing = true;
            //    foreach(var behavior in db.PiecePrototypes) {
            //        if(behavior.IsKing) {
            //            noKing = false;
            //            break;
            //        }
            //    }
            //    if(noKing) {
            //        throw new ModException(string.Format("Win/loss type requires at least one piece be flagged as a king"));
            //    }
            //}

            db.WinLossCheck = loaded.luaFunctions[loaded.toc.winLossFunction];
        }

        private void TranslateActionIndicators(Dictionary<string, int> modelNameMap, GameDatabase db, LoadedMod loaded) {
            IndicatorModelData d = new IndicatorModelData();
            db.Indicators = d;
            d.Full = new int[3];
            d.Small = new int[d.Full.Length];
            d.Mouseover = new int[d.Full.Length];

            for(int i = 0; i < d.Full.Length; i++) {
                d.Full[i] = -1;
                d.Small[i] = -1;
                d.Mouseover[i] = -1;
            }
            
            for(int i = 0; i < loaded.toc.actionIndicators.Length; i++) {
                int typeIndex = -1;
                int[] strengthArray = d.Full;
                ActionIndicator mai = loaded.toc.actionIndicators[i];
                switch(mai.type.ToUpperInvariant()) {
                case "MOVE":
                    typeIndex = 0;
                    break;
                case "CAPTURE":
                    typeIndex = 1;
                    break;
                case "PROMOTE":
                case "PROMOTION":
                    typeIndex = 2;
                    break;
                default:
                    throw new ModException(string.Format("Unknown indicator type \"{1}\" at position {0}", i, mai.type));
                }
                if(!string.IsNullOrEmpty(mai.strength)) {
                    switch(mai.strength.ToUpperInvariant()) {
                    case "CHOSEN":
                    case "ACTIVE":
                    case "DEFAULT":
                    case "SELECTED":
                        break;
                    case "UNSELECTED":
                    case "INACTIVE":
                    case "SMALL":
                        strengthArray = d.Small;
                        break;
                    case "MOUSEOVER":
                        strengthArray = d.Mouseover;
                        break;
                    default:
                        throw new ModException(string.Format("Unknown indicator strength \"{1}\" at position {0}", i, mai.strength));
                    }
                }
                if(strengthArray[typeIndex] >= 0) {
                    throw new ModException(string.Format("Indicator already registered for type {0} and strength {1}, duplicate at position {2}",
                        mai.type, string.IsNullOrEmpty(mai.strength) ? "default" : mai.strength, i));
                }
                strengthArray[typeIndex] = SetAndMapModel(modelNameMap, db, loaded, mai.model);
            }

            for(int i = 0; i < d.Full.Length; i++) {
                if(d.Full[i] < 0) {
                    throw new ModException(string.Format("No indicator registered for type {0}", new string[] { "move", "capture", "promote" }[i]));
                }
            }

            if(loaded.toc.indicatorStackingHeight > 0) {
                db.Indicators.StackingHeight = loaded.toc.indicatorStackingHeight;
            } else {
                db.Indicators.StackingHeight = 1;
            }
        }
    }
}
