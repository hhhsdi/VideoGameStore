using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VideoGameStore.Models;
using System.Collections.Generic;

namespace VideoGameStore.ViewModels
{
    public partial class AddEditGameViewModel : ObservableObject
    {
        [ObservableProperty]
        private Game _currentGame;

        public List<string> Platforms { get; } = new List<string>
        {
            "PC", "PlayStation", "Xbox", "Nintendo Switch", "Mobile"
        };

        public AddEditGameViewModel(Game game)
        {
            CurrentGame = game;
        }

    }
}