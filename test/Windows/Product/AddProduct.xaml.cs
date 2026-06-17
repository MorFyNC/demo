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
    /// Логика взаимодействия для AddProduct.xaml
    /// </summary>
    public partial class AddProduct : Window
    {
        private string _newImage;

        public AddProduct()
        {
            InitializeComponent();

            FillComboBoxes();
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

            measurementСВ.SelectedIndex = 0;
            manufacturerCB.SelectedIndex = 0;
            supplierсCB.SelectedIndex = 0;
            CategoryCB.SelectedIndex = 0;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string article = ArticleTB.Text.Trim();
            string productName = NameTB.Text.Trim();
            string description = DescriptionTB.Text.Trim();
            string priceText = PriceTB.Text.Trim();
            string discountText = DiscountTB.Text.Trim();
            string amountText = AmountTB.Text.Trim();

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
                ShowError("Скидка должна находиться в диапазоне от 0 до 100.");
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
                ShowError("Количество товара не может быть отрицательным.");
                return;
            }

            if (measurementСВ.SelectedValue == null
                || manufacturerCB.SelectedValue == null
                || supplierсCB.SelectedValue == null
                || CategoryCB.SelectedValue == null)
            {
                ShowError("Выберите значения во всех выпадающих списках.");
                return;
            }

            bool articleExists = DBconection.BicycleEntities.Product
                .Any(x => x.Article == article);

            if (articleExists)
            {
                ShowError("Товар с таким артикулом уже существует.");
                return;
            }

            DatabaseContext.Product newProduct = null;
            string imageName = null;

            try
            {
                if (!string.IsNullOrEmpty(_newImage))
                {
                    imageName = SaveImage(_newImage);
                }

                newProduct = new DatabaseContext.Product
                {
                    Article = article,
                    Name = productName,
                    Description = description,
                    Price = price,
                    Discount = discount,
                    Amount = amount,
                    Photo = imageName,

                    ID_Measurement =
                        Convert.ToInt32(measurementСВ.SelectedValue),

                    ID_Manufacturer =
                        Convert.ToInt32(manufacturerCB.SelectedValue),

                    ID_Supplier =
                        Convert.ToInt32(supplierсCB.SelectedValue),

                    ID_Category =
                        Convert.ToInt32(CategoryCB.SelectedValue)
                };

                DBconection.BicycleEntities.Product.Add(newProduct);
                DBconection.BicycleEntities.SaveChanges();

                MessageBox.Show(
                    "Товар успешно добавлен.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            catch (Exception ex)
            {
                if (newProduct != null)
                {
                    try
                    {
                        DBconection.BicycleEntities.Product.Remove(newProduct);
                    }
                    catch
                    {
                        // Не перекрываем основную ошибку сохранения.
                    }
                }

                if (!string.IsNullOrWhiteSpace(imageName))
                {
                    DeleteSavedImage(imageName);
                }

                MessageBox.Show(
                    "Не удалось добавить товар.\n\n"
                    + ex.GetBaseException().Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
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

            var image = new BitmapImage();

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(_newImage);
            image.EndInit();

            ProductImage.Source = image;
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

        private void DeleteSavedImage(string imageName)
        {
            string imagePath =
                Path.Combine(GetProductsFolder(), imageName);

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