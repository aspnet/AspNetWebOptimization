// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.TestUtil.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Optimization.Test {
    [TestClass]
    public class DebugTests : HostedTestBase {

        [TestMethod]
        public void Debug_ResolveBundleUrl_DefaultBundlesTest() {
            string[] bundleUrls = { "/app/js", "/app/css", "/app/nested/js", "/app/static1" };
            var result = ProcessRequest("list.aspx");
            foreach (string url in bundleUrls) {
                Assert.IsTrue(result.Output.Contains(url), "Did not find in bundle urls: " + url);
            }
        }

        [TestMethod]
        public void Debug_StylesUrl_DefaultBundlesTest() {
            string[] bundleUrls = { "/app/js?v=", "/app/css?v=", "/app/nested/js?v=", "/app/static1?v=" };
            var result = ProcessRequest("listStyles.aspx");
            foreach (string url in bundleUrls) {
                Assert.IsTrue(result.Output.Contains(url), "Did not find in bundle urls: " + url);
            }
        }

        [TestMethod]
        public void Debug_ScriptsUrl_DefaultBundlesTest() {
            string[] bundleUrls = { "/app/js?v=", "/app/css?v=", "/app/nested/js?v=", "/app/static1?v=" };
            var result = ProcessRequest("listScripts.aspx");
            foreach (string url in bundleUrls) {
                Assert.IsTrue(result.Output.Contains(url), "Did not find in bundle urls: " + url);
            }
        }

        [TestMethod]
        public void Debug_DefaultBundles_BasicTest() {
            var result = ProcessRequest("nested\\js");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("nested", result.Output);

            result = ProcessRequest("js");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("First;Middle;Last", result.Output);

            result = ProcessRequest("css");
            VerifyHeaders(result, CssMinify.CssContentType);
            Assert.AreEqual("Css1{color:blue}Css2{color:blue}Css3{color:blue}Css4{color:blue}", result.Output);

            result = ProcessRequest("nested\\css");
            VerifyHeaders(result, CssMinify.CssContentType);
            Assert.AreEqual("nested{color:blue}", result.Output);
        }

        [TestMethod]
        public void Debug_NonExistantDynamicBundleFolderReturns500Test() {
            var result = ProcessRequest("noway\\js");
            Assert.AreEqual(500, result.ResponseCode);
        }

        [TestMethod]
        public void Debug_NestedNonExistantDynamicBundleFolderReturns500Test() {
            var result = ProcessRequest("nested\\noway\\js");
            Assert.AreEqual(500, result.ResponseCode);
        }

        [TestMethod]
        public void Debug_BasicDynamicBundleTest() {
            var result = ProcessRequest("hao");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("hao", result.Output);
        }

        [TestMethod]
        public void Debug_IncludeSubdirectoriesWorksForDynamicBundlesTest() {
            var result = ProcessRequest("nested\\jsrec");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("nested;nested2", result.Output);
        }

        [TestMethod]
        public void Debug_DynamicBundle_NoTransform_AppendsLinesTest() {
            var result = ProcessRequest("none");
            VerifyHeaders(result, null);
            Assert.AreEqual("aaa" + Environment.NewLine + "bbb" + Environment.NewLine, result.Output);
        }

        [TestMethod]
        public void Debug_BasicStaticBundleTest() {
            var result = ProcessRequest("static1");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("b.a;a.a;a.b;b.b;c.b;c", result.Output);
        }

        [TestMethod]
        public void Debug_StaticBundleWithCustomOrdererTest() {
            var result = ProcessRequest("static2");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("a.b;b.b;c.b;a.a;b.a", result.Output);
        }

        [TestMethod]
        public void Debug_VerifyNestedDuplicateFileNamesAreIncludedTest() {
            // Act
            var result = ProcessRequest("txt");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("aaa;bbb;aaa;bbb;haomin;hao", result.Output);
        }

        [TestMethod]
        public void Debug_VerifyNestedFilesNotIncludedByDefaultTest() {
            // Act
            var result = ProcessRequest("txt2");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("aaa;bbb", result.Output);
        }

        [TestMethod]
        public void Debug_MinificationErrorTest() {
            var result = ProcessRequest("err");
            VerifyHeaders(result, CssMinify.CssContentType);
            // Make sure it starts with the error header
            Assert.IsTrue(result.Output.StartsWith("/* Minification failed. Returning unminified contents."), result.Output);
            // Verify we get Ajax min errors
            Assert.IsTrue(result.Output.Contains("run-time error CSS"), result.Output);
            // Verify we get the unbundled file
            Assert.IsTrue(result.Output.Contains("p {width: }"));
        }

        [TestMethod]
        public void Debug_DefaultOrderer_NoDupesFirstOneWinsTest() {
            var result = ProcessRequest("dup");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("a.a;c;rootc", result.Output);
        }

        [TestMethod]
        public void Debug_RequestToExistingFileNotBundledTest() {
            var result = ProcessRequest("Exists.foo");
            Assert.AreEqual("I exist", result.Output);
        }

        private string ExtractVersionCode(string bundleUrl) {
            return bundleUrl.Substring(bundleUrl.IndexOf("v="));
        }

        [TestMethod]
        public void Debug_SameBundleContentsHaveSameVersionCodeTest() {
            var result = ProcessRequest("versionCode.aspx?url=~/Include2/sub/jsrec");
            Assert.IsTrue(result.Output.Contains("v="), result.Output);
            string versionCodeA = ExtractVersionCode(result.Output);

            result = ProcessRequest("versionCode.aspx?url=~/Include2/jsrec");
            string versionCodeB = ExtractVersionCode(result.Output);
            Assert.IsTrue(result.Output.Contains("v="), result.Output);
            Assert.AreEqual(versionCodeA, versionCodeB, "Same bundles should have the same version code");
        }

        [TestMethod]
        public void Debug_DiffBundleContentsHaveDifferentVersionCodeTest() {
            var result = ProcessRequest("versionCode.aspx?url=~/Include2/sub/js");
            Assert.IsTrue(result.Output.Contains("v="), result.Output);
            string versionCodeA = ExtractVersionCode(result.Output);

            result = ProcessRequest("versionCode.aspx?url=~/Include2/js");
            string versionCodeB = ExtractVersionCode(result.Output);
            Assert.IsTrue(result.Output.Contains("v="), result.Output);
            Assert.AreNotEqual(versionCodeA, versionCodeB, "Diff bundles should have diff version codes");
        }

        [TestMethod]
        public void Debug_JqueryOrderTest() {
            var result = ProcessRequest("jquery\\js");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual(@"jquery_js;jquery1_0_js;jquery1_1_1_js;jquery1_1_js;jquery10_0_js;jquery2_1_js;jquery-ui1_0_js;jquery-ui1_1_js;jquery-ui2_0_js;jquery-ui200_0_js;jquery_ui1_0_js;jquery_ui1_11_js;jquery_ui11_0_js;jquery_unobtrusive_ajax;jquery_validateA_js;jquery_validateB_js;a", result.Output);
        }

        [TestMethod]
        public void Debug_MvcBundlesJsTest() {
            var result = ProcessRequest("bundles\\js");
            VerifyHeaders(result, JsMinify.JsContentType);
            result.AssertLogMatchesBaseline();
        }

        [TestMethod]
        public void Debug_MvcBundlesMobileJsTest() {
            var result = ProcessRequest("bundles\\mobileJs");
            VerifyHeaders(result, JsMinify.JsContentType);
            result.AssertLogMatchesBaseline();
        }

        [TestMethod]
        public void Debug_MvcContentMobileCssTest() {
            var result = ProcessRequest("content\\mobileCss");
            VerifyHeaders(result, CssMinify.CssContentType);
            result.AssertLogMatchesBaseline();
        }

        [TestMethod]
        public void Debug_MvcContentCssTest() {
            var result = ProcessRequest("content\\css");
            VerifyHeaders(result, CssMinify.CssContentType);
            result.AssertLogMatchesBaseline();
        }

        [TestMethod]
        public void Debug_MvcContentThemesBaseCssTest() {
            var result = ProcessRequest("Content\\themes\\base\\css");
            VerifyHeaders(result, CssMinify.CssContentType);
            result.AssertLogMatchesBaseline();
        }
        

        [TestMethod]
        public void Debug_DojoOrderTest() {
            var result = ProcessRequest("dojo\\js");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual(@"dojoOne;dojoTwo", result.Output);
        }

        [TestMethod]
        public void Debug_PrototypeOrderTest() {
            var result = ProcessRequest("prototype\\js");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual(@"prototype;prototypev100;scriptaculous-v11", result.Output);
        }

        [TestMethod]
        public void Debug_MooOrderTest() {
            var result = ProcessRequest("moo\\js");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual(@"mooCoreOne;mooCoreOnePtOne;mooOne;mooThree", result.Output);
        }

        [TestMethod]
        public void Debug_ExtOrderTest() {
            var result = ProcessRequest("ext\\js");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual(@"ext;extv1000", result.Output);
        }

        [TestMethod]
        public void Debug_ModernizrOrderTest() {
            var result = ProcessRequest("mod\\js");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual(@"modernizr1;modernizr11", result.Output);
        }

        [TestMethod]
        public void Debug_ModernizrInstrumentedTest() {
            var result = ProcessRequest("mod\\jsdebug");
            VerifyHeaders(result, JsMinify.JsContentType, shouldCache: false);
            Assert.AreEqual("/* Bundle=System.Web.Optimization.Bundle;Boundary=MQA2ADkAMgA2ADIANgAwADYANwA=; */\r\n/* MQA2ADkAMgA2ADIANgAwADYANwA= \"~/mod/modernizr-1.0.js\" */\r\nmodernizr1\r\n/* MQA2ADkAMgA2ADIANgAwADYANwA= \"~/mod/modernizr-1.1.js\" */\r\nmodernizr11\r\n", result.Output);
        }

        [TestMethod]
        public void Debug_CssOrderTest() {
            var result = ProcessRequest("cssd\\css");
            VerifyHeaders(result, CssMinify.CssContentType);
            Assert.AreEqual(@"reset{color:blue}normalize{color:blue}", result.Output);
        }

        [TestMethod]
        public void Debug_CssInstrumentedTest() {
            var result = ProcessRequest("cssd\\cssdebug");
            VerifyHeaders(result, CssMinify.CssContentType, shouldCache: false);
            Assert.AreEqual("/* Bundle=System.Web.Optimization.Bundle;Boundary=MQA1ADkAMAAxADAANgAxADYA; */\r\n/* MQA1ADkAMAAxADAANgAxADYA \"~/cssd/reset.css\" */\r\nreset {\r\ncolor: blue\r\n}\r\n/* MQA1ADkAMAAxADAANgAxADYA \"~/cssd/normalize.css\" */\r\nnormalize {\r\ncolor: blue\r\n}\r\n", result.Output);
        }

        [TestMethod]
        public void Debug_StaticInstrumentedTest() {
            var result = ProcessRequest("staticdebug");
            VerifyHeaders(result, "text/html", shouldCache: false);
            Assert.AreEqual("/* Bundle=System.Web.Optimization.Bundle;Boundary=MQAxADYAOQA0ADUAMAAwADYANAA=; */\r\n/* MQAxADYAOQA0ADUAMAAwADYANAA= \"~/a.txt\" */\r\naaa\r\n/* MQAxADYAOQA0ADUAMAAwADYANAA= \"~/b.txt\" */\r\nbbb\r\n", result.Output);
        }

        [TestMethod]
        public void Debug_AllBuiltInFileOrdersTest() {
            var result = ProcessRequest("uber");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual(@"jquery_js;jquery1_0_js;jquery1_1_1_js;jquery1_1_js;jquery10_0_js;jquery2_1_js;jquery-ui1_0_js;jquery-ui1_1_js;jquery-ui2_0_js;jquery-ui200_0_js;jquery_ui1_0_js;jquery_ui1_11_js;jquery_ui11_0_js;jquery_unobtrusive_ajax;jquery_validateA_js;jquery_validateB_js;modernizr1;modernizr11;dojoOne;dojoTwo;mooCoreOne;mooCoreOnePtOne;mooOne;mooThree;prototype;prototypev100;scriptaculous-v11;ext;extv1000;a", result.Output);
        }

        [TestMethod]
        public void Debug_ReplacementWithMinAndOptTest() {
            var result = ProcessRequest("replacementTestWithOptAndMin");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("fake\r\nfooopt\r\nhaomin\r\nhao\r\n", result.Output);
        }

        [TestMethod]
        public void Debug_ReplacementOffTest() {
            var result = ProcessRequest("replacementOffTest");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("fake\r\nfoo\r\nfooopt\r\nhaomin\r\nhao\r\n", result.Output);
        }

        [TestMethod]
        public void Debug_ResetCssFirstByDefaultTest() {
            var result = ProcessRequest("resetTest");
            VerifyHeaders(result, CssMinify.CssContentType);
            Assert.AreEqual("reset{color:blue}normalize{color:blue}jquery-foo{color:blue}", result.Output);
        }

        [TestMethod]
        public void Debug_RequestToEmptyScriptManagerDoesNotThrow() {
            var result = ProcessRequest("lonelysm.aspx");
            Assert.AreEqual(200, result.ResponseCode);
        }

        [TestMethod]
        public void Debug_ConcatWithScriptBundleTest() {
            var result = ProcessRequest("concatJs");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("(function(){})(),function(){}(\"z\");\"i show up\"", result.Output);
        }

        [TestMethod]
        public void Debug_ConcatSemiColonWithNoTransformTest() {
            var result = ProcessRequest("concatJsNoMinify");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual("(function() {})();(function() {})(\"z\");\"i get minified away :(\";\"i show up\";", result.Output);
        }

        [TestMethod]
        public void Debug_ConcatWithHashTest() {
            var result = ProcessRequest("concatHashToken");
            Assert.AreEqual("a#b#", result.Output);
        }

        [TestMethod]
        public void Debug_ConcatWithJsMinifyUsesSemicolonTest() {
            var result = ProcessRequest("concatJsMinify");
            Assert.AreEqual("(function(){})(),function(){}(\"z\");\"i show up\"", result.Output);
        }

        [TestMethod]
        public void Debug_ConcatWithNoTokenTest() {
            var result = ProcessRequest("concatNoToken");
            Assert.AreEqual("a\r\nb\r\n", result.Output);
        }

        [TestMethod]
        public void Debug_StylesTest() {
            var result = ProcessRequest("StylesTest.aspx");
            result.AssertLogMatchesBaseline();
        }

        [TestMethod]
        public void Debug_ScriptsTest() {
            var result = ProcessRequest("ScriptsTest.aspx");
            result.AssertLogMatchesBaseline();
        }

        [TestMethod]
        public void Debug_JqueryVersionedOnlyTest() {
            var result = ProcessRequest("jqueryv");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual(@"jquery1_0_js;jquery1_1_1_js;jquery1_1_js;jquery10_0_js;jquery2_1_js", result.Output);
        }

        [TestMethod]
        public void Debug_JqueryUIVersionedOnlyTest() {
            var result = ProcessRequest("jqueryuiv");
            VerifyHeaders(result, JsMinify.JsContentType);
            Assert.AreEqual(@"jquery-ui1_0_js;jquery-ui1_1_js;jquery-ui2_0_js;jquery-ui200_0_js", result.Output);
        }

        [TestMethod]
        public void Debug_BundleReferenceRedCssTest() {
            var result = ProcessRequest("BundleReferenceRed.aspx");
            result.AssertLogMatchesBaselineAfterFilters();
        }

        private void VerifyHeaders(ProcessPageResult result, string contentType, bool shouldCache = true) {
            Assert.AreEqual(200, result.ResponseCode);
            if (contentType != null) {
                Assert.AreEqual(contentType + "; charset=utf-8", result.ResponseHeaders["Content-Type"]);
            }
            if (shouldCache) {
                Assert.AreEqual("public", result.ResponseHeaders["Cache-Control"]);
                string expires = result.ResponseHeaders["Expires"];
                Assert.IsFalse(string.IsNullOrEmpty(expires));
                Assert.IsTrue(DateTime.UtcNow.AddYears(1) > DateTime.Parse(expires));
                string varyby = result.ResponseHeaders["Vary"];
                Assert.AreEqual("User-Agent", varyby);
                string lastModified = result.ResponseHeaders["Last-Modified"];
                Assert.IsTrue(DateTime.Now > DateTime.Parse(lastModified));
                Assert.IsTrue(DateTime.Now.AddHours(-1.0) < DateTime.Parse(lastModified));
            }
        }
    }
}
