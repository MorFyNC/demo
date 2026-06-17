using System;
using System.Linq;
using System.Windows;
using test.DatabaseContext;

namespace test.Windows.Order
{
    /// <summary>
    /// Логика взаимодействия для EditOrder.xaml
    /// </summary>
    public partial class EditOrder : Window
    {
        private readonly DatabaseContext.Order _currentOrder;

        private Product_Order _orderProduct;

        public EditOrder(DatabaseContext.Order order)
        {
            InitializeComponent();

            _currentOrder = order;

            FillComboBoxes();
            FillOrderData();
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
        }

        private void FillOrderData()
        {
            _orderProduct = DBconection.BicycleEntities.Product_Order
                .FirstOrDefault(x => x.ID_Order == _currentOrder.ID);

            if (_orderProduct != null)
            {
                ArticleCB.SelectedValue = _orderProduct.ID_Product;
            }
            else
            {
                ArticleCB.SelectedIndex = 0;
            }

            StatusCB.SelectedValue = _currentOrder.ID_Status;
            AddressCB.SelectedValue = _currentOrder.ID_Address;

            OrderDateDP.SelectedDate = _currentOrder.OrderDate;
            ShipmentOrderDP.SelectedDate = _currentOrder.DeliverDate;
        }

        private void editButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (ArticleCB.SelectedValue == null)
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

            try
            {
                int productId =
                    Convert.ToInt32(ArticleCB.SelectedValue);

                _currentOrder.OrderDate = orderDate;
                _currentOrder.DeliverDate = deliveryDate;

                _currentOrder.ID_Status =
                    Convert.ToInt32(StatusCB.SelectedValue);

                _currentOrder.ID_Address =
                    Convert.ToInt32(AddressCB.SelectedValue);

                if (_orderProduct == null)
                {
                    _orderProduct = new Product_Order
                    {
                        ID_Order = _currentOrder.ID,
                        ID_Product = productId,
                        Amount = 1
                    };

                    DBconection.BicycleEntities.Product_Order
                        .Add(_orderProduct);
                }
                else
                {
                    _orderProduct.ID_Product = productId;
                }

                DBconection.BicycleEntities.SaveChanges();

                MessageBox.Show(
                    "Заказ успешно изменён.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось изменить заказ.\n" + ex.Message,
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