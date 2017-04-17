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
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModdableChess.Logic {
    public enum WinLossType {
        None=0, CaptureTheKing, Elimination, StrictCheckMate
    }

    [Serializable]
    public class GameDatabase : IDisposable {
        [SerializeField]
        private string modFolder;
        [SerializeField]
        private List<PiecePrototype> prototypes;
        [SerializeField]
        private IntVector2 boardDimensions;
        [SerializeField]
        private float worldTileSize;
        [SerializeField]
        private int boardModelIndex;
        [SerializeField]
        private IndicatorModelData indicatorData;
        [SerializeField]
        private List<GameObject> modelPrefabs;
        [SerializeField]
        private Closure winLossCheck;
        [SerializeField]
        private Closure setupFunction;

        public List<PiecePrototype> PiecePrototypes {
            get {
                return prototypes;
            }

            set {
                this.prototypes = value;
            }
        }

        public IntVector2 BoardDimensions {
            get {
                return boardDimensions;
            }

            set {
                this.boardDimensions = value;
            }
        }

        public List<GameObject> ModelPrefabs {
            get {
                return modelPrefabs;
            }

            set {
                this.modelPrefabs = value;
            }
        }

        public int BoardModelIndex {
            get {
                return boardModelIndex;
            }

            set {
                this.boardModelIndex = value;
            }
        }

        public float WorldTileSize {
            get {
                return worldTileSize;
            }

            set {
                this.worldTileSize = value;
            }
        }

        public string ModFolder {
            get {
                return modFolder;
            }

            set {
                this.modFolder = value;
            }
        }

        public IndicatorModelData Indicators {
            get {
                return indicatorData;
            }

            set {
                indicatorData = value;
            }
        }

        public Closure WinLossCheck {
            get {
                return winLossCheck;
            }

            set {
                winLossCheck = value;
            }
        }

        public Closure SetupFunction {
            get {
                return setupFunction;
            }

            set {
                setupFunction = value;
            }
        }

        public int PieceNameToIndex(string name) {
            for(int i = 0; i < prototypes.Count; i++) {
                if(prototypes[i].LuaTag.ToUpperInvariant() == name.ToUpperInvariant()) {
                    return i;
                }
            }
            return -1;
        }

        public void Dispose() {
            foreach(var model in modelPrefabs) {
                GameObject.DestroyImmediate(model, true);
            }
            modelPrefabs.Clear();
            indicatorData = null;
            prototypes.Clear();
            winLossCheck = null;
            setupFunction = null;
        }
    }
}
