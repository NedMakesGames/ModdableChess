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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic.Lobby {
    public enum GUICompKeys {
        None=0,
        ChooseToHostBtnPress,
        ChooseToJoinBtnPress,
        BtnStartHostPress,
        BtnHostCancelPress,
        HostPortChange,
        BtnHostStartEnabledCommand,
        HostPortSetCommand,
        HostConnectionStatusLabel,
        BtnClientCancelPress,
        BtnStartClientPress,
        ClientPortChange,
        BtnClientStartEnabledCommand,
        ClientPortSetCommand,
        ClientConnectionStatusLabel,
        ClientAddressTextField,
        ClientAddressChange,
        HostStartInputInteractable,
        ClientStartInputInteractable,
        TOSOpt1Set,
        TOSOpt1Change,
        TOSOpt2Set,
        TOSOpt2Change,
        TOSOpt3Set,
        TOSOpt3Change,
        TOSOtherSelectionLabel,
        TOSOtherSelectionValue,
        TOSAllowAllTogglesOff,
        ModList,
        ModListLabelTop,
        ModListLabelBottom,
        ModListPickedTop,
        ModListPickedBottom,
        ModListReloadBtnPress,
        ModListHideBtnPress,
        ReadyBtnEnabled,
        ReadyBtnLabel,
        OtherPlayerReadyLabel,
        ReadyBtnClicked,
        BackButtonClicked,
        BackButtonInteractable,
        PasswordTextSet,
        PasswordChange,
        HostAddressInteractable,
        StartGameTitleText,
        StartGameStartButtonText,
        ChooseToRejoinBtnPress,
        ChooseToRejoinBtnVisible,
        RejoinText,
        RejoinCancelButton,
        TOSInteractable,
    }
}
