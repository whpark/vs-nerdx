﻿using System;
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
        private const string SolutionPivotNavigator = "Microsoft.VisualStudio.PlatformUI.SolutionPivotNavigator";
        private const string SolutionPivotTreeView = "Microsoft.VisualStudio.PlatformUI.SolutionPivotTreeView";
        private const string WorkspaceTreeView = "Microsoft.VisualStudio.Workspace.VSIntegration.UI.WorkspaceTreeViewControl";

        private readonly ILogger logger;
        private readonly VsNerdXPackage vsNerdXPackage;

        public IVsUIHierarchyWindow SolutionHierarchy { get; set; }

        internal HierarchyControl(VsNerdXPackage vsNerdXPackage, ILogger logger)
        {
            this.logger = logger;
            this.vsNerdXPackage = vsNerdXPackage;
        }

        public void GoDown()
        {
            var listBox = GetHierarchyListBox();
            if (listBox == null) return;
            var index = listBox.Items.IndexOf(listBox.SelectedItem) + 1;

            if (index < listBox.Items.Count)
            {
                listBox.SelectedItem = listBox.Items.GetItemAt(index);
                EnsureSelection(listBox);
            }
        }

        public void GoToParent()
        {
            var listBox = GetHierarchyListBox();
            var item = listBox.SelectedItem;

            if (item == null) return;

            var parent = item.GetType().GetProperty("Parent")?.GetValue(item);
            if (parent == null) return;

            listBox.SelectedItem = parent;
            EnsureSelection(listBox);
        }

        public void CloseParentNode()
        {
            GoToParent();
            var parent = GetHierarchyListBox().SelectedItem;
            parent.GetType().GetProperty("IsExpanded")
                ?.SetValue(parent, false);
        }

        public void OpenOrCloseNode()
        {
            var listBox = GetHierarchyListBox();
            var item = listBox.SelectedItem;
            var expandable = (bool?)item.GetType().GetProperty("IsExpandable")?.GetValue(item);
            var expanded = (bool?)item.GetType().GetProperty("IsExpanded")?.GetValue(item);

            if (expandable != true) return;

            item.GetType().GetProperty("IsExpanded")?.SetValue(item, expanded != true);
        }

        public void GoUp()
        {
            var listBox = GetHierarchyListBox();
            if (listBox == null) return;
            var index = listBox.Items.IndexOf(listBox.SelectedItem) - 1;
            if (index >= 0)
            {
                listBox.SelectedItem = listBox.Items.GetItemAt(index);
                EnsureSelection(listBox);
            }
        }

        public void GoToTop()
        {
            var listBox = GetHierarchyListBox();
            if (listBox == null) return;
            listBox.SelectedItem = listBox.Items.GetItemAt(0);
            EnsureSelection(listBox);
        }

        public void GoToBottom()
        {
            var listBox = GetHierarchyListBox();
            if (listBox == null) return;
            listBox.SelectedItem = listBox.Items.GetItemAt(listBox.Items.Count - 1);
            EnsureSelection(listBox);
        }

        public void EnsureSelection(object listBox)
        {
            if (listBox == null) return;
            listBox.GetType().GetMethod("FocusSelectedItem")
                ?.Invoke(listBox, new Object[] { });
        }

        public ListBox GetHierarchyListBox()
        {
            SolutionHierarchy = VsShellUtilities.GetUIHierarchyWindow(vsNerdXPackage, VSConstants.StandardToolWindows.SolutionExplorer);

            if (!(SolutionHierarchy is WindowPane solutionPane))
            {
                return null;
            }

            if (!(solutionPane.Content is Panel paneContent) || paneContent.Children.Count == 0)
            {
                return null;
            }

            if (!(paneContent.Children[0] is ContentPresenter contentPresenter))
            {
                return null;
            }

            ListBox listBox = null;

            switch (contentPresenter.Content.GetType().FullName)
            {
                case SolutionPivotNavigator:
                    listBox = contentPresenter.Content.GetType().GetProperties()
                        .Single(p => p.Name == "TreeView" && p.PropertyType.FullName == SolutionPivotTreeView)
                        .GetValue(contentPresenter.Content) as ListBox;
                    break;
                case WorkspaceTreeView:
                    listBox = contentPresenter.Content as ListBox;
                    break;
            }

            return listBox;
        }
    }
}