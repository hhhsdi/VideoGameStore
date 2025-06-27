// ViewModels/ShopViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using VideoGameStore.Models;
using VideoGameStore.Services;
using System.Linq; // Добавляем эту директиву
using System.Windows;

namespace VideoGameStore.ViewModels
{
    public partial class ShopViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<Game> _availableGames;

        [ObservableProperty]
        private ObservableCollection<Game> _cartItems;

        [ObservableProperty]
        private Game _selectedAvailableGame;

        [ObservableProperty]
        private Game _selectedCartItem;

        [ObservableProperty]
        private decimal _cartTotal;

        // НОВОЕ: Свойство для ввода поискового запроса в магазине
        [ObservableProperty]
        private string _searchText;

        public ShopViewModel()
        {
            _databaseService = new DatabaseService();
            CartItems = new ObservableCollection<Game>();
            LoadAvailableGames();
            UpdateCartTotal();
        }

        /// <summary>
        /// Загружает (или перезагружает) список доступных игр из базы данных,
        /// применяя сортировку и фильтрацию по поисковому тексту.
        /// </summary>
        private void LoadAvailableGames()
        {
            // Получаем агрегированный список игр из БД (только те, у кого > 0 ключей)
            var allAvailableGamesFromDb = _databaseService.GetGamesForShop();

            // Применяем сортировку (уже есть в GetGamesForShop, но можно продублировать для ясности или изменить)
            var sortedGames = allAvailableGamesFromDb.OrderBy(game => game.Title);

            // Применяем фильтрацию, если SearchText не пуст
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.ToLower();
                AvailableGames = new ObservableCollection<Game>(
                    sortedGames.Where(game => game.Title.ToLower().StartsWith(searchLower))
                );
            }
            else
            {
                // Если поисковый текст пуст, показываем все отсортированные доступные игры
                AvailableGames = new ObservableCollection<Game>(sortedGames);
            }
        }

        // НОВОЕ: Команда для выполнения поиска в магазине
        [RelayCommand]
        private void SearchGames()
        {
            LoadAvailableGames(); // Просто вызываем LoadAvailableGames, которая теперь включает логику фильтрации
        }

        // НОВОЕ: Команда для сброса поиска в магазине
        [RelayCommand]
        private void ResetSearch()
        {
            SearchText = string.Empty; // Очищаем поисковый текст
            LoadAvailableGames(); // Перезагружаем все игры без фильтра
        }

        // --- Существующие команды без изменений ---

        [RelayCommand]
        private void AddToCart()
        {
            if (SelectedAvailableGame == null)
            {
                MessageBox.Show("Пожалуйста, выберите игру для добавления в корзину.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (SelectedAvailableGame.AvailableKeysCount <= 0)
            {
                MessageBox.Show("Ключей для этой игры нет в наличии.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var gameToAdd = new Game
            {
                Title = SelectedAvailableGame.Title,
                Genre = SelectedAvailableGame.Genre,
                Price = SelectedAvailableGame.Price,
                Platform = SelectedAvailableGame.Platform
            };

            CartItems.Add(gameToAdd);
            SelectedAvailableGame.AvailableKeysCount--; // Уменьшаем счетчик в UI

            UpdateCartTotal();
        }

        [RelayCommand]
        private void RemoveFromCart()
        {
            if (SelectedCartItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите товар для удаления из корзины.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            CartItems.Remove(SelectedCartItem);
            var gameInShop = AvailableGames.FirstOrDefault(g =>
                g.Title == SelectedCartItem.Title &&
                g.Genre == SelectedCartItem.Genre &&
                g.Price == SelectedCartItem.Price &&
                g.Platform == SelectedCartItem.Platform);

            if (gameInShop != null)
            {
                gameInShop.AvailableKeysCount++; // Увеличиваем счетчик обратно
            }
            UpdateCartTotal();
        }

        [RelayCommand]
        private void Checkout()
        {
            if (!CartItems.Any())
            {
                MessageBox.Show("Корзина пуста. Нечего оформлять.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var cartItem in CartItems)
            {
                var gameToDelete = _databaseService.GetGames()
                                                    .FirstOrDefault(g =>
                                                        g.Title == cartItem.Title &&
                                                        g.Genre == cartItem.Genre &&
                                                        g.Price == cartItem.Price &&
                                                        g.Platform == cartItem.Platform);

                if (gameToDelete != null)
                {
                    _databaseService.DeleteGame(gameToDelete.Id);
                }
            }

            MessageBox.Show($"Заказ на сумму {CartTotal:C2} успешно оформлен!", "Оформление заказа", MessageBoxButton.OK, MessageBoxImage.Information);

            CartItems.Clear();
            UpdateCartTotal();
            LoadAvailableGames(); // Перезагружаем игры, чтобы отразить изменения наличия
        }

        private void UpdateCartTotal()
        {
            CartTotal = CartItems.Sum(item => item.Price);
        }
    }
}