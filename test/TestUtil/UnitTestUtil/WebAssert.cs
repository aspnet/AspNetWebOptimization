// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Web.UnitTest {
    public sealed class WebAssert {
        public static void IsRenderingEquivalent(string expected, string actual) {
            IsRenderingEquivalent(expected, actual, null);
        }

        public static void IsRenderingEquivalent(string expected, string actual, string message) {
            IsRenderingEquivalent(expected, actual, message, null);
        }

        public static void IsRenderingEquivalent(string expected, string actual, string message, params object[] args) {
            int expectedChecksum = ComputeRenderingChecksum((string)expected);
            int actualChecksum = ComputeRenderingChecksum((string)actual);
            Assert.AreEqual(expectedChecksum, actualChecksum, message, args);
        }

        private static int ComputeRenderingChecksum(string text) {
            int xorChecksum = 0;
            int addChecksum = 0;

            for (int i = 0; i < text.Length; i++) {
                byte b = (byte)text[i];
                xorChecksum ^= b;
                addChecksum += b;
            }

            return text.Length ^ xorChecksum ^ addChecksum;
        }
    }
}
