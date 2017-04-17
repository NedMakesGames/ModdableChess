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

namespace ModdableChess.Logic {
    public class ChessScene : AutoController {

        public ChessScene(ModdableChessGame game) : base(game) {

            new MatchManager(this);
            new PieceMover(this);
            new TurnOptionCalculator(this);
            new TurnManager(this);
            new TurnChooser(this);
            new WinConditionChecker(this);
            new PromotionManager(this);
            new SelectedMoveOptionIndicators(this);
            new MouseOverMoveOptionIndicators(this);
            new ReconnectionManager(this);
            new GameStateCompiler(this);
            new BoardCreator(this);
            new OpponentTurnActionWatcher(this);
            new ActionIndicatorPatternCompiler(this);
            new CaptureManager(this);
            new PlayUI.ForfeitButtonController(this);
            new PlayUI.GameOverScreen(this);
            new PlayUI.ActiveTurnDisplay(this);
            new PlayUI.CameraController(this);

            Components.GetOrRegister<Command>((int)ComponentKeys.EndScreenExitPressed, Command.Create)
                .Handler = ExitToLobby;
        }

        public override void Enter() {
            base.Enter();
            UnityEngine.SceneManagement.SceneManager.LoadScene("PlayScene");
        }

        private void ExitToLobby() {
            GameDatabase db = Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            db.Dispose();
            Game.Controller = new Lobby.LobbyScene((ModdableChessGame)Game);
        }
    }
}
