﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TumblThree.Applications.ViewModels;
using TumblThree.Applications.Views;
using TumblThree.Domain.Models;

namespace TumblThree.Presentation.Views
{
    /// <summary>
    /// Interaction logic for ManagerView.xaml
    /// </summary>
    [Export(typeof(IManagerView))]
    public partial class ManagerView : IManagerView
    {
        private readonly Lazy<ManagerViewModel> viewModel;

        public ManagerView()
        {
            InitializeComponent();
            this.viewModel = new Lazy<ManagerViewModel>(() => ViewHelper.GetViewModel<ManagerViewModel>(this));

            Loaded += LoadedHandler;
            blogFilesGrid.Sorting += BlogFilesGridSorting;
        }

        private ManagerViewModel ViewModel { get { return viewModel.Value; } }

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            FocusBlogFilesGrid();
        }

        private void BlogFilesGridSorting(object sender, DataGridSortingEventArgs e)
        {
            var collectionView = CollectionViewSource.GetDefaultView(blogFilesGrid.ItemsSource) as ListCollectionView;
            if (collectionView == null)
            {
                return;
            }
            var newDirection = e.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }

        private void FocusBlogFilesGrid()
        {
            blogFilesGrid.Focus();
            blogFilesGrid.CurrentCell = new DataGridCellInfo(blogFilesGrid.SelectedItem, blogFilesGrid.Columns[0]);
        }

        private void DataGridRowContextMenuOpening(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).ContextMenu.DataContext = ViewModel;
        }

        private void DataGridRowMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.CrawlerService.EnqueueSelectedCommand.Execute(null);
        }

        private void DataGridRowMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var draggedItem = (DataGridRow)sender;
                var items = blogFilesGrid.ItemsSource.Cast<Blog>().ToList();
                var selectedItems = blogFilesGrid.SelectedItems.Cast<Blog>().OrderBy(x => items.IndexOf(x)).ToArray();
                DragDrop.DoDragDrop(draggedItem, selectedItems.ToArray(), DragDropEffects.Copy);
            }
        }

        public Dictionary<object, Tuple<int, double>> DataGridColumnRestore
        {
            get
            {
                Dictionary<object, Tuple<int, double>> columnWidths = new Dictionary<object, Tuple<int,double>>();
                foreach (DataGridColumn column in blogFilesGrid.Columns)
                {
                    columnWidths.Add(column.Header, Tuple.Create(column.DisplayIndex, column.Width.Value));
                }
                return columnWidths;
            }
            set
            {
                foreach (DataGridColumn column in blogFilesGrid.Columns)
                {
                    Tuple<int, double> entry;
                    value.TryGetValue(column.Header, out entry);
                    column.DisplayIndex = entry.Item1;
                    column.Width = new DataGridLength(entry.Item2, DataGridLengthUnitType.Pixel);
                }
            }
        }
    }
}
