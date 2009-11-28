// 
// OscRcAccountCollection.cs
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

using ICSharpCode.SharpZipLib.BZip2;

namespace OpenSuse.BuildService
{
    public class OscRcAccountCollection : AccountCollection
    {
        public OscRcAccountCollection () 
            : this (Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".oscrc"))
        {
        }
    
        public OscRcAccountCollection (string oscrcPath)
        {
            using (var reader = new StreamReader (oscrcPath)) {
                Account account = null;
                string line = null;
                while ((line = reader.ReadLine ()) != null) {
                    line = line.Trim ();
                    if (String.IsNullOrEmpty (line)) {
                        continue;
                    } else if (line.StartsWith ("[http") && line.EndsWith ("]")) {
                        account = new Account () {
                            ApiUrl = line.Substring (1, line.Length - 2).Trim ().ToLower ().Trim ('/')
                        };
                        Add (account);
                    } else if (account != null && (line.StartsWith ("user") || line.StartsWith ("pass"))) {
                        var parts = line.Split (new char [] { '=' }, 2);
                        if (parts.Length == 2) {
                            switch (parts[0].Trim ()) {
                                case "user": account.Username = parts[1].Trim (); break;
                                case "pass": account.Password = parts[1].Trim (); break;
                                case "passx":
                                    var passx_decoded = PassxDecode (parts[1].Trim ());
                                    if (passx_decoded != null) {
                                        account.Password = passx_decoded;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private string PassxDecode (string passx)
        {
            using (var input = new MemoryStream ()) {
                var passx_base64_decoded = Convert.FromBase64String (passx);
                input.Write (passx_base64_decoded, 0, passx_base64_decoded.Length);
                input.Seek (0, SeekOrigin.Begin);
                using (var output = new BZip2InputStream (input)) {
                    var passx_bzip2_decoded = new byte[1024];
                    output.Read (passx_bzip2_decoded, 0, passx_bzip2_decoded.Length);
                    return System.Text.Encoding.UTF8.GetString (passx_bzip2_decoded);
                }
            }

            return null;
        }
    }
}