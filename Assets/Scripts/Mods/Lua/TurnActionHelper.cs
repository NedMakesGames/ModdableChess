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

using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Mods.Lua {
    [MoonSharpUserData]
    public class TurnActionHelper {

        [MoonSharpHidden]
        public TurnActionHelper() {

        }

        public string MoveType {
            get {
                return "MOVE";
            }
        }

        public string CaptureType {
            get {
                return "CAPTURE";
            }
        }

        public string PromotionType {
            get {
                return "PROMOTION";
            }
        }

        public TurnActionComponent NewAction() {
            return new TurnActionComponent();
        }

        public TurnActionComponent NewMoveAction(Piece piece, int moveToX, int moveToY) {
            if(piece != null) {
                return NewMoveAction(piece.PositionX, piece.PositionY, moveToX, moveToY);
            } else {
                return null;
            }
        }

        public TurnActionComponent NewMoveAction(int startX, int startY, int moveToX, int moveToY) {
            return new TurnActionComponent() {
                Type = MoveType,
                ActorX = startX,
                ActorY = startY,
                TargetX = moveToX,
                TargetY = moveToY,
            };
        }

        public TurnActionComponent NewCaptureAction(Piece capturePiece) {
            if(capturePiece != null) {
                return NewCaptureAction(capturePiece.PositionX, capturePiece.PositionY);
            } else {
                return null;
            }
        }

        public TurnActionComponent NewCaptureAction(int x, int y) {
            return new TurnActionComponent() {
                Type = CaptureType,
                TargetX = x,
                TargetY = y,
            };
        }

        public TurnActionComponent NewPromoteAction(Piece piece, string promoteToPiece) {
            if(piece != null) {
                return NewPromoteAction(piece.PositionX, piece.PositionY, promoteToPiece);
            } else {
                return null;
            }
        }

        public TurnActionComponent NewPromoteAction(int x, int y, string promoteToPiece) {
            return new TurnActionComponent() {
                Type = PromotionType,
                ActorX = x,
                ActorY = y,
                PromotionPiece = promoteToPiece,
            };
        }
    }
}
