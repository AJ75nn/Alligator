using System;
using System.Windows;
using System.Windows.Media.Animation;
using AlligatorGh.Components.UI.Dashboard.Wpf.ViewModels;

namespace AlligatorGh.Components.UI.Dashboard.Wpf
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
            HamburgerBtn.Checked += HamburgerBtn_Checked;
            HamburgerBtn.Unchecked += HamburgerBtn_Unchecked;
        }

        private void HamburgerBtn_Checked(object sender, RoutedEventArgs e)
        {
            Storyboard sb = this.FindResource("ExpandSidebar") as Storyboard;
            if (sb != null)
                sb.Begin();
        }

        private void HamburgerBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            Storyboard sb = this.FindResource("CollapseSidebar") as Storyboard;
            if (sb != null)
                sb.Begin();
        }
    }
}