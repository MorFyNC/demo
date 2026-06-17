using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using test.DatabaseContext;
using test.Windows.Order;

namespace test.pages
{
    /// <summary>
    /// Логика взаимодействия для Orderinfo.xaml
    /// </summary>
    public partial class Orderinfo : Page
    {
        public Orderinfo()
        {
            InitializeComponent();

            ConfigureAccess();
            RefreshData();
        }

        private void ConfigureAccess()
        {
            if (!IsAdministrator())
            {
                ControlPanel.Visibility = Visibility.Collapsed;
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

            var selectedOrder =
                MainListView.SelectedItem as DatabaseContext.Order;

            if (selectedOrder == null)
            {
                return;
            }

            var editOrderWindow =
                new EditOrder(selectedOrder);

            editOrderWindow.ShowDialog();

            RefreshData();
        }

        private void AddButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsAdministrator())
            {
                MessageBox.Show(
                    "Добавлять заказы может только администратор.",
                    "Доступ запрещён",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var addWindow = new AddOrder();
            addWindow.ShowDialog();

            RefreshData();
        }

        private void DeleteButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsAdministrator())
            {
                MessageBox.Show(
                    "Удалять заказы может только администратор.",
                    "Доступ запрещён",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var selectedOrder =
                MainListView.SelectedItem as DatabaseContext.Order;

            if (selectedOrder == null)
            {
                MessageBox.Show(
                    "Выберите заказ для удаления.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Вы точно хотите удалить выбранный заказ?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var orderProducts =
                    DBconection.BicycleEntities.Product_Order
                        .Where(x =>
                            x.ID_Order == selectedOrder.ID)
                        .ToList();

                foreach (var orderProduct in orderProducts)
                {
                    DBconection.BicycleEntities.Product_Order
                        .Remove(orderProduct);
                }

                DBconection.BicycleEntities.Order
                    .Remove(selectedOrder);

                DBconection.BicycleEntities.SaveChanges();

                MessageBox.Show(
                    "Заказ успешно удалён.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                RefreshData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить заказ.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool IsAdministrator()
        {
            return CurrentUser.currentUser != null
                && CurrentUser.currentUser.ID_Role == 1;
        }

        private void RefreshData()
        {
            MainListView.ItemsSource =
                DBconection.BicycleEntities.Order
                    .Include("Status")
                    .Include("Address.Streets.City")
                    .Include("Product_Order.Product")
                    .OrderBy(x => x.ID)
                    .ToList();
        }
    }
}