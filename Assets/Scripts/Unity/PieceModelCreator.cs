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

namespace ModdableChess.Unity {
    public class PieceModelCreator : MonoBehaviour {

        private GameDatabase db;
        private Board board;
        private Dictionary<int, PieceModel> modelGObjs;
        private int pieceLayer; 

        private void Start() {
            ModdableChessGame game = GameLink.Game;
            game.SceneComponents.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            board = game.Components.Get<Board>((int)ComponentKeys.GameBoard);
            db = game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            modelGObjs = new Dictionary<int, PieceModel>();
            pieceLayer = LayerMask.NameToLayer("Pieces");

            game.SceneComponents.GetOrRegister<Message<int>>((int)ComponentKeys.PiecePromotionEvent, Message<int>.Create)
                .Subscribe(new SimpleListener<int>(DoPromotion));

            CreateInitialPieces();
        }

        private void DoPromotion(int piece) {
            PieceModel oldModel = GetModelObject(piece);
            GameObject.Destroy(oldModel.gameObject);
            CreateModelForPiece(piece);
        }

        private void CreateInitialPieces() {
            for(int id = 0; id < board.Pieces.Count; id++) {
                CreateModelForPiece(id);
            }
        }

        private void CreateModelForPiece(int id) {
            Piece piece = board.Pieces[id];
            PiecePrototype behavior = db.PiecePrototypes[piece.PrototypeID];
            int modelIndex = 0;
            if(piece.OwnerPlayer == PlayerTurnOrder.First) {
                modelIndex = behavior.FirstModelIndex;
            } else if(piece.OwnerPlayer == PlayerTurnOrder.Second) {
                modelIndex = behavior.SecondModelIndex;
            }
            if(modelIndex > db.ModelPrefabs.Count) {
                modelIndex = 0;
            }
            GameObject gobj = (GameObject)Instantiate(db.ModelPrefabs[modelIndex]);
            gobj.layer = pieceLayer;
            foreach(var child in gobj.GetComponentsInChildren<Transform>()) {
                child.gameObject.layer = pieceLayer;
            }
            PieceModel model = gobj.AddComponent<PieceModel>();
            model.PieceID = id;
            gobj.transform.position = PiecePositionHelper.FigureCenter(db, piece.BoardPosition);
            gobj.transform.rotation = Quaternion.Euler(0, piece.OwnerPlayer == PlayerTurnOrder.Second ? 180 : 0, 0);
            modelGObjs[id] = model;
        }

        public PieceModel GetModelObject(int index) {
            return modelGObjs[index];
        }
    }
}
