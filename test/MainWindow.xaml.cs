using System;
using System.Linq;
using System.Windows;
using test.DatabaseContext;
using test.Windows;

namespace test
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AuthBtn_Click(object sender, RoutedEventArgs e)
        {
            errorText.Text = "";

            string login = LoginTB.Text.Trim();
            string password = passwordPB.Password.Trim();

            if (string.IsNullOrEmpty(login))
            {
                ShowError("Введите логин.");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Введите пароль.");
                return;
            }

            try
            {
                Users user = DBconection.BicycleEntities.Users
                    .FirstOrDefault(x => x.Login == login);

                if (user == null || user.Password != password)
                {
                    ShowError("Неверный логин или пароль.");
                    return;
                }

                CurrentUser.currentUser = user;

                var infoWindow = new infoWindow();
                infoWindow.Show();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось выполнить авторизацию.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void GuesBtn_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.currentUser = new Users
            {
                Login = "Гость",
                FullName = "Гость",
                ID_Role = 0
            };

            var infoWindow = new infoWindow();
            infoWindow.Show();

            Close();
        }

        private void ShowError(string message)
        {
            errorText.Text = message;
        }
    }
}