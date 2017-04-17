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
using ModdableChess.Unity;
using Baluga3.Input;

namespace ModdableChess.Logic {

    public class TurnChooser : ITicking {

        public enum State {
            Inactive, ChoosePiece, PieceChosenWait, ChooseAction,
        }

        //private GameDatabase db;
        //private Board board;
        private SubscribableBool backBtn;
        private StateMachine states;
        private SubscribableInt chosenPiece;
        private SubscribableObject<TurnAction> chosenAction;
        private Message<TurnAction> choiceMadeEvent;
        private Command<TurnAction> requestSendAction;
        private TickingJanitor tickHelper;
        private StateMachine connectionState;

        public TurnChooser(ChessScene scene) {
            //scene.Components.GetOrRegister<Message<Board>>((int)ComponentKeys.BoardCreatedMessage, Message<Board>.Create)
            //    .Subscribe(new SimpleListener<Board>((b) => board = b));
            //db = scene.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);

            tickHelper = new TickingJanitor(scene.Game, this);
            tickHelper.ForceInactive = true;
            scene.ActivatableList.Add(tickHelper);

            states = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.TurnChooserState, StateMachine.Create);
            states.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnEnterChoosingState()));

            backBtn = scene.Components.GetOrRegister<SubscribableBool>((int)ComponentKeys.PlayerSelectionCancelBtn, SubscribableBool.Create);
            backBtn.Subscribe(new TriggerListener(OnBackBtnTrigger));
            choiceMadeEvent = scene.Components.GetOrRegister<Message<TurnAction>>((int)ComponentKeys.MoveChoiceMadeEvent, Message<TurnAction>.Create);
            chosenPiece = scene.Components.GetOrRegister<SubscribableInt>((int)ComponentKeys.TurnChooserChosenPiece, SubscribableInt.Create);
            chosenPiece.Subscribe(new SimpleListener<int>(OnPieceChosen));
            chosenAction = scene.Components.GetOrRegister<SubscribableObject<TurnAction>>
                ((int)ComponentKeys.TurnChooserChosenAction, SubscribableObject<TurnAction>.Create);
            chosenAction.Subscribe(new SimpleListener<TurnAction>(OnActionChosen));

            scene.Components.GetOrRegister<Command>((int)ComponentKeys.StartTurnChoosingCommand, Command.Create)
                .Handler = ChoosingStartCommand;
            scene.Components.GetOrRegister<Command>((int)ComponentKeys.StopTurnChoosingCommand, Command.Create)
                .Handler = ChoosingStopCommand;
            scene.Components.GetOrRegister<Command>((int)ComponentKeys.ForfeitGame, Command.Create)
                .Handler = ForfeitGameCommand;

            requestSendAction = scene.Game.Components.Get<Command<TurnAction>>((int)ComponentKeys.SendTurnAction);

            connectionState = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.ReconnectionState, StateMachine.Create);
            connectionState.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnConnectionStateChange()));

            new PieceChooser(scene);
            new BoardSpaceChooser(scene);
        }

        private void OnEnterChoosingState() {
            State s = (State)states.State;
            BalugaDebug.Log(string.Format("Chooser state enter {0}", s));
            tickHelper.ForceInactive = s != State.PieceChosenWait;
            if(s == State.Inactive) {
                chosenPiece.Value = -1;
                //chosenAction.Value = null;
            }
        }

        public void Tick(float deltaTime) {
            states.State = (int)State.ChooseAction;
        }

        private void OnConnectionStateChange() {
            if((State)states.State != State.Inactive && connectionState.State != (int)ReconnectionState.Connected) {
                chosenPiece.Value = -1;
                chosenAction.Value = null;
                states.State = (int)State.Inactive;
            }
        }

        private void ChoosingStartCommand() {
            switch((State)states.State) {
            case State.Inactive:
                chosenPiece.Value = -1;
                chosenAction.Value = null;
                states.State = (int)State.ChoosePiece;
                break;
            //default:
            //    BalugaDebug.Log("Already choosing!");
            //    break;
            }
        }

        private void OnBackBtnTrigger() {
            if(states.State == (int)State.ChooseAction) {
                chosenPiece.Value = -1;
                states.State = (int)State.ChoosePiece;
            }
        }

        private void OnPieceChosen(int pieceIndex) {
            if(states.State == (int)State.ChoosePiece && pieceIndex >= 0) {
                states.State = (int)State.PieceChosenWait;
            }
        }

        private void OnActionChosen(TurnAction action) {
            if(states.State == (int)State.ChooseAction && action != null) {
                states.State = (int)State.Inactive;
                choiceMadeEvent.Send(action);
                requestSendAction.Send(action);
            }
        }

        private void ForfeitGameCommand() {
            switch((State)states.State) {
            case State.ChoosePiece:
            case State.PieceChosenWait:
            case State.ChooseAction:
                TurnAction action = new TurnAction();
                action.components = new List<TurnActionComponent>();
                action.components.Add(new TurnActionComponent() {
                    type = TurnActionComponentType.Forfeit,
                });
                action.searchPiece = new IntVector2(-1, -1);
                choiceMadeEvent.Send(action);
                requestSendAction.Send(action);
                break;
            }
        }

        private void ChoosingStopCommand() {
            states.State = (int)State.Inactive;
        }
    }
}
