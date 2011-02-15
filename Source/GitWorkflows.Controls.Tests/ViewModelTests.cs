using System;
using System.Collections.Generic;
using System.Windows.Input;
using GitWorkflows.Controls.ViewModels;
using Microsoft.Practices.Prism.Commands;
using NUnit.Framework;

namespace GitWorkflows.Controls.Tests
{
    [TestFixture]
    public class ViewModelTests
    {
        class TestViewModel : ViewModel
        {
            private readonly Action<string> _onExecute;

            public TestViewModel(Action<string> onExecute)
            { _onExecute = onExecute; }

            [CommandExecute("x")]
            public void ExecuteX()
            { _onExecute("x"); }

            [CommandCanExecute("x")]
            public bool CanExecuteX()
            { return false; }
        }

        [Test]
        public void WhenTreatedDynamically_ProvidesAccessToCommands()
        {
            var executedCommands = new List<string>();
            dynamic vm = new TestViewModel(executedCommands.Add);

            Assert.That(vm.x, Is.InstanceOf<DelegateCommandBase>());
            Assert.That(() => ((ICommand)vm.x).Execute(null), Throws.Nothing);
            Assert.That(vm.x.CanExecute(null), Is.False);
            Assert.That(executedCommands, Is.EqualTo(new[]{"x"}));
        }
    }
}
