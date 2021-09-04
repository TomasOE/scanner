﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Uwp.Helpers;
using Scanner.Models;
using Serilog;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Scanners;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using static Utilities;

namespace Scanner.Services
{
    class ScanService : ObservableObject, IScanService
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // DECLARATIONS /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private readonly IAppCenterService AppCenterService = Ioc.Default.GetService<IAppCenterService>();

        private bool _IsScanInProgress;
        public bool IsScanInProgress
        {
            get => _IsScanInProgress;
            set => SetProperty(ref _IsScanInProgress, value);
        }

        public event EventHandler ScanStarted;
        public event EventHandler ScanCompleted;

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS / FACTORIES /////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public ScanService()
        {

        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public async Task<BitmapImage> GetPreviewAsync(DiscoveredScanner scanner, ImageScannerScanSource config)
        {
            AppCenterService?.TrackEvent(AppCenterEvent.Preview,
                new Dictionary<string, string> 
                {
                        { "Source", config.ToString() },
                });

            return await scanner.GetPreviewAsync(config);
        }

        public void TryCancelScan()
        {
            throw new NotImplementedException();
        }
    }
}
