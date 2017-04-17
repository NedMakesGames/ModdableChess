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

namespace Baluga3.GameFlowLogic {
    public class OrderedMessage {
        private List<IListener> listeners;
        private Dictionary<IListener, int> priorities;
        private bool needsSorting;
        private Comparison<IListener> cachedSortHelper;

        public OrderedMessage() {
            this.listeners = new List<IListener>();
            this.priorities = new Dictionary<IListener, int>();
            this.cachedSortHelper = SortHelper;
        }

        public void Insert(IListener listener, int priority) {
            priorities[listener] = priority;
            listeners.Add(listener);
            needsSorting = true;
        }

        public void Remove(IListener listener) {
            priorities.Remove(listener);
            listeners.Remove(listener);
        }

        public void Reprioritize(IListener listener, int priority) {
            priorities[listener] = priority;
            needsSorting = true;
        }

        private int SortHelper(IListener a, IListener b) {
            return priorities[a] - priorities[b];
        }

        public void Send() {
            if(needsSorting) {
                needsSorting = false;
                listeners.Sort(cachedSortHelper);
            }
        }
    }
}
