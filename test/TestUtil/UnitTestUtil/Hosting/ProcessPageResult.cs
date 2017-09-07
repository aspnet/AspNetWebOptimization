// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.TestUtil.Hosting {
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class ProcessPageResult {

        public string Path {
            get;
            internal set;
        }

        public string Output {
            get;
            internal set;
        }

        public string LogFile {
            get;
            internal set;
        }

        public string BaselineFile {
            get;
            internal set;
        }

        public int ResponseCode {
            get;
            internal set;
        }

        public NameValueCollection ResponseHeaders {
            get;
            internal set;
        }

        public void AssertLogMatchesBaseline() {
            AssertLogMatchesBaseline(Output);
        }

        public void AssertLogMatchesBaselineAfterFilters() {
            AssertLogMatchesBaseline(HostedTestBase.FixupVariableTokens(Output));
        }

        private void AssertLogMatchesBaseline(string output) {
            string baseLine = File.ReadAllText(BaselineFile, Encoding.UTF8);
            if (!String.Equals(output, baseLine)) {
                string message = String.Format("The output file '{0}' did not match the baseline file '{1}'.", LogFile, BaselineFile);
                Assert.Fail(message);
            }
        }
    }
}
