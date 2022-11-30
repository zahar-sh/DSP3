using DSP3.Model;
using DSP3.ViewModel;
using System.Windows;

namespace DSP3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SignalTypeChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (harmonicRadioButton.IsChecked.GetValueOrDefault())
                {
                    viewModel.SignalType = SignalType.Harmonic;
                }
                else if (polyharmonicRadioButton.IsChecked.GetValueOrDefault())
                {
                    viewModel.SignalType = SignalType.Polyharmonic;
                }
            }
        }

        private void FourierTransformationTypeChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (simpleFtRadioButton.IsChecked.GetValueOrDefault())
                {
                    viewModel.FourierTransformationType = FourierTransformationType.Simple;
                }
                else if (fastFtRadioButton.IsChecked.GetValueOrDefault())
                {
                    viewModel.FourierTransformationType = FourierTransformationType.Fast;
                }
            }
        }
    }
}
