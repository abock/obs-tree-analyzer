// 
// ApiRequest.cs
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
using System.Security.Cryptography.X509Certificates;

namespace OpenSuse.BuildService
{
    public class ApiRequest
    {
        private class IgnoreAllCertificatePolicy : ICertificatePolicy
        {
            public bool CheckValidationResult (ServicePoint point, X509Certificate certificate,
                WebRequest request, int certificateProb)
            {
                return true;
            }
        }

        static ApiRequest ()
        {
            ServicePointManager.CertificatePolicy = new IgnoreAllCertificatePolicy ();
        }

        public Account Account { get; set; }

        private HttpWebRequest CreateRequest (string httpMethod, string obsResource)
        {
            if (obsResource[0] != '/') {
                throw new ArgumentException ("resource must start with a /", "obsResource");
            }
            
            var request = (HttpWebRequest)WebRequest.Create (new Uri (Account.ApiUrl + obsResource));
            request.Method = httpMethod;
            request.AllowAutoRedirect = true;
            request.Credentials = new NetworkCredential (Account.Username, Account.Password);
            request.PreAuthenticate = true;
            return request;
        }
        
        private Stream Invoke (string httpMethod, string obsResource)
        {
            var request = CreateRequest (httpMethod, obsResource);
            var response = (HttpWebResponse)request.GetResponse ();
            return response.GetResponseStream ();
        }

        public Stream Get (string obsResource)
        {
            return Invoke ("GET", obsResource);
        }
    }
}