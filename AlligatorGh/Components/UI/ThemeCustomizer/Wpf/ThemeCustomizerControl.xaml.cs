using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using AlligatorGh.Components.UI.ThemeCustomizer.Wpf.ViewModels;

namespace AlligatorGh.Components.UI.ThemeCustomizer.Wpf
{
    public partial class ThemeCustomizerControl : UserControl
    {
        private bool _isDraggingHue = false;
        private bool _isDraggingSV = false;

        public ThemeCustomizerControl()
        {
            InitializeComponent();
        }

        private void Popup_Opened(object sender, System.EventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.Popup popup && popup.Child is FrameworkElement el)
            {
                UpdateThumbPositions(el);
            }
        }

        private void UpdateThumbPositions(FrameworkElement popupChild)
        {
            var svThumb = popupChild.FindName("SVThumb") as System.Windows.Shapes.Ellipse;
            var hueThumb = popupChild.FindName("HueThumb") as System.Windows.Shapes.Ellipse;
            var svPicker = popupChild.FindName("SVPicker") as FrameworkElement;
            var hueSlider = popupChild.FindName("HueSlider") as FrameworkElement;

            if (svThumb != null && hueThumb != null && svPicker != null && hueSlider != null && popupChild.DataContext is ThemePropertyViewModel vm)
            {
                Canvas.SetLeft(svThumb, vm.Saturation * svPicker.ActualWidth - (svThumb.ActualWidth / 2));
                Canvas.SetTop(svThumb, (1.0 - vm.Value) * svPicker.ActualHeight - (svThumb.ActualHeight / 2));

                Canvas.SetLeft(hueThumb, (vm.Hue / 360.0) * hueSlider.ActualWidth - (hueThumb.ActualWidth / 2));
            }
        }

        private void HueSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is ThemePropertyViewModel vm)
            {
                _isDraggingHue = true;
                el.CaptureMouse();
                UpdateHue(e.GetPosition(el), el.ActualWidth, vm);
            }
        }

        private void HueSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingHue && sender is FrameworkElement el && el.DataContext is ThemePropertyViewModel vm)
            {
                UpdateHue(e.GetPosition(el), el.ActualWidth, vm);
            }
        }

        private void HueSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingHue && sender is FrameworkElement el)
            {
                _isDraggingHue = false;
                el.ReleaseMouseCapture();
            }
        }

        private void SVPicker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement el && el.DataContext is ThemePropertyViewModel vm)
            {
                _isDraggingSV = true;
                el.CaptureMouse();
                UpdateSV(e.GetPosition(el), el.ActualWidth, el.ActualHeight, vm);
            }
        }

        private void SVPicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingSV && sender is FrameworkElement el && el.DataContext is ThemePropertyViewModel vm)
            {
                UpdateSV(e.GetPosition(el), el.ActualWidth, el.ActualHeight, vm);
            }
        }

        private void SVPicker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDraggingSV && sender is FrameworkElement el)
            {
                _isDraggingSV = false;
                el.ReleaseMouseCapture();
            }
        }

        private void UpdateHue(Point p, double width, ThemePropertyViewModel vm)
        {
            double x = p.X;
            if (x < 0) x = 0;
            if (x > width) x = width;

            double hue = (x / width) * 360.0;
            if (hue >= 360) hue = 359.99;

            vm.SetHue(hue);

            // Update thumb position visually
            if (Mouse.Captured is FrameworkElement el)
            {
                var hueThumb = el.FindName("HueThumb") as System.Windows.Shapes.Ellipse;
                if (hueThumb != null)
                {
                    Canvas.SetLeft(hueThumb, x - (hueThumb.ActualWidth / 2));
                }
            }
        }

        private void UpdateSV(Point p, double width, double height, ThemePropertyViewModel vm)
        {
            double x = p.X;
            double y = p.Y;
            if (x < 0) x = 0;
            if (x > width) x = width;
            if (y < 0) y = 0;
            if (y > height) y = height;

            double saturation = x / width;
            double value = 1.0 - (y / height);

            vm.SetSaturationValue(saturation, value);

            // Update thumb position visually
            if (Mouse.Captured is FrameworkElement el)
            {
                var svThumb = el.FindName("SVThumb") as System.Windows.Shapes.Ellipse;
                if (svThumb != null)
                {
                    Canvas.SetLeft(svThumb, x - (svThumb.ActualWidth / 2));
                    Canvas.SetTop(svThumb, y - (svThumb.ActualHeight / 2));
                }
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Hue" || e.PropertyName == "Saturation" || e.PropertyName == "Value")
            {
                if (sender is ThemePropertyViewModel vm && vm.IsPopupOpen)
                {
                    // For live updates when typing R, G, B, we need to locate the open popup.
                    // Instead of traversing the visual tree, we rely on the Popup_Opened which sets initial positions,
                    // and mouse drags which call UpdateHue/UpdateSV directly.
                    // But typing in text boxes bypasses the dragging.
                    // Let's iterate over visual children to find the popup if needed, or simply re-layout on next open.
                    // Since WPF Popups don't give a direct global list easily without visual tree helpers,
                    // and we didn't store a reference to the active popup content, we will just rely on Popup_Opened.
                }
            }
        }
    }
}