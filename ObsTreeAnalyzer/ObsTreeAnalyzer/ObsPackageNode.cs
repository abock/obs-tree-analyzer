// 
// ObsPackageNode.cs
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

namespace ObsTreeAnalyzer
{
    public class ObsPackageNode : ObsXmlNode
    {
        public override void Load ()
        {
            var xp = XPathLoadOsc ("_files");

            Name = xp.SelectSingleNode ("/directory/@name").Value;

            var iter = xp.Select ("/directory/entry/@name");
            while (iter.MoveNext ()) {
                var node_path = BuildPath (BasePath, iter.Current.Value);
                var child = iter.Current.Value == "_link"
                    ? (Node)new ObsLinkNode () { BasePath = node_path }
                    : (Node)FileNode.Resolve (node_path);

                child.Parent = this;
                Children.Add (child);

                // We will load these nodes after all other nodes
                if (child is ObsLinkNode || child is SpecFileNode) {
                    continue;
                }

                child.Load ();
            }

            // Always load the link and spec files last since they depend on other children
            WithChildren<ObsLinkNode> (child => child.Load ());
            WithChildren<SpecFileNode> (child => child.Load ());
        }
    }
}