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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModdableChess.Logic {
    public class ConnectionHelper {

        public enum State {
            None, Unconnected, Connected, Stopped
        }

        public enum Mode {
            None, Host, Client
        }

        private LocalPlayerInformation localInfo;
        private ServerInformation serverInfo;
        private Query<bool, StartHostCommand> startHostCommand;
        private Query<bool, StartClientCommand> startClientCommand;
        private Command stopHostCommand;
        private Command stopClientCommand;
        private StateMachine state;
        private Mode mode;

        public StateMachine Connection {
            get {
                return state;
            }
        }

        public Mode CurrentMode {
            get {
                return mode;
            }
        }

        public ConnectionHelper(ModdableChessGame game) {
            game.Components.Register((int)ComponentKeys.ConnectionHelper, this);

            localInfo = game.Components.GetOrRegister<LocalPlayerInformation>((int)ComponentKeys.LocalPlayerInformation, LocalPlayerInformation.Create);
            serverInfo = game.Components.GetOrRegister<ServerInformation>((int)ComponentKeys.ServerInformation, ServerInformation.Create);
            startHostCommand = game.Components.GetOrRegister<Query<bool, StartHostCommand>>
                ((int)ComponentKeys.StartHostCommand, Query<bool, StartHostCommand>.Create);
            stopHostCommand = game.Components.GetOrRegister<Command>((int)ComponentKeys.StopHostCommand, Command.Create);
            startClientCommand = game.Components.GetOrRegister<Query<bool, StartClientCommand>>
                ((int)ComponentKeys.StartClientCommand, Query<bool, StartClientCommand>.Create);
            stopClientCommand = game.Components.GetOrRegister<Command>((int)ComponentKeys.StopClientCommand, Command.Create);
            state = new StateMachine((int)State.None);

            localInfo.Connection.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnClientConnectionChange()));
            serverInfo.Connection.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) => OnServerConnectionChange()));
            state.EnterStateMessenger.Subscribe(new SimpleListener<int>((s) =>
                UnityEngine.Debug.LogError(string.Format("{0}: Helper connection change {2}-{1}", game.TickCount, (State)s, mode))));
        }

        public bool StartHost(int port, string password, bool wantsInProgressGames) {
            UnityEngine.Debug.LogError(string.Format("{0}: Start host {1} \"{2}\" {3}", UnityEngine.Time.frameCount, port, password, wantsInProgressGames));
            mode = Mode.Host;
            return startHostCommand.Send(new StartHostCommand() {
                port = port,
                password = password,
                wantsInProgressGame = wantsInProgressGames,
            });
        }

        public bool StartClient(string address, int port, string password, bool isGameInProgress) {
            UnityEngine.Debug.LogError(string.Format("{0}: Start client {1} {2} \"{3}\" {4}", UnityEngine.Time.frameCount, address, port, password, isGameInProgress));
            mode = Mode.Client;
            return startClientCommand.Send(new StartClientCommand() {
                address = address,
                port = port,
                password = password,
                gameInProgress = isGameInProgress,
            });
        }

        public void StopConnection() {
            switch(mode) {
            case Mode.Host:
                stopHostCommand.Send();
                break;
            case Mode.Client:
                stopClientCommand.Send();
                break;
            default:
                UnityEngine.Debug.LogError("No connection active to stop");
                break;
            }
        }

        public bool RestartConnection() {
            switch(mode) {
            case Mode.Host:
                return StartHost(serverInfo.Port, serverInfo.Password, serverInfo.WantsInProgressGame);
            case Mode.Client:
                return StartClient(localInfo.ServerAddress, localInfo.ServerPort, localInfo.Password, localInfo.InProgressGame);
            default:
                UnityEngine.Debug.LogError("Tried to restart connection when none ever existed");
                return false;
            }
        }

        public void SetGameInProgress(bool inProgress) {
            switch(mode) {
            case Mode.Host:
                serverInfo.WantsInProgressGame = inProgress;
                break;
            case Mode.Client:
                localInfo.InProgressGame = inProgress;
                break;
            default:
                UnityEngine.Debug.LogError("No current connection");
                break;
            }
        }

        private void OnServerConnectionChange() {
            if(mode == Mode.Host) {
                switch((ServerConnectionState)serverInfo.Connection.State) {
                case ServerConnectionState.None:
                    mode = Mode.None;
                    state.State = (int)State.None;
                    break;
                case ServerConnectionState.Open:
                    state.State = (int)State.Unconnected;
                    break;
                case ServerConnectionState.Connected:
                    state.State = (int)State.Connected;
                    break;
                case ServerConnectionState.Stopped:
                    state.State = (int)State.Stopped;
                    break;
                }
            }
        }

        private void OnClientConnectionChange() {
            if(mode == Mode.Client) {
                switch((ClientConnectionState)localInfo.Connection.State) {
                case ClientConnectionState.None:
                    mode = Mode.None;
                    state.State = (int)State.None;
                    break;
                case ClientConnectionState.Seeking:
                    state.State = (int)State.Unconnected;
                    break;
                case ClientConnectionState.Validated:
                    state.State = (int)State.Connected;
                    break;
                case ClientConnectionState.Stopped:
                    state.State = (int)State.Stopped;
                    break;
                }
            }
        }

        public void WriteRejoinFile(string modFolder) {
            switch(mode) {
            case Mode.Host:
                RejoinFile.WriteHost(serverInfo.Port, serverInfo.Password, modFolder);
                break;
            case Mode.Client:
                RejoinFile.WriteClient(localInfo.ServerAddress, localInfo.ServerPort, localInfo.Password, modFolder);
                break;
            default:
                UnityEngine.Debug.LogError("No current mode to write to file");
                break;
            }
        }

        public enum RejoinFileRead {
            None=0, WrongMode, Success, NoData, StartupFail
        }

        public RejoinFileRead RejoinFromFile(bool wantsInProgress, out RejoinFile.Data data) {
            data = null;
            if(mode == Mode.None) {
                if(RejoinFile.Read(out data)) {
                    if(data.isHost) {
                        if(StartHost(data.port, data.password, wantsInProgress)) {
                            return RejoinFileRead.Success;
                        } else {
                            return RejoinFileRead.StartupFail;
                        }
                    } else {
                        if(StartClient(data.address, data.port, data.password, wantsInProgress)) {
                            return RejoinFileRead.Success;
                        } else {
                            return RejoinFileRead.StartupFail;
                        }
                    }
                } else {
                    return RejoinFileRead.NoData;
                }
            } else {
                UnityEngine.Debug.LogError("Cannot rejoin from file while a connection is active.");
                return RejoinFileRead.WrongMode;
            }
        }
    }
}
