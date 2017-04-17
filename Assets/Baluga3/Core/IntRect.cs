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
using UnityEngine;

namespace Baluga3.Core {
    [Serializable]
    public struct IntRect : IEquatable<IntRect> {
        private int x, y, width, height;

        public static readonly IntRect Empty = new IntRect(0, 0, 0, 0);

        public IntRect(int x, int y, int w, int h) {
            this.x = x;
            this.y = y;
            this.width = w;
            this.height = h;
        }

        public IntRect(IntVector2 pos, IntVector2 dimensions)
            : this(pos.X, pos.Y, dimensions.X, dimensions.Y) {
        }

        public IntVector2 Dimensions {
            get {
                return new IntVector2(width, height);
            }
        }

        public int X {
            get {
                return x;
            }
        }

        public int Y {
            get {
                return y;
            }
        }

        public int Width {
            get {
                return width;
            }
        }

        public int Height {
            get {
                return height;
            }
        }

        public int MinX {
            get {
                return x;
            }
        }

        public int MaxX {
            get {
                return x + width - 1;
            }
        }

        public int MinY {
            get {
                return y;
            }
        }

        public int MaxY {
            get {
                return y + height - 1;
            }
        }

        public bool IsEmpty() {
            return width == 0 || height == 0;
        }

        public IntVector2 Min() {
            return new IntVector2(x, y);
        }

        public IntVector2 Max() {
            return new IntVector2(MaxX, MaxY);
        }

        public int Area() {
            return width * height;
        }

        public Vector2 Clamp(Vector2 v) {
            return new Vector2(
                Mathf.Clamp(v.x, MinX, MaxX),
                Mathf.Clamp(v.y, MinY, MaxY)
                );
        }

        public static IntRect FromPoints(IntVector2 a, IntVector2 b) {
            return new IntRect(Math.Min(a.X, b.Y),
                               Math.Min(a.Y, b.Y),
                               Math.Abs(b.X - a.X) + 1,
                               Math.Abs(b.Y - a.Y) + 1);
        }

        public static IntRect Enclose(IntRect a, IntRect b) {
            return FromPoints(
                new IntVector2(Math.Min(a.MinX, b.MinX), Math.Min(a.MinY, b.MinY)),
                new IntVector2(Math.Max(a.MaxX, b.MaxX), Math.Max(a.MaxY, b.MaxY)));
        }

        public static IntRect FromRadius(IntVector2 center, IntVector2 radius) {
            return FromPoints(center - radius, center + radius);
        }

        public IntRect Intersect(IntRect o) {
            IntRect overlap = new IntRect();
            int minMaxX = Math.Min(x + width, o.x + o.width);
            int maxMinX = Math.Max(x, o.x);
            int minMaxY = Math.Min(y + height, o.y + o.height);
            int maxMinY = Math.Max(y, o.y);
            overlap.x = maxMinX;
            overlap.y = maxMinY;
            overlap.width = minMaxX - maxMinX;
            overlap.height = minMaxY - maxMinY;
            return overlap;
        }

        public IEnumerator<IntVector2> GetEnumerator() {
            for(int xi = 0; xi < width; xi++) {
                for(int yi = 0; yi < height; yi++) {
                    yield return new IntVector2(x + xi, y + yi);
                }
            }
        }

        public List<IntVector2> BorderPoints() {
            List<IntVector2> p = new List<IntVector2>();
            BorderPoints(p);
            return p;
        }

        public void BorderPoints(List<IntVector2> p) {
            p.Clear();
            for(int xi = 0; xi < width; xi++) {
                p.Add(new IntVector2(x + xi, y));
            }
            for(int yi = 1; yi < height - 1; yi++) {
                p.Add(new IntVector2(x + width - 1, y + yi));
            }
            for(int xi = width - 1; xi >= 0; xi--) {
                p.Add(new IntVector2(x + xi, y + height - 1));
            }
            for(int yi = height - 2; yi >= 1; yi--) {
                p.Add(new IntVector2(x + width + 1, y + yi));
            }
        }

        public IntRect Enlarge(int xRadius, int yRadius) {
            return new IntRect(x - xRadius, y - yRadius, width + xRadius * 2, height + yRadius * 2);
        }

        public bool ContainsPoint(IntVector2 p) {
            int difx = p.X - x;
            int dify = p.Y - y;
            return difx >= 0 && difx < width && dify >= 0 && dify < height;
        }

        public bool ContainsRect(IntRect p) {
            return (p.x >= x && p.y >= y && p.MaxX <= MaxX && p.MaxY <= MaxY);
        }

        public override string ToString() {
            return string.Format("[{0} {1} {2} {3}]", x, y, width, height);
        }

        public override bool Equals(object obj) {
            if(obj is IntRect) {
                return Equals((IntRect)obj);
            } else {
                return false;
            }
        }

        public bool Equals(IntRect o) {
            return x == o.X && y == o.Y && width == o.Width && height == o.Height;
        }

        public static bool operator ==(IntRect a, IntRect b) {
            return a.Equals(b);
        }

        public static bool operator !=(IntRect a, IntRect b) {
            return !a.Equals(b);
        }

        public override int GetHashCode() {
            uint hash = (uint)x;
            hash *= 37;
            hash += (uint)y;
            hash *= 37;
            hash += (uint)width;
            hash *= 37;
            hash += (uint)height;
            return (int)hash;
        }
    }
}
