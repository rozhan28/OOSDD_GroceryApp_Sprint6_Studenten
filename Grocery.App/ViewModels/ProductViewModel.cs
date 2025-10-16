using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Grocery.App.ViewModels
{
    public class ProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        public ObservableCollection<Product> Products { get; set; }

        public IAsyncRelayCommand AddNewProductCommand { get; }

        public ProductViewModel(IProductService productService)
        {
            _productService = productService;

            Products = new ObservableCollection<Product>(_productService.GetAll());

            AddNewProductCommand = new AsyncRelayCommand(GoToNewProductViewAsync);
        }

        private async Task GoToNewProductViewAsync()
        {
            await Shell.Current.GoToAsync(nameof(Grocery.App.Views.NewProductView));
        }
    }
}
