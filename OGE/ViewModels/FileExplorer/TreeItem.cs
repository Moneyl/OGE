//Source: https://github.com/condron/ReactiveUI-TreeView

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using FontAwesome.WPF;
using ReactiveUI;

namespace OGE.ViewModels.FileExplorer
{
    public abstract class TreeItem : ReactiveObject
    {
        private bool _isExpanded;
        private bool _isSelected;
        private FontAwesomeIcon _icon;
        private Brush _foregroundBrush;
        private TreeItem _parent;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                this.RaiseAndSetIfChanged(ref _isExpanded, value);
                OnExpansionChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public FontAwesomeIcon Icon
        {
            get => _icon;
            set => this.RaiseAndSetIfChanged(ref _icon, value);
        }

        public Brush ForegroundBrush
        {
            get => _foregroundBrush;
            set => this.RaiseAndSetIfChanged(ref _foregroundBrush, value);
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

        protected virtual void OnExpansionChanged()
        {

        }
    }
}