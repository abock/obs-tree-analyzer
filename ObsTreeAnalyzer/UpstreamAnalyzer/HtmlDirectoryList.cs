// 
// HtmlDirectoryList.cs
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
using System.Net;
using System.Text.RegularExpressions;

namespace UpstreamAnalyzer
{
    public abstract class HtmlDirectoryList : SourceList
    {
        protected abstract Uri DirectoryListingUri { get; }
        protected abstract Regex MatchFileExpression { get; }

        protected virtual bool ParseNameVersion (string s, Source source)
        {
            for (int i = 0, l = s.Length; i < l; i++) {
                if (i < l - 2 && s[i] == '-' && Char.IsDigit (s[i + 1])) {
                    source.Version = s.Substring (i + 1);
                    source.Name = s.Substring (0, i);
                    break;
                }
            }

            return source.Name != null && source.Version != null;
        }

        public override void Load ()
        {
            using (var stream = FetchListing ()) {
                using (var reader = new StreamReader (stream)) {
                    string line = null;
                    while ((line = reader.ReadLine ()) != null) {
                        var match = MatchFileExpression.Match (line);
                        if (match.Success) {
                            var source = new Source () {
                                FileName = match.Groups[1].Value,
                                FetchUri = new Uri (DirectoryListingUri, match.Groups[1].Value)
                            };

                            if (ParseNameVersion (source.FileName, source)) {
                                Add (source.Name, source);
                            }
                        }
                    }
                }
            }
        }

        protected virtual Stream FetchListing ()
        {
            var request = (HttpWebRequest)WebRequest.Create (DirectoryListingUri);
            request.Method = "GET";
            request.AllowAutoRedirect = true;
            return ((HttpWebResponse)request.GetResponse ()).GetResponseStream ();
        }
    }
}
