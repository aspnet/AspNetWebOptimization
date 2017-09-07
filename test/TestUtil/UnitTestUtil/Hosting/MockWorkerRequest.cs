// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.TestUtil.Hosting {
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Web.Hosting;

    internal sealed class MockWorkerRequest : SimpleWorkerRequest {

        public NameValueCollection ResponseHeaders {
            get;
            private set;
        }

        public string LastModifiedSince {
            get;
            set;
        }

        public int StatusCode {
            get;
            private set;
        }

        private MockWorkerRequest(string page, string queryString, TextWriter textWriter)
            : base(page, queryString, textWriter) {

            ResponseHeaders = new NameValueCollection();
        }

        public static MockWorkerRequest Create(string path, TextWriter textWriter) {
            // extract query string from path
            string queryString = String.Empty;
            int idxQuery = path.IndexOf('?');
            if (idxQuery >= 0) {
                queryString = path.Substring(idxQuery + 1);
                path = path.Substring(0, idxQuery);
            }

            return new MockWorkerRequest(path, queryString, textWriter);
        }

        public override void SendKnownResponseHeader(int index, string value) {
            string name = GetKnownResponseHeaderName(index);
            ResponseHeaders[name] = value;
        }

        public override void SendResponseFromFile(string filename, long offset, long length) {
            byte[] fileBytes = File.ReadAllBytes(filename);
            byte[] subset = new byte[(int)length];
            Buffer.BlockCopy(fileBytes, (int)offset, subset, 0, (int)length);
            SendResponseFromMemory(subset, (int)length);
        }

        public override void SendStatus(int statusCode, string statusDescription) {
            StatusCode = statusCode;
        }

        public override void SendUnknownResponseHeader(string name, string value) {
            ResponseHeaders[name] = value;
        }

        public override string GetKnownRequestHeader(int index) {
            switch (index) {
                case HeaderUserAgent:
                    return "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 2.0.50727)";
                case HeaderIfModifiedSince:
                    if (LastModifiedSince != null) {
                        return LastModifiedSince;
                    }
                    return base.GetKnownRequestHeader(index);
                default:
                    return base.GetKnownRequestHeader(index);
            }
        }

    }
}
