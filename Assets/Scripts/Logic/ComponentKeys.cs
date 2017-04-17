﻿// Copyright (c) 2017, Timothy Ned Atton.
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

namespace ModdableChess.Logic {
    public enum ComponentKeys {
        CreateBoardCommand,
        GameBoard,
        BoardCreatedMessage,
        PlayerSelectionChange,
        PlayerSelectionAcceptBtn,
        NextTurnCommand,
        StartTurnChoosingCommand,
        MoveOptionListQuery,
        GameDatabase,
        CurrentTurn,
        MoveChoiceMadeEvent,
        MovePieceCommand,
        PieceMovedEvent,
        SelectedIndicatorPattern,
        NumConsecutiveTurns,
        CalculateStartConditions,
        ConnectionEstablished,
        StartConditionsBroadcastReq,
        ServerInformation,
        LocalPlayerInformation,
        TurnOrderDecidedMessage,
        MatchState,
        ReceivedStartConditionsFromServer,
        NetworkTurnActionMessage,
        NetworkTurnActionRelayReq,
        PieceCapturedEvent,
        TableOfContentsLoadRequest,
        LobbyChoices,
        ModAssetsLoaded,
        ModTranslationComplete,
        StartConditions,
        Send_RelayLobbyReadyStatus,
        LobbyOtherPlayerReadyReceived,
        O_LobbyAllPlayersReadyServerMessage,
        LobbyExitMessageReceived,
        Server_LoadingPlayerNotifiedReady,
        LoadingExitToPlayCommand,
        NotifyServerReadyToExitLoading,
        LoadingAllPlayersReadyServerMessage,
        DeepLoadMod,
        Server_LoadingPlayerRequestedStartConditions,
        LoadingBroadcastStartConditionsRPC,
        CheckQuery,
        GameEnd,
        NextTurnInfoQuery,
        EndOfTurnCalcs,
        CheckPromotionQuery,
        PiecePromotionEvent,
        GameOverQuery,
        TurnChooserState,
        TurnChooserChosenPiece,
        TurnActivePlayer,
        MouseOverIndicatorPattern,
        LobbyGUIMessages,
        LobbyScreen,
        StartHostCommand,
        StartClientCommand,
        SearchModFolderRequest,
        GetNetworkModList,
        NetworkModListSendRequest,
        NetworkModListReceived,
        NetworkPickedModSendRequest,
        NetworkPickedModReceived,
        LobbyTurnOrderChoiceNotify,
        LobbyTurnOrderChoiceReceived,
        LobbyModSelected,
        LobbyReadyStatus,
        LobbyPlayerNotifedReadyForClients,
        LobbyTurnOrderSelected,
        LobbyBackNetworkMessageRequest,
        LobbyBackNetworkMessageReceived,
        StopHostCommand,
        StopClientCommand,
        DisconnectOnServerNotification,
        DisconnectOnClientNotification,
        ServerRejectedOurConnectionMessage,
        NetworkPlayerInterfaceReady,
        ModLoadError,
        Server_LoadingPlayerNotifiedError,
        LoadingErrorReceived,
        LoadingClientNetworkEvent,
        ReconnectionState,
        Server_StartGameHelloReceived,
        LoadingReadySend,
        Server_ReadyNoticeReceived,
        LoadingGameStateReceived,
        LoadingExitReceived,
        Send_GameStateHello,
        LoadingErrorSend,
        CompileCurrentGameState,
        FirstPlayerConnectionID,
        LobbyInitStatus,
        Server_Callbacks,
        Client_Callbacks,
        ConnectionHelper,
        UnityNetworking,
        LobbyInitSendReady,
        LobbyInitAllReadyReceived,
        Server_LobbyInitialized,
        LobbyReadyNoticeSendRequest,
        GetCachedModFolder,
        LoadingGameStateSend,
        LuaScripts,
        OpponentTurnActionReceived,
        SendTurnAction,
        TurnChooserChosenAction,
        PlayerSelectionCancelBtn,
        TurnChooserHighlightChange,
        TurnChooserTurnOptions,
        GetActionIndicatorPattern,
        PlayerSelectionCycleBtn,
        ScrollWheel,
        CapturePieceCommand,
        PromotePieceCommand,
        HandleEndOfTurn,
        LoadingGameStateStopRefreshing,
        EndOfTurnStateReceived,
        SendEndOfTurnState,
        StopRefreshEndOfTurnState,
        PlayForfeitButton,
        TurnState,
        StopTurnChoosingCommand,
        ForfeitGame,
        EndScreenExitBtn,
        EndScreenHeaderText,
        EndScreenResultText,
        EndScreenExitPressed,
        ActiveTurnText,
        TurnChange,
        GameCameraRotation,
        CameraTrackMouseBtn,
        MouseChange,
        CameraRecenterBtn,
        GameCameraZoom
    }
}
