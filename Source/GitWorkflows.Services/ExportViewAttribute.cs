using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace GitWorkflows.Services
{
    /// <summary>
    /// A MEF export attribute that can be used for views that correspond to a view model.
    /// </summary>
    /// 
    /// <remarks>
    ///     <para>The view class should use this attribute instead of the MEF's 
    ///     <see cref="ExportAttribute"/>, in order to provide custom metadata information to the
    ///     system.</para>
    /// </remarks>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportViewAttribute : ExportAttribute
    {
        /// <summary>
        /// Gets or sets the string to be displayed as window title if the view is used as window
        /// content.
        /// </summary>
        /// 
        /// <value>The window title.</value>
        public string WindowTitle
        { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportViewAttribute"/> class.
        /// </summary>
        /// 
        /// <param name="viewModelName">Name of the view model.</param>
        public ExportViewAttribute(string viewModelName)
            : base(viewModelName, typeof(Control))
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportViewAttribute"/> class.
        /// </summary>
        /// 
        /// <param name="viewModelType">Type of the view model.</param>
        public ExportViewAttribute(Type viewModelType)
            : this(viewModelType.Name)
        {}
    }
}