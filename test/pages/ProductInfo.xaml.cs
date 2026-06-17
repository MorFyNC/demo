using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using test.DatabaseContext;
using test.Windows.Product;

namespace test.pages
{
    /// <summary>
    /// Логика взаимодействия для ProductInfo.xaml
    /// </summary>
    public partial class ProductInfo : Page
    {
        public ProductInfo()
        {
            InitializeComponent();

            FilterCB.SelectedIndex = 0;
            SortCB.SelectedIndex = 0;

            ConfigureAccess();
            ApplyFilters();
        }

        private void ConfigureAccess()
        {
            if (CurrentUser.currentUser == null)
            {
                ControlPanel.Visibility = Visibility.Collapsed;
                return;
            }

            int roleId = CurrentUser.currentUser.ID_Role;

            if (roleId == 0 || roleId == 3)
            {
                ControlPanel.Visibility = Visibility.Collapsed;
                return;
            }

            if (roleId == 2)
            {
                AddButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchTB_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void FilterCB_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortCB_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ResetButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SearchTB.Text = "";
            FilterCB.SelectedIndex = 0;
            SortCB.SelectedIndex = 0;

            ApplyFilters();
        }

        private void AddButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsAdministrator())
            {
                MessageBox.Show(
                    "Добавлять товары может только администратор.",
                    "Доступ запрещён",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var addWindow = new AddProduct();
            addWindow.ShowDialog();

            ApplyFilters();
        }

        private void DeleteButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsAdministrator())
            {
                MessageBox.Show(
                    "Удалять товары может только администратор.",
                    "Доступ запрещён",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var selectedProduct =
                MainListView.SelectedItem as Product;

            if (selectedProduct == null)
            {
                MessageBox.Show(
                    "Выберите товар для удаления.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            bool productInOrder =
                DBconection.BicycleEntities.Product_Order
                    .Any(x =>
                        x.ID_Product == selectedProduct.ID);

            if (productInOrder)
            {
                MessageBox.Show(
                    "Удаление невозможно. Товар присутствует в заказе.",
                    "Удаление запрещено",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Вы точно хотите удалить товар «"
                + selectedProduct.Name
                + "»?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                string imageName = selectedProduct.Photo;

                DBconection.BicycleEntities.Product.Remove(
                    selectedProduct);

                DBconection.BicycleEntities.SaveChanges();

                DeleteProductImage(imageName);

                MessageBox.Show(
                    "Товар успешно удалён.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить товар.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void MainListView_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!IsAdministrator())
            {
                return;
            }

            var selectedProduct =
                MainListView.SelectedItem as Product;

            if (selectedProduct == null)
            {
                return;
            }

            var editProductWindow =
                new EditProduct(selectedProduct);

            editProductWindow.ShowDialog();

            ApplyFilters();
        }

        private bool IsAdministrator()
        {
            return CurrentUser.currentUser != null
                && CurrentUser.currentUser.ID_Role == 1;
        }

        private void ApplyFilters()
        {
            string search = SearchTB.Text.Trim();

            int filterIndex = FilterCB.SelectedIndex;
            int sortIndex = SortCB.SelectedIndex;

            IQueryable<Product> products =
                DBconection.BicycleEntities.Product
                    .Include("Category")
                    .Include("Manufacturer")
                    .Include("Supplier")
                    .Include("Measurement");

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(x =>
                    x.Article.Contains(search)
                    || x.Name.Contains(search)
                    || x.Description.Contains(search)
                    || x.Category.Name.Contains(search)
                    || x.Manufacturer.Name.Contains(search)
                    || x.Supplier.Name.Contains(search)
                    || x.Measurement.Name.Contains(search));
            }

            switch (filterIndex)
            {
                case 1:
                    products = products.Where(x =>
                        x.Discount >= 0
                        && x.Discount < 12);
                    break;

                case 2:
                    products = products.Where(x =>
                        x.Discount >= 12
                        && x.Discount < 19);
                    break;

                case 3:
                    products = products.Where(x =>
                        x.Discount >= 19);
                    break;
            }

            switch (sortIndex)
            {
                case 1:
                    products = products.OrderBy(x => x.Price);
                    break;

                case 2:
                    products = products.OrderByDescending(
                        x => x.Price);
                    break;

                case 3:
                    products = products.OrderBy(x => x.Amount);
                    break;

                case 4:
                    products = products.OrderByDescending(
                        x => x.Amount);
                    break;

                default:
                    products = products.OrderBy(x => x.ID);
                    break;
            }

            MainListView.ItemsSource = products.ToList();
        }

        private void DeleteProductImage(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName)
                || imageName == "picture.png")
            {
                return;
            }

            string productsFolder =
                System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        @"..\..\Materials\Products"));

            string imagePath =
                System.IO.Path.Combine(
                    productsFolder,
                    imageName);

            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
    }
}

