// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.TestUtil.Hosting {
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Hosting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    // base class used for unit tests which require hosting the ASP.NET runtime
    public abstract class HostedTestBase : IDisposable {

        private readonly string _appId;
        private readonly object _lockObj = new object();
        private ApplicationManager _manager;
        private TestHost _testHost;

        protected HostedTestBase() {
            _appId = GetType().AssemblyQualifiedName;
        }

        // magic property understood by the VSTS testing runtime which gives us information on the current test
        public TestContext TestContext {
            get;
            set;
        }

        private string PageDirectory {
            get {
                string outerPagesDir = Path.Combine(TestContext.TestDeploymentDir, "Pages");
                string innerPagesDir = Path.Combine(outerPagesDir, GetType().FullName);
                return innerPagesDir;
            }
        }

        private void EnsureTestHost() {
            if (_manager == null) {
                lock (_lockObj) {
                    if (_manager == null) {
                        _manager = ApplicationManager.GetApplicationManager();
                        
                        // copy assembly containing this class to the ~/bin folder
                        string binDir = Path.Combine(PageDirectory, "bin");
                        Directory.CreateDirectory(binDir);
                        string assemblyPath = typeof(TestHost).Assembly.Location;
                        string destPath = Path.Combine(binDir, Path.GetFileName(assemblyPath));
                        if (!File.Exists(destPath)) {
                            File.Copy(assemblyPath, destPath);
                        }

                        // bring up the AppDomain
                        _testHost = (TestHost)_manager.CreateObject(_appId, typeof(TestHost), "/app",
                            PageDirectory, true /* failIfExists */, true /* throwOnError */);
                    }
                }
            }
        }

        protected ProcessPageResult ProcessRequest() {
            // use the current test name as the path
            // Debugger.Launch();
            string path = TestContext.TestName + ".aspx";
            return ProcessRequest(path);
        }

        protected ProcessPageResult ProcessRequest(string path, string lastModified = null) {
            // Debugger.Launch();
            Assert.IsNotNull(TestContext, "TestContext was null.");
            EnsureTestHost();

            string output;
            int statusCode;
            NameValueCollection responseHeaders;
            Assert.IsNotNull(_testHost, "_testhost was null.");
            _testHost.ProcessRequest(path, lastModified, out output, out statusCode, out responseHeaders);

            string baselineDir = Path.Combine(PageDirectory, "Baselines");
            if (!Directory.Exists(baselineDir)) {
                Directory.CreateDirectory(baselineDir);
            }

            string baselineFile = Path.Combine(baselineDir, TestContext.TestName + ".bsl");
            string logFile = Path.Combine(baselineDir, TestContext.TestName + ".log");
            File.WriteAllText(logFile, output, Encoding.UTF8);

            return new ProcessPageResult() {
                Path = path,
                Output = output,
                LogFile = logFile,
                BaselineFile = baselineFile,
                ResponseCode = statusCode,
                ResponseHeaders = responseHeaders
            };
        }

        // due to a nit in the unit test framework, this method needs to be an implicit interface implementation
        // rather than an explicit interface implementation.
        public void Dispose() {
            // all of the tests have been run, so tear down the host
            ApplicationManager manager = _manager;
            if (manager != null) {
                manager.StopObject(_appId, typeof(TestHost));
            }
        }

        // The following code is copied from ddsuites\src\fx\WebForms\TestUtil\Internal\Test\IETestBase.cs
        // (TestUtil cannot be referenced because it isn't signed, and it cannot be signed because it references tools\x86\Microsoft.Web.IISHelper.dll which is not signed.)
        // Replaces use of __ViewState and WebResource tokens.
        public static string FilterVariableScript(string s) {
            string expr = @"<script src=.*?WebResource\.axd[^>]*></script>";
            s = Regex.Replace(s, expr, @"[variable script removed]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            expr = @"<script src=.*?ScriptResource\.axd[^>]*></script>";
            s = Regex.Replace(s, expr, @"[variable script removed]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            expr = @"<INPUT [^<]*type=hidden[^<]*name=__VIEWSTATE(\d)*>";
            s = Regex.Replace(s, expr, @"[state removed]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            expr = @"<INPUT [^<]*name=""__VIEWSTATE(\d)*""[^<]*type=""hidden""[^<]*>";
            s = Regex.Replace(s, expr, @"[state removed]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            expr = @"<INPUT [^<]*type=""hidden""[^<]*name=""__VIEWSTATE(\d)*""[^<]*>";
            s = Regex.Replace(s, expr, @"[state removed]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            expr = @"<INPUT [^<]*type=hidden[^<]*name=__VIEWSTATEFIELDCOUNT>";
            s = Regex.Replace(s, expr, @"[state removed]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            expr = @"<INPUT [^<]*name=""__VIEWSTATEFIELDCOUNT""[^<]*type=""hidden""[^<]*>";
            s = Regex.Replace(s, expr, @"[state removed]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            expr = @"<INPUT [^<]*type=hidden[^<]*name=__EVENTVALIDATION(\d)*>";
            s = Regex.Replace(s, expr, @"[eventvalidation removed]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            expr = @"<INPUT [^<]*name=""__EVENTVALIDATION(\d)*""[^<]*type=""hidden""[^<]*>";
            s = Regex.Replace(s, expr, @"[eventvalidation removed]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            expr = @"<INPUT [^<]*type=""hidden""[^<]*name=""__EVENTVALIDATION(\d)*""[^<]*>";
            s = Regex.Replace(s, expr, @"[eventvalidation removed]",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return s;
        }

        // Replaces variable tokens used by WebResource.axd
        public static string FixupWebResourceUrl(string html) {
            html = Regex.Replace(html, @"WebResource\.axd\?d=[^&]*(.*?)""", @"WebResource.axd?d=[AssemblyResourceData]$1""");
            html = Regex.Replace(html, @"WebResource\.axd\?(.*?)t=\d*(.*?)""", @"WebResource.axd?$1t=[TimeStamp]$2""");
            return html;
        }

        public static string FixupVariableTokens(string html) {
            return FixupWebResourceUrl(FilterVariableScript(html));
        }

    }
}
