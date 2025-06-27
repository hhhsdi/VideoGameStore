using System.ComponentModel;

namespace VideoGameStore.Models
{
    public class Game 
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public string Platform { get; set; } 
        public string Key { get; set; } 
        public int AvailableKeysCount { get; set; } 
    }
}