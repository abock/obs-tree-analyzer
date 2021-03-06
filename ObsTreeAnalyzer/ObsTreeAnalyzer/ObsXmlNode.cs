// 
// ObsXmlNode.cs
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
using System.Xml;
using System.Xml.XPath;

namespace ObsTreeAnalyzer
{
    public class ObsXmlNode : Node
    {
        protected XPathNavigator XPathLoadOsc (string oscFileName)
        {
            return XPathLoad (BasePath, ".osc", oscFileName);
        }
        
        protected XPathNavigator XPathLoad (string path, params string [] extraPath)
        {
            try {
                using (var stream = new FileStream (BuildPath (path, extraPath), FileMode.Open, FileAccess.Read)) {
                    return new XPathDocument (stream).CreateNavigator ();
                }
            } catch (Exception e) {
                Console.WriteLine ("Failed to parse: {0}", path);
                throw e;
            }
        }

        protected static string XPathSelectSingle (XPathNavigator nav, string expr)
        {
            var node = nav.SelectSingleNode (expr);
            return node == null ? null : node.Value;
        }
    }   
}
