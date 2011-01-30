using System;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package
{
    /// <summary>
    /// This attribute registers the source control provider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ProvideSourceControlProviderAttribute : RegistrationAttribute
    {
        /// <summary>
        /// </summary>
        public ProvideSourceControlProviderAttribute(string regName, string uiName)
        {
            RegName = regName;
            UIName = uiName;
        }

        /// <summary>
        /// Get the friendly name of the provider (written in registry)
        /// </summary>
        public string RegName
        { get; private set; }

        /// <summary>
        /// Get the unique guid identifying the provider
        /// </summary>
        public Guid RegGuid
        {
            get { return Constants.guidSccProvider; }
        }

        /// <summary>
        /// Get the UI name of the provider (string resource ID)
        /// </summary>
        public string UIName
        { get; private set; }

        /// <summary>
        /// Get the package containing the UI name of the provider
        /// </summary>
        public Guid UINamePkg
        {
            get { return Constants.guidPackagePkg; }
        }

        /// <summary>
        /// Get the guid of the provider's service
        /// </summary>
        public Guid SccProviderService
        {
            get { return Constants.guidSccProviderService; }
        }

        /// <summary>
        ///     Called to register this attribute with the given context.  The context
        ///     contains the location where the registration inforomation should be placed.
        ///     It also contains other information such as the type being registered and path information.
        /// </summary>
        public override void Register(RegistrationContext context)
        {
            // Write to the context's log what we are about to do
            context.Log.WriteLine(String.Format(CultureInfo.CurrentCulture, "GitWorkflowsPackage:\t\t{0}\n", RegName));

            // Declare the source control provider, its name, the provider's service 
            // and aditionally the packages implementing this provider
            using (var sccProviders = context.CreateKey("SourceControlProviders"))
            using (var sccProviderKey = sccProviders.CreateSubkey(RegGuid.ToString("B")))
            {
                sccProviderKey.SetValue(string.Empty, RegName);
                sccProviderKey.SetValue("Service", SccProviderService.ToString("B"));

                using (var sccProviderNameKey = sccProviderKey.CreateSubkey("Name"))
                {
                    sccProviderNameKey.SetValue(string.Empty, UIName);
                    sccProviderNameKey.SetValue("Package", UINamePkg.ToString("B"));
                }

                // Additionally, you can create a "Packages" subkey where you can enumerate the dll
                // that are used by the source control provider, something like "Package1"="GitWorkflowsPackage.dll"
                // but this is not a requirement.
            }
        }

        /// <summary>
        /// Unregister the source control provider
        /// </summary>
        /// <param name="context"></param>
        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey("SourceControlProviders\\" + RegGuid.ToString("B"));
        }
    }
}
