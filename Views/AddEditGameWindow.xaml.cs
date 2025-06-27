using System.Windows;
using VideoGameStore.ViewModels;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VideoGameStore.Views
{
    public partial class AddEditGameWindow : Window
    {
        public AddEditGameWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as AddEditGameViewModel;
            if (viewModel != null)
            {
                if (string.IsNullOrWhiteSpace(viewModel.CurrentGame.Title) ||
                    string.IsNullOrWhiteSpace(viewModel.CurrentGame.Key))
                {
                    MessageBox.Show("Поля 'Название' и 'Ключ' не могут быть пустыми.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DialogResult = true; 
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; 
        }
    }
}