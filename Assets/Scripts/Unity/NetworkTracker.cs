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
using System.Collections.Generic;
using UnityEngine.Networking;
using Baluga3.GameFlowLogic;
using ModdableChess.Logic;
using Baluga3.UnityCore;
using System;
using Baluga3.Core;
using ModdableChess.Logic.Server;
using ModdableChess.Logic.Client;
using ModdableChess.Logic.Lobby;
using UnityEngine.Networking.NetworkSystem;

namespace ModdableChess.Unity {
    public class NetworkTracker : NetworkManager, IServerCommandable, IClientCommandable {

        //[SerializeField]
        //private float clientPingMessagePeriod = 30;

        private static NetworkTracker Instance;

        private enum Msgs {
            ValidateAsk = MsgType.Highest + 1,
            ValidateAnswer,
            LobbyInitReadyNotice,
            LobbyInitAllReady,
            LobbyPickedTurnOrderNotify,
            LobbyPickedTurnOrderRelay,
            LobbyModsListNotify,
            LobbyModsListRelay,
            LobbyModsPickNotify,
            LobbyModsPickRelay,
            LobbyNotifyReady,
            LobbyReadyRelay,
            LobbyExitSettingsMessage,
            LoadingGameStateSend,
            LoadingGameStateRefresh,
            LoadingGameStateReply,
            LoadingErrorNotify,
            LoadingErrorRelay,
            LoadingReadyNotify,
            LoadingReadyExitMessage,
            BoardChooseAction,
            BoardActionRelay,
            BoardClientEOT,
            BoardClientEOTRefresh,
            BoardServerEOT
        }

        private bool suppressServerStart;
        private IServerCallbacks serverCBs;
        private IClientCallbacks clientCBs;
        //private float pingTimer;

        private void Awake() {
            if(NetworkTracker.Instance != null) {
                Destroy(gameObject);
            } else {
                NetworkTracker.Instance = this;
            }
        }

        private void Start() {
            GameLink.Game.Components.Register((int)ComponentKeys.UnityNetworking, this);
            serverCBs = GameLink.Game.Components.Get<IServerCallbacks>((int)ComponentKeys.Server_Callbacks);
            serverCBs.RegisterCommandable(this);
            clientCBs = GameLink.Game.Components.Get<IClientCallbacks>((int)ComponentKeys.Client_Callbacks);
            clientCBs.RegisterCommandable(this);
        }

        //private void Update() {
        //    pingTimer -= Time.deltaTime;
        //    if(pingTimer <= 0) {
        //        pingTimer = clientPingMessagePeriod;
        //        if(client != null && client.isConnected) {
        //            ClientSendMessage(Msgs.Ping, new EmptyMessage());
        //        }
        //        if(NetworkServer.connections.Count > 0) {
        //            ServerSendMessage(Msgs.Ping, new EmptyMessage());
        //        }
        //    }
        //}

        public bool StartClient(string address, int port) {
            this.networkAddress = address;
            this.networkPort = port;
            StartClient();
            
            return true;
        }

        public override void OnStartClient(NetworkClient client) {
            //pingTimer = clientPingMessagePeriod;
            base.OnStartClient(client);
            SetupClientHandlers();
            clientCBs.OnStart();
        }

        public override void OnClientConnect(NetworkConnection conn) {
            base.OnClientConnect(conn);
            clientCBs.OnConnect();
        }

        public override void OnClientError(NetworkConnection conn, int errorCode) {
            base.OnClientError(conn, errorCode);
            clientCBs.OnError((NetworkError)errorCode);
        }

        public override void OnClientDisconnect(NetworkConnection conn) {
            base.OnClientDisconnect(conn);
            clientCBs.OnDisconnect();
        }

        public void ForceStopClient() {
            StopClient();
        }

        public override void OnStopClient() {
            base.OnStopClient();
            clientCBs.OnStop();
        }

        public bool StartServer(int port) {
            this.networkPort = port;

            suppressServerStart = true;
            StartHost();
            suppressServerStart = false;
            if(client != null) {
                serverCBs.OnStart();
                return true;
            } else {
                return false;
            }
        }

        public override void OnStartServer() {
            base.OnStartServer();
            SetupServerHandlers();
            if(!suppressServerStart) {
                serverCBs.OnStart();
            }
        }

        public override void OnServerError(NetworkConnection conn, int errorCode) {
            base.OnServerError(conn, errorCode);
            serverCBs.OnError(conn.connectionId, (NetworkError)errorCode);
        }

