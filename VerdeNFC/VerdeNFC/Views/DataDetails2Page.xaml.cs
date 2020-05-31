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
    public partial class DataDetails2Page : ContentPage
    {
        DataDetails2ViewModel viewModel;

        public DataDetails2Page(DataDetails2ViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = this.viewModel = viewModel;
        }

        public DataDetails2Page()
        {
            InitializeComponent();

            viewModel = new DataDetails2ViewModel();
            BindingContext = viewModel;
        }
    }
}