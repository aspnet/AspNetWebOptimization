// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.TestUtil {
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Web.UI;
    using Moq;

    public static class UnitTestHelper {
        public static bool EnglishBuildAndOS {
            get {
                bool englishBuild = String.Equals(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "en",
                    StringComparison.OrdinalIgnoreCase);
                bool englishOS = String.Equals(CultureInfo.CurrentCulture.TwoLetterISOLanguageName, "en",
                    StringComparison.OrdinalIgnoreCase);
                return englishBuild && englishOS;
            }
        }
    }
}
