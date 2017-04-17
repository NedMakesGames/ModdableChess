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

namespace ModdableChess.Logic.Lobby {

    public class TurnOrderSelectionController : ITicking {

        private StateMachine screen;
        private bool isHost;
        private SubscribableBool[] toggleValues;
        private SubscribableBool allowTogglesOff;
        private SubscribableObject<string> otherSelLabel, otherSelValue;
        private int selectedToggle;
        private Command<LobbyTurnOrderChoice> reqSendPick;
        private SubscribableInt selected;
        private StateMachine initStatus;
        private LobbyChoices prevChoices;
        private Dictionary<LobbyTurnOrderChoice, int> choiceBtnIndex;
        private TickingJanitor tickHelper;
        private SubscribableBool readyStatus;
        private SubscribableBool btnsInteractable;

        public TurnOrderSelectionController(AutoController scene) {

            tickHelper = new TickingJanitor(scene.Game, this);
            tickHelper.Active = false;

            ComponentRegistry guiComps = scene.Components.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages);

            btnsInteractable = guiComps.GetOrRegister<SubscribableBool>((int)GUICompKeys.TOSInteractable, SubscribableBool.Create);
            btnsInteractable.Value = true;

            toggleValues = new SubscribableBool[3];
            toggleValues[0] = new SubscribableBool(false);
            guiComps.Register((int)GUICompKeys.TOSOpt1Set, toggleValues[0]);
            toggleValues[1] = new SubscribableBool(false);
            guiComps.Register((int)GUICompKeys.TOSOpt2Set, toggleValues[1]);
            toggleValues[2] = new SubscribableBool(false);
            guiComps.Register((int)GUICompKeys.TOSOpt3Set, toggleValues[2]);

            Message<bool>[] toggleChanges = new Message<bool>[3];
            toggleChanges[0] = new Message<bool>();
            guiComps.Register((int)GUICompKeys.TOSOpt1Change, toggleChanges[0]);
            toggleChanges[1] = new Message<bool>();
            guiComps.Register((int)GUICompKeys.TOSOpt2Change, toggleChanges[1]);
            toggleChanges[2] = new Message<bool>();
            guiComps.Register((int)GUICompKeys.TOSOpt3Change, toggleChanges[2]);
            for(int i = 0; i < toggleChanges.Length; i++) {
                int index = i;
                toggleChanges[i].Subscribe(new SimpleListener<bool>((v) => OnToggleChange(index, v)));
            }

            choiceBtnIndex = new Dictionary<LobbyTurnOrderChoice, int>();
            choiceBtnIndex[LobbyTurnOrderChoice.HostIsFirst] = 0;
            choiceBtnIndex[LobbyTurnOrderChoice.ClientIsFirst] = 1;
            choiceBtnIndex[LobbyTurnOrderChoice.Random] = 2;
            choiceBtnIndex[LobbyTurnOrderChoice.None] = -1;

