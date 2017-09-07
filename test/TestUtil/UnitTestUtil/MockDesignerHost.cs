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
    internal class MockDesignerHost : IDesignerHost {
        #region IDesignerHost Members

        void IDesignerHost.Activate() {
            throw new Exception("The method or operation is not implemented.");
        }

        event EventHandler IDesignerHost.Activated {
            add { throw new Exception("The method or operation is not implemented."); }
            remove { throw new Exception("The method or operation is not implemented."); }
        }

        IContainer IDesignerHost.Container {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        IComponent IDesignerHost.CreateComponent(Type componentClass, string name) {
            throw new Exception("The method or operation is not implemented.");
        }

        IComponent IDesignerHost.CreateComponent(Type componentClass) {
            throw new Exception("The method or operation is not implemented.");
        }

        DesignerTransaction IDesignerHost.CreateTransaction(string description) {
            throw new Exception("The method or operation is not implemented.");
        }

        DesignerTransaction IDesignerHost.CreateTransaction() {
            throw new Exception("The method or operation is not implemented.");
        }

        event EventHandler IDesignerHost.Deactivated {
            add { throw new Exception("The method or operation is not implemented."); }
            remove { throw new Exception("The method or operation is not implemented."); }
        }

        void IDesignerHost.DestroyComponent(IComponent component) {
            throw new Exception("The method or operation is not implemented.");
        }

        IDesigner IDesignerHost.GetDesigner(IComponent component) {
            throw new Exception("The method or operation is not implemented.");
        }

        Type IDesignerHost.GetType(string typeName) {
            throw new Exception("The method or operation is not implemented.");
        }

        bool IDesignerHost.InTransaction {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        event EventHandler IDesignerHost.LoadComplete {
            add { throw new Exception("The method or operation is not implemented."); }
            remove { throw new Exception("The method or operation is not implemented."); }
        }

        bool IDesignerHost.Loading {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        IComponent IDesignerHost.RootComponent {
            get {
                return null;
            }
        }

        string IDesignerHost.RootComponentClassName {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        event DesignerTransactionCloseEventHandler IDesignerHost.TransactionClosed {
            add { throw new Exception("The method or operation is not implemented."); }
            remove { throw new Exception("The method or operation is not implemented."); }
        }

        event DesignerTransactionCloseEventHandler IDesignerHost.TransactionClosing {
            add { throw new Exception("The method or operation is not implemented."); }
            remove { throw new Exception("The method or operation is not implemented."); }
        }

        string IDesignerHost.TransactionDescription {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        event EventHandler IDesignerHost.TransactionOpened {
            add { throw new Exception("The method or operation is not implemented."); }
            remove { throw new Exception("The method or operation is not implemented."); }
        }

        event EventHandler IDesignerHost.TransactionOpening {
            add { throw new Exception("The method or operation is not implemented."); }
            remove { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region IServiceContainer Members

        void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback, bool promote) {
            throw new Exception("The method or operation is not implemented.");
        }

        void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback) {
            throw new Exception("The method or operation is not implemented.");
        }

        void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote) {
            throw new Exception("The method or operation is not implemented.");
        }

        void IServiceContainer.AddService(Type serviceType, object serviceInstance) {
            throw new Exception("The method or operation is not implemented.");
        }

        void IServiceContainer.RemoveService(Type serviceType, bool promote) {
            throw new Exception("The method or operation is not implemented.");
        }

        void IServiceContainer.RemoveService(Type serviceType) {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IServiceProvider Members

        object IServiceProvider.GetService(Type serviceType) {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
