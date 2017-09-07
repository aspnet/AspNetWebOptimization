// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]

namespace System.Web.TestUtil.Hosting {
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Web;
    using System.Web.Hosting;

    // Calls to members on this object are cross-domain, so they are relatively expensive.
    internal class TestHost : MarshalByRefObject, IRegisteredObject {

        public void ProcessRequest(string path, string lastModified, out string response, out int statusCode, out NameValueCollection responseHeaders) {
            StringWriter writer = new StringWriter();
            MockWorkerRequest request = MockWorkerRequest.Create(path, writer);
            request.LastModifiedSince = lastModified;
            HttpRuntime.ProcessRequest(request);

            response = writer.ToString();
            statusCode = request.StatusCode;
            responseHeaders = request.ResponseHeaders;
        }

        public override object InitializeLifetimeService() {
            // Never expire lease.  Needed to prevent appdomain from shutting down before
            // the tests are finished.
            return null;
        }

        #region IRegisteredObject Members
        void IRegisteredObject.Stop(bool immediate) {
            // see http://msdn.microsoft.com/en-us/library/system.web.hosting.iregisteredobject.stop.aspx

            // Removed call to UnregisterObject(), as it fails in partial trust after the recent changes to our
            // partial trust config files, and it seems unnecessary (at least for hosting unit tests).
            // HostingEnvironment.UnregisterObject(this);
        }
        #endregion

    }
}
