// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Security;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security.Permissions;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Web.UnitTest {
    public class MockSite : ISite {
        private bool _designMode;

        public MockSite(bool designMode) {
            _designMode = designMode;
        }

        #region ISite Members

        IComponent ISite.Component {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        IContainer ISite.Container {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        bool ISite.DesignMode {
            get { return _designMode; }
        }

        string ISite.Name {
            get {
                return "MockSite";
            }
            set {
                // noop
            }
        }

        #endregion

        #region IServiceProvider Members

        object IServiceProvider.GetService(Type serviceType) {
            if (serviceType == typeof(IDesignerHost)) {
                return new MockDesignerHost();
            }
            else {
                return null;
            }
        }

        #endregion
    }
}
