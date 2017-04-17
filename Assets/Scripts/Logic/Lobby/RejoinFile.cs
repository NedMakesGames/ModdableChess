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
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModdableChess.Logic.Lobby {
    public class RejoinFile {

        public class Data {
            public bool isHost;
            public string address;
            public int port;
            public string password;
            public string modFolder;
        }

        private static readonly string FILE_NAME = "rejoin.txt"; 

        private static string GetFilePath() {
            return Path.Combine(Application.dataPath, FILE_NAME);
        }

        public static void WriteHost(int port, string password, string modFolder) {
            try {
                using(BinaryWriter writer = new BinaryWriter(new BufferedStream(new FileStream(GetFilePath(), FileMode.Create, FileAccess.Write)))) {
                    writer.Write(true);
                    writer.Write(port);
                    writer.Write(password);
                    writer.Write(modFolder);
                }
            } catch (IOException iox) {
                UnityEngine.Debug.LogException(iox);
            }
        }

        public static void WriteClient(string address, int port, string password, string modFolder) {
            try {
                using(BinaryWriter writer = new BinaryWriter(new BufferedStream(new FileStream(GetFilePath(), FileMode.Create, FileAccess.Write)))) {
                    writer.Write(false);
                    writer.Write(address);
                    writer.Write(port);
                    writer.Write(password);
                    writer.Write(modFolder);
                }
            } catch(IOException iox) {
                UnityEngine.Debug.LogException(iox);
            }
        }

        public static void Clear() {
            string file = GetFilePath();
            if(File.Exists(file)) {
                File.Delete(GetFilePath());
            }
        }

        public static bool Exists() {
            return File.Exists(GetFilePath());
        }

        public static bool Read(out Data data) {
            data = new Data();
            string file = GetFilePath();
            if(File.Exists(file)) {
                try {
                    using(BinaryReader reader = new BinaryReader(new BufferedStream(new FileStream(GetFilePath(), FileMode.Open, FileAccess.Read)))) {
                        if(reader.ReadBoolean()) {
                            data.isHost = true;
                        } else {
                            data.isHost = false;
                            data.address = reader.ReadString();
                        }
                        data.port = reader.ReadInt32();
                        data.password = reader.ReadString();
                        data.modFolder = reader.ReadString();
                    }
                    return true;
                } catch (IOException iox) {
                    UnityEngine.Debug.LogException(iox);
                    return false;
                }
            } else {
                return false;
            }
        }
    }
}
