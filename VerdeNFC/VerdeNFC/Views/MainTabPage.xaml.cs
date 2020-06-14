using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using VerdeNFC.ViewModels;
using VerdeNFC.Models;

namespace VerdeNFC.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainTabPage : ContentPage
    {
        // TransactionViewModel ViewModel { get => BindingContext as TransactionViewModel; set => BindingContext = value; }

        public MainTabPage()
        {
            InitializeComponent();
        }

        private void Picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((MainTabViewModel)BindingContext).RoastProfileSelChanged();
        }
    }
}