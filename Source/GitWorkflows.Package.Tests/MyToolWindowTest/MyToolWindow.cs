/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GitWorkflows.Package.Tests.MyToolWindowTest
{
    /// <summary>
    ///This is a test class for MyToolWindowTest and is intended
    ///to contain all MyToolWindowTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MyToolWindowTest
    {

        /// <summary>
        ///PendingChangesWindow Constructor test
        ///</summary>
        [TestMethod()]
        public void MyToolWindowConstructorTest()
        {

            PendingChangesWindow target = new PendingChangesWindow();
            Assert.IsNotNull(target, "Failed to create an instance of PendingChangesWindow");

            MethodInfo method = target.GetType().GetMethod("get_Content", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(method.Invoke(target, null), "PendingChangesControl object was not instantiated");

        }

        /// <summary>
        ///Verify the Content property is valid.
        ///</summary>
        [TestMethod()]
        public void WindowPropertyTest()
        {
            PendingChangesWindow target = new PendingChangesWindow();
            Assert.IsNotNull(target.Content, "Content property was null");
        }

    }
}