            otherSelLabel = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.TOSOtherSelectionLabel, otherSelLabel);
            otherSelValue = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.TOSOtherSelectionValue, otherSelValue);

            allowTogglesOff = new SubscribableBool(false);
            guiComps.Register((int)GUICompKeys.TOSAllowAllTogglesOff, allowTogglesOff);

            prevChoices = scene.Game.Components.Get<LobbyChoices>((int)ComponentKeys.LobbyChoices);

            selected = new SubscribableInt(0);
            //scene.SceneComponents.Register((int)ComponentKeys.LobbyTurnOrderSelected, selected);

            initStatus = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.LobbyInitStatus, StateMachine.Create);
            initStatus.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => InitStatusChange()));

            reqSendPick = scene.Game.Components.Get<Command<LobbyTurnOrderChoice>>((int)ComponentKeys.LobbyTurnOrderChoiceNotify);
            scene.ActivatableList.Add(new ListenerJanitor<IListener<LobbyTurnOrderChoice>>(
                scene.Game.Components.Get<IMessage<LobbyTurnOrderChoice>>((int)ComponentKeys.LobbyTurnOrderChoiceReceived),
                new SimpleListener<LobbyTurnOrderChoice>(OnNetworkPickReceived)));

            readyStatus = scene.Components.GetOrRegister<SubscribableBool>((int)ComponentKeys.LobbyReadyStatus, SubscribableBool.Create);
            readyStatus.Subscribe(new SimpleListener<bool>((v) => OnReadyStatusChange()));

            screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => {
                isHost = s == (int)LobbyScreen.HostGamePrefs;
                if(s == (int)LobbyScreen.HostGamePrefs || s == (int)LobbyScreen.ClientGamePrefs) {
                    ScreenEnter();
                }
            }));
        }

        private void OnReadyStatusChange() {
            btnsInteractable.Value = !readyStatus.Value;
        }

        private void InitStatusChange() {
            if(initStatus.State == (int)LobbyInitializedCheckStatus.Initialized) {
                SendPickedOverNetwork();
            }
        }

        private LobbyTurnOrderChoice GetChoiceFromButtons() {
            if(selectedToggle == choiceBtnIndex[LobbyTurnOrderChoice.HostIsFirst]) {
                return LobbyTurnOrderChoice.HostIsFirst;
            } else if(selectedToggle == choiceBtnIndex[LobbyTurnOrderChoice.ClientIsFirst]) {
                return LobbyTurnOrderChoice.ClientIsFirst;
            } else if(selectedToggle == choiceBtnIndex[LobbyTurnOrderChoice.Random]) {
                return LobbyTurnOrderChoice.Random;
            } else {
                return 0;
            }
        }

        private void SendPickedOverNetwork() {
            if(initStatus.State == (int)LobbyInitializedCheckStatus.Initialized) {
                reqSendPick.Send((LobbyTurnOrderChoice)selected.Value);
            }
        }

        private void ScreenEnter() {
            allowTogglesOff.Value = !isHost;
            if(isHost) {
                switch(prevChoices.MatchState) {
                case LobbyMatchState.FreshLobby:
                case LobbyMatchState.Rejoining:
                    selected.Value = (int)LobbyTurnOrderChoice.Random;
                    break;
                default:
                    if(prevChoices.OrderChoice != LobbyTurnOrderChoice.None) {
                        selected.Value = (int)prevChoices.OrderChoice;
                    } else {
                        selected.Value = (int)LobbyTurnOrderChoice.Random;
                    }
                    break;
                }
                otherSelLabel.Value = "Friend's preference:";
                otherSelValue.Value = "";
            } else {
                selected.Value = (int)LobbyTurnOrderChoice.None;
                otherSelLabel.Value = "Host's selection:";
                otherSelValue.Value = "Randomize";
            }
            for(int i = 0; i < toggleValues.Length; i++) {
                toggleValues[i].Value = i == choiceBtnIndex[(LobbyTurnOrderChoice)selected.Value];
            }
            selectedToggle = choiceBtnIndex[(LobbyTurnOrderChoice)selected.Value];

            SendPickedOverNetwork();
        }

        private void OnToggleChange(int toggleIndex, bool isOn) {
            bool change = false;
            if(selectedToggle == toggleIndex) {
                if(!isOn) {
                    selectedToggle = -1;
                    change = true;
                }
            } else {
                if(isOn) {
                    selectedToggle = toggleIndex;
                    change = true;
                }
            }
            BalugaDebug.Log(string.Format("Toggle change {0} is {1}, selected {2}", toggleIndex, isOn, selectedToggle));
            toggleValues[toggleIndex].Value = isOn;
            if(change) {
                tickHelper.Active = true;
                selected.Value = (int)GetChoiceFromButtons();
            }
        }

        private void OnNetworkPickReceived(LobbyTurnOrderChoice pick) {
            string pickedName = null;
            switch(pick) {
            case LobbyTurnOrderChoice.HostIsFirst:
                pickedName = "Host first";
                break;
            case LobbyTurnOrderChoice.ClientIsFirst:
                pickedName = "Host second";
                break;
            case LobbyTurnOrderChoice.Random:
                pickedName = "Random";
                break;
            default:
                pickedName = "None";
                break;
            }
            otherSelValue.Value = pickedName;
        }

        public void Tick(float deltaTime) {
            tickHelper.Active = false;
            SendPickedOverNetwork();
        }
    }
}
