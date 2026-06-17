using System.Windows;
using test.DatabaseContext;
using test.pages;

namespace test.Windows
{
    /// <summary>
    /// Логика взаимодействия для infoWindow.xaml
    /// </summary>
    public partial class infoWindow : Window
    {
        public infoWindow()
        {
            InitializeComponent();

            var currentUser = CurrentUser.currentUser;

            FullNameTextBlock.Text =
                string.IsNullOrWhiteSpace(currentUser.FullName)
                    ? currentUser.Login
                    : currentUser.FullName;

            if (currentUser.ID_Role == 3 || currentUser.ID_Role == 0)
            {
                OrderButton.Visibility = Visibility.Collapsed;
            }

            OpenProductPage();
        }

        private void ProductButton_Click(object sender, RoutedEventArgs e)
        {
            OpenProductPage();
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new Orderinfo();
            Title = "МебельОрг - Заказы";
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.currentUser = null;

            var mainWindow = new MainWindow();
            mainWindow.Show();

            Close();
        }

        private void OpenProductPage()
        {
            MainFrame.Content = new ProductInfo();
            Title = "МебельОрг - Товары";
        }
    }
}