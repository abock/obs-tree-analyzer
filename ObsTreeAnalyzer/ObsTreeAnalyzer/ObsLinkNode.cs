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
using System.Linq;
using System.Collections.Generic;

namespace ObsTreeAnalyzer
{
    public class ObsLinkNode : ObsXmlNode
    {
        public ObsPackageNode Package { get; internal protected set; }

        public string TargetProjectName { get; protected set; }
        public string TargetPackageName { get; protected set; }
        public string TargetBaseRevision { get; protected set; }
        public string TargetRevision { get; protected set; }
        public string CommitCountAction { get; protected set; }

        private List<FileNode> deleted_files = new List<FileNode> ();
        public List<FileNode> DeletedFiles {
            get { return deleted_files; }
        }

        public int ModificationCount {
            get { return DeletedFiles.Count + Package.AllFiles.Count; }
        }

        public override void Load ()
        {
            Name = "_link";
            var xp = XPathLoad (BasePath);
            TargetProjectName = XPathSelectSingle (xp, "/link/@project") ?? Package.Project.Name;
            TargetPackageName = XPathSelectSingle (xp, "/link/@package") ?? Package.Name;
            TargetBaseRevision = XPathSelectSingle (xp, "/link/@baserev");
            TargetRevision = XPathSelectSingle (xp, "/link/@rev");
            CommitCountAction = XPathSelectSingle (xp, "/link/@cicount");

            if (TargetPackageName == Package.Name) {
                TargetPackageName = null;
            }

            var iter = xp.Select ("/link/patches/delete/@name");
            while (iter.MoveNext ()) {
                var file = new FileNode () { BasePath = iter.Current.Value };
                file.Load ();
                DeletedFiles.Add (file);
            }

            iter = xp.Select ("/link/patches/apply/@name");
            int application_index = 0;
            while (iter.MoveNext ()) {
                var patch = (from p in Package.PatchFiles
                    where p.Name == iter.Current.Value
                    select p).FirstOrDefault ();
                if (patch != null) {
                    patch.ApplicationIndex = application_index++;
                }
            }
        }

        public override string ToString ()
        {
            return String.Format ("{0} => {1}/{2} @ {3}",
                Name, TargetProjectName, TargetPackageName, TargetBaseRevision ?? "<HEAD>");
        }
    }
}
