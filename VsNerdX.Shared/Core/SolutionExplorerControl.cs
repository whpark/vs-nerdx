﻿using System;
using System.Collections;
using System.Linq;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VsNerdX.Util;

namespace VsNerdX.Core
{
    public class HierarchyControl : IHierarchyControl
    {
        public IVsUIHierarchyWindow SolutionHierarchy;
        public Panel ContentGrid;
        public readonly HelpViewControl helpViewControl;

        private const string SolutionPivotNavigator = "Microsoft.VisualStudio.PlatformUI.SolutionPivotNavigator";
        private const string SolutionPivotTreeView = "Microsoft.VisualStudio.PlatformUI.SolutionPivotTreeView";
        private const string WorkspaceTreeView = "Microsoft.VisualStudio.Workspace.VSIntegration.UI.WorkspaceTreeViewControl";

        private readonly ILogger logger;
        private readonly VsNerdXPackage vsNerdXPackage;

        private ContentPresenter ContentPresenter;

        internal HierarchyControl(VsNerdXPackage vsNerdXPackage, ILogger logger)
        {
            this.logger = logger;
            this.vsNerdXPackage = vsNerdXPackage;
            this.helpViewControl = new HelpViewControl(this);
        }

        public void GoDown()
        {
            var listBox = GetHierarchyListBox();
            if (listBox == null) return;
            var index = listBox.Items.IndexOf(listBox.SelectedItem) + 1;

            if (index < listBox.Items.Count)
            {
                listBox.SelectedItem = listBox.Items.GetItemAt(index);
                EnsureSelection();
            }

        }

        public void GoToFirstChild()
        {
            var listBox = GetHierarchyListBox();
            var item = listBox.SelectedItem;

            if (item == null) return;

            var parent = item.GetType().GetProperty("Parent")?.GetValue(item);
            if (parent == null) return;

            var childNodes = parent.GetType().GetProperty("ChildNodes").GetValue(parent);
            if (childNodes == null) return;

            var first = listBox.SelectedItem = ((IEnumerable)childNodes).Cast<Object>().ToList().First();
            EnsureSelection();
        }

        public void GoToLastChild()
        {
            var listBox = GetHierarchyListBox();
            var item = listBox.SelectedItem;

            if (item == null) return;

            var parent = item.GetType().GetProperty("Parent")?.GetValue(item);
            if (parent == null) return;

            var childNodes = parent.GetType().GetProperty("ChildNodes").GetValue(parent);
            if (childNodes == null) return;

            var last = listBox.SelectedItem = ((IEnumerable)childNodes).Cast<Object>().ToList().Last();
            EnsureSelection();
        }

        public void GoToParent()
        {
            var listBox = GetHierarchyListBox();
            var item = listBox.SelectedItem;

            if (item == null) return;

            var parent = item.GetType().GetProperty("Parent")?.GetValue(item);
            if (parent == null) return;

            listBox.SelectedItem = parent;
            EnsureSelection();
        }

        public void GoToChild()
        {
            var listBox = GetHierarchyListBox();
            var item = listBox.SelectedItem;

            if (item == null) return;

            var childNodes = item.GetType().GetProperty("ChildNodes").GetValue(item);
            if (childNodes == null) return;

            var first = listBox.SelectedItem = ((IEnumerable)childNodes).Cast<Object>().ToList().First();
            EnsureSelection();
        }

        public void CloseParentNode()
        {
            GoToParent();
            var parent = GetHierarchyListBox().SelectedItem;
            parent.GetType().GetProperty("IsExpanded")
                ?.SetValue(parent, false);
        }

        public bool OpenOrCloseNode(eEXPAND_CODE eExpandCode)
        {
            var listBox = GetHierarchyListBox();
            var item = listBox.SelectedItem;
            var expandable = (bool?)item.GetType().GetProperty("IsExpandable")?.GetValue(item);
            var expanded = (bool?)item.GetType().GetProperty("IsExpanded")?.GetValue(item);

            //if (expandable != true) return false;

            var newExpandState = (eExpandCode == eEXPAND_CODE.toggle) ? !expanded : eExpandCode == eEXPAND_CODE.open;
            if (newExpandState == expanded)
				return false;
            if ( (newExpandState == true) && (expandable == false))
                return false;

			item.GetType().GetProperty("IsExpanded")?.SetValue(item, newExpandState == true);
            return true;
        }

        public Object GetSelectedItem()
        {
            var listBox = GetHierarchyListBox();
            return listBox.SelectedItem;
        }

        public void GoUp()
        {
            var listBox = GetHierarchyListBox();
            if (listBox == null) return;
            var index = listBox.Items.IndexOf(listBox.SelectedItem) - 1;
            if (index >= 0)
            {
                listBox.SelectedItem = listBox.Items.GetItemAt(index);
                EnsureSelection();
            }
        }

        public void GoToTop()
        {
            var listBox = GetHierarchyListBox();
            if (listBox == null) return;
            listBox.SelectedItem = listBox.Items.GetItemAt(0);
            EnsureSelection();
        }

        public void GoToBottom()
        {
            var listBox = GetHierarchyListBox();
            if (listBox == null) return;
            listBox.SelectedItem = listBox.Items.GetItemAt(listBox.Items.Count - 1);
            EnsureSelection();
        }

        public void EnsureSelection()
        {
            var listBox = GetHierarchyListBox();
            if (listBox == null) return;
            listBox.GetType().GetMethod("FocusSelectedItem")
                ?.Invoke(listBox, new Object[] { });
        }

        public ListBox GetHierarchyListBox()
        {
            SolutionHierarchy = VsShellUtilities.GetUIHierarchyWindow(ServiceLocator.GetInstance<IServiceProvider>(), VSConstants.StandardToolWindows.SolutionExplorer);

            ContentGrid = SolutionHierarchy.GetValue("Content") as Panel;
            if (ContentGrid == null || ContentGrid.Children.Count == 0)
            {
                return null;
            }

            ContentPresenter = ContentGrid.Children[0] as ContentPresenter;
            if (ContentPresenter == null)
            {
                return null;
            }

            ListBox listBox = null;

            switch (ContentPresenter.Content.GetType().FullName)
            {
                case SolutionPivotNavigator:
                    listBox = ContentPresenter.Content.GetType().GetProperties()
                        .Single(p => p.Name == "TreeView" && p.PropertyType.FullName == SolutionPivotTreeView)
                        .GetValue(ContentPresenter.Content) as ListBox;
                    break;
                case WorkspaceTreeView:
                    listBox = ContentPresenter.Content as ListBox;
                    break;
            }

            return listBox;
        }

        public void ToggleHelp()
        {
            helpViewControl.ToggleHelp();
        }
    }
}
