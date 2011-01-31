// Guids.cs
// MUST match guids.h
using System;

namespace GitWorkflows.Package
{
    static class Constants
    {
        // The guid of the source control provider package (implementing IVsPackage interface)
        public const string guidPackagePkgString = "72192AEA-3808-4CAF-B4E4-8D91038A6F92";
        
        // Unique ID of the source control provider; this is also used as the command UI context to show/hide the pacakge UI
        public const string guidSccProviderString = "0B8BF143-8FA6-4E07-8D16-73344CF5C51D";

        // The guid of the source control provider service (implementing IVsSccProvider interface)
        public const string guidSccProviderServiceString = "0AD903E3-250E-4C6C-8D79-ABBB6198FFA8";

        public const string guidPackageCmdSetString = "10AD7DBC-5B95-4B38-87F0-4C12590AE6A6";
        public const string guidToolWindowPersistanceString = "459F8AD1-6802-4D74-BD43-86FE92ED898B";

        public static readonly Guid guidPackageCmdSet = new Guid(guidPackageCmdSetString);
        public static readonly Guid guidPackagePkg = new Guid(guidPackagePkgString);
        public static readonly Guid guidSccProvider = new Guid(guidSccProviderString);
        public static readonly Guid guidSccProviderService = new Guid(guidSccProviderServiceString);

        public const int cmdidCommit =        0x100;
        public const int cmdidPendingChanges =    0x101;
        public const int idBranchCombo = 0x401;
        public const int idBranchComboGetBranches =    0x402;
        public const int cmdidNewBranch =    0x403;
    };
}