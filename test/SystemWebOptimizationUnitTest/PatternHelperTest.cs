// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.RegularExpressions;
using System.Web.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Web.Optimization.Test {

    [TestClass]
    public class PatternHelperTest {
        internal static string InvalidPatternError = "Invalid pattern: '{0}'. Wildcards are only allowed in the last path segment, can contain only one leading or trailing wildcard, and cannot be used with {{version}}.\r\nParameter name: {1}";

        [TestMethod]
        public void WildcardRegexTest() {
            Validate("{version}", PatternType.Version);
        }

        [TestMethod]
        public void NoWildcardsAndPatterns() {
            string input = "*{version}";
            string arg = "arg";
            Exception error = PatternHelper.ValidatePattern(PatternType.Version, input, arg);
            Assert.IsNotNull(error);
            Assert.AreEqual(String.Format(PatternHelperTest.InvalidPatternError, "*{version}", arg), error.Message);
        }

        [TestMethod]
        public void VersionBuildRegexTest() {
            string input = "{version}";
            Validate(input, PatternType.Version);
            Regex ex = PatternHelper.BuildRegex(input);
            Assert.AreEqual(@"^(\d+(\s*\.\s*\d+){1,3})(-[a-z][0-9a-z-]*)?$", ex.ToString());
            Assert.IsTrue(ex.IsMatch("1.3.5.0"));
            Assert.IsTrue(ex.IsMatch("10.0"));
            Assert.IsTrue(ex.IsMatch("1.0"));
            Assert.IsTrue(ex.IsMatch("1.0.2"));
            Assert.IsFalse(ex.IsMatch("1000.10.102.1.10"));
            Assert.IsFalse(ex.IsMatch("1.0.2abc"));
            Assert.IsFalse(ex.IsMatch("1a0"));
            Assert.IsFalse(ex.IsMatch("1"));
            Assert.IsFalse(ex.IsMatch("10"));
        }

        [TestMethod]
        public void PreleaseVersionTest() {
            string input = "{version}";
            Validate(input, PatternType.Version);
            Regex ex = PatternHelper.BuildRegex(input);
            Assert.AreEqual(@"^(\d+(\s*\.\s*\d+){1,3})(-[a-z][0-9a-z-]*)?$", ex.ToString());
            Assert.IsTrue(ex.IsMatch("1.0-alpha1"));
            Assert.IsTrue(ex.IsMatch("1.0.0-alpha1"));
            Assert.IsTrue(ex.IsMatch("1.0.0.0-alpha1"));
            Assert.IsTrue(ex.IsMatch("1.0.0.0-foobaz"));
            Assert.IsTrue(ex.IsMatch("1.2.3-Beta"));
            Assert.IsFalse(ex.IsMatch("1000.10.102.1.10-alpha1"));
            Assert.IsFalse(ex.IsMatch("1000.10.102.1.10-1alpha"));
            Assert.IsFalse(ex.IsMatch("1-alpha1"));
            Assert.IsFalse(ex.IsMatch("1-"));
            Assert.IsFalse(ex.IsMatch("This.no.good"));
            Assert.IsFalse(ex.IsMatch("1.2.3.Beta"));
        }


        [TestMethod]
        public void JqueryVersionRegexTest() {
            string input = "jquery-{version}.js";
            Validate(input, PatternType.Version);
            Regex ex = PatternHelper.BuildRegex(input);
            Assert.AreEqual(@"^jquery-(\d+(\s*\.\s*\d+){1,3})(-[a-z][0-9a-z-]*)?\.js$", ex.ToString());
            Assert.IsTrue(ex.IsMatch("jquery-1.3.5.0.js"));
            Assert.IsTrue(ex.IsMatch("jquery-1.6.2.js"));
            Assert.IsFalse(ex.IsMatch("jquery-1000.10.102.1.10.js"));
            Assert.IsFalse(ex.IsMatch("jquery-ui1.8.11.js"));
            Assert.IsFalse(ex.IsMatch("jquery-1.0.2"));
            Assert.IsFalse(ex.IsMatch("notjquery-1.0.2.js"));
            Assert.IsFalse(ex.IsMatch("jquery-1.0.2Xjs"));
            Assert.IsFalse(ex.IsMatch("jquery-.1.0.2.js"));
        }

        [TestMethod]
        public void EmberPrerelSemVersion2NotSupportedTest() {
            string input = "ember-{version}.js";
            Validate(input, PatternType.Version);
            Regex ex = PatternHelper.BuildRegex(input);
            Assert.AreEqual(@"^ember-(\d+(\s*\.\s*\d+){1,3})(-[a-z][0-9a-z-]*)?\.js$", ex.ToString());
            Assert.IsTrue(ex.IsMatch("ember-1.0.0-rc1.js"));
            Assert.IsFalse(ex.IsMatch("ember-1.0.0-rc.1.js"));
        }

        [TestMethod]
        public void JqueryUiVersionRegexTest() {
            string input = "jquery-ui-{version}.js";
            Validate(input, PatternType.Version);
            Regex ex = PatternHelper.BuildRegex(input);
            Assert.AreEqual(@"^jquery-ui-(\d+(\s*\.\s*\d+){1,3})(-[a-z][0-9a-z-]*)?\.js$", ex.ToString());
            Assert.IsTrue(ex.IsMatch("jquery-ui-1.3.5.0.js"));
            Assert.IsTrue(ex.IsMatch("jquery-ui-1.6.2.js"));
            Assert.IsFalse(ex.IsMatch("jquery-ui-1000.10.102.1.10.js"));
            Assert.IsFalse(ex.IsMatch("jquery-ui1.8.11.js"));
            Assert.IsFalse(ex.IsMatch("jquery-1.0.2"));
            Assert.IsFalse(ex.IsMatch("notjquery-1.0.2.js"));
            Assert.IsFalse(ex.IsMatch("jquery-ui-1.0.2Xjs"));
            Assert.IsFalse(ex.IsMatch("jquery-ui-.1.0.2.js"));
        }

        [TestMethod]
        public void VersionRegexCaseInsenstiveTest() {
            string input = "{version}.I_AM_sometimes_CAPS";
            Validate(input, PatternType.Version);
            Regex ex = PatternHelper.BuildRegex(input);
            Assert.AreEqual(@"^(\d+(\s*\.\s*\d+){1,3})(-[a-z][0-9a-z-]*)?\.I_AM_sometimes_CAPS$", ex.ToString());
            Assert.IsTrue(ex.IsMatch("1.0.2.i_am_sometimes_CAPS"));
            Assert.IsTrue(ex.IsMatch("1.0.i_am_SOMETIMES_CAPS"));
            Assert.IsTrue(ex.IsMatch("10.100.I_AM_SOMETIMES_CAPS"));
            Assert.IsFalse(ex.IsMatch("10.100XI_AM_SOMETIMES_CAPS"));
            Assert.IsFalse(ex.IsMatch("10X100.I_AM_SOMETIMES_CAPS"));
            Assert.IsFalse(ex.IsMatch(".10.100.I_AM_SOMETIMES_CAPS"));
        }

        [TestMethod]
        public void UnknownCurlyGroupsAreIgnoredExTest() {
            string input = "yes-{version}-{unknown}.sweet";
            string arg = "arg";
            Assert.AreEqual(PatternType.Version, PatternHelper.GetPatternType(input));
            Exception error = PatternHelper.ValidatePattern(PatternType.Version, input, arg);
            Assert.IsNull(error);
        }

        [TestMethod]
        public void SoManyVersionsTest() {
            string input = "{version}--{version}--{version}";
            Validate(input, PatternType.Version);
            Regex ex = PatternHelper.BuildRegex(input);
            Assert.AreEqual(@"^" + PatternHelper.VersionRegEx + "--" + PatternHelper.VersionRegEx + "--" + PatternHelper.VersionRegEx + "$", ex.ToString());
            Assert.IsTrue(ex.IsMatch("1.0.2--3.5--1.0"));
            Assert.IsTrue(ex.IsMatch("1.0--2.0--3.0"));
            Assert.IsTrue(ex.IsMatch("1000.100--2.0--3000.100"));
            Assert.IsTrue(ex.IsMatch("1.0.2-alpha--3.5-beta--1.0"));
            Assert.IsFalse(ex.IsMatch("1.0-2.0--3.0"));
            Assert.IsFalse(ex.IsMatch("1.0--2.0-3.0"));
        }

        private void Validate(string pattern, PatternType expectedType) {
            Assert.AreEqual(expectedType, PatternHelper.GetPatternType(pattern));
            Assert.IsNull(PatternHelper.ValidatePattern(expectedType, pattern, "ignored"));
        }

    }
}
