﻿using System;
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

namespace DidacticalEnigma
{
    /// <summary>
    /// Interaction logic for KanaBoard.xaml
    /// </summary>
    public partial class KanaBoard : UserControl
    {
        public KanaBoard()
        {
            InitializeComponent();
        }

        public ICommand KeyClickCommand
        {
            get { return (ICommand)GetValue(KeyClickCommandProperty); }
            set { SetValue(KeyClickCommandProperty, value); }
        }

        /// <summary>Identifies the <see cref="KeyClickCommand"/> dependency property.</summary>
        public static readonly DependencyProperty KeyClickCommandProperty =
            DependencyProperty.Register("KeyClickCommand", typeof(ICommand), typeof(KanaBoard), new PropertyMetadata(null));


    }
}