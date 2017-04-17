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
using ModdableChess.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;

namespace ModdableChess.Logic.Server {
    public class ServerLobbyMessages {

        [Serializable]
        public class Mod {
            public string modName;
            public string displayName;
            public string displayVersion;
            public int numericVersion;
            public string author;
            public ModNetworkStatus networkStatus;
        }

        private ServerInformation info;
        private List<Mod> combinedMods, hostMods, clientMods;
        private bool receivedHostModList, receivedClientModList;
        private bool hasPickedMod;
        private string pickedMod;
        private LobbyTurnOrderChoice pickedTurnOrder;
        private ServerReadyCheckManager readyChecker;
        private Dictionary<string, int> nameMap;

        public ServerLobbyMessages(ModdableChessGame game) {
            combinedMods = new List<Mod>();
            hostMods = new List<Mod>();
            clientMods = new List<Mod>();
            nameMap = new Dictionary<string, int>();
            readyChecker = new ServerReadyCheckManager(game, 10);

            game.Components.GetOrRegister<Message>((int)ComponentKeys.Server_LobbyInitialized, Message.Create)
                .Subscribe(new SimpleListener(OnLobbyInitialized));
            info = game.Components.GetOrRegister<ServerInformation>((int)ComponentKeys.ServerInformation, ServerInformation.Create);
            NetworkTracker.ServerMsgCallbacks.lobbyPickedTurnOrderReceived = ReceivedPickedTurnOrder;
            NetworkTracker.ServerMsgCallbacks.lobbyModsListReceived = ReceivedModList;
            NetworkTracker.ServerMsgCallbacks.lobbyModsPickReceived = ReceivedModPick;
            NetworkTracker.ServerMsgCallbacks.lobbyReadyReceived = ReceivedReady;
        }

        private void OnLobbyInitialized() {
            combinedMods.Clear();
            hostMods.Clear();
            clientMods.Clear();
            receivedClientModList = false;
            receivedHostModList = false;
            hasPickedMod = false;
            pickedMod = null;
            pickedTurnOrder = LobbyTurnOrderChoice.Random;
        }

        private void ReceivedPickedTurnOrder(int connectionID, LobbyPickedTurnOrderMessage picked) {
            if(info.IsHost(connectionID)) {
                pickedTurnOrder = picked.choice;
            }
            if(info.IsConnected(connectionID)) {
                NetworkTracker.ServerSendLobbyPickedTurnOrder(info.OtherConnectionID(connectionID), picked);
            }
        }

        private void ReceivedModList(int connectionID, ModListNotifyMessage list) {
            if(info.IsConnected(connectionID)) {
                if(info.IsHost(connectionID)) {
                    CompileNetworkModList(list, hostMods);
                    receivedHostModList = true;
                } else {
                    CompileNetworkModList(list, clientMods);
                    receivedClientModList = true;
                }
                if(receivedHostModList && receivedClientModList) {
                    CombineModLists();
                    SendModList();
                }

                Unity.GameLink.ServerHostMods = hostMods;
                Unity.GameLink.ServerClientMods = clientMods;
                Unity.GameLink.ServerComboMods = combinedMods;
            }
        }

        private void CompileNetworkModList(ModListNotifyMessage nlist, List<Mod> slist) {
            slist.Clear();
            nameMap.Clear();

            foreach(var nmod in nlist.entries) {
                int repeatIndex;
                if(nameMap.TryGetValue(nmod.modName, out repeatIndex)) {
                    if(nmod.numericVersion > slist[repeatIndex].numericVersion) {
                        slist[repeatIndex] = Transform(nmod);
                    }
                } else {
                    slist.Add(Transform(nmod));
                    nameMap[nmod.modName] = slist.Count - 1;
                }
            }
        }

        private Mod Transform(ModListNotifyEntry nmod) {
            return new Mod() {
                modName = nmod.modName,
                displayName = nmod.displayName,
                numericVersion = nmod.numericVersion,
                displayVersion = nmod.displayVersion,
                author = nmod.author,
                networkStatus = ModNetworkStatus.Unknown,
            };
        }

        private void ReceivedModPick(int connectionID, NetworkPickedMod pick) {
            if(info.IsHost(connectionID)) {
                hasPickedMod = !pick.noneSelected;
                pickedMod = pick.modName;
            }
            if(info.IsConnected(connectionID)) {
                NetworkTracker.ServerSendLobbyModsPick(info.OtherConnectionID(connectionID), pick);
            }
        }

        private void CombineModLists() {
            nameMap.Clear();
            combinedMods.Clear();
            for(int i = 0; i < hostMods.Count; i++) {
                Mod mod = hostMods[i];
                combinedMods.Add(mod);
                nameMap[mod.modName] = combinedMods.Count - 1;
                mod.networkStatus = ModNetworkStatus.OnlyHost;
            }

            for(int i = 0; i < clientMods.Count; i++) {
                Mod clientMod = clientMods[i];
                int matchIndex;
                if(nameMap.TryGetValue(clientMod.modName, out matchIndex)) {
                    Mod hostMod = combinedMods[matchIndex];
                    if(hostMod.numericVersion > clientMod.numericVersion) {
                        hostMod.networkStatus = ModNetworkStatus.ClientOutOfDate;
                    } else if(hostMod.numericVersion < clientMod.numericVersion) {
                        clientMod.networkStatus = ModNetworkStatus.HostOutOfDate;
                        combinedMods[matchIndex] = clientMod;
                    } else {
                        hostMod.networkStatus = ModNetworkStatus.Playable;
                    }
                } else {
                    combinedMods.Add(clientMod);
                    clientMod.networkStatus = ModNetworkStatus.OnlyClient;
                }
            }
        }

        private void SendModList() {
            ModListServerMessage msg = new ModListServerMessage();
            msg.entries = new ModListServerEntry[combinedMods.Count];
            for(int i = 0; i < combinedMods.Count; i++) {
                Mod mod = combinedMods[i];
                msg.entries[i] = new ModListServerEntry() {
                    modName = mod.modName,
                    displayName = mod.displayName,
                    numericVersion = mod.numericVersion,
                    displayVersion = mod.displayVersion,
                    author = mod.author,
                    status = mod.networkStatus,
                };
            }
            NetworkTracker.ServerSendLobbyModsList(msg);
        }

        private void ReceivedReady(int connectionID, NetworkPlayerReadyNotice notice) {
            bool allReady = readyChecker.PlayerNotifiedReady(connectionID, notice.ready);

            if(info.IsConnected(connectionID)) {
                NetworkTracker.ServerSendLobbyReadyRelay(info.OtherConnectionID(connectionID), notice);
            }

            if(allReady) {
                readyChecker.Clear();
                NetworkTracker.ServerSendLobbyExitMessage(new LobbyExitMessage() {
                    valid = LobbyInValidExitState(),
                    modName = pickedMod,
                    turnOrderChoice = pickedTurnOrder,
                });
            }
        }

        private bool LobbyInValidExitState() {
            if(hasPickedMod && pickedTurnOrder != LobbyTurnOrderChoice.None) {
                for(int i = 0; i < combinedMods.Count; i++) {
                    Mod m = combinedMods[i];
                    if(m.modName == pickedMod && m.networkStatus == ModNetworkStatus.Playable) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
