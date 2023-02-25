using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SilverCraft.AvaloniaUtils
{
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
            this.DoAfterInitTasks(true);

        }
        public MessageBox(string Title, string Message):this()
        {
            DataContext = new { Title , Message};
        }

        private void CloseButtonClick(object? o, RoutedEventArgs e)
        {
            Close();
        }
    }
}
