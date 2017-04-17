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
using System;
using Baluga3.GameFlowLogic;
using ModdableChess.Logic;

namespace ModdableChess.Unity {
    public class SimplePieceMover : MonoBehaviour {

        private PieceModelCreator models;
        private Board board;
        private GameDatabase db;

        private void Start() {
            ModdableChessGame game = GameLink.Game;
            models = FindObjectOfType<PieceModelCreator>();
            game.SceneComponents.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
                .Subscribe(new SimpleListener<Board>((b) => board = b));
            board = game.Components.Get<Board>((int)ComponentKeys.GameBoard);
            db = game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);

            game.SceneComponents.Get<Message<int>>((int)ComponentKeys.PieceMovedEvent).Subscribe(new SimpleListener<int>(OnPieceMove));
            game.SceneComponents.Get<Message<int>>((int)ComponentKeys.PieceCapturedEvent).Subscribe(new SimpleListener<int>(OnPieceCapture));
        }

        private void OnPieceMove(int pieceIndex) {
            PieceModel model = models.GetModelObject(pieceIndex);
            Piece data = board.Pieces[pieceIndex];
            model.transform.position = PiecePositionHelper.FigureCenter(db, data.BoardPosition);
        }

        private void OnPieceCapture(int pieceIndex) {
            PieceModel model = models.GetModelObject(pieceIndex);
            GameObject.Destroy(model.gameObject);
        }
    }
}
