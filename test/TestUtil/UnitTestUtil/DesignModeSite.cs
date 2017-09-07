// Copyright (c) Microsoft Corporation, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Web.UI.Test {
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;

    public sealed class DesignModeSite : ISite {
        private ServiceContainer _services;
        private string _name;
        private IComponent _component;

        public DesignModeSite() {
        }

        public DesignModeSite(ServiceContainer services, string name, IComponent component) {
            _services = services;
            _name = name;
            _component = component;
        }

        #region ISite Members
        IComponent ISite.Component {
            get {
                return _component;
            }
        }

        IContainer ISite.Container {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        bool ISite.DesignMode {
            get {
                return true;
            }
        }

        string ISite.Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }
        #endregion

        #region IServiceProvider Members
        object IServiceProvider.GetService(Type serviceType) {
            if (_services == null) {
                return null;
            }
            return _services.GetService(serviceType);
        }
        #endregion
    }
}
