using System.Windows.Forms;
using VsNerdX.Core;

namespace VsNerdX.Command.Directory
{
    public class OpenNodeOrGotoChild : ICommand
    {
        private readonly IHierarchyControl _hierarchyControl;

		public OpenNodeOrGotoChild(IHierarchyControl hierarchyControl)
        {
            this._hierarchyControl = hierarchyControl;
        }

        public ExecutionResult Execute(IExecutionContext executionContext, Keys key)
        {
            if (!this._hierarchyControl.OpenOrCloseNode(eEXPAND_CODE.open)) {
                this._hierarchyControl.GoToChild();
			}
            return new ExecutionResult(executionContext.Clear(), CommandState.Handled);
        }
    }
}