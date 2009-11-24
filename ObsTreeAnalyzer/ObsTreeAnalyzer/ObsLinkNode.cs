// 
// ObsLinkNode.cs
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
    public class ObsLinkNode : ObsXmlNode
    {
        public string TargetProjectName { get; protected set; }
        public string TargetPackageName { get; protected set; }
        public string TargetBaseRevision { get; protected set; }

        public override void Load ()
        {
            Name = "_link";
            var xp = XPathLoad (BasePath);
            TargetProjectName = XPathSelectSingle (xp, "/link/@project") ?? RootAncestor.Name;
            TargetPackageName = XPathSelectSingle (xp, "/link/@package") ?? Parent.Name;
            TargetBaseRevision = XPathSelectSingle (xp, "/link/@baserev");

            if (TargetPackageName == Parent.Name) {
                TargetPackageName = null;
            }
        }

        public override string ToString ()
        {
            return String.Format ("{0} => {1}/{2} @ {3}",
                Name, TargetProjectName, TargetPackageName, TargetBaseRevision ?? "<HEAD>");
        }
    }
}