        public override void OnServerDisconnect(NetworkConnection conn) {
            base.OnServerDisconnect(conn);
            serverCBs.OnDisconnect(conn.connectionId);
        }

        public void ForceDisconnectConnection(int connectionID) {
            Debug.Log("Force disconnection " + connectionID);
            NetworkServer.connections[connectionID].Disconnect();
        }

        public void ForceStopServer() {
            StopHost();
        }

        public override void OnStopServer() {
            base.OnStopServer();
            serverCBs.OnStop();
        }

        private void ClientSendMessage(Msgs msgCode, MessageBase msg) {
            Debug.Log(string.Format("{0}: Client MSG on {1}: {2}", Time.frameCount, msgCode, msg.ToString()));
            client.Send((short)msgCode, msg);
        }

        private void ServerSendMessage(Msgs msgCode, MessageBase msg) {
            Debug.Log(string.Format("{0}: Server MSG to all on {1}: {2}", Time.frameCount, msgCode, msg.ToString()));
            NetworkServer.SendToAll((short)msgCode, msg);
        }

        private void ServerSendMessage(int connectionID, Msgs msgCode, MessageBase msg) {
            Debug.Log(string.Format("{0}: Server MSG to {3} on {1}: {2}", Time.frameCount, msgCode, msg.ToString(), connectionID));
            NetworkServer.connections[connectionID].Send((short)msgCode, msg);
        }

        public static class ClientMsgCallbacks {
            public static Action lobbyInitAllReady;
            public static Action<LobbyPickedTurnOrderMessage> lobbyPickedTurnOrderRelay;
            public static Action<ModListServerMessage> lobbyModsListRelay;
            public static Action<NetworkPickedMod> lobbyModsPickRelay;
            public static Action<NetworkPlayerReadyNotice> lobbyReadyNoticeRelay;
            public static Action<LobbyExitMessage> lobbyExitMessageReceived;
            public static Action<LoadingGameStateMessage> loadingGameStateReply;
            public static Action loadingErrorRelay;
            public static Action loadingExitReceived;
            public static Action<BoardActionMessage> boardActionReceived;
            public static Action<ServerEndOfTurnStateMessage> boardEndOfTurnReceived;
        }

        private void SetupClientHandlers() {
            //client.RegisterHandler((short)Msgs.Ping, (m) => Dummy());
            client.RegisterHandler((short)Msgs.ValidateAnswer, (m) => clientCBs.OnValidationResponse(m.ReadMessage<ValidationResponse>()));
            client.RegisterHandler((short)Msgs.LobbyInitAllReady, (m) => ClientMsgCallbacks.lobbyInitAllReady());
            client.RegisterHandler((short)Msgs.LobbyPickedTurnOrderRelay, (m) => 
                ClientMsgCallbacks.lobbyPickedTurnOrderRelay(m.ReadMessage<LobbyPickedTurnOrderMessage>()));
            client.RegisterHandler((short)Msgs.LobbyModsListRelay, (m) =>
                ClientMsgCallbacks.lobbyModsListRelay(m.ReadMessage<ModListServerMessage>()));
            client.RegisterHandler((short)Msgs.LobbyModsPickRelay, (m) =>
                ClientMsgCallbacks.lobbyModsPickRelay(m.ReadMessage<NetworkPickedMod>()));
            client.RegisterHandler((short)Msgs.LobbyReadyRelay, (m) =>
                ClientMsgCallbacks.lobbyReadyNoticeRelay(m.ReadMessage<NetworkPlayerReadyNotice>()));
            client.RegisterHandler((short)Msgs.LobbyExitSettingsMessage, (m) =>
                ClientMsgCallbacks.lobbyExitMessageReceived(m.ReadMessage<LobbyExitMessage>()));
            client.RegisterHandler((short)Msgs.LoadingGameStateReply, (m) =>
                ClientMsgCallbacks.loadingGameStateReply(m.ReadMessage<LoadingGameStateMessage>()));
            client.RegisterHandler((short)Msgs.LoadingErrorRelay, (m) =>
                ClientMsgCallbacks.loadingErrorRelay());
            client.RegisterHandler((short)Msgs.LoadingReadyExitMessage, (m) =>
                ClientMsgCallbacks.loadingExitReceived());
            client.RegisterHandler((short)Msgs.BoardActionRelay, (m) =>
                ClientMsgCallbacks.boardActionReceived(m.ReadMessage<BoardActionMessage>()));
            client.RegisterHandler((short)Msgs.BoardServerEOT, (m) =>
                ClientMsgCallbacks.boardEndOfTurnReceived(m.ReadMessage<ServerEndOfTurnStateMessage>()));
        }

