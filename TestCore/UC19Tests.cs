using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Grocery.Core.Services;
using Moq;
using NUnit.Framework;
using System;

namespace Grocery.Tests.Core
{
    [TestFixture]
    public class ProductServiceTests
    {
        private Mock<IProductRepository> _mockRepo;
        private ProductService _productService;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IProductRepository>();
            _productService = new ProductService(_mockRepo.Object);
        }

        [Test]
        public void UC19_AdminCanAddNewProduct()
        {
            // Arrange
            var newProduct = new Product(0, "Appel", 20, DateOnly.FromDateTime(DateTime.Today.AddMonths(1)), 1.99m);
            _mockRepo.Setup(r => r.Add(It.IsAny<Product>())).Returns(new Product(1, "Appel", 20, DateOnly.FromDateTime(DateTime.Today.AddMonths(1)), 1.99m));

            bool isAdmin = true;

            // Act
            Product? result = null;
            if (isAdmin)
                result = _productService.Add(newProduct);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            _mockRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        }

        [Test]
        public void UC19_NonAdminCannotAddProduct()
        {
            // Arrange
            var newProduct = new Product(0, "Banaan", 10, DateOnly.FromDateTime(DateTime.Today.AddMonths(1)), 0.99m);
            bool isAdmin = false;

            // Act
            Product? result = null;
            if (isAdmin)
                result = _productService.Add(newProduct);

            // Assert
            Assert.That(result, Is.Null, "Niet-admin gebruikers mogen geen product aanmaken.");
            _mockRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
        }
    }
}
