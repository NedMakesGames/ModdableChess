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


using System.Collections;
using System.Collections.Generic;
using System;

namespace Baluga3.Core {
    [Serializable]
    public class RemovalSafeList<T> : IList<T> {

        private List<T> list;
        private int iterAt, newAfter;

        public RemovalSafeList() : this(new List<T>()) {
        }

        public RemovalSafeList(List<T> list) {
            this.list = list;
            iterAt = -1;
        }

        public int IndexOf(T item) {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item) {
            if(iterAt >= index) {
                iterAt++;
            }
            list.Insert(index, item);
            // New object skipping not supported for insertion
            newAfter++;
        }

        public void RemoveAt(int index) {
            if(iterAt >= index) {
                iterAt--;
            }
            list.RemoveAt(index);
            newAfter--;
        }

        public void Add(T item) {
            list.Add(item);
        }

        public void Clear() {
            list.Clear();
            newAfter = 0;
        }

        public T this[int index] {
            get {
                return list[index];
            }
            set {
                list[index] = value;
            }
        }

        public bool Contains(T item) {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            for(int i = 0; i < list.Count; i++) {
                array[arrayIndex + i] = list[i];
            }
        }

        public int Count {
            get { return list.Count; }
        }

        public bool IsReadOnly {
            get { return false; }
        }

        public bool Remove(T item) {
            int indexOf = list.IndexOf(item);
            if(indexOf >= 0) {
                RemoveAt(indexOf);
                return true;
            } else {
                return false;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            if(IsIterationLocked) {
                //Debug.Log(iterAt);
                throw new System.Exception("Cannot start another iteration while iteration in progress.");
            }
            //Debug.Log("Start");
            iterAt = 0;
            newAfter = list.Count;
            try {
                for(iterAt = 0; iterAt < newAfter; iterAt++) {
                    yield return list[iterAt];
                }
            } finally {
                iterAt = -1;
            }
            iterAt = -1; // Add this in case WebGL does something wierd
            //Debug.Log("End");
        }

        IEnumerator IEnumerable.GetEnumerator() {
            foreach(var obj in this) {
                yield return obj; 
            }
        }

        public bool IsIterationLocked {
            get {
                return iterAt >= 0;
            }
        }
    }
}
