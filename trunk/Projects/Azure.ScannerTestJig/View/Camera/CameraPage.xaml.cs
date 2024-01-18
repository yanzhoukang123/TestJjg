﻿using Azure.Image.Processing;
using Azure.ScannerTestJig.ViewModule;
using Azure.ScannerTestJig.ViewModule.Camera;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Azure.ScannerTestJig.View.Camera
{
    /// <summary>
    /// CameraPage.xaml 的交互逻辑
    /// </summary>
    public partial class CameraPage : Window
    {
        private Toupcam.DeviceV2 dev_;
        private Toupcam cam_ = null;
        private WriteableBitmap bmp_ = null;
        private WriteableBitmap prvbmp_ = null;
        private bool started_ = false;
        private DispatcherTimer timer_ = null;
        private uint count_ = 0;
        uint prebitsperpixel = 8;



        private void ImageLibSaveTif16Mono(IntPtr data, int width, int height, ushort bitsperpixel)
        {
            ImageLib.BITMAPINFOHEADER h = new ImageLib.BITMAPINFOHEADER();
            h.biSize = (uint)Marshal.SizeOf(typeof(ImageLib.BITMAPINFOHEADER));
            h.biWidth = width;
            h.biHeight = height;
            h.biPlanes = 1;
            h.biBitCount = bitsperpixel;

            ImageLib.XIMAGEINFO info = new ImageLib.XIMAGEINFO();
            //info.cCamera = dev_.model.name; /* just to demo exif */
            //info.cSN = cam_.SerialNumber;
            IntPtr dib = Marshal.AllocCoTaskMem((int)h.biSize + ImageLib.TDIBWIDTHBYTES(width * 16) * height);
            Toupcam.memcpy(Toupcam.IncIntPtr(dib, (int)h.biSize), data, new IntPtr(ImageLib.TDIBWIDTHBYTES(width * 16) * height));
            Marshal.StructureToPtr(h, dib, false);
            ImageLib.Save("democs.tif", dib, ref info);
            Marshal.FreeCoTaskMem(dib);
        }
        private IntPtr _ImageLibSaveTif16Mono(IntPtr data, int width, int height, ushort bitsperpixel)
        {
            ImageLib.BITMAPINFOHEADER h = new ImageLib.BITMAPINFOHEADER();
            h.biSize = (uint)Marshal.SizeOf(typeof(ImageLib.BITMAPINFOHEADER));
            h.biWidth = width;
            h.biHeight = height;
            h.biPlanes = 1;
            h.biBitCount = bitsperpixel;

            ImageLib.XIMAGEINFO info = new ImageLib.XIMAGEINFO();
            info.cCamera = dev_.model.name; /* just to demo exif */
            info.cSN = cam_.SerialNumber;
            IntPtr dib = Marshal.AllocCoTaskMem((int)h.biSize + ImageLib.TDIBWIDTHBYTES(width * 16) * height);
            Toupcam.memcpy(Toupcam.IncIntPtr(dib, (int)h.biSize), data, new IntPtr(ImageLib.TDIBWIDTHBYTES(width * 16) * height));
            Marshal.StructureToPtr(h, dib, false);
            return dib;
            //ImageLib.Save("democs.tif", dib, ref info);
            //Marshal.FreeCoTaskMem(dib);
        }

        public CameraPage()
        {
            InitializeComponent();
            snap_.IsEnabled = false;
            combo_.IsEnabled = false;
            auto_exposure_.IsEnabled = false;
            white_balance_once_.IsEnabled = false;
            slider_expotime_.IsEnabled = false;
            slider_temp_.IsEnabled = false;
            slider_tint_.IsEnabled = false;
            Singe_.IsEnabled = false;
            label_expotime_.IsEnabled = false;
            slider_temp_.Minimum = Toupcam.TEMP_MIN;
            slider_temp_.Maximum = Toupcam.TEMP_MAX;
            slider_tint_.Minimum = Toupcam.TINT_MIN;
            slider_tint_.Maximum = Toupcam.TINT_MAX;

            Closing += (sender, e) =>
            {
                cam_?.Close();
                cam_ = null;
                Workspace.This.Owner.Show();
                Workspace.This.CameraViewModule.CP = null;

            };
            this.Loaded += new RoutedEventHandler(IVContorl_Loaded);
        }
        void IVContorl_Loaded(object sender, EventArgs e)
        {

            CameraViewModule vm = Workspace.This.CameraViewModule;
            if (vm != null)
            {
                this.DataContext = vm;
            }
        }
        private void OnEventError()
        {
            cam_.Close();
            cam_ = null;
            MessageBox.Show("Generic error.");
        }

        private void OnEventDisconnected()
        {
            cam_.Close();
            cam_ = null;
            MessageBox.Show("Camera disconnect.");
        }

        private void OnEventExposure()
        {
            uint nTime = 0;
            if (cam_.get_ExpoTime(out nTime))
            {
                slider_expotime_.Value = (int)nTime;
                label_expotime_.Text = nTime.ToString();
            }
        }

        private unsafe void OnEventImage()
        {
            if (bmp_ != null)
            {
                Toupcam.FrameInfoV3 info = new Toupcam.FrameInfoV3();
                bool bOK = false;
                try
                {
                    bmp_.Lock();
                    try
                    {
                        Console.WriteLine(DateTime.Now);
                        bOK = cam_.PullImageV3(prvbmp_.BackBuffer, 0, 16, 0, out info); // check the return value

                        int nWidth = (int)info.width;
                        int nHeight = (int)info.height;
                        int nWidthBytesMono = ImageLib.TDIBWIDTHBYTES(8 * nWidth);
                        int nWidthBytesRGB = ImageLib.TDIBWIDTHBYTES(24 * nWidth);

                        uint nFourCC = 0, bitsperpixel = 0;
                        cam_.get_RawFormat(out nFourCC, out bitsperpixel);
                        int nshift = (int)(bitsperpixel - 8);

                        if (bOK)
                        {
                            ushort* pSrc = (ushort*)prvbmp_.BackBuffer.ToPointer();
                            byte* pDst = (byte*)bmp_.BackBuffer.ToPointer();
                            for (int i = 0; i < nHeight; ++i)
                            {
                                for (int j = 0; j < nWidth; ++j)
                                {
                                    int t = i * nWidthBytesMono + j;
                                    int p = i * nWidthBytesRGB + j * 3;
                                    pDst[p + 2] = (byte)(pSrc[t] >> nshift);
                                    pDst[p + 1] = (byte)(pSrc[t] >> nshift);
                                    pDst[p + 0] = (byte)(pSrc[t] >> nshift);
                                }
                            }
                        }

                        bmp_.AddDirtyRect(new Int32Rect(0, 0, bmp_.PixelWidth, bmp_.PixelHeight));
                    }
                    finally
                    {
                        bmp_.Unlock();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                if (bOK)
                    image_.Source = bmp_;
            }
        }

        private void SaveToFile(BitmapSource bmp)
        {
            using (FileStream fileStream = new FileStream(string.Format("demowpf_{0}.jpg", ++count_), FileMode.Create))
            {
                if (fileStream != null)
                {
                    BitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bmp));
                    encoder.Save(fileStream);
                }
            }
        }

        private void OnEventStillImage()
        {
            Toupcam.FrameInfoV3 info = new Toupcam.FrameInfoV3();
            if (cam_.PullImageV3(IntPtr.Zero, 1, 16, 0, out info))   /* peek the width and height */
            {
                WriteableBitmap bmp = new WriteableBitmap((int)info.width, (int)info.height, 0, 0, PixelFormats.Gray16, null);
                bool bOK = false;
                try
                {
                    bmp.Lock();
                    try
                    {
                        bOK = cam_.PullImageV3(bmp.BackBuffer, 1, 16, bmp.BackBufferStride, out info); // check the return value
                    }
                    finally
                    {
                        bmp.Unlock();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                if (bOK)
                    SaveToFile(bmp);
            }
        }

        private void DelegateOnEventCallback(Toupcam.eEVENT evt)
        {
            /* this is called by internal thread of toupcam.dll which is NOT the same of UI thread.
             * So we use BeginInvoke
             */
            Dispatcher.BeginInvoke((Action)(() =>
            {
                /* this run in the UI thread */
                if (cam_ != null)
                {
                    switch (evt)
                    {
                        case Toupcam.eEVENT.EVENT_ERROR:
                            OnEventError();
                            break;
                        case Toupcam.eEVENT.EVENT_DISCONNECTED:
                            OnEventDisconnected();
                            break;
                        case Toupcam.eEVENT.EVENT_EXPOSURE:
                            OnEventExposure();
                            break;
                        case Toupcam.eEVENT.EVENT_IMAGE:
                            OnEventImage();
                            break;
                        case Toupcam.eEVENT.EVENT_STILLIMAGE:
                            OnEventStillImage();
                            break;
                        case Toupcam.eEVENT.EVENT_TEMPTINT:
                            OnEventTempTint();
                            break;
                        default:
                            break;
                    }
                }
            }));
        }

        private void OnEventTempTint()
        {
            int nTemp = 0, nTint = 0;
            if (cam_.get_TempTint(out nTemp, out nTint))
            {
                label_temp_.Content = nTemp.ToString();
                label_tint_.Content = nTint.ToString();
                slider_temp_.Value = nTemp;
                slider_tint_.Value = nTint;
            }
        }

        private void startDevice(string camId)
        {
            cam_ = Toupcam.Open(camId);
            if (cam_ != null)
            {
                auto_exposure_.IsEnabled = true;
                combo_.IsEnabled = true;
                //snap_.IsEnabled = true;
                auto_exposure_.IsEnabled = true;
                Singe_.IsEnabled = true;
                label_expotime_.IsEnabled = true;
                InitExpoTime();
                if (cam_.MonoMode)
                {
                    slider_temp_.IsEnabled = false;
                    slider_tint_.IsEnabled = false;
                    white_balance_once_.IsEnabled = false;
                }
                else
                {
                    slider_temp_.IsEnabled = true;
                    slider_tint_.IsEnabled = true;
                    white_balance_once_.IsEnabled = true;
                    OnEventTempTint();
                }

                uint resnum = cam_.ResolutionNumber;
                uint eSize = 0;
                if (cam_.get_eSize(out eSize))
                {
                    for (uint i = 0; i < resnum; ++i)
                    {
                        int w = 0, h = 0;
                        if (cam_.get_Resolution(i, out w, out h))
                            combo_.Items.Add(w.ToString() + "*" + h.ToString());
                    }
                    combo_.SelectedIndex = (int)eSize;

                    int width = 0, height = 0;
                    if (cam_.get_Size(out width, out height))
                    {
                        cam_.put_Option(Toupcam.eOPTION.OPTION_BITDEPTH, 1);
                        cam_.put_Option(Toupcam.eOPTION.OPTION_RGB, 4); // RGB32
                        prebitsperpixel = cam_.MaxBitDepth;

                        /* The backend of WPF/UWP/WinUI is Direct3D/Direct2D, which is different from Winform's backend GDI.
                         * We use their respective native formats, Bgr32 in WPF/UWP/WinUI, and Bgr24 in Winform
                         */
                        bmp_ = new WriteableBitmap(width, height, 0, 0, PixelFormats.Rgb24, null);
                        prvbmp_ = new WriteableBitmap(width, height, 0, 0, PixelFormats.Gray16, null);
                        if (!cam_.StartPullModeWithCallback(new Toupcam.DelegateEventCallback(DelegateOnEventCallback)))
                            MessageBox.Show("Failed to start camera");
                        else
                        {
                            bool autoexpo = true;
                            cam_.get_AutoExpoEnable(out autoexpo);
                            auto_exposure_.IsChecked = autoexpo;
                            slider_expotime_.IsEnabled = !autoexpo;
                        }

                        timer_ = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
                        timer_.Tick += (sender, e) =>
                        {
                            if (cam_ != null)
                            {
                                uint nFrame = 0, nTime = 0, nTotalFrame = 0;
                                if (cam_.get_FrameRate(out nFrame, out nTime, out nTotalFrame) && (nTime > 0))
                                    label_fps_.Content = string.Format("{0}; fps = {1:#.0}", nTotalFrame, ((double)nFrame) * 1000.0 / (double)nTime);
                            }
                        };
                        timer_.Start();

                        started_ = true;
                    }
                }
            }
        }

        private void DelegateHotplugCallback()
        {
            MessageBox.Show("HotPlug");
        }


        private void onClick_start(object sender, RoutedEventArgs e)
        {
            if (cam_ != null)
                return;
            Toupcam.GigeEnable(null);
            Thread.Sleep(1000);

            Toupcam.DeviceV2[] arr = Toupcam.EnumV2();
            if (arr.Length <= 0)
                MessageBox.Show("No camera found.");
            else if (1 == arr.Length)
            {
                dev_ = arr[0];
                startDevice(arr[0].id);
            }
            else
            {
                ContextMenu menu = new ContextMenu() { PlacementTarget = start_, Placement = PlacementMode.Bottom };
                for (int i = 0; i < arr.Length; ++i)
                {
                    MenuItem mitem = new MenuItem() { Header = arr[i].displayname, CommandParameter = arr[i].id };
                    mitem.Click += (nsender, ne) =>
                    {
                        string camId = (string)(((MenuItem)nsender).CommandParameter);
                        if ((camId != null) && (camId.Length > 0))
                        {
                            startDevice(camId);
                        }

                    };
                    menu.Items.Add(mitem);
                }
                menu.IsOpen = true;
            }
        }

        private void onClick_whitebalanceonce(object sender, RoutedEventArgs e)
        {
            if (started_)
                cam_?.AwbOnce();
        }

        private void onChanged_temptint(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((cam_ != null) && started_)
            {
                cam_.put_TempTint((int)slider_temp_.Value, (int)slider_tint_.Value);
                label_temp_.Content = ((int)slider_temp_.Value).ToString();
                label_tint_.Content = ((int)slider_tint_.Value).ToString();
            }
        }

        private void onChanged_expotime(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((cam_ != null) && started_)
            {
                cam_.put_ExpoTime((uint)slider_expotime_.Value);
                label_expotime_.Text = ((uint)slider_expotime_.Value).ToString();
            }
        }

        private void onClick_auto_exposure(object sender, RoutedEventArgs e)
        {
            if (started_)
            {
                cam_?.put_AutoExpoEnable(auto_exposure_.IsChecked ?? false);
                slider_expotime_.IsEnabled = !auto_exposure_.IsChecked ?? false;
            }
        }

        private void OnClick_snap(object sender, RoutedEventArgs e)
        {
            if (cam_ != null)
            {

                if (prvbmp_ != null)
                {
                    ImageLibSaveTif16Mono(prvbmp_.BackBuffer, prvbmp_.PixelWidth, prvbmp_.PixelHeight, (ushort)prebitsperpixel);
                    //ImageLibSaveTif16Mono(prvbmp_.BackBuffer, prvbmp_.PixelWidth / 2, prvbmp_.PixelHeight, (ushort)prebitsperpixel);

                }
                //if (cam_.StillResolutionNumber <= 0)
                //{
                //    if (bmp_ != null)
                //        SaveToFile(bmp_);
                //}
                //else
                //{
                //    ContextMenu menu = new ContextMenu() { PlacementTarget = snap_, Placement = PlacementMode.Bottom };
                //    for (uint i = 0; i < cam_.ResolutionNumber; ++i)
                //    {
                //        int w = 0, h = 0;
                //        cam_.get_Resolution(i, out w, out h);
                //        MenuItem mitem = new MenuItem() { Header = string.Format("{0} * {1}", w, h), CommandParameter = i }; //inbox
                //        mitem.Click += (nsender, ne) =>
                //        {
                //            uint k = (uint)(((MenuItem)nsender).CommandParameter); //unbox
                //            if (k < cam_.StillResolutionNumber)
                //                cam_.Snap(k);
                //        };
                //        menu.Items.Add(mitem);
                //    }
                //    menu.IsOpen = true;
                //}
            }
        }
        private void InitExpoTime()
        {
            if (cam_ == null)
                return;

            uint nMin = 0, nMax = 0, nDef = 0;
            if (cam_.get_ExpTimeRange(out nMin, out nMax, out nDef))
            {
                slider_expotime_.Minimum = nMin;
                slider_expotime_.Maximum = nMax;
            }
            OnEventExposure();
        }

        private void onSelchange_combo(object sender, SelectionChangedEventArgs e)
        {
            if (cam_ != null)
            {
                uint eSize = 0;
                if (cam_.get_eSize(out eSize))
                {
                    if (eSize != combo_.SelectedIndex)
                    {
                        cam_.Stop();
                        cam_.put_eSize((uint)combo_.SelectedIndex);

                        InitExpoTime();
                        OnEventTempTint();

                        int width = 0, height = 0;
                        if (cam_.get_Size(out width, out height))
                        {
                            bmp_ = new WriteableBitmap(width, height, 0, 0, PixelFormats.Bgr32, null);
                            cam_.StartPullModeWithCallback(new Toupcam.DelegateEventCallback(DelegateOnEventCallback));
                        }
                    }
                }
            }
        }

        private void label_expotime__TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((cam_ != null) && started_)
            {
                slider_expotime_.Value = Convert.ToInt32(label_expotime_.Text);
                cam_.put_ExpoTime((uint)slider_expotime_.Value);
            }
        }

        private void OnClick_singe(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                if (prvbmp_ != null)
                {

                    try
                    {
                      
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "Image Files (*.tiff)|*.tiff | All Files | *.*";
                        sfd.RestoreDirectory = true;//保存对话框是否记忆上次打开的目录
                        if (sfd.ShowDialog() == true)
                        {
                            ImageProcessing.Save(sfd.FileName, prvbmp_, null, 1, false);

                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    break;
                }
                Thread.Sleep(100);
            }
        }
    }
}