        public static class ServerMsgCallbacks {
            public static Action<int, NetworkPlayerReadyNotice> lobbyInitNotifyReady;
            public static Action<int, LobbyPickedTurnOrderMessage> lobbyPickedTurnOrderReceived;
            public static Action<int, ModListNotifyMessage> lobbyModsListReceived;
            public static Action<int, NetworkPickedMod> lobbyModsPickReceived;
            public static Action<int, NetworkPlayerReadyNotice> lobbyReadyReceived;
            public static Action<int, LoadingGameStateMessage> loadingGameStateReceived;
            public static Action<int, NetworkPlayerReadyNotice> loadingGameStateRefresh;
            public static Action<int> loadingErrorReceived;
            public static Action<int, NetworkPlayerReadyNotice> loadingReadyReceived;
            public static Action<int, BoardActionMessage> boardActionReceived;
            public static Action<int, EndOfTurnStateMessage> boardEndOfTurnReceived;
            public static Action<int, NetworkPlayerReadyNotice> boardEndOfTurnRefreshed;
        }

        private void SetupServerHandlers() {
            //NetworkServer.RegisterHandler((short)Msgs.Ping, (m) => Dummy());
            NetworkServer.RegisterHandler((short)Msgs.ValidateAsk, (m) => serverCBs.OnValidationRequest(m.conn.connectionId, m.ReadMessage<ValidationInfo>()));
            NetworkServer.RegisterHandler((short)Msgs.LobbyInitReadyNotice, (m) => 
                ServerMsgCallbacks.lobbyInitNotifyReady(m.conn.connectionId, m.ReadMessage<NetworkPlayerReadyNotice>()));
            NetworkServer.RegisterHandler((short)Msgs.LobbyPickedTurnOrderNotify, (m) => 
                ServerMsgCallbacks.lobbyPickedTurnOrderReceived(m.conn.connectionId, m.ReadMessage<LobbyPickedTurnOrderMessage>()));
            NetworkServer.RegisterHandler((short)Msgs.LobbyModsListNotify, (m) =>
                ServerMsgCallbacks.lobbyModsListReceived(m.conn.connectionId, m.ReadMessage<ModListNotifyMessage>()));
            NetworkServer.RegisterHandler((short)Msgs.LobbyModsPickNotify, (m) =>
                ServerMsgCallbacks.lobbyModsPickReceived(m.conn.connectionId, m.ReadMessage<NetworkPickedMod>()));
            NetworkServer.RegisterHandler((short)Msgs.LobbyNotifyReady, (m) =>
                ServerMsgCallbacks.lobbyReadyReceived(m.conn.connectionId, m.ReadMessage<NetworkPlayerReadyNotice>()));
            NetworkServer.RegisterHandler((short)Msgs.LoadingGameStateSend, (m) =>
                ServerMsgCallbacks.loadingGameStateReceived(m.conn.connectionId, m.ReadMessage<LoadingGameStateMessage>()));
            NetworkServer.RegisterHandler((short)Msgs.LoadingGameStateRefresh, (m) =>
                ServerMsgCallbacks.loadingGameStateRefresh(m.conn.connectionId, m.ReadMessage<NetworkPlayerReadyNotice>()));
            NetworkServer.RegisterHandler((short)Msgs.LoadingErrorNotify, (m) =>
                ServerMsgCallbacks.loadingErrorReceived(m.conn.connectionId));
            NetworkServer.RegisterHandler((short)Msgs.LoadingReadyNotify, (m) =>
                ServerMsgCallbacks.loadingReadyReceived(m.conn.connectionId, m.ReadMessage<NetworkPlayerReadyNotice>()));
            NetworkServer.RegisterHandler((short)Msgs.BoardChooseAction, (m) =>
                ServerMsgCallbacks.boardActionReceived(m.conn.connectionId, m.ReadMessage<BoardActionMessage>()));
            NetworkServer.RegisterHandler((short)Msgs.BoardClientEOT, (m) =>
                ServerMsgCallbacks.boardEndOfTurnReceived(m.conn.connectionId, m.ReadMessage<EndOfTurnStateMessage>()));
            NetworkServer.RegisterHandler((short)Msgs.BoardClientEOTRefresh, (m) =>
                ServerMsgCallbacks.boardEndOfTurnRefreshed(m.conn.connectionId, m.ReadMessage<NetworkPlayerReadyNotice>()));
        }

        //private void Dummy() {

        //}

        public void SendValidationRequest(ValidationInfo info) {
            ClientSendMessage(Msgs.ValidateAsk, info);
        }

        public void SendValidationResponse(int connectionID, ValidationResponse response) {
            ServerSendMessage(connectionID, Msgs.ValidateAnswer, response);
        }

        public static void ClientSendLobbyInitReady(NetworkPlayerReadyNotice info) {
            Instance.ClientSendMessage(Msgs.LobbyInitReadyNotice, info);
        }

        public static void ServerSendLobbyInitAllReady() {
            Instance.ServerSendMessage(Msgs.LobbyInitAllReady, new EmptyMessage());
        }

        public static void ClientSendLobbyPickedTurnOrder(LobbyPickedTurnOrderMessage picked) {
            Instance.ClientSendMessage(Msgs.LobbyPickedTurnOrderNotify, picked);
        }

        public static void ServerSendLobbyPickedTurnOrder(int connectionID, LobbyPickedTurnOrderMessage picked) {
            Instance.ServerSendMessage(connectionID, Msgs.LobbyPickedTurnOrderRelay, picked);
        }

        public static void ClientSendLobbyModsList(ModListNotifyMessage list) {
            Instance.ClientSendMessage(Msgs.LobbyModsListNotify, list);
        }

        public static void ServerSendLobbyModsList(ModListServerMessage list) {
            Instance.ServerSendMessage(Msgs.LobbyModsListRelay, list);
        }

        public static void ClientSendLobbyModsPick(NetworkPickedMod picked) {
            Instance.ClientSendMessage(Msgs.LobbyModsPickNotify, picked);
        }

        public static void ServerSendLobbyModsPick(int connectionID, NetworkPickedMod picked) {
            Instance.ServerSendMessage(connectionID, Msgs.LobbyModsPickRelay, picked);
        }

        public static void ClientSendLobbyReadyNotice(NetworkPlayerReadyNotice notice) {
            Instance.ClientSendMessage(Msgs.LobbyNotifyReady, notice);
        }

        public static void ServerSendLobbyReadyRelay(int connectionID, NetworkPlayerReadyNotice message) {
            Instance.ServerSendMessage(connectionID, Msgs.LobbyReadyRelay, message);
        }

        public static void ServerSendLobbyExitMessage(LobbyExitMessage message) {
            Instance.ServerSendMessage(Msgs.LobbyExitSettingsMessage, message);
        }

        public static void ClientSendLoadingGameState(LoadingGameStateMessage msg) {
            Instance.ClientSendMessage(Msgs.LoadingGameStateSend, msg);
        }

        public static void ClientSendLoadingGameStateRefresh(NetworkPlayerReadyNotice msg) {
            Instance.ClientSendMessage(Msgs.LoadingGameStateRefresh, msg);
        }

        public static void ServerSendLoadingGameState(LoadingGameStateMessage msg) {
            Instance.ServerSendMessage(Msgs.LoadingGameStateReply, msg);
        }

        public static void ClientSendLoadingError() {
            Instance.ClientSendMessage(Msgs.LoadingErrorNotify, new EmptyMessage());
        }

        public static void ServerSendLoadingError() {
            Instance.ServerSendMessage(Msgs.LoadingErrorRelay, new EmptyMessage());
        }

        public static void ClientSendLoadingReadyState(NetworkPlayerReadyNotice msg) {
            Instance.ClientSendMessage(Msgs.LoadingReadyNotify, msg);
        }

        public static void ServerSendLoadingExit() {
            Instance.ServerSendMessage(Msgs.LoadingReadyExitMessage, new EmptyMessage());
        }

        public static void ClientSendBoardAction(BoardActionMessage msg) {
            Instance.ClientSendMessage(Msgs.BoardChooseAction, msg);
        }

        public static void ServerSendBoardAction(int connectionID, BoardActionMessage msg) {
            Instance.ServerSendMessage(connectionID, Msgs.BoardActionRelay, msg);
        }

        public static void ClientSendEoT(EndOfTurnStateMessage msg) {
            Instance.ClientSendMessage(Msgs.BoardClientEOT, msg);
        }

        public static void ClientSendEoTRefresh(NetworkPlayerReadyNotice msg) {
            Instance.ClientSendMessage(Msgs.BoardClientEOTRefresh, msg);
        }

        public static void ServerSendEoT(ServerEndOfTurnStateMessage msg) {
            Instance.ServerSendMessage(Msgs.BoardServerEOT, msg);
        }
    }
}
