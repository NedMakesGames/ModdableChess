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
using Baluga3.GameFlowLogic;
using Baluga3.Core;
using ModdableChess.Logic;
using System;

namespace ModdableChess.Unity {
    public class MoveOptionModels : MonoBehaviour {

        private class ModelCache {
            public List<GameObject> alive;
            public Stack<GameObject> cached;
        }

        private int[,] heights;
        private Dictionary<int, ModelCache> caches;
        private GameDatabase db;
        private ActionIndicatorPattern selPattern;
        private ActionIndicatorPattern mousePattern;

        private void Start() {
            db = GameLink.Game.Components.Get<GameDatabase>((int)ComponentKeys.GameDatabase);
            heights = new int[db.BoardDimensions.x, db.BoardDimensions.y];
            caches = new Dictionary<int, ModelCache>();

            GameLink.Game.SceneComponents.GetOrRegister<Message<ActionIndicatorPattern>>((int)ComponentKeys.SelectedIndicatorPattern,
                Message<ActionIndicatorPattern>.Create).Subscribe(new SimpleListener<ActionIndicatorPattern>(OnReceivedSelected));
            GameLink.Game.SceneComponents.GetOrRegister<Message<ActionIndicatorPattern>>((int)ComponentKeys.MouseOverIndicatorPattern,
                Message<ActionIndicatorPattern>.Create).Subscribe(new SimpleListener<ActionIndicatorPattern>(OnReceivedMouseOver));
        }

        private void OnReceivedSelected(ActionIndicatorPattern pattern) {
            selPattern = pattern;
            RefreshModels();
        }

        private void OnReceivedMouseOver(ActionIndicatorPattern pattern) {
            mousePattern = pattern;
            RefreshModels();
        }

        private void RefreshModels() {
            ClearCaches();
            for(int x = 0; x < heights.GetLength(0); x++) {
                for(int y = 0; y < heights.GetLength(1); y++) {
                    heights[x, y] = 0;
                }
            }
            if(selPattern != null) {
                BuildPattern(selPattern);
            }
            if(mousePattern != null) {
                BuildPattern(mousePattern);
            }
        }

        private void ClearCaches() {
            foreach(var cache in caches.Values) {
                foreach(var model in cache.alive) {
                    model.SetActive(false);
                    cache.cached.Push(model);
                }
                cache.alive.Clear();
            }
        }

        private GameObject GetModel(int index) {
            ModelCache cache;
            if(!caches.TryGetValue(index, out cache)) {
                cache = new ModelCache() {
                    alive = new List<GameObject>(),
                    cached = new Stack<GameObject>(),
                };
                caches[index] = cache;
            }
            GameObject model;
            if(cache.cached.Count > 0) {
                model = cache.cached.Pop();
                model.SetActive(true);
            } else {
                model = (GameObject)Instantiate(db.ModelPrefabs[index]);
            }
            cache.alive.Add(model);
            return model;
        }

        private void BuildPattern(ActionIndicatorPattern pattern) {
            foreach(var ind in pattern.indicators) {
                IntVector2 boardPos = ind.boardPosition;
                int height = heights[boardPos.x, boardPos.y];
                heights[boardPos.x, boardPos.y]++;
                int modelIndex = db.Indicators.GetIndexFor(ind.type, ind.strength);
                GameObject model = GetModel(modelIndex);
                Vector3 basePosition = PiecePositionHelper.FigureCenter(db, boardPos);
                Vector3 finalPosition = new Vector3(basePosition.x, basePosition.y + db.Indicators.StackingHeight * height, basePosition.z);
                model.transform.position = finalPosition;

                if(ind.decorationModel >= 0) {
                    GameObject decorModel = GetModel(ind.decorationModel);
                    decorModel.transform.position = finalPosition;
                    //decorModel.transform.rotation = Quaternion.Euler(90, 0, 90);
                }
            }
        }
    }
}
