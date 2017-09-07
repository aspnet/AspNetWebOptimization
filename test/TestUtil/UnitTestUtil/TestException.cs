// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Web.UnitTest {
    // Derived exception class that can be used by the test framework to throw an exception
    // type that does not exist in the framework.
    public class TestException : Exception {
    }
}
