// 
// ObsProjectNode.cs
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
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Collections.Generic;

using OpenSuse.BuildService;

namespace ObsTreeAnalyzer
{
    public class ObsProjectNode : ObsXmlNode
    {
        private List<ObsPackageNode> packages = new List<ObsPackageNode> ();
        public List<ObsPackageNode> Packages {
            get { return packages; }
        }

        private List<ObsRequestNode> requests = new List<ObsRequestNode> ();
        public List<ObsRequestNode> Requests {
            get { return requests; }
        }

        public override void Load ()
        {
            LoadPackages ();
            LoadSubmitRequests ();
        }

        private void LoadPackages ()
        {
            var xp = XPathLoadOsc ("_packages");
            Name = xp.SelectSingleNode ("/project/@name").Value;
            var iter = xp.Select ("/project/package/@name");
            while (iter.MoveNext ()) {
                var package = new ObsPackageNode () {
                    BasePath = BuildPath (BasePath, iter.Current.Value),
                    Project = this
                };
                package.Load ();
                Packages.Add (package);
            }
        }

        private void LoadSubmitRequests ()
        {
            var accounts = new OscRcAccountCollection ();
            var account = accounts.DefaultAccount;
            using (var reader = new StreamReader (BuildPath (BasePath, ".osc", "_apiurl"))) {
                account = accounts[reader.ReadToEnd ().Trim ()];
            }

            Console.WriteLine ("Checking for submit requests with OBS account:");
            Console.WriteLine ("    {0}", account);

            var doc = new XPathDocument (XmlReader.Create (account.ApiRequest.Get (
                @"/search/request?match=" +
                @"state/@name=""new"" and " +
                @"action/target/@project=""" + Name + @""""
            )));

            var nav = doc.CreateNavigator ();
            var iter = nav.Select ("/collection/request");
            while (iter.MoveNext ()) {
                Requests.Add (new ObsRequestNode (iter.Current));
            }
        }
    }
}