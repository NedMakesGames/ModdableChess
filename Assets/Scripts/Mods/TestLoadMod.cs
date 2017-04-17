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

using ModdableChess.Mods;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModdableChess.TestMod {
    public class TestLoadMod : MonoBehaviour {

        private IEnumerator Start() {
            //StartCoroutine(LoadTestModel());
            yield return 1;
            //FindObjectOfType<ModDeepLoader>().LoadMod(@"F:\Documents\Snicker Games\Games\Moddable Chess\ChessMod");
        }

        //IEnumerator LoadTestModel() {
        //    while(!Caching.ready)
        //        yield return null;
        //    // Start a download of the given URL
        //    WWW www = WWW.LoadFromCacheOrDownload("file://F:/Documents/Snicker Games/Games/Moddable Chess/AssetBundles/chess", 1);

        //    // Wait for download to complete
        //    yield return www;

        //    // Load and retrieve the AssetBundle
        //    AssetBundle bundle = www.assetBundle;

        //    // Load the object asynchronously
        //    AssetBundleRequest request = bundle.LoadAssetAsync("Pawn", typeof(GameObject));

        //    // Wait for completion
        //    yield return request;

        //    // Get the reference to the loaded object
        //    GameObject obj = request.asset as GameObject;

        //    // Unload the AssetBundles compressed contents to conserve memory
        //    bundle.Unload(false);

        //    // Frees the memory from the web stream
        //    www.Dispose();

        //    Instantiate(obj);
        //}
    }
}
