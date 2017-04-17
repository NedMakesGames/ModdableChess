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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Baluga3.GameFlowLogic {
    public class ActivatableList : IActivatable, IList<IActivatable> {

        private bool activated;
        private bool disposed;
        private List<IActivatable> list;

        public ActivatableList(bool startActive)  {
            list = new List<IActivatable>();
            activated = startActive;
        }

        public ActivatableList() : this(false) {
        }

        public IActivatable this[int index] {
            get {
                return disposed ? list[index] : null;
            }

            set {
                if(!disposed) {
                    list[index] = value;
                }
            }
        }

        public int Count {
            get {
                return disposed ? 0 : list.Count;
            }
        }

        public bool IsReadOnly {
            get {
                return false;
            }
        }

        public bool Active {
            get {
                return activated;
            }

            set {
                this.activated = value;
                foreach(var r in list) {
                    r.Active = activated;
                }
            }
        }

        public void Add(IActivatable item) {
            if(disposed) {
                return;
            }
            list.Add(item);
            item.Active = activated;
        }

        public void Clear() {
            if(disposed) {
                return;
            }
            list.Clear();
        }

        public bool Contains(IActivatable item) {
            if(disposed) {
                return false;
            }
            return list.Contains(item);
        }

        public void CopyTo(IActivatable[] array, int arrayIndex) {
            if(disposed) {
                return;
            }
            list.CopyTo(array, arrayIndex);
        }

        public void Dispose() {
            if(!disposed) {
                disposed = true;
                activated = false;
                foreach(var r in list) {
                    r.Dispose();
                }
                list.Clear();
                list = null;
            }
        }

        public IEnumerator<IActivatable> GetEnumerator() {
            return list.GetEnumerator();
        }

        public int IndexOf(IActivatable item) {
            if(disposed) {
                return -1;
            }
            return list.IndexOf(item);
        }

        public void Insert(int index, IActivatable item) {
            if(disposed) {
                return;
            }
            list.Insert(index, item);
            item.Active = activated;
        }

        public bool Remove(IActivatable item) {
            if(disposed) {
                return false;
            }
            return list.Remove(item);
        }

        public void RemoveAt(int index) {
            if(disposed) {
                return;
            }
            list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return list.GetEnumerator();
        }
    }
}
