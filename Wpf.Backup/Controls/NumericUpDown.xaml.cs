using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf.Backup.Controls
{
    /// <summary>
    /// Interaction logic for NumericUpDown.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        public int NumValue
        {
            get { return (int)GetValue(NumValueProperty); }
            set { SetValue(NumValueProperty, value); }
        }
        
        public static readonly DependencyProperty NumValueProperty =
            DependencyProperty.Register("NumValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0, NumValueChanged));
        
        public int MinValue
        {
            get { return (int)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }
        
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(int.MinValue));
        
        public int MaxValue
        {
            get { return (int)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
        
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericUpDown), new PropertyMetadata(int.MaxValue));
        
        private static void NumValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as NumericUpDown;
            control.txtNum.Text = e.NewValue.ToString();
        }

        public NumericUpDown()
        {
            InitializeComponent();
            txtNum.Text = NumValue.ToString();
        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            if (NumValue < MaxValue)
                NumValue++;
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (NumValue > MinValue)
                NumValue--;
        }

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNum == null)
            {
                return;
            }
            int numValue;
            if (!int.TryParse(txtNum.Text, out numValue))
            {
                NumValue = numValue;
                txtNum.Text = NumValue.ToString();
            }
        }
    }

}
