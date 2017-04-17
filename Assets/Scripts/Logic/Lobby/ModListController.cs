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
using ModdableChess.Mods;
using System;
using System.Collections.Generic;

namespace ModdableChess.Logic.Lobby {
    public class ModListController {

        [Serializable]
        public class ComboModInfo {
            public string filePath;
            public string modName;
            public string displayName;
            public string displayVersion;
            public int numericVersion;
            public string author;
            public ModNetworkStatus networkStatus;
        }

        private StateMachine screen;
        private ModList modButtons;
        private SubscribableObject<string> pickedLabelTop;
        private SubscribableObject<string> pickedLabelBottom;
        private SubscribableObject<string> pickedModTop;
        private SubscribableObject<string> pickedModBottom;
        private SubscribableObject<string> selectedModName;

        private Query<List<LocalModInfo>, string> modSearcher;
        private List<LocalModInfo> localMods;
        private List<NetworkModInfo> networkMods;
        private List<ComboModInfo> allMods;
        private bool showNotLocalMods;
        private Command<List<LocalModInfo>> reqSendNetworkModList;
        private Command<bool, string> reqSendPickedMod;
        private Dictionary<int, int> statusSortOrder;
        private ComboModInfo pickedMod;
        private LobbyChoices prevChoices;
        private StateMachine initState;
        private LocalPlayerInformation localInfo;
        private string otherPlayerPickModName;
        private SubscribableBool readyStatus;

