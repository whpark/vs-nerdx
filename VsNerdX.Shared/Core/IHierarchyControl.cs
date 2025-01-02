namespace VsNerdX.Core
{
    public enum eEXPAND_CODE { close, open, toggle };

    public interface IHierarchyControl
    {
        void GoUp();
        void GoDown();
        void GoToTop();
        void GoToBottom();
        void GoToParent();
        void GoToChild();
        void GoToFirstChild();
        void GoToLastChild();
        void CloseParentNode();
        bool OpenOrCloseNode(eEXPAND_CODE eExpandCode = eEXPAND_CODE.toggle);
        void ToggleHelp();
        object GetSelectedItem();
    }
}
