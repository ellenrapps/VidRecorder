using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace vid2_25022021846pm
{
    public sealed partial class FaqPage : Page
    {
        public FaqPage()
        {
            this.InitializeComponent();
        }

        #region Buttons
        private void ButtonClick2(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name == "HamburgerButton2")
            {
                AlterHamburger2();
            }

            else if ((sender as Button).Name == "HomeButton")
            {
                AlterHome();
            }

            #endregion Buttons

            #region AlterFaq
            void AlterHamburger2()
            {
                MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
            }

            #endregion AlterFaq

            #region AlterHome
            void AlterHome()
            {
                this.Frame.Navigate(typeof(MainPage));
            }

            #endregion AlterHome
        }

    }
}
