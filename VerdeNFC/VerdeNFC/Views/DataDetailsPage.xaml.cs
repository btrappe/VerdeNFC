using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

//using VerdeNFC.Models;
using VerdeNFC.ViewModels;

namespace VerdeNFC.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class DataDetailsPage : ContentPage
    {
        DataDetailsViewModel viewModel;

        public DataDetailsPage(DataDetailsViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;
        }

        public DataDetailsPage()
        {
            InitializeComponent();

            viewModel = new DataDetailsViewModel();
            BindingContext = viewModel;
        }
    }
}