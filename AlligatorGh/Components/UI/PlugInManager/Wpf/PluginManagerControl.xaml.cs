using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using AlligatorGh.Components.UI.PlugInManager.Wpf.Services;
using AlligatorGh.Components.UI.PlugInManager.Wpf.ViewModels;

namespace AlligatorGh.Components.UI.PlugInManager.Wpf
{
    public partial class PluginManagerControl : UserControl
    {
        private Point _dragStart;
        private bool _mouseDownOnHandle;
        private PluginRowViewModel _dragCandidate;
        private PluginRowViewModel _draggingRow;
        private InsertionAdorner _adorner;

        public PluginManagerControl()
        {
            InitializeComponent();

            RowsControl.PreviewMouseLeftButtonDown += RowsControl_PreviewMouseLeftButtonDown;
            RowsControl.PreviewMouseMove += RowsControl_PreviewMouseMove;
            RowsControl.PreviewMouseLeftButtonUp += RowsControl_PreviewMouseLeftButtonUp;
            ListScroller.DragOver += ListScroller_DragOver;
            ListScroller.Drop += ListScroller_Drop;
            ListScroller.DragLeave += (_, _) => RemoveAdorner();
        }

        private PluginManagerViewModel ViewModel => DataContext as PluginManagerViewModel;

        // ---- Drag start detection -------------------------------------------------------------

        private void RowsControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(RowsControl);
            _dragCandidate = ResolveRow(e.OriginalSource as DependencyObject);
            _mouseDownOnHandle = IsOnDragHandle(e.OriginalSource as DependencyObject);
        }

        private void RowsControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _dragCandidate == null || !_mouseDownOnHandle)
            {
                return;
            }

            Point pos = e.GetPosition(RowsControl);
            if (Math.Abs(pos.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(pos.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            _draggingRow = _dragCandidate;
            try
            {
                DragDrop.DoDragDrop(RowsControl, _draggingRow, DragDropEffects.Move);
            }
            finally
            {
                _draggingRow = null;
                _mouseDownOnHandle = false;
                RemoveAdorner();
            }
        }

        // ---- Multi-select on click (not a drag) -----------------------------------------------

        private void RowsControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // A drag consumed the gesture, or the click landed on the checkbox: don't change selection.
            if (_draggingRow != null || IsOnCheckBox(e.OriginalSource as DependencyObject))
            {
                _dragCandidate = null;
                return;
            }

            var row = ResolveRow(e.OriginalSource as DependencyObject);
            if (row != null && ViewModel != null)
            {
                bool isShift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                ViewModel.HandleRowClick(row, isCtrl, isShift);
            }

            _dragCandidate = null;
        }

        // ---- Drag over / drop -----------------------------------------------------------------

        private void ListScroller_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(PluginRowViewModel)))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            e.Effects = DragDropEffects.Move;
            e.Handled = true;

            var (target, container) = ResolveRowAndContainer(e.OriginalSource as DependencyObject);
            if (target != null && container != null)
            {
                ShowAdorner(container, IsBelowMidpoint(container, e));
            }
        }

        private void ListScroller_Drop(object sender, DragEventArgs e)
        {
            RemoveAdorner();

            if (!e.Data.GetDataPresent(typeof(PluginRowViewModel)) || ViewModel == null)
            {
                return;
            }

            var dragged = (PluginRowViewModel)e.Data.GetData(typeof(PluginRowViewModel));
            var (target, container) = ResolveRowAndContainer(e.OriginalSource as DependencyObject);
            if (dragged == null || target == null || dragged == target)
            {
                return;
            }

            int from = ViewModel.Rows.IndexOf(dragged);
            int to = ViewModel.Rows.IndexOf(target);
            if (from < 0 || to < 0)
            {
                return;
            }

            // Dropping on the lower half of the target inserts after it.
            if (IsBelowMidpoint(container, e) && to < from)
            {
                to++;
            }
            else if (!IsBelowMidpoint(container, e) && to > from)
            {
                to--;
            }

            ViewModel.MoveRow(from, to);
        }

        // ---- Helpers --------------------------------------------------------------------------

        private static bool IsOnDragHandle(DependencyObject source)
        {
            while (source != null)
            {
                if (source is FrameworkElement fe && Equals(fe.Tag, "DragHandle"))
                {
                    return true;
                }

                if (source is ContentPresenter)
                {
                    return false;
                }

                source = VisualTreeHelper.GetParent(source);
            }

            return false;
        }

        private static bool IsOnCheckBox(DependencyObject source)
        {
            while (source != null)
            {
                if (source is CheckBox)
                {
                    return true;
                }

                if (source is ContentPresenter)
                {
                    return false;
                }

                source = VisualTreeHelper.GetParent(source);
            }

            return false;
        }

        private PluginRowViewModel ResolveRow(DependencyObject source)
        {
            return ResolveRowAndContainer(source).Row;
        }

        private (PluginRowViewModel Row, ContentPresenter Container) ResolveRowAndContainer(DependencyObject source)
        {
            while (source != null)
            {
                if (source is ContentPresenter cp && cp.Content is PluginRowViewModel row)
                {
                    return (row, cp);
                }

                source = VisualTreeHelper.GetParent(source);
            }

            return (null, null);
        }

        private static bool IsBelowMidpoint(ContentPresenter container, DragEventArgs e)
        {
            if (container == null)
            {
                return false;
            }

            Point p = e.GetPosition(container);
            return p.Y > container.ActualHeight / 2;
        }

        private void ShowAdorner(ContentPresenter container, bool below)
        {
            RemoveAdorner();

            var layer = AdornerLayer.GetAdornerLayer(RowsControl);
            if (layer == null)
            {
                return;
            }

            var accent = TryFindResource(ThemeService.Accent) as Brush ?? Brushes.DodgerBlue;
            _adorner = new InsertionAdorner(container, below, accent);
            layer.Add(_adorner);
        }

        private void RemoveAdorner()
        {
            if (_adorner == null)
            {
                return;
            }

            var layer = AdornerLayer.GetAdornerLayer(RowsControl);
            layer?.Remove(_adorner);
            _adorner = null;
        }
    }

    /// <summary>
    /// A thin accent line drawn at the top or bottom edge of a row to show where a dragged row will
    /// be inserted.
    /// </summary>
    internal sealed class InsertionAdorner : Adorner
    {
        private readonly bool _below;
        private readonly Brush _brush;

        public InsertionAdorner(UIElement adornedElement, bool below, Brush brush)
            : base(adornedElement)
        {
            _below = below;
            _brush = brush;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double y = _below ? AdornedElement.RenderSize.Height : 0;
            double width = AdornedElement.RenderSize.Width;
            var pen = new Pen(_brush, 2);
            drawingContext.DrawLine(pen, new Point(0, y), new Point(width, y));
        }
    }
}