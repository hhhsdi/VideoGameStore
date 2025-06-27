// ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq; // Добавлено для LINQ
using VideoGameStore.Models;
using VideoGameStore.Services;
using VideoGameStore.Views;

namespace VideoGameStore.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<Game> _games;

        [ObservableProperty]
        private Game _selectedGame;

        // НОВОЕ: Свойство для ввода поискового запроса
        [ObservableProperty]
        private string _searchText;

        public MainViewModel()
        {
            _databaseService = new DatabaseService();
            LoadGames(); // Загружаем игры с сортировкой при старте
        }

        /// <summary>
        /// Загружает (или перезагружает) список всех игр из базы данных, отсортированных по названию.
        /// Применяет фильтр, если задан поисковый текст.
        /// </summary>
        private void LoadGames()
        {
            // Получаем все игры из базы данных
            var allGamesFromDb = _databaseService.GetGames();

            // Применяем сортировку
            var sortedGames = allGamesFromDb.OrderBy(game => game.Title);

            // Применяем фильтрацию, если SearchText не пуст
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                // Приводим поисковый текст к нижнему регистру для нечувствительного к регистру поиска
                string searchLower = SearchText.ToLower();
                Games = new ObservableCollection<Game>(
                    sortedGames.Where(game => game.Title.ToLower().StartsWith(searchLower))
                );
            }
            else
            {
                // Если поисковый текст пуст, показываем все отсортированные игры
                Games = new ObservableCollection<Game>(sortedGames);
            }
        }

        // НОВОЕ: Команда для выполнения поиска
        [RelayCommand]
        private void SearchGames()
        {
            LoadGames(); // Просто вызываем LoadGames, которая теперь включает логику фильтрации
        }

        // НОВОЕ: Команда для сброса поиска
        [RelayCommand]
        private void ResetSearch()
        {
            SearchText = string.Empty; // Очищаем поисковый текст
            LoadGames(); // Перезагружаем все игры без фильтра
        }

        // --- Существующие команды без изменений ---

        [RelayCommand]
        private void AddGame()
        {
            var addEditWindow = new AddEditGameWindow();
            var addEditViewModel = new AddEditGameViewModel(new Game());
            addEditWindow.DataContext = addEditViewModel;

            if (addEditWindow.ShowDialog() == true)
            {
                try
                {
                    _databaseService.AddGame(addEditViewModel.CurrentGame);
                    LoadGames();
                    MessageBox.Show("Игра успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Microsoft.Data.Sqlite.SqliteException ex)
                {
                    MessageBox.Show($"Ошибка при добавлении игры: {ex.Message}\nВозможно, ключ уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void EditGame()
        {
            if (SelectedGame == null)
            {
                MessageBox.Show("Пожалуйста, выберите игру для редактирования.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var gameToEdit = new Game
            {
                Id = SelectedGame.Id,
                Title = SelectedGame.Title,
                Genre = SelectedGame.Genre,
                Price = SelectedGame.Price,
                Platform = SelectedGame.Platform,
                Key = SelectedGame.Key
            };

            var addEditWindow = new AddEditGameWindow();
            var addEditViewModel = new AddEditGameViewModel(gameToEdit);
            addEditWindow.DataContext = addEditViewModel;

            if (addEditWindow.ShowDialog() == true)
            {
                try
                {
                    _databaseService.UpdateGame(addEditViewModel.CurrentGame);
                    LoadGames();
                    MessageBox.Show("Игра успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Microsoft.Data.Sqlite.SqliteException ex)
                {
                    MessageBox.Show($"Ошибка при обновлении игры: {ex.Message}\nВозможно, ключ уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void DeleteGame()
        {
            if (SelectedGame == null)
            {
                MessageBox.Show("Пожалуйста, выберите игру для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить игру '{SelectedGame.Title}' (ID: {SelectedGame.Id})?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _databaseService.DeleteGame(SelectedGame.Id);
                    LoadGames();
                    MessageBox.Show("Игра успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Microsoft.Data.Sqlite.SqliteException ex)
                {
                    MessageBox.Show($"Ошибка при удалении игры: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void OpenShop()
        {
            var shopWindow = new ShopWindow();
            shopWindow.ShowDialog();
            LoadGames();
        }
    }
}