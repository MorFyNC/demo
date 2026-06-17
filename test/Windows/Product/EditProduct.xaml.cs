using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using test.DatabaseContext;

namespace test.Windows.Product
{
    /// <summary>
    /// Логика взаимодействия для EditProduct.xaml
    /// </summary>
    public partial class EditProduct : Window
    {
        private readonly DatabaseContext.Product _currentProduct;

        private string _newImage;

        public EditProduct(DatabaseContext.Product product)
        {
            InitializeComponent();

            _currentProduct = product;

            FillComboBoxes();
            FillProductData();
        }

        private void FillComboBoxes()
        {
            measurementСВ.ItemsSource = DBconection.BicycleEntities.Measurement
                .OrderBy(x => x.ID)
                .ToList();

            measurementСВ.DisplayMemberPath = "Name";
            measurementСВ.SelectedValuePath = "ID";

            manufacturerCB.ItemsSource = DBconection.BicycleEntities.Manufacturer
                .OrderBy(x => x.ID)
                .ToList();

            manufacturerCB.DisplayMemberPath = "Name";
            manufacturerCB.SelectedValuePath = "ID";

            supplierсCB.ItemsSource = DBconection.BicycleEntities.Supplier
                .OrderBy(x => x.ID)
                .ToList();

            supplierсCB.DisplayMemberPath = "Name";
            supplierсCB.SelectedValuePath = "ID";

            CategoryCB.ItemsSource = DBconection.BicycleEntities.Category
                .OrderBy(x => x.ID)
                .ToList();

            CategoryCB.DisplayMemberPath = "Name";
            CategoryCB.SelectedValuePath = "ID";
        }

        private void FillProductData()
        {
            IDTB.Text = _currentProduct.ID.ToString();
            ArticleTB.Text = _currentProduct.Article;
            NameTB.Text = _currentProduct.Name;
            DescriptionTB.Text = _currentProduct.Description;
            PriceTB.Text = _currentProduct.Price.ToString();
            DiscountTB.Text = _currentProduct.Discount.ToString();
            countTB.Text = _currentProduct.Amount.ToString();

            measurementСВ.SelectedValue =
                _currentProduct.ID_Measurement;

            manufacturerCB.SelectedValue =
                _currentProduct.ID_Manufacturer;

            supplierсCB.SelectedValue =
                _currentProduct.ID_Supplier;

            CategoryCB.SelectedValue =
                _currentProduct.ID_Category;

            ProductImage.Source =
                LoadImage(_currentProduct.FullImagePath);
        }

        private void EditProductButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string article = ArticleTB.Text.Trim();
            string productName = NameTB.Text.Trim();
            string description = DescriptionTB.Text.Trim();
            string priceText = PriceTB.Text.Trim();
            string discountText = DiscountTB.Text.Trim();
            string amountText = countTB.Text.Trim();

            if (string.IsNullOrEmpty(article))
            {
                ShowError("Введите артикул товара.");
                return;
            }

            if (string.IsNullOrEmpty(productName))
            {
                ShowError("Введите название товара.");
                return;
            }

            if (string.IsNullOrEmpty(description))
            {
                ShowError("Введите описание товара.");
                return;
            }

            decimal price;

            if (!decimal.TryParse(priceText, out price))
            {
                ShowError("Цена должна быть числом.");
                return;
            }

            if (price < 0)
            {
                ShowError("Цена товара не может быть отрицательной.");
                return;
            }

            decimal discount = 0;

            if (!string.IsNullOrEmpty(discountText)
                && !decimal.TryParse(discountText, out discount))
            {
                ShowError("Скидка должна быть числом.");
                return;
            }

            if (discount < 0 || discount > 100)
            {
                ShowError(
                    "Скидка должна находиться в диапазоне от 0 до 100.");

                return;
            }

            int amount;

            if (!int.TryParse(amountText, out amount))
            {
                ShowError("Количество должно быть целым числом.");
                return;
            }

            if (amount < 0)
            {
                ShowError(
                    "Количество товара не может быть отрицательным.");

                return;
            }

            if (measurementСВ.SelectedValue == null
                || manufacturerCB.SelectedValue == null
                || supplierсCB.SelectedValue == null
                || CategoryCB.SelectedValue == null)
            {
                ShowError(
                    "Выберите значения во всех выпадающих списках.");

                return;
            }

            bool articleExists = DBconection.BicycleEntities.Product
                .Any(x =>
                    x.Article == article
                    && x.ID != _currentProduct.ID);

            if (articleExists)
            {
                ShowError(
                    "Другой товар с таким артикулом уже существует.");

                return;
            }

            try
            {
                string oldImageName = _currentProduct.Photo;

                _currentProduct.Article = article;
                _currentProduct.Name = productName;
                _currentProduct.Description = description;
                _currentProduct.Price = price;
                _currentProduct.Discount = discount;
                _currentProduct.Amount = amount;

                _currentProduct.ID_Measurement =
                    Convert.ToInt32(measurementСВ.SelectedValue);

                _currentProduct.ID_Manufacturer =
                    Convert.ToInt32(manufacturerCB.SelectedValue);

                _currentProduct.ID_Supplier =
                    Convert.ToInt32(supplierсCB.SelectedValue);

                _currentProduct.ID_Category =
                    Convert.ToInt32(CategoryCB.SelectedValue);

                if (!string.IsNullOrEmpty(_newImage))
                {
                    string newImageName = SaveImage(_newImage);

                    _currentProduct.Photo = newImageName;

                    DBconection.BicycleEntities.SaveChanges();

                    DeleteOldImage(oldImageName);
                }
                else
                {
                    DBconection.BicycleEntities.SaveChanges();
                }

                MessageBox.Show(
                    "Изменения успешно сохранены.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить изменения.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ImageButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            BitmapFrame imageInfo =
                BitmapFrame.Create(new Uri(dialog.FileName));

            if (imageInfo.PixelWidth > 300
                || imageInfo.PixelHeight > 200)
            {
                MessageBox.Show(
                    "Размер изображения не должен превышать 300×200 пикселей.",
                    "Недопустимый размер изображения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            _newImage = dialog.FileName;

            ProductImage.Source = LoadImage(_newImage);
        }

        private BitmapImage LoadImage(string file)
        {
            var image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(
                file,
                UriKind.RelativeOrAbsolute);

            image.EndInit();

            return image;
        }

        private string SaveImage(string file)
        {
            string imageName = 
                Guid.NewGuid() + Path.GetExtension(file);

            string productsFolder = GetProductsFolder();

            Directory.CreateDirectory(productsFolder);

            string fullPath =
                Path.Combine(productsFolder, imageName);

            File.Copy(file, fullPath, true);

            return imageName;
        }

        private void DeleteOldImage(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName)
                || imageName == "picture.png")
            {
                return;
            }

            string imagePath = Path.Combine(
                GetProductsFolder(),
                imageName);

            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }

        private string GetProductsFolder()
        {
            return Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    @"..\..\Materials\Products"));
        }

        private void cancelProductButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "Ошибка заполнения",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
