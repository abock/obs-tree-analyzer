// 
// ObsRequestNode.cs
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
using System.Xml;
using System.Xml.XPath;

namespace ObsTreeAnalyzer
{
    public class ObsRequestNode : Node
    {
        public class PackageAction
        {
            internal PackageAction (XPathNavigator nav)
            {
                Project = nav.SelectSingleNode ("@project").Value;
                Package = nav.SelectSingleNode ("@package").Value;
            }

            public string Project { get; protected set; }
            public string Package { get; protected set; }
        }

        public class RequestAction
        {
            internal RequestAction (XPathNavigator nav)
            {
                Type = nav.SelectSingleNode ("@type").Value;
                Source = new PackageAction (nav.SelectSingleNode ("source"));
                Target = new PackageAction (nav.SelectSingleNode ("target"));
            }

            public string Type { get; protected set; }
            public PackageAction Source { get; protected set; }
            public PackageAction Target { get; protected set; }
        }

        public class RequestState
        {
            internal RequestState (XPathNavigator nav)
            {
                Name = nav.SelectSingleNode ("@name").Value;
                When = nav.SelectSingleNode ("@when").ValueAsDateTime;
                Who = nav.SelectSingleNode ("@who").Value;
            }

            public string Name { get; protected set; }
            public DateTime When { get; protected set; }
            public string Who { get; protected set; }
        }

        public string ID { get; protected set; }
        public string Description { get; protected set; }
        public RequestState State { get; protected set; }
        public RequestAction Action { get; protected set; }

        public ObsRequestNode ()
        {
        }

        public ObsRequestNode (XPathNavigator nav)
        {
            ID = nav.SelectSingleNode ("@id").Value;
            Description = nav.SelectSingleNode ("description").Value;
            State = new RequestState (nav.SelectSingleNode ("state"));
            Action = new RequestAction (nav.SelectSingleNode ("action"));
        }
    }
}
