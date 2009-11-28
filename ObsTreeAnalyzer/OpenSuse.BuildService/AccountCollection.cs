// 
// AccountCollection.cs
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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace OpenSuse.BuildService
{
    public class AccountCollection : IEnumerable<Account>
    {
        private OrderedDictionary accounts = new OrderedDictionary ();
        
        public Account DefaultAccount {
            get { return Count > 0 ? this[0] : null; }
        }
        
        public int Count {
            get { return accounts.Count; }
        }
        
        public Account this[int index] {
            get { return (Account)accounts[index]; }
        }
        
        public Account this[string apiUrl] {
            get { return (Account)accounts[apiUrl]; }
        }
        
        public void Add (Account account)
        {
            accounts.Add (account.ApiUrl, account);
        }
        
        public void Remove (Account account)
        {
            Remove (account.ApiUrl);
        }
        
        public void Remove (string apiUrl)
        {
            accounts.Remove (apiUrl);
        }
        
        public IEnumerator<Account> GetEnumerator ()
        {
            foreach (var account in accounts.Values) {
                yield return (Account)account;
            }
        }
        
        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator ();
        }

        public override string ToString ()
        {
            var builder = new StringBuilder ();
            for (int i = 0; i < accounts.Count; i++) {
                var account = this[i];
                builder.AppendFormat ("{0} => {1}", i, account);
                if (account == DefaultAccount) {
                    builder.Append (" (default)");
                }
                builder.AppendLine ();
            }
            return builder.ToString ();
        }

    }
}
