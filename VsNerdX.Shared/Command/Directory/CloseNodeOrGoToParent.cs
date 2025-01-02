using System.Windows.Forms;
using VsNerdX.Core;

namespace VsNerdX.Command.Directory
{
    public class CloseNodeOrGoToParent : ICommand
    {
        private readonly IHierarchyControl _hierarchyControl;

        public CloseNodeOrGoToParent(IHierarchyControl hierarchyControl)
        {
            this._hierarchyControl = hierarchyControl;
        }

        public ExecutionResult Execute(IExecutionContext executionContext, Keys key)
        {
            if (!this._hierarchyControl.OpenOrCloseNode(eEXPAND_CODE.close)) {
				this._hierarchyControl.GoToParent();
			}
            return new ExecutionResult(executionContext.Clear(), CommandState.Handled);
        }
    }
}