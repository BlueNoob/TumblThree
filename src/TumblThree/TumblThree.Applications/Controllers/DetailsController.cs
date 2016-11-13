﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using TumblThree.Applications.Services;
using TumblThree.Applications.ViewModels;
using TumblThree.Domain.Models;
using TumblThree.Domain.Queue;

namespace TumblThree.Applications.Controllers
{
    [Export, Export(typeof(IDetailsService))]
    internal class DetailsController : IDetailsService
    {
        private readonly IShellService shellService;
        private readonly ISelectionService selectionService;
        private readonly Lazy<DetailsViewModel> detailsViewModel;

        [ImportingConstructor]
        public DetailsController(IShellService shellService, ISelectionService selectionService, Lazy<DetailsViewModel> detailsViewModel)
        {
            this.shellService = shellService;
            this.selectionService = selectionService;
            this.detailsViewModel = detailsViewModel;
        }

        public QueueManager QueueManager { get; set; }

        private DetailsViewModel DetailsViewModel { get { return detailsViewModel.Value; } }


        public void Initialize()
        {
            //QueueManager.PropertyChanged += QueueManagerPropertyChanged;
            ((INotifyCollectionChanged)selectionService.SelectedBlogFiles).CollectionChanged += SelectedBlogFilesCollectionChanged;
            shellService.DetailsView = DetailsViewModel.View;
        }

        public void SelectBlogFiles(IReadOnlyList<Blog> blogFiles)
        {
            if (blogFiles.Count() <= 1)
            {
                DetailsViewModel.Count = 1;
                DetailsViewModel.BlogFile = blogFiles.FirstOrDefault();
            }
            else
            {
                DetailsViewModel.Count = 2;
                DetailsViewModel.BlogFile = CreateFromMultiple(blogFiles);
            }
        }

        public Blog CreateFromMultiple(IEnumerable<Blog> blogFiles)
        {
            if (blogFiles.Count() < 1) { throw new ArgumentException("The collection must have at least one item.", "blogFiles"); }

            var localBlogFiles = blogFiles.Cast<TumblrBlog>().ToArray();
            return new TumblrBlog()
            {
                TotalCount = (uint)localBlogFiles.Sum(blogs => blogs.TotalCount),
                Texts = (uint)localBlogFiles.Sum(blogs => blogs.Texts),
                Quotes = (uint)localBlogFiles.Sum(blogs => blogs.Quotes),
                Photos = (uint)localBlogFiles.Sum(blogs => blogs.Photos),
                NumberOfLinks = (uint)localBlogFiles.Sum(blogs => blogs.NumberOfLinks),
                Conversations = (uint)localBlogFiles.Sum(blogs => blogs.Conversations),
                Videos = (uint)localBlogFiles.Sum(blogs => blogs.Videos),
                Audios = (uint)localBlogFiles.Sum(blogs => blogs.Audios)
            };
        }

        private void SelectedBlogFilesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SelectBlogFiles(selectionService.SelectedBlogFiles.Cast<Blog>().ToArray());
        }
    }
}
