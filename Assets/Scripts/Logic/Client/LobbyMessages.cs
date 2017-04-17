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
using ModdableChess.Logic.Lobby;
using ModdableChess.Mods;
using ModdableChess.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic.Client {
    public class LobbyMessages {

        private Message<LobbyTurnOrderChoice> turnOrderReceived;
        private Message<List<NetworkModInfo>> modListReceived;
        private Message<bool, string> modPickReceived;
        private Message<bool> readyRelayReceived;
        private Message<LobbyExitState> exitMessageReceived;
        private List<NetworkModInfo> netMods;
        private ReadyCheckSender readySender;

        public LobbyMessages(ModdableChessGame game) {
            netMods = new List<NetworkModInfo>();

            game.Components.GetOrRegister<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation, LocalPlayerInformation.Create)
                .Connection.EnterStateMessenger.Subscribe(new SimpleListener<int>(OnConnectionChange));

            turnOrderReceived = game.Components.GetOrRegister<Message<LobbyTurnOrderChoice>>
                ((int)ComponentKeys.LobbyTurnOrderChoiceReceived, Message<LobbyTurnOrderChoice>.Create);
            game.Components.GetOrRegister<Command<LobbyTurnOrderChoice>>
                ((int)ComponentKeys.LobbyTurnOrderChoiceNotify, Command<LobbyTurnOrderChoice>.Create).Handler = SendPickedTurnOrder;

            modListReceived = game.Components.GetOrRegister<Message<List<NetworkModInfo>>>
                ((int)ComponentKeys.NetworkModListReceived, Message<List<NetworkModInfo>>.Create);
            game.Components.GetOrRegister<Command<List<LocalModInfo>>>
                ((int)ComponentKeys.NetworkModListSendRequest, Command<List<LocalModInfo>>.Create).Handler = SendModList;

            modPickReceived = game.Components.GetOrRegister<Message<bool, string>>
                ((int)ComponentKeys.NetworkPickedModReceived, Message<bool, string>.Create);
            game.Components.GetOrRegister<Command<bool, string>>
                ((int)ComponentKeys.NetworkPickedModSendRequest, Command<bool, string>.Create).Handler = SendPickedMod;

            readySender = new ReadyCheckSender(game, 5, (s) => SendReadyNotice(s));
            readyRelayReceived = game.Components.GetOrRegister<Message<bool>>
                ((int)ComponentKeys.LobbyOtherPlayerReadyReceived, Message<bool>.Create);
            exitMessageReceived = game.Components.GetOrRegister<Message<LobbyExitState>>
                ((int)ComponentKeys.LobbyExitMessageReceived, Message<LobbyExitState>.Create);
            game.Components.GetOrRegister<Command<bool>>
                ((int)ComponentKeys.LobbyReadyNoticeSendRequest, Command<bool>.Create).Handler = ReadyStatusChange;

            NetworkTracker.ClientMsgCallbacks.lobbyPickedTurnOrderRelay = ReceivedPickedTurnOrder;
            NetworkTracker.ClientMsgCallbacks.lobbyModsListRelay = ReceiveModList;
            NetworkTracker.ClientMsgCallbacks.lobbyModsPickRelay = ReceiveModPick;
            NetworkTracker.ClientMsgCallbacks.lobbyReadyNoticeRelay = ReceiveReadyRelay;
            NetworkTracker.ClientMsgCallbacks.lobbyExitMessageReceived = ReceivedExitMessage;
        }

        private void OnConnectionChange(int state) {
            if(state != (int)ClientConnectionState.Validated) {
                readySender.Active = false;
            }
        }

        private void SendPickedTurnOrder(LobbyTurnOrderChoice choice) {
            NetworkTracker.ClientSendLobbyPickedTurnOrder(new LobbyPickedTurnOrderMessage() {
                choice = choice,
            });
        }

        private void SendModList(List<LocalModInfo> list) {

            ModListNotifyMessage msg = new ModListNotifyMessage();
            msg.entries = new ModListNotifyEntry[list.Count];
            for(int i = 0; i < list.Count; i++) {
                LocalModInfo lmod = list[i];
                msg.entries[i] = new ModListNotifyEntry() {
                    modName = lmod.modName,
                    displayName = lmod.displayName,
                    author = lmod.author,
                    numericVersion = lmod.numericVersion,
                    displayVersion = lmod.displayVersion,
                };
            }

            NetworkTracker.ClientSendLobbyModsList(msg);
        }

        private void SendPickedMod(bool hasPick, string modName) {
            NetworkTracker.ClientSendLobbyModsPick(new NetworkPickedMod() {
                noneSelected = !hasPick,
                modName = modName,
            });
        }

        public void ReceivedPickedTurnOrder(LobbyPickedTurnOrderMessage picked) {
            turnOrderReceived.Send(picked.choice);
        }

        private void ReceiveModList(ModListServerMessage modList) {
            netMods.Clear();

            for(int i = 0; i < modList.entries.Length; i++) {
                ModListServerEntry entry = modList.entries[i];
                netMods.Add(new NetworkModInfo() {
                    modName = entry.modName,
                    displayName = entry.displayName,
                    numericVersion = entry.numericVersion,
                    displayVersion = entry.displayVersion,
                    author = entry.author,
                    networkStatus = entry.status,
                });
            }

            modListReceived.Send(netMods);

            netMods.Clear();
        }

        private void ReceiveModPick(NetworkPickedMod pickedMod) {
            modPickReceived.Send(!pickedMod.noneSelected, pickedMod.modName);
        }

        private void ReadyStatusChange(bool ready) {
            readySender.Active = ready;
        }

        private void SendReadyNotice(bool ready) {
            NetworkTracker.ClientSendLobbyReadyNotice(new NetworkPlayerReadyNotice() {
                ready = ready
            });
        }

        private void ReceiveReadyRelay(NetworkPlayerReadyNotice msg) {
            readyRelayReceived.Send(msg.ready);
        }

        private void ReceivedExitMessage(LobbyExitMessage msg) {
            exitMessageReceived.Send(new LobbyExitState() {
                valid = msg.valid,
                modName = msg.modName,
                turnOrderChoice = msg.turnOrderChoice,
            });
        }
    }
}
