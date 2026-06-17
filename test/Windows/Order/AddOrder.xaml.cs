using System;
using System.Linq;
using System.Windows;
using test.DatabaseContext;

namespace test.Windows.Order
{
    /// <summary>
    /// Логика взаимодействия для AddOrder.xaml
    /// </summary>
    public partial class AddOrder : Window
    {
        public AddOrder()
        {
            InitializeComponent();

            FillComboBoxes();

            OrderDateDP.SelectedDate = DateTime.Today;
            ShipmentOrderDP.SelectedDate = DateTime.Today;
        }

        private void FillComboBoxes()
        {
            ArticleCB.ItemsSource = DBconection.BicycleEntities.Product
                .OrderBy(x => x.Article)
                .ToList();

            ArticleCB.DisplayMemberPath = "Article";
            ArticleCB.SelectedValuePath = "ID";

            StatusCB.ItemsSource = DBconection.BicycleEntities.Status
                .OrderBy(x => x.ID)
                .ToList();

            StatusCB.DisplayMemberPath = "Name";
            StatusCB.SelectedValuePath = "ID";

            AddressCB.ItemsSource = DBconection.BicycleEntities.Address
                .OrderBy(x => x.ID)
                .ToList();

            AddressCB.SelectedValuePath = "ID";

            ArticleCB.SelectedIndex = 0;
            StatusCB.SelectedIndex = 0;
            AddressCB.SelectedIndex = 0;
        }

        private void SubmitButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ArticleCB.SelectedItem == null)
            {
                ShowError("Выберите товар.");
                return;
            }

            if (StatusCB.SelectedValue == null)
            {
                ShowError("Выберите статус заказа.");
                return;
            }

            if (AddressCB.SelectedValue == null)
            {
                ShowError("Выберите адрес пункта выдачи.");
                return;
            }

            if (!OrderDateDP.SelectedDate.HasValue)
            {
                ShowError("Выберите дату заказа.");
                return;
            }

            if (!ShipmentOrderDP.SelectedDate.HasValue)
            {
                ShowError("Выберите дату выдачи.");
                return;
            }

            DateTime orderDate = OrderDateDP.SelectedDate.Value;
            DateTime deliveryDate = ShipmentOrderDP.SelectedDate.Value;

            if (deliveryDate < orderDate)
            {
                ShowError(
                    "Дата выдачи не может быть раньше даты заказа.");

                return;
            }

            if (CurrentUser.currentUser == null)
            {
                ShowError("Не удалось определить текущего пользователя.");
                return;
            }

            try
            {
                var selectedProduct =
                    ArticleCB.SelectedItem as DatabaseContext.Product;

                int nextCode = 1;

                if (DBconection.BicycleEntities.Order.Any())
                {
                    nextCode =
                        DBconection.BicycleEntities.Order
                            .Max(x => x.Code) + 1;
                }

                var newOrder = new DatabaseContext.Order
                {
                    OrderDate = orderDate,
                    DeliverDate = deliveryDate,

                    ID_Address =
                        Convert.ToInt32(AddressCB.SelectedValue),

                    ID_Status =
                        Convert.ToInt32(StatusCB.SelectedValue),

                    ID_User = CurrentUser.currentUser.ID,
                    Code = nextCode
                };

                var newOrderProduct = new Product_Order
                {
                    Order = newOrder,
                    ID_Product = selectedProduct.ID,
                    Amount = 1
                };

                DBconection.BicycleEntities.Order.Add(newOrder);

                DBconection.BicycleEntities.Product_Order
                    .Add(newOrderProduct);

                DBconection.BicycleEntities.SaveChanges();

                MessageBox.Show(
                    "Заказ успешно добавлен.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось добавить заказ.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(
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