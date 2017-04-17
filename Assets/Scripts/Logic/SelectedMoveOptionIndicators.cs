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
    public class SelectedMoveOptionIndicators {

        //private Board board;
        //private GameDatabase db;
        private Message<ActionIndicatorPattern> selectedPattern;
        private Query<ActionIndicatorPattern, ActionIndicatorPatternCompileArgs> compiler;

        public SelectedMoveOptionIndicators(AutoController scene) {
            //scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
            //    .Subscribe(new SimpleListener<Board>((b) => board = b));
            //db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            scene.Components.GetOrRegister<Message<TurnChooserHighlight>>
                ((int)ComponentKeys.TurnChooserHighlightChange, Message<TurnChooserHighlight>.Create)
                .Subscribe(new SimpleListener<TurnChooserHighlight>(OnHighlightChange));
            compiler = scene.Components.GetOrRegister<Query<ActionIndicatorPattern, ActionIndicatorPatternCompileArgs>>(
                (int)ComponentKeys.GetActionIndicatorPattern, Query<ActionIndicatorPattern, ActionIndicatorPatternCompileArgs>.Create);

            selectedPattern = scene.Components.GetOrRegister<Message<ActionIndicatorPattern>>
                ((int)ComponentKeys.SelectedIndicatorPattern, Message<ActionIndicatorPattern>.Create);
        }

        private void OnHighlightChange(TurnChooserHighlight hl) {
            if(hl.options != null) {
                selectedPattern.Send(compiler.Send(new ActionIndicatorPatternCompileArgs() {
                    options = hl.options,
                    showOnly = hl.indexesOnSpace,
                    highlightedIndex = hl.highlightedIndex,
                    mouseOverMode = false,
                }));
            } else {
                selectedPattern.Send(null);
            }
        }
    }
}