        public ModListController(AutoController scene) {
            statusSortOrder = new Dictionary<int, int>();
            statusSortOrder[(int)ModNetworkStatus.Playable] = 0;
            statusSortOrder[(int)ModNetworkStatus.HostOutOfDate] = 1;
            statusSortOrder[(int)ModNetworkStatus.ClientOutOfDate] = 1;
            statusSortOrder[(int)ModNetworkStatus.OnlyHost] = 2;
            statusSortOrder[(int)ModNetworkStatus.OnlyClient] = 3;
            statusSortOrder[(int)ModNetworkStatus.Unknown] = 4;
            localMods = new List<LocalModInfo>();
            networkMods = new List<NetworkModInfo>();
            allMods = new List<ComboModInfo>();
            showNotLocalMods = true;

            prevChoices = scene.Game.Components.Get<LobbyChoices>((int)ComponentKeys.LobbyChoices);

            ComponentRegistry guiComps = scene.Components.Get<ComponentRegistry>((int)ComponentKeys.LobbyGUIMessages);
            modButtons = new ModList();
            guiComps.Register((int)GUICompKeys.ModList, modButtons);
            modButtons.OnButtonClick.Subscribe(new SimpleListener<int>(OnModButtonClick));
            pickedLabelTop = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.ModListLabelTop, pickedLabelTop);
            pickedLabelBottom = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.ModListLabelBottom, pickedLabelBottom);
            pickedModTop = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.ModListPickedTop, pickedModTop);
            pickedModBottom = new SubscribableObject<string>("");
            guiComps.Register((int)GUICompKeys.ModListPickedBottom, pickedModBottom);
            Message reloadBtnPressed = new Message();
            guiComps.Register((int)GUICompKeys.ModListReloadBtnPress, reloadBtnPressed);
            reloadBtnPressed.Subscribe(new SimpleListener(OnReloadButton));
            Message hideBtnPressed = new Message();
            guiComps.Register((int)GUICompKeys.ModListHideBtnPress, hideBtnPressed);
            hideBtnPressed.Subscribe(new SimpleListener(OnHideButton));

            reqSendNetworkModList = scene.Game.Components.Get<Command<List<LocalModInfo>>>((int)ComponentKeys.NetworkModListSendRequest);
            scene.ActivatableList.Add(new ListenerJanitor<IListener<List<NetworkModInfo>>>(
                scene.Game.Components.Get<Message<List<NetworkModInfo>>>((int)ComponentKeys.NetworkModListReceived),
                new SimpleListener<List<NetworkModInfo>>(OnNetworkModListReceived)));

            reqSendPickedMod = scene.Game.Components.Get<Command<bool, string>>((int)ComponentKeys.NetworkPickedModSendRequest);
            scene.ActivatableList.Add(new ListenerJanitor<IListener<bool, string>> (
                scene.Game.Components.Get<Message<bool, string>>((int)ComponentKeys.NetworkPickedModReceived),
                new SimpleListener<bool, string>(OnNetworkPickedModReceived)));

            selectedModName = scene.Components.GetOrRegister((int)ComponentKeys.LobbyModSelected, SubscribableObject<string>.Create);

            modSearcher = scene.Components.GetOrRegister<Query<List<LocalModInfo>, string>>
                ((int)ComponentKeys.SearchModFolderRequest, Query<List<LocalModInfo>, string>.Create);

            localInfo = scene.Game.Components.Get<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation);

            initState = scene.Components.GetOrRegister<StateMachine>((int)ComponentKeys.LobbyInitStatus, StateMachine.Create);
            initState.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => InitStatusChange()));

            scene.Components.GetOrRegister<Query<string, string>>((int)ComponentKeys.GetCachedModFolder, Query<string, string>.Create)
                .Handler = GetCachedModFolder;

            readyStatus = scene.Components.GetOrRegister<SubscribableBool>((int)ComponentKeys.LobbyReadyStatus, SubscribableBool.Create);
            readyStatus.Subscribe(new SimpleListener<bool>((v) => OnReadyStatusChange()));

            screen = scene.Components.Get<StateMachine>((int)ComponentKeys.LobbyScreen);
            screen.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => {
                switch((LobbyScreen)s) {
                case LobbyScreen.ClientGamePrefs:
                case LobbyScreen.HostGamePrefs:
                    OnEnterScreen();
                    break;
                }
            }));
        }

        private void OnReadyStatusChange() {
            foreach(var button in modButtons.Buttons) {
                button.Choosable = !readyStatus.Value;
            }
            modButtons.OnListUpdate.Send();
        }

        private void InitStatusChange() {
            if(screen.State == (int)LobbyScreen.HostGamePrefs || screen.State == (int)LobbyScreen.ClientGamePrefs) {
                if(initState.State == (int)LobbyInitializedCheckStatus.Initialized) {
                    GenerateAndSendNetworkModList();
                    SendPickedModOverNetwork();
                }
            }
        }

        private void OnEnterScreen() {
            if(screen.State == (int)LobbyScreen.HostGamePrefs) {
                pickedLabelTop.Value = "Mod to play:";
                pickedLabelBottom.Value = "Friend's preferred mod:";
                pickedModBottom.Value = "None";
            } else {
                pickedLabelTop.Value = "Mod to play:";
                pickedLabelBottom.Value = "My preferred mod:";
                pickedModTop.Value = "None selected";
                pickedModBottom.Value = "Pick a mod!";
            }
            otherPlayerPickModName = null;

            SearchLocalFileMods();
            CombineLocalAndNetworkMods();
            PopulateModList();
            GenerateAndSendNetworkModList();

            if(screen.State == (int)LobbyScreen.HostGamePrefs) {
                switch(prevChoices.MatchState) {
                case LobbyMatchState.RepeatGame:
                    bool prevChoiceNotFound = true;
                    foreach(ComboModInfo mod in allMods) {
                        if(mod.filePath == prevChoices.ModFolder) {
                            SetPickedMod(mod);
                            prevChoiceNotFound = false;
                            break;
                        }
                    }
                    if(prevChoiceNotFound) {
                        SetPickedMod(null);
                    }
                    break;
                default:
                    SetPickedMod(null);
                    break;
                }
            } else {
                SetPickedMod(null);
            }
        }

        private void OnNetworkModListReceived(List<NetworkModInfo> networkModList) {
            networkMods.Clear();
            networkMods.AddRange(networkModList);
            CombineLocalAndNetworkMods();
            PopulateModList();
        }

        private void SearchLocalFileMods() {
            localMods = modSearcher.Send(ModStrings.GetModFolder());
            Dictionary<string, int> nameMap = new Dictionary<string, int>();
            for(int i = localMods.Count - 1; i >= 0; i--) {
                int prevIndex;
                if(nameMap.TryGetValue(localMods[i].modName, out prevIndex)) {
                    int prevVesion = localMods[prevIndex].numericVersion;
                    int iVersion = localMods[i].numericVersion;
                    if(iVersion > prevVesion) {
                        nameMap[localMods[i].modName] = i;
                        localMods.RemoveAt(prevIndex);
                    } else {
                        localMods.RemoveAt(i);
                    }
                }
            }

            Unity.GameLink.LocalMods = localMods;
        }

        private void GenerateAndSendNetworkModList() {
            if(initState.State == (int)LobbyInitializedCheckStatus.Initialized) {
                reqSendNetworkModList.Send(localMods);
            }
        }

        private void CombineLocalAndNetworkMods() {

            Dictionary<string, int> nameMap = new Dictionary<string, int>();
            allMods.Clear();

            ModNetworkStatus onlyMe = localInfo.IsHost ? ModNetworkStatus.OnlyHost : ModNetworkStatus.OnlyClient;
            ModNetworkStatus onlyOther = localInfo.IsHost ? ModNetworkStatus.OnlyClient : ModNetworkStatus.OnlyHost;

            for(int i = 0; i < networkMods.Count; i++) {
                NetworkModInfo mod = networkMods[i];
                if(showNotLocalMods || mod.networkStatus != onlyOther) {
                    allMods.Add(new ComboModInfo() {
                        filePath = null,
                        modName = mod.modName,
                        numericVersion = mod.numericVersion,
                        displayName = mod.displayName,
                        displayVersion = mod.displayVersion,
                        author = mod.author,
                        networkStatus = mod.networkStatus,
                    });
                    nameMap[mod.modName] = allMods.Count - 1;
                }
            }

            for(int i = 0; i < localMods.Count; i++) {
                LocalModInfo local = localMods[i];
                int index = 0;
                ComboModInfo mod;
                if(nameMap.TryGetValue(local.modName, out index)) {
                    mod = allMods[index];
                    if(mod.numericVersion <= local.numericVersion) {
                        mod.numericVersion = local.numericVersion;
                        mod.displayName = local.displayName;
                        mod.displayVersion = local.displayVersion;
                        mod.author = local.author;
                    }
                } else {
                    mod = new ComboModInfo() {
                        modName = local.modName,
                        numericVersion = local.numericVersion,
                        displayName = local.displayName,
                        displayVersion = local.displayVersion,
                        author = local.author,
                        networkStatus = onlyMe,
                    };
                    allMods.Add(mod);
                }
                mod.filePath = local.filePath;
            }

            allMods.Sort(SortModList);

            if(pickedMod != null) {
                bool pickedNotModFound = true;
                for(int i = 0; i < allMods.Count; i++) {
                    ComboModInfo mod = allMods[i];
                    if(mod.modName == pickedMod.modName) {
                        pickedNotModFound = false;
                        SetPickedMod(mod);
                        break;
                    }
                }
                if(pickedNotModFound) {
                    SetPickedMod(null);
                }
            }

            Unity.GameLink.NetMods = allMods;

            RefreshOtherPlayerPick();
        }

        private int SortModList(ComboModInfo a, ComboModInfo b) {
            int statusOrderA = statusSortOrder[(int)a.networkStatus];
            int statusOrderB = statusSortOrder[(int)b.networkStatus];
            if(statusOrderA == statusOrderB) {
                return string.Compare(a.displayName, b.displayName);
            } else {
                return statusOrderA - statusOrderB;
            }
        }

        private void PopulateModList() {
            bool isHost = localInfo.IsHost;
            modButtons.Buttons.Clear();
            for(int i = 0; i < allMods.Count; i++) {
                ComboModInfo mod = allMods[i];
                ModListButton btn = new ModListButton();
                btn.NameString = string.Format("{0} v{1} by {2}", mod.displayName, mod.displayVersion, mod.author);
                switch(mod.networkStatus) {
                case ModNetworkStatus.Unknown:
                    btn.StatusString = "";
                    break;
                case ModNetworkStatus.Playable:
                    btn.StatusString = "Playable!";
                    break;
                case ModNetworkStatus.HostOutOfDate:
                    if(isHost) {
                        btn.StatusString = "Out of date";
                    } else {
                        btn.StatusString = "Friend's out of date";
                    }
                    break;
                case ModNetworkStatus.ClientOutOfDate:
                    if(!isHost) {
                        btn.StatusString = "Out of date";
                    } else {
                        btn.StatusString = "Friend's out of date";
                    }
                    break;
                case ModNetworkStatus.OnlyHost:
                    if(isHost) {
                        btn.StatusString = "Friend does not have";
                    } else {
                        btn.StatusString = "Friend's only";
                    }
                    break;
                case ModNetworkStatus.OnlyClient:
                    if(!isHost) {
                        btn.StatusString = "Friend does not have";
                    } else {
                        btn.StatusString = "Friend's only";
                    }
                    break;
                }
                btn.Order = i;
                btn.Choosable = !readyStatus.Value;
                modButtons.Buttons.Add(btn);
            }
            modButtons.OnListUpdate.Send();
        }

        private void OnModButtonClick(int buttonIndex) {
            SetPickedMod(allMods[buttonIndex]);
        }

        private void SetPickedMod(ComboModInfo mod) {
            pickedMod = mod;
            if(screen.State == (int)LobbyScreen.HostGamePrefs) {
                if(pickedMod != null) {
                    pickedModTop.Value = pickedMod.displayName;
                } else {
                    pickedModTop.Value = "Pick a mod!";
                } 
            } else {
                if(pickedMod != null) {
                    pickedModBottom.Value = pickedMod.displayName;
                } else {
                    pickedModBottom.Value = "Pick a mod!";
                }
            }
            SendPickedModOverNetwork();
            if(mod == null || mod.networkStatus != ModNetworkStatus.Playable) {
                selectedModName.Value = null;
            } else {
                selectedModName.Value = mod.filePath;
            }
            BalugaDebug.Log("Mod picked: " + (mod == null ? "none" : mod.modName + " " + mod.networkStatus));
        }

        private void SendPickedModOverNetwork() {
            if(initState.State == (int)LobbyInitializedCheckStatus.Initialized) {
                reqSendPickedMod.Send(
                    pickedMod != null,
                    pickedMod == null ? "" : pickedMod.modName
                );
            }
        }

        private void OnNetworkPickedModReceived(bool hasPick, string modName) {
            if(hasPick) {
                otherPlayerPickModName = modName;
            } else {
                otherPlayerPickModName = null;
            }
            RefreshOtherPlayerPick();
        }

        private void RefreshOtherPlayerPick() {
            ComboModInfo foundMod = null;
            if(otherPlayerPickModName != null) {
                for(int i = 0; i < allMods.Count; i++) {
                    ComboModInfo mod = allMods[i];
                    if(mod.modName == otherPlayerPickModName) {
                        foundMod = mod;
                        break;
                    }
                }
            }

            if(screen.State == (int)LobbyScreen.HostGamePrefs) {
                if(foundMod == null) {
                    pickedModBottom.Value = "None";
                } else {
                    pickedModBottom.Value = foundMod.displayName;
                }
            } else {
                if(foundMod == null) {
                    pickedModTop.Value = "None selected";
                } else {
                    pickedModTop.Value = foundMod.displayName;
                }
            }
        }

        private void OnHideButton() {
            showNotLocalMods = !showNotLocalMods;
            CombineLocalAndNetworkMods();
            PopulateModList();
        }

        private void OnReloadButton() {
            SearchLocalFileMods();
            CombineLocalAndNetworkMods();
            PopulateModList();
            GenerateAndSendNetworkModList();
        }

        private string GetCachedModFolder(string modName) {
            foreach(ComboModInfo mod in allMods) {
                if(mod.modName == modName) {
                    return mod.filePath;
                }
            }
            return null;
        }
    }
}
