//Source: https://github.com/condron/ReactiveUI-TreeView

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace OGE.ViewModels.FileExplorer
{
    public abstract class TreeItem : ReactiveObject
    {
        private bool _isExpanded;
        private bool _isSelected;
        private TreeItem _parent;

        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public abstract object ViewModel { get; }
        public ObservableCollection<TreeItem> Children { get; }

        protected TreeItem(IEnumerable<TreeItem> children = null)
        {
            Children = new ObservableCollection<TreeItem>();
            if (children == null) return;
            foreach (var child in children)
            {
                AddChild(child);
            }
        }

        public void AddChild(TreeItem child)
        {
            child._parent = this;
            Children.Add(child);
        }

        public void Expand()
        {
            IsExpanded = true;
        }

        public void Collapse()
        {
            IsExpanded = false;
        }

        public void ExpandPath()
        {
            IsExpanded = true;
            _parent?.ExpandPath();
        }

        public void CollapsePath()
        {
            IsExpanded = false;
            _parent?.CollapsePath();
        }
    }
}