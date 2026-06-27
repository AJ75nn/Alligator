using System.Collections.ObjectModel;

namespace AlligatorGh.Components.UI.ThemeCustomizer.Wpf.ViewModels
{
    public class ThemeGroupViewModel
    {
        public string Title { get; }
        public ObservableCollection<ThemePropertyViewModel> Properties { get; }

        public ThemeGroupViewModel(string title, params ThemePropertyViewModel[] properties)
        {
            Title = title;
            Properties = new ObservableCollection<ThemePropertyViewModel>(properties);
        }
    }
}
