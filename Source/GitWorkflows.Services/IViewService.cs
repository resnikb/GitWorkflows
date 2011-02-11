using System;
using System.Windows.Controls;

namespace GitWorkflows.Services
{
    /// <summary>
    /// Represents a service for displaying dialogs.
    /// </summary>
    public interface IViewService
    {
        /// <summary>
        /// Shows dialog with the given view model as the data context.
        /// </summary>
        /// 
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// 
        /// <param name="viewModel">The view model.</param>
        /// 
        /// <param name="onSuccess">Action to execute if the dialog is closed regularly (not
        /// canceled).</param>
        /// 
        /// <param name="onCancel">Action to execute if the dialog is canceled.</param>
        /// 
        /// <param name="afterClose">Action to execute always after the dialog has closed. The
        /// second parameter can be used to determine if the dialog was closed successfully or
        /// canceled. If the parameter is <c>true</c>, the dialog was closed successfully, and if it
        /// is <c>false</c>, the dialog was canceled. The <paramref name="afterClose"/> action is
        /// executed after <paramref name="onSuccess"/> or <paramref name="onCancel"/>.</param>
        /// 
        /// <remarks>
        ///     <para>This method can queue execution of the dialog on a different thread than the
        ///     one invoking it. For this reason, clients can pass in callback delegates to be
        ///     executed when the dialog is closed. Clients should be aware that any code after the
        ///     call to this method may execute before the dialog has been shown.</para>
        /// 
        ///     <para>This method does not have to display a modal dialog. It is up to the service
        ///     to determine the best way to display the dialog, taking into account the style of
        ///     the application and possibly other environment considerations.</para>
        /// </remarks>
        void ShowDialog<TViewModel>(
            TViewModel viewModel, 
            Action<TViewModel> onSuccess        = null, 
            Action<TViewModel> onCancel         = null,
            Action<TViewModel, bool> afterClose = null
        );

        /// <summary>
        /// Creates view for the given view model.
        /// </summary>
        /// 
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// 
        /// <param name="viewModel">The view model.</param>
        /// 
        /// <returns>Control bound to the given view model.</returns>
        Control CreateView<TViewModel>(TViewModel viewModel);
    }
}