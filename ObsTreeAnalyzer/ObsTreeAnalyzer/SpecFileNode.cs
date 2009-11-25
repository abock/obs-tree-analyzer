// 
// SpecFileNode.cs
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
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ObsTreeAnalyzer
{
    public class SpecFileNode : FileNode
    {
        public string SpecName { get; protected set; }
        public string Version { get; protected set; }
        public bool HasChangeLog { get; protected set; }

        private List<FileNode> sources = new List<FileNode> ();
        private List<PatchFileNode> patches = new List<PatchFileNode> ();

        public override void Load ()
        {
            base.Load ();

            var patch_map = new Dictionary<string, PatchFileNode> ();
            var patch_application_count = 0;
            var processed_setup = false;
            var in_changelog = false;

            using (var reader = new StreamReader (BasePath)) {
                string line = null;
                while ((line = reader.ReadLine ()) != null) {
                    // Strip leading/trailing whitespace and comments
                    line = line.Trim ();
                    line = Regex.Replace (line, @"^#.*", String.Empty);
                    line = Regex.Replace (line, @"\s+#.*", String.Empty);

                    var match = Regex.Match (line, @"^(\w+):\s*(.+)$");
                    if (match.Success) {
                        var field = match.Groups[1].Value.ToLower ();
                        var value = match.Groups[2].Value;

                        if (!String.IsNullOrEmpty (SpecName)) {
                            value = value.Replace ("%{name}", SpecName).Replace ("%name", SpecName);
                        }

                        if (!String.IsNullOrEmpty (Version)) {
                            value = value.Replace ("%{version}", Version).Replace ("%version", Version);
                        }

                        if (field == "name") {
                            SpecName = value;
                        } else if (field == "version") {
                            Version = value;
                        } else if (field == "changelog") {
                            in_changelog = true;
                        } else if (field.StartsWith ("patch")) {
                            foreach (var patch in
                                from patch in Package.PatchFiles
                                where patch.Name == value
                                select patch) {
                                patch.ApplicationIndex = -1;
                                patch_map.Add (field, patch);
                            };
                        } else if (field.StartsWith ("source")) {
                            foreach (var source in
                                from source in Package.SourceFiles
                                where source.Name == value
                                select source) {
                                source.IsSpecSource = true;
                                sources.Add (source);
                            }
                        } else {
                            in_changelog = false;
                        }

                        continue;
                    }

                    match = Regex.Match (line, @"^%(\w+)\s*");
                    if (match.Success) {
                        var directive = match.Groups[1].Value.ToLower ();
                        PatchFileNode patch = null;

                        if (directive == "setup") {
                            processed_setup = true;
                        } else if (directive.StartsWith ("patch") && processed_setup &&
                            patch_map.TryGetValue (directive, out patch)) {
                            patch.ApplicationIndex = patch_application_count++;
                            patches.Add (patch);
                        }

                        continue;
                    }

                    if (in_changelog && !String.IsNullOrEmpty (line)) {
                        HasChangeLog = true;
                    }
                }
            }
        }

        public override string ToString ()
        {
            return String.Format ("{0} [SpecName={1}, Version={2}, AppliedPatches={3}, Sources={4}, HasChangeLog={5}]",
                Path.GetFileName (BasePath), SpecName, Version, patches.Count, sources.Count, HasChangeLog);
        }
    }
}
