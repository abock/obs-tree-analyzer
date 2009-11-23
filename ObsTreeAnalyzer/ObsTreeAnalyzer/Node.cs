// 
// Node.cs
//  
// Author:
//   Aaron Bockover <abockover@novell.com>
// 
// Copyright 2009 Novell, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Collections.Generic;

namespace ObsTreeAnalyzer
{
    public abstract class Node
    {
        private List<Node> children = new List<Node> ();
        public List<Node> Children {
            get { return children; }
        }

        public virtual string Name { get; protected set; }
        public virtual string BasePath { get; set; }
        public virtual Node Parent { get; internal protected set; }

        public virtual Node RootAncestor {
            get {
                var root = Parent ?? this;
                while (root != null && root.Parent != null) {
                    root = root.Parent;
                }
                return root;
            }
        }

        public virtual void Load ()
        {
        }

        protected static string BuildPath (string first, params string [] components)
        {
            if (components == null || components.Length == 0) {
                return first;
            }
            
            var result = first;
            foreach (var component in components) {
                result = Path.Combine (result, component);
            }
            return result;
        }

        public Node GetChild (string name)
        {
            return GetChild (name, null);
        }

        public T GetChild<T> (string name) where T : Node
        {
            return GetChild (name, typeof (T)) as T;
        }

        public Node GetChild (string name, Type type)
        {
            return GetChild (child => child.Name == name
                && (type == null || child.GetType () == type));
        }

        public T GetChild<T> () where T : Node
        {
            return GetChild (child => child is T) as T;
        }

        public Node GetChild (Predicate<Node> match)
        {
            return Children.Find (match);
        }

        public IEnumerable<T> GetChildren<T> () where T : Node
        {
            return GetChildren<T> (false);
        }

        public IEnumerable<T> GetChildren<T> (bool exactType) where T : Node
        {
            foreach (var child in GetChildren (child => exactType
                ? child is T
                : typeof (T).IsInstanceOfType (child))) {
                yield return child as T;
            }
        }

        public IEnumerable<Node> GetChildren (Predicate<Node> match)
        {
            return Children.FindAll (match);
        }

        public void WithChildren (Predicate<Node> match, Action<Node> action)
        {
             foreach (var child in GetChildren (match)) {
                action (child);
             }
        }

        public void WithChildren<T> (Action<T> action) where T : Node
        {
            WithChildren<T> (action, false);
        }

        public void WithChildren<T> (Action<T> action, bool exactType) where T : Node
        {
            foreach (var child in GetChildren<T> (exactType)) {
                action (child);
            }
        }

        public override string ToString ()
        {
            return String.Format ("{0} [{1}]", Name, Children.Count);
        }
    }
}
