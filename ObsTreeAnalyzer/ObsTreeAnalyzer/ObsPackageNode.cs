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
using System.Linq;
using System.Collections.Generic;

namespace ObsTreeAnalyzer
{
    public class ObsPackageNode : ObsXmlNode
    {
        public ObsProjectNode Project { get; internal protected set; }

        public ObsLinkNode Link { get; set; }

        private List<SpecFileNode> spec_files = new List<SpecFileNode> ();
        public List<SpecFileNode> SpecFiles {
            get { return spec_files; }
        }

        private List<PatchFileNode> patch_files = new List<PatchFileNode> ();
        public List<PatchFileNode> PatchFiles {
            get { return patch_files; }
        }

        public List<PatchFileNode> AppliedPatchFiles {
            get { return PatchFiles.FindAll (patch => patch.ApplicationIndex >= 0); }
        }

        public List<PatchFileNode> UnappliedPatchFiles {
            get { return new List<PatchFileNode> (PatchFiles.Except (AppliedPatchFiles)); }
        }

        private List<FileNode> source_files = new List<FileNode> ();
        public List<FileNode> SourceFiles {
            get { return source_files; }
        }

        public List<FileNode> AllFiles {
            get {
                var all = new List<FileNode> ();
                SpecFiles.ForEach (file => all.Add (file));
                SourceFiles.ForEach (file => all.Add (file));
                PatchFiles.ForEach (file => all.Add (file));
                return all;
            }
        }

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

                // We will load these nodes after all other nodes
                var link = child as ObsLinkNode;
                if (link != null) {
                    link.Package = this;
                    Link = link;
                    continue;
                }

                var file = child as FileNode;
                if (file == null) {
                    continue;
                }

                file.Package = this;

                var spec = child as SpecFileNode;
                if (spec != null) {
                    SpecFiles.Add (spec);
                    continue;
                }

                var patch = child as PatchFileNode;
                if (patch != null) {
                    PatchFiles.Add (patch);
                    file.Load ();
                    continue;
                }

                SourceFiles.Add (file);
                file.Load ();
            }

            // Always load the link and spec files last since they depend on other children
            if (Link != null) {
                Link.Load ();
            }

            SpecFiles.ForEach (spec => spec.Load ());
        }
    }
}
