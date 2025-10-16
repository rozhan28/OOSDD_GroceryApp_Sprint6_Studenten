using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Grocery.App.ViewModels
{
    public partial class NewProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly ProductViewModel _productViewModel;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private int stock;

        [ObservableProperty]
        private DateOnly shelfLife = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

        [ObservableProperty]
        private decimal price;

        public bool IsAdmin { get; set; } = true;

        public IAsyncRelayCommand SaveCommand { get; }

        public NewProductViewModel(IProductService productService, ProductViewModel productViewModel)
        {
            _productService = productService;
            _productViewModel = productViewModel;

            SaveCommand = new AsyncRelayCommand(SaveProductAsync);
        }

        private async Task SaveProductAsync()
        {
            if (!IsAdmin)
            {
                await Shell.Current.DisplayAlert("Geen toegang", "Alleen admin gebruikers mogen nieuwe producten toevoegen.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlert("Fout", "Naam mag niet leeg zijn.", "OK");
                return;
            }

            Product newProduct = new(0, Name, Stock, ShelfLife, Price);
            var added = _productService.Add(newProduct);

            _productViewModel.Products.Add(added);

            await Shell.Current.DisplayAlert("Succes", $"{Name} is toegevoegd.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}