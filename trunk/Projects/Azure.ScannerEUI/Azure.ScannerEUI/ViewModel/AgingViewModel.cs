using Azure.Avocado.EthernetCommLib;
using Azure.Configuration.Settings;
using Azure.ImagingSystem;
using Azure.WPF.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Azure.ScannerEUI.ViewModel
{

    class AgingViewModel : ViewModelBase
    {

        #region privert data
        private RelayCommand _StartBatchProcessCommand = null;
        private RelayCommand _StopBatchProcessCommand = null;
        private bool _IsProcessing;
        private Thread _Process = null;
        private Thread _ProcessDoorState = null;
        private bool _IsNextBtnClicked = false;
        private bool _IsNextStepBtnEnabled = false;
        private RelayCommand _NextStepCommand = null;
        //private static readonly string[] _ChannelNames = { "NIR", "Green", "Red", "Blue" };

        private string _SubProcessName;
        private int _TotalProcessPercent;
        private int _SubProcessPercent;

        private string _DeviceSerialNum = string.Empty;
        private int _LoopTimes;
        private string _StoreFolder = string.Empty;
        private RelayCommand _StoreFolderCommand = null;
        private bool _IsProcessCanceled = false;
        private bool singe = false;
        #endregion

        #region Store Folder Command
        public ICommand StoreFolderCommand
        {
            get
            {
                if (_StoreFolderCommand == null)
                {
                    _StoreFolderCommand = new RelayCommand(ExecuteStoreFolderCommand, CanExecuteStoreFolderCommand);
                }
                return _StoreFolderCommand;
            }
        }
        public void ExecuteStoreFolderCommand(object parameter)
        {
            if (StoreFolder == string.Empty)
            {
                System.Windows.Forms.FolderBrowserDialog fdDlg = new System.Windows.Forms.FolderBrowserDialog();
                //string commonPictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //string appCommonPictureFolder = commonPictureFolder + "\\" + "Scanner老化测试记录\\";
                fdDlg.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult dlgResult = fdDlg.ShowDialog();
                if (dlgResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    StoreFolder = fdDlg.SelectedPath;
                }
            }
            else if (Directory.Exists(StoreFolder))
            {
                System.Diagnostics.Process.Start(StoreFolder);
            }
        }
        public bool CanExecuteStoreFolderCommand(object parameter)
        {
            return true;
        }
        #endregion Store Folder Command

        #region Start Batch Process Command
        public ICommand StartBatchProcessCommand
        {
            get
            {
                if (_StartBatchProcessCommand == null)
                {
                    _StartBatchProcessCommand = new RelayCommand(ExecuteStartBatchProcessCommand, CanExecuteBatchProcessCommand);
                }
                return _StartBatchProcessCommand;
            }
        }
        public void ExecuteStartBatchProcessCommand(object param)
        {
            
            if (!Workspace.This.MotorVM.IsNewFirmware)
            {
                MessageBox.Show("通讯异常！");
                return;
            }
            //if(Workspace.This.IVVM.SensorML1==IvSensorType.NA&& Workspace.This.IVVM.SensorMR1 == IvSensorType.NA&&Workspace.This.IVVM.SensorMR2 == IvSensorType.NA)
            //{
            //    MessageBox.Show("至少有一个光学模块！");
            //    return;
            //}
            if (!Workspace.This.MotorVM.MotorAlreadyHome)
            {
                MessageBox.Show("电机尚未找到零点！");
                return;
            }
            if (DeviceSerialNum == string.Empty)
            {
                MessageBox.Show("请输入设备编号再重试！");
                return;
            }
            if (LoopTimes <= 0)
            {
                MessageBox.Show("请输入合法循环次数再重试！");
                return;
            }
            StoreFolder = string.Empty;         // prompt user to select folder
            if (StoreFolder == string.Empty)
            {
                System.Windows.Forms.FolderBrowserDialog fdDlg = new System.Windows.Forms.FolderBrowserDialog();
                //string commonPictureFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //string appCommonPictureFolder = commonPictureFolder + "\\" + "Scanner老化测试记录\\";
                fdDlg.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult dlgResult = fdDlg.ShowDialog();
                if (dlgResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                else
                {
                    StoreFolder = fdDlg.SelectedPath;
                }
            }
            _ProcessDoorState = new Thread(CheckDoorState);
            _ProcessDoorState.IsBackground = true;
            _ProcessDoorState.Start();

            _Process = new Thread(BatchProcessMethod);
            _Process.IsBackground = true;
            _Process.Start();

            TotalProcessPercent = 0;
            SubProcessPercent = 0;
            SubProcessName = "漏光测试";
            IsProcessing = true;
        }

        private void CheckDoorState()
        {
            while (singe)
            {
                Thread.Sleep(500);
                if (Workspace.This.ApdVM.LIDIsOpen == true)
                {
                    Workspace.This.IVVM.GainTxtModuleL1 = 4000;
                    Workspace.This.IVVM.GainTxtModuleR1 = 4000;
                    Workspace.This.IVVM.GainTxtModuleR2 = 4000;
                    MessageBox.Show("检测到Door处于Open状态，已将Gain设置到4000。", "老化测试", MessageBoxButton.OK);
                }
            }
        }

        private void BatchProcessMethod()
        {
            int totalStep = 0;
            int singleLoopStep = 33;
            int totalStepDef = LoopTimes * singleLoopStep;

            int APDGain = 0;
            int PMTGain = 0;

            // setup storage folder
            string saveFolder = string.Empty;
            string storePath = string.Empty;
            string newStorePath = string.Empty;
            #region  get IV moule Count
            int IvWLL1 = Workspace.This.IVVM.WL1;
            int IvWLR1 = Workspace.This.IVVM.WR1;
            int IvWLR2 = Workspace.This.IVVM.WR2;
            int Power = ConfigPower;
            #endregion
            for (int loopCount = 0; loopCount < LoopTimes; loopCount++)
            {
                saveFolder = string.Format(StoreFolder + "\\SN" + DeviceSerialNum + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "\\");
                storePath = saveFolder;
                newStorePath = saveFolder;
                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }
                Thread.Sleep(500);
                if (loopCount == 0)
                {
                    #region resolution board scan confirmation
                    MessageBoxResult boxResult = MessageBoxResult.None;
                    Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                    {
                        boxResult = MessageBox.Show("是否扫描分辨率板？", "梯度增益测试", MessageBoxButton.YesNo);
                    }));
                    if (boxResult == MessageBoxResult.Yes)
                    {
                        RemoveAllImages();
                        saveFolder = string.Format(newStorePath + "\\Resolution Board" + "\\");
                        storePath = saveFolder;
                        if (!Directory.Exists(saveFolder))
                        {
                            Directory.CreateDirectory(saveFolder);
                        }
                        Thread.Sleep(500);
                        SubProcessName = "分辨率板扫描";
                        SubProcessPercent = 0;
                        MessageBox.Show("请确认焦距，扫描区域\n准备好后，点击主页面“继续”按钮开始执行。",
                            "老化测试", MessageBoxButton.OK);
                        IsNextBtnClicked = false;
                        IsNextStepBtnEnabled = true;
                        while (!IsNextBtnClicked)
                        {
                            Thread.Sleep(10);
                        }
                        IsNextStepBtnEnabled = false;
                        // set resolution = 10um, Quality=1, PGA=1, Gain = 500(APD)/5000(PMT), 3 Channels Lasers 10mW 扫描1次
                        //select resolution 10um
                        for (int i = 0; i < Workspace.This.ScannerVM.ResolutionOptions.Count; i++)
                        {
                            if (Workspace.This.ScannerVM.ResolutionOptions[i].DisplayName == "10")
                            {
                                Workspace.This.ScannerVM.SelectedResolution = Workspace.This.ScannerVM.ResolutionOptions[i];
                                break;
                            }
                        }
                        //select Quality=1,
                        for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                        {
                            if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "1")
                            {
                                Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                                break;
                            }
                        }
                        //select PGA=1
                        for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                        {
                            if (Workspace.This.IVVM.PGAOptionsModule[i].DisplayName == "1")
                            {
                                if (IvWLL1 != 0)
                                    Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                                if (IvWLR1 != 0)
                                    Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                                if (IvWLR2 != 0)
                                    Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                                break;
                            }
                        }
                        //Set APD Gain  500    //Set PMT Gain 5000       //Set lasers 10mw        //        // turn on the lasers
                        APDGain = 500;
                        PMTGain = 5000;
                        for (int k = 0; k < Workspace.This.IVVM.GainComModule.Count; k++)
                        {
                            if (Workspace.This.IVVM.GainComModule[k].DisplayName == APDGain.ToString())
                            {
                                if (IvWLL1 != 0)
                                {
                                    Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                                    Workspace.This.IVVM.LaserAPower = Power;
                                    Workspace.This.IVVM.IsLaserL1Selected = true;
                                }
                                if (IvWLR1 != 0)
                                {
                                    Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                                    Workspace.This.IVVM.LaserBPower = Power;
                                    Workspace.This.IVVM.IsLaserR1Selected = true;
                                }
                                if (IvWLR2 != 0)
                                {
                                    Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                                    Workspace.This.IVVM.LaserCPower = Power;
                                    Workspace.This.IVVM.IsLaserR2Selected = true;
                                }
                                break;
                            }
                        }
                        Thread.Sleep(1000);
                        if (_IsProcessCanceled)
                        {
                            _IsProcessCanceled = false;
                            IsNextStepBtnEnabled = false;
                            IsNextBtnClicked = false;
                            IsProcessing = false;
                            return;
                        }
                        Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                        {
                            //start Scan
                            Workspace.This.ScannerVM.ExecuteScanCommand(null);
                        });

                        try
                        {
                            WaitScanningEnded();
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                        }
                        catch (ThreadAbortException e)
                        {
                            return;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                            return;
                        }
                        if (Workspace.This.Files.Count > 0)
                        {
                            try
                            {
                               
                                for (int count = 0; count < Workspace.This.Files.Count; count++)
                                {

                                    if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString()) && Workspace.This.Files[count].FileName.Contains("L"))
                                    //if (Workspace.This.Files[count].FileName.Contains("L"))
                                    {
                                        DateTime dt = DateTime.Now;
                                        string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                        if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                        {

                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1"+strFileTime + ".tif", storePath, IvWLL1 + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                        else
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }

                                    }
                                    if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString()) && Workspace.This.Files[count].FileName.Contains("R2"))
                                    //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                    {
                                        DateTime dt = DateTime.Now;
                                        string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                        if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                        else
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }

                                    }
                                    if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString()) && Workspace.This.Files[count].FileName.Contains("R1"))
                                    //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                    {
                                        DateTime dt = DateTime.Now;
                                        string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                        if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }
                                        else
                                        {
                                            Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                            SavePicture(count, false);
                                        }

                                    }
                                }
                                // clear all Images in Gallery Tab
                                Thread.Sleep(1000);
                                RemoveAllImages();
                                Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                            }
                            catch (Exception exception)
                            {
                                // Rethrow to preserve stack details
                                // Satisfies the rule. 
                                //throw;
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    MessageBox.Show(exception.Message);
                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                });
                            }
                        }
                        else        // failed to capture picture
                        {
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                MessageBox.Show("采集图片失败！", "分辨率板扫描失败", MessageBoxButton.OK, MessageBoxImage.Error);
                            });

                            IsNextStepBtnEnabled = false;
                            IsNextBtnClicked = false;
                            IsProcessing = false;
                            return;
                        }
                    }
                    Workspace.This.SelectedTabIndex = 0;
                    #endregion

                    #region phosphor scan confirmation
                    boxResult = MessageBoxResult.None;
                    Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                    {
                        boxResult = MessageBox.Show("是否进行Phosphor扫描？", "Phosphor Scan", MessageBoxButton.YesNo);
                    }));
                    if (boxResult == MessageBoxResult.Yes)
                    {
                        if (Workspace.This.IVVM.SensorML1 != IvSensorType.PMT && Workspace.This.IVVM.SensorMR1 != IvSensorType.PMT && Workspace.This.IVVM.SensorMR2 != IvSensorType.PMT)
                        {
                            MessageBox.Show("Phosphor扫描至少有一个PMT通道，点击主页面“继续”按钮继续执行。", "老化测试", MessageBoxButton.OK);
                            IsNextBtnClicked = false;
                            while (!IsNextBtnClicked)
                            {
                                Thread.Sleep(10);
                            }
                            Thread.Sleep(1000);
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                        }
                        else {
                        singe = true;
                        // setup storage folder
                        saveFolder = string.Format(newStorePath + "\\Phosphor" + "\\");
                        storePath = saveFolder;
                        // clear all Images in Gallery Tab
                        RemoveAllImages();
                        if (!Directory.Exists(saveFolder))
                        {
                            Directory.CreateDirectory(saveFolder);
                        }
                        Thread.Sleep(500);
                        SubProcessName = "Phosphor扫描";
                        SubProcessPercent = 0;
                        MessageBox.Show("请确认焦距，扫描区域，请注意避光！\n准备好后，点击主页面“继续”按钮开始执行。", "老化测试", MessageBoxButton.OK);
                        IsNextBtnClicked = false;
                        IsNextStepBtnEnabled = true;
                        while (!IsNextBtnClicked)
                        {
                            Thread.Sleep(10);
                        }
                        IsNextStepBtnEnabled = false;
                        //Res=200um,
                        for (int i = 0; i < Workspace.This.ScannerVM.ResolutionOptions.Count; i++)
                        {
                            if (Workspace.This.ScannerVM.ResolutionOptions[i].DisplayName == "200")
                            {
                                Workspace.This.ScannerVM.SelectedResolution = Workspace.This.ScannerVM.ResolutionOptions[i];
                                break;
                            }
                        }
                        //select Quality=1,
                        for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                        {
                            if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "1")
                            {
                                Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                                break;
                            }
                        }
                        //select PGA=8
                        for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                        {
                            if (Workspace.This.IVVM.PGAOptionsModule[i].DisplayName == "8")
                            {
                                if (IvWLL1 != 0)
                                    Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                                if (IvWLR1 != 0)
                                    Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                                if (IvWLR2 != 0)
                                    Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                                break;
                            }
                        }
                        // turn on the lasers
                        Workspace.This.IVVM.IsLaserL1Selected = false;
                        Workspace.This.IVVM.IsLaserR1Selected = false;
                        Workspace.This.IVVM.IsLaserR2Selected = false;
                        for (int tempLoop = 0; tempLoop < 2; tempLoop++)
                        {
                            //if (tempLoop == 1)
                            {
                                //Set lasers 0mw
                                Workspace.This.IVVM.LaserAPower = 0;
                                Workspace.This.IVVM.LaserBPower = 0;
                                Workspace.This.IVVM.LaserCPower = 0;
                            }
                            // set gainD=10000 (1st), 11000 (2nd)
                            PMTGain = 10000 + tempLoop * 1000;
                            // move to (X0, Y0) position
                            Workspace.This.MotorVM.AbsXPos = Workspace.This.ScannerVM.ScanX0;
                            Workspace.This.MotorVM.ExecuteGoAbsPosCommand(MotorType.X);
                            Workspace.This.MotorVM.AbsYPos = Workspace.This.ScannerVM.ScanY0;
                            Workspace.This.MotorVM.ExecuteGoAbsPosCommand(MotorType.Y);
                            while (Workspace.This.MotorVM.CurrentXPos != Math.Round(Workspace.This.ScannerVM.ScanX0, 0))
                            {
                                Thread.Sleep(100);
                            }
                            while (Workspace.This.MotorVM.CurrentYPos != Workspace.This.ScannerVM.ScanY0)
                            {
                                Thread.Sleep(100);
                            }
                            // PMT Channel Laser 20mW  On Laser PMT Channel
                            if (Workspace.This.IVVM.SensorML1 == IvSensorType.PMT)
                            {
                                Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                                Workspace.This.IVVM.LaserAPower = Power;
                                Workspace.This.IVVM.IsLaserL1Selected = true;
                            }
                            if (Workspace.This.IVVM.SensorMR1 == IvSensorType.PMT)
                            {
                                Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                                Workspace.This.IVVM.LaserBPower = Power;
                                Workspace.This.IVVM.IsLaserR1Selected = true;
                            }
                            if (Workspace.This.IVVM.SensorMR2 == IvSensorType.PMT)
                            {
                                Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                                Workspace.This.IVVM.LaserCPower = Power;
                                Workspace.This.IVVM.IsLaserR2Selected = true;
                            }
                            Thread.Sleep(1000);
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                Workspace.This.ScannerVM.ExecuteScanCommand(null);
                            });
                                try
                                {
                                    WaitScanningEnded();
                                    if (_IsProcessCanceled)
                                    {
                                        _IsProcessCanceled = false;
                                        IsNextStepBtnEnabled = false;
                                        IsNextBtnClicked = false;
                                        IsProcessing = false;
                                        return;
                                    }
                                    singe = false;
                                    //scan done PMT Gain 500//
                                    Workspace.This.IVVM.GainTxtModuleL1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR2 = 4000;
                                }
                                catch (ThreadAbortException e)
                                {
                                    singe = false;
                                    Workspace.This.IVVM.GainTxtModuleL1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR2 = 4000;
                                    return;
                                }
                                catch (Exception e)
                                {
                                    singe = false;
                                    Workspace.This.IVVM.GainTxtModuleL1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR1 = 4000;
                                    Workspace.This.IVVM.GainTxtModuleR2 = 4000;
                                    MessageBox.Show(e.Message);
                                    return;
                                }
                            if (Workspace.This.Files.Count > 0)     // image captured successfully
                            {
                              
                                try
                                {
                                    for (int count = 0; count < Workspace.This.Files.Count; count++)
                                        {
                                            if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString())&& Workspace.This.Files[count].FileName.Contains("L"))
                                            //if (Workspace.This.Files[count].FileName.Contains("L"))
                                            {
                                                if (Workspace.This.IVVM.SensorML1 == IvSensorType.PMT)
                                                {
                                                    DateTime dt = DateTime.Now;
                                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                                    Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);//C
                                                    SavePicture(count, false);
                                                }

                                            }
                                            if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString())&& Workspace.This.Files[count].FileName.Contains("R1"))
                                            //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                            {
                                                if (Workspace.This.IVVM.SensorMR1 == IvSensorType.PMT)
                                                {
                                                    DateTime dt = DateTime.Now;
                                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                                    Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);//B
                                                    SavePicture(count, false);
                                                }
                                            }
                                             if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString())&& Workspace.This.Files[count].FileName.Contains("R2"))
                                            //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                            {

                                                if (Workspace.This.IVVM.SensorMR2 == IvSensorType.PMT)
                                                {
                                                    DateTime dt = DateTime.Now;
                                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                                    Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);//A
                                                    SavePicture(count, false);
                                                }
                                            }
                                    }
                                    // clear all Images in Gallery Tab
                                    Thread.Sleep(1000);
                                    RemoveAllImages();
                                    Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                                }
                                catch (Exception exception)
                                {
                                    // Rethrow to preserve stack details
                                    // Satisfies the rule. 
                                    //throw;
                                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                    {
                                        MessageBox.Show(exception.Message);
                                        IsNextStepBtnEnabled = false;
                                        IsNextBtnClicked = false;
                                        IsProcessing = false;
                                        return;
                                    });
                                }
                            }
                            else        // failed to capture picture
                            {
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    MessageBox.Show("采集图片失败！", "Phosphor扫描失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                });

                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }

                        }
                    }
                     }
                    #endregion

                    #region   darknoise scan configrmation
                    Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                    {
                        //boxResult = MessageBox.Show("请确认焦距和扫描区域，以及设置激光功率！准备好后，点击主页面“继续”按钮执行。\n" +
                        //    "若想跳过测试，请点“否”！", "梯度增益测试", MessageBoxButton.YesNo);
                        MessageBox.Show("请确认焦距，分辨率，扫描区域！\n准备好后，点击主页面“继续”按钮开始执行。",
                            "老化测试", MessageBoxButton.OK);
                        Workspace.This.SelectedTabIndex = 0;
                    }));
                }
                try
                {
                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    {
                        while (Workspace.This.Files.Count > 0)
                        {
                            Workspace.This.Remove(Workspace.This.Files[0]);
                        }
                    });
                    storePath = string.Empty;
                    if (!Directory.Exists(saveFolder))
                    {
                        Directory.CreateDirectory(saveFolder);
                    }
                    Thread.Sleep(500);

                    #region step 0: noise test without lasers on
                    SubProcessName = "噪声测试";
                    SubProcessPercent = 0;
                    storePath = newStorePath + "Dark Noise\\";
                    if (loopCount == 0)
                    {
                        IsNextBtnClicked = false;
                        IsNextStepBtnEnabled = true;
                    }
                    else
                    {
                        IsNextBtnClicked = true;
                        IsNextStepBtnEnabled = true;
                    }
                    IsNextStepBtnEnabled = true;
                    while (!IsNextBtnClicked)
                    {
                        Thread.Sleep(100);
                    }
                    IsNextStepBtnEnabled = false;

                    if (!Directory.Exists(storePath))
                    {
                        Directory.CreateDirectory(storePath);
                    }
                    Workspace.This.IVVM.LaserAPower = 0;
                    Workspace.This.IVVM.LaserBPower = 0;
                    Workspace.This.IVVM.LaserCPower = 0;
                    Workspace.This.IVVM.IsLaserL1Selected = false;
                    Workspace.This.IVVM.IsLaserR1Selected = false;
                    Workspace.This.IVVM.IsLaserR2Selected = false;
                    //select Quality=1,
                    for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                    {
                        if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "1")
                        {
                            Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                            break;
                        }
                    }
                    //select PGA=8
                    for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                    {
                        if (Workspace.This.IVVM.PGAOptionsModule[i].DisplayName == "8")
                        {
                            if (IvWLL1 != 0)
                                Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                            if (IvWLR1 != 0)
                                Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                            if (IvWLR2 != 0)
                                Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                            break;
                        }
                    }
                    //Set APD Gain  500             //Set PMT Gain 5000    
                    //Set lasers 10mw         //        // turn off the lasers  

                    APDGain = 500;
                    PMTGain = 4000;
                    for (int k = 0; k < Workspace.This.IVVM.GainComModule.Count; k++)
                    {
                        if (Workspace.This.IVVM.GainComModule[k].DisplayName == APDGain.ToString())
                        {
                            if (IvWLL1 != 0) 
                            { 
                                Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[k];
                                Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                            }
                            if (IvWLR1 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[k];
                                Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                            }
                            if (IvWLR2 != 0)
                            { 
                                Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[k];
                                Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                            }
                            break;
                        }
                    }

                    Thread.Sleep(1000);
                    if (_IsProcessCanceled)
                    {
                        _IsProcessCanceled = false;
                        IsNextStepBtnEnabled = false;
                        IsNextBtnClicked = false;
                        IsProcessing = false;
                        return;
                    }
                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    {
                        Workspace.This.ScannerVM.ExecuteScanCommand(null);
                    });
                    try
                    {
                        WaitScanningEnded();
                        if (_IsProcessCanceled)
                        {
                            _IsProcessCanceled = false;
                            IsNextStepBtnEnabled = false;
                            IsNextBtnClicked = false;
                            IsProcessing = false;
                            return;
                        }
                    }
                    catch (ThreadAbortException e)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        return;
                    }
                    if (Workspace.This.Files.Count > 0)     // image captured successfully
                    {
                        try
                        {
                            for (int count = 0; count < Workspace.This.Files.Count; count++)
                            {
                                 if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString())&& Workspace.This.Files[count].FileName.Contains("L"))
                                //if (Workspace.This.Files[count].FileName.Contains("L"))
                                {
                                    DateTime dt = DateTime.Now;
                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                    if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }
                                    else
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLL1 + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }

                                }
                                if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString())&& Workspace.This.Files[count].FileName.Contains("R1"))
                                //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                {
                                    DateTime dt = DateTime.Now;
                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                    if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)//B
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }
                                    else
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }
                                }
                                 if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString())&& Workspace.This.Files[count].FileName.Contains("R2"))
                                //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                {
                                    DateTime dt = DateTime.Now;
                                    string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                    if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }
                                    else
                                    {
                                        Workspace.This.Files[count].FilePath = string.Format("{0}{1}-0mW-Gain{2}-PGA8-Res{3}um-Quality1" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value);
                                        SavePicture(count, false);
                                    }
                                }
                            }
                            // clear all Images in Gallery Tab
                            Thread.Sleep(1000);
                            RemoveAllImages();
                            Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                        }
                        catch (Exception exception)
                        {
                            // Rethrow to preserve stack details
                            // Satisfies the rule. 
                            //throw;
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                MessageBox.Show(exception.Message);
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            });
                        }
                        finally
                        {
                            SubProcessPercent = 100;
                            totalStep++;
                            TotalProcessPercent = totalStep * 100 / totalStepDef;
                        }
                    }
                    else        // failed to capture picture
                    {
                        Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                        {
                            MessageBox.Show("采集图片失败！", "噪声测试失败", MessageBoxButton.OK, MessageBoxImage.Error);
                        });

                        IsNextStepBtnEnabled = false;
                        IsNextBtnClicked = false;
                        IsProcessing = false;
                        return;
                    }

                    #endregion

                    #region step 2: scan with different gains, 4 loops, 1st & 3rd use A, C; 2nd & 4th use B, D
                    SubProcessName = "梯度增益测试";
                    SubProcessPercent = 0;
                    storePath = newStorePath + "Gains\\";

                    if (!Directory.Exists(storePath))
                    {
                        Directory.CreateDirectory(storePath);
                    }

                    //select Quality=1,
                    for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                    {
                        if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "1")
                        {
                            Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                            break;
                        }
                    }
                    //select PGA=1
                    for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                    {
                        if (Workspace.This.IVVM.PGAOptionsModule[i].DisplayName == "1")
                        {
                            if (IvWLL1 != 0)
                                Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                            if (IvWLR1 != 0)
                                Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                            if (IvWLR2 != 0)
                                Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                            break;
                        }
                    }
                    //Set lasers 10mw
                    if (IvWLL1 != 0)
                        Workspace.This.IVVM.LaserAPower = Power;
                    if (IvWLR1 != 0)
                        Workspace.This.IVVM.LaserBPower = Power;
                    if (IvWLR2 != 0)
                        Workspace.This.IVVM.LaserCPower = Power;
                    for (int i = 0; i < 2; i++)     // 4 loops, gain differs from 500 to 50
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (j)
                            {
                                case 0:
                                    APDGain = 500;
                                    PMTGain = 5500;
                                    break;
                                case 1:
                                    APDGain = 400;
                                    PMTGain = 5000;
                                    break;
                                case 2:
                                    APDGain = 300;
                                    PMTGain = 4500;
                                    break;
                                case 3:
                                    APDGain = 250;
                                    PMTGain = 4000;
                                    break;
                                case 4:
                                    APDGain = 200;
                                    PMTGain = 3500;
                                    break;
                                case 5:
                                    APDGain = 150;
                                    PMTGain = 3000;
                                    break;
                                case 6:
                                    APDGain = 100;
                                    PMTGain = 2500;
                                    break;
                                case 7:
                                    APDGain = 50;
                                    PMTGain = 2000;
                                    break;
                            }
                            //Set APD Gain          //Set PMT Gain  // turn off the lasers
                            for (int k = 0; k < Workspace.This.IVVM.GainComModule.Count; k++)
                            {
                                if (Workspace.This.IVVM.GainComModule[k].DisplayName == APDGain.ToString())
                                {
                                    if (IvWLL1 != 0) 
                                    {
                                        Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[k];
                                        Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                                        Workspace.This.IVVM.IsLaserL1Selected = true;
                                    }
                                    if (IvWLR1 != 0)
                                    {
                                        Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[k];
                                        Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                                        Workspace.This.IVVM.IsLaserR1Selected = true;
                                    }
                                    if (IvWLR2 != 0)
                                    { 
                                        Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[k];
                                        Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                                        Workspace.This.IVVM.IsLaserR2Selected = true;
                                    }
                                }
                            }
                            Thread.Sleep(1000);
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                Workspace.This.ScannerVM.ExecuteScanCommand(null);
                            });
                            try
                            {
                                WaitScanningEnded();
                                if (_IsProcessCanceled)
                                {
                                    _IsProcessCanceled = false;
                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                }
                            }
                            catch (ThreadAbortException e)
                            {
                                return;
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                                return;
                            }
                            if (Workspace.This.Files.Count > 0)     // image captured successfully
                            {
                                try
                                {
                                    for (int count = 0; count < Workspace.This.Files.Count; count++)
                                    {
                                         if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString())&& Workspace.This.Files[count].FileName.Contains("L"))
                                        //if (Workspace.This.Files[count].FileName.Contains("L"))
                                        {
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLL1 + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLL1 + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }

                                        }
                                        if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString())&& Workspace.This.Files[count].FileName.Contains("R1"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                        {
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }

                                        }
                                         if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString())&& Workspace.This.Files[count].FileName.Contains("R2"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                        {
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }

                                        }
                                    }
                                    // clear all Images in Gallery Tab
                                    Thread.Sleep(1000);
                                    RemoveAllImages();
                                    Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                                }
                                catch (Exception exception)
                                {
                                    // Rethrow to preserve stack details
                                    // Satisfies the rule. 
                                    //throw;
                                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                    {
                                        MessageBox.Show(exception.Message);
                                        IsNextStepBtnEnabled = false;
                                        IsNextBtnClicked = false;
                                        IsProcessing = false;
                                        return;
                                    });
                                }
                                finally
                                {
                                    SubProcessPercent = i * 25 + (int)((j + 1) * 25.0 / 6.0);
                                    totalStep++;
                                    TotalProcessPercent = totalStep * 100 / totalStepDef;
                                }
                            }
                            else        // failed to capture picture
                            {
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    MessageBox.Show("采集图片失败！", "梯度增益测试失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                });

                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                        }
                    }
                    #endregion

                    #region step 3: scan with maximum gain, 10x2 loops
                    //if (boxResult != MessageBoxResult.No)
                    {
                        SubProcessName = "增益稳定性测试";
                        SubProcessPercent = 0;

                        storePath = newStorePath + "Stability\\";
                        if (!Directory.Exists(storePath))
                        {
                            Directory.CreateDirectory(storePath);
                        }
                        //Set APD Gain  500     //Set PMT Gain 4000         //Set lasers 10mw
                        APDGain = 500;
                        PMTGain = 4000;
                        for (int k = 0; k < Workspace.This.IVVM.GainComModule.Count; k++)
                        {
                            if (Workspace.This.IVVM.GainComModule[k].DisplayName == APDGain.ToString())
                            {
                                if (IvWLL1 != 0)
                                { 
                                    Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                                    Workspace.This.IVVM.LaserAPower = Power;
                                }
                                if (IvWLR1 != 0)
                                { 
                                    Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                                    Workspace.This.IVVM.LaserBPower = Power;
                                }
                                if (IvWLR2 != 0)
                                { 
                                    Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[k];
                                    Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                                    Workspace.This.IVVM.LaserCPower = Power;
                                }
                            }
                        }
                        for (int i = 0; i < 10; i++)
                        {

                            // turn on the lasers
                            if (IvWLL1 != 0)
                                Workspace.This.IVVM.IsLaserL1Selected = true;
                            if (IvWLR1 != 0)
                                Workspace.This.IVVM.IsLaserR1Selected = true;
                            if (IvWLR2 != 0)
                                Workspace.This.IVVM.IsLaserR2Selected = true;

                            Thread.Sleep(1000);
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                Workspace.This.ScannerVM.ExecuteScanCommand(null);
                            });

                            try
                            {
                                WaitScanningEnded();
                                if (_IsProcessCanceled)
                                {
                                    _IsProcessCanceled = false;
                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                }
                            }
                            catch (ThreadAbortException e)
                            {
                                return;
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                                return;
                            }

                            if (Workspace.This.Files.Count > 0)     // image captured successfully
                            {
                                try
                                {
                                    for (int count = 0; count < Workspace.This.Files.Count; count++)
                                    {
                                        if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString())&& Workspace.This.Files[count].FileName.Contains("L"))
                                        //if (Workspace.This.Files[count].FileName.Contains("L"))
                                        {
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLL1 + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLL1 + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                        }
                                        if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString())&& Workspace.This.Files[count].FileName.Contains("R1"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                        {
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)
                                            {

                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                        }
                                        if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString())&& Workspace.This.Files[count].FileName.Contains("R2"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                        {
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality1--{4}" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, i);
                                                SavePicture(count, false);
                                            }
                                        }
                                    }
                                           
                                 
                                    // clear all Images in Gallery Tab
                                    Thread.Sleep(1000);
                                    RemoveAllImages();
                                    Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                                }
                                catch (Exception exception)
                                {
                                    // Rethrow to preserve stack details
                                    // Satisfies the rule. 
                                    //throw;
                                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                    {
                                        MessageBox.Show(exception.Message);
                                        IsNextStepBtnEnabled = false;
                                        IsNextBtnClicked = false;
                                        IsProcessing = false;
                                        return;
                                    });
                                }
                                finally
                                {
                                    SubProcessPercent = 5 + (i) * 10;
                                    totalStep++;
                                    TotalProcessPercent = totalStep * 100 / totalStepDef;
                                }
                            }
                            else        // failed to capture picture
                            {
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    MessageBox.Show("采集图片失败！", "最大增益测试失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                });

                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }

                        }
                    }
                    #endregion step 3: scan with maximum gain, 10x2 loops

                    #region step 4: scan with diffrent qualities(2, 4), 4x2 loops
                    SubProcessName = "Quality测试";
                    SubProcessPercent = 0;
                    storePath = newStorePath + "Quality \\";
                    if (!Directory.Exists(storePath))
                    {
                        Directory.CreateDirectory(storePath);
                    }
                    //select PGA=1
                    for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++)
                    {
                        if (Workspace.This.IVVM.PGAOptionsModule[i].DisplayName == "1")
                        {
                            if (IvWLL1 != 0)
                                Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                            if (IvWLR1 != 0)
                                Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                            if (IvWLR2 != 0)
                                Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                            break;
                        }
                    }
                    //Set PMT Gain 4000      //Set lasers 10mw
                    APDGain = 500;
                    PMTGain = 4000;
                    for (int j = 0; j < Workspace.This.IVVM.GainComModule.Count; j++)
                    {
                        if (Workspace.This.IVVM.GainComModule[j].DisplayName == APDGain.ToString())
                        {
                            if (IvWLL1 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[j];
                                Workspace.This.IVVM.GainTxtModuleL1 = PMTGain;
                                Workspace.This.IVVM.LaserAPower = Power;
                            }
                     
                            if (IvWLR1 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[j];
                                Workspace.This.IVVM.GainTxtModuleR1 = PMTGain;
                                Workspace.This.IVVM.LaserBPower = Power;
                            }
                   
                            if (IvWLR2 != 0)
                            {
                                Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[j];
                                Workspace.This.IVVM.GainTxtModuleR2 = PMTGain;
                                Workspace.This.IVVM.LaserCPower = Power;
                            }
                        }
                    }
                    for (int loopQualityNum = 0; loopQualityNum < 2; loopQualityNum++)
                    {
                        for (int loopQuality = 0; loopQuality < 2; loopQuality++)
                        {
                            if (loopQualityNum == 0)
                            {
                                for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                                {
                                    if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "2")
                                    {
                                        Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                                        break;
                                    }

                                }
                            }
                            if (loopQualityNum == 1)
                            {
                                for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                                {
                                    if (Workspace.This.ScannerVM.QualityOptions[i].DisplayName == "4")
                                    {
                                        Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                                        break;
                                    }

                                }
                            }
                            // turn on the lasers
                            if (IvWLL1 != 0)
                            {
                                Workspace.This.IVVM.IsLaserL1Selected = true;
                            }
                            if (IvWLR1 != 0)
                            {
                                Workspace.This.IVVM.IsLaserR1Selected = true;
                            }
                            if (IvWLR2 != 0)
                            {
                                Workspace.This.IVVM.IsLaserR2Selected = true;
                            }
                            Thread.Sleep(1000);
                            if (_IsProcessCanceled)
                            {
                                _IsProcessCanceled = false;
                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }
                            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                            {
                                Workspace.This.ScannerVM.ExecuteScanCommand(null);
                            });

                            try
                            {
                                WaitScanningEnded();
                                if (_IsProcessCanceled)
                                {
                                    _IsProcessCanceled = false;
                                    IsNextStepBtnEnabled = false;
                                    IsNextBtnClicked = false;
                                    IsProcessing = false;
                                    return;
                                }
                            }
                            catch (ThreadAbortException e)
                            {
                                return;
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                                return;
                            }

                            if (Workspace.This.Files.Count > 0)     // image captured successfully
                            {
                                try
                                {
                                    for (int count = 0; count < Workspace.This.Files.Count; count++)
                                    {
                                         if (IvWLL1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLL1.ToString())&& Workspace.This.Files[count].FileName.Contains("L"))
                                        //if (Workspace.This.Files[count].FileName.Contains("L"))
                                        {
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorML1 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLL1 + "-L", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLL1 + "-L", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }

                                        }
                                        if (IvWLR1 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR1.ToString())&& Workspace.This.Files[count].FileName.Contains("R1"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R1"))
                                        {
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR1 == IvSensorType.APD)//B
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLR1 + "-R1", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }

                                        }
                                        if (IvWLR2 != 0 && Workspace.This.Files[count].FileName.Contains(IvWLR2.ToString())&& Workspace.This.Files[count].FileName.Contains("R2"))
                                        //if (Workspace.This.Files[count].FileName.Contains("R2"))
                                        {
                                            DateTime dt = DateTime.Now;
                                            string strFileTime = "_" + string.Format("{0}-{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}", dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                                            if (Workspace.This.IVVM.SensorMR2 == IvSensorType.APD)
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", APDGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }
                                            else
                                            {
                                                Workspace.This.Files[count].FilePath = string.Format("{0}{1}-" + Power + "mW-Gain{2}-PGA1-Res{3}um-Quality{4}--{5}" + strFileTime + ".tif", storePath, IvWLR2 + "-R2", PMTGain, Workspace.This.ScannerVM.SelectedResolution.Value, Workspace.This.ScannerVM.SelectedQuality.Value, loopQuality);
                                                SavePicture(count, false);
                                            }

                                        }
                                    }
                                    // clear all Images in Gallery Tab
                                    Thread.Sleep(1000);
                                    RemoveAllImages();
                                    Workspace.This.SelectedTabIndex = (int)ApplicationTabType.Imaging;
                                }
                                catch (Exception exception)
                                {
                                    // Rethrow to preserve stack details
                                    // Satisfies the rule. 
                                    //throw;
                                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                    {
                                        MessageBox.Show(exception.Message);
                                        IsNextStepBtnEnabled = false;
                                        IsNextBtnClicked = false;
                                        IsProcessing = false;
                                        return;
                                    });
                                }
                                finally
                                {
                                    SubProcessPercent = (loopQualityNum) * 25 + (int)((loopQuality + 1) * 12.5);
                                    totalStep++;
                                    TotalProcessPercent = totalStep * 100 / totalStepDef;
                                }
                            }
                            else        // failed to capture picture
                            {
                                Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                                {
                                    MessageBox.Show("采集图片失败！", "Quality测试失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                });

                                IsNextStepBtnEnabled = false;
                                IsNextBtnClicked = false;
                                IsProcessing = false;
                                return;
                            }

                        }
                    }
                   
                    #endregion
                }
                catch (Exception ex)
                {
                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    });
                }
            }
            Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
            {
                //  _ProcessDoorState.Abort();
                MessageBox.Show("老化测试完毕！");
            });

            if (Directory.Exists(StoreFolder))
            {
                System.Diagnostics.Process.Start("Explorer.exe", StoreFolder);
            }
            IsNextStepBtnEnabled = false;
            IsNextBtnClicked = false;
            IsProcessing = false;
            #endregion

        }
        public bool CanExecuteBatchProcessCommand(object param)
        {
            return true;
        }
        #endregion Start Batch Process Command



        #region Stop Batch Process Command
        public ICommand StopBatchProcessCommand
        {
            get
            {
                if (_StopBatchProcessCommand == null)
                {
                    _StopBatchProcessCommand = new RelayCommand(ExecuteStopBatchProcessCommand, CanExecuteBatchProcessCommand);
                }
                return _StopBatchProcessCommand;
            }
        }
        public void ExecuteStopBatchProcessCommand(object param)
        {
            if (_Process != null)
            {
                if (Workspace.This.IsScanning || Workspace.This.IsPreparing)
                {
                    Workspace.This.Owner.Dispatcher.Invoke((Action)delegate ()
                    {
                        Workspace.This.ScannerVM.ExecuteStopScanCommand(null);
                    });
                }
                _IsProcessCanceled = true;
            }
        }
        #endregion Stop Batch Process Command

        #region Next Step Command
        public ICommand NextStepCommand
        {
            get
            {
                if (_NextStepCommand == null)
                {
                    _NextStepCommand = new RelayCommand(ExecuteNextStepCommand, CanExecuteNextStepCommand);
                }
                return _NextStepCommand;
            }
        }

        public void ExecuteNextStepCommand(object param)
        {
            _IsNextBtnClicked = true;
            IsNextStepBtnEnabled = false;
        }
        public bool CanExecuteNextStepCommand(object param)
        {
            return true;
        }
        #endregion Next Step Command

        #region  public property
        private void SavePicture(int tabIndex, bool CloseAfterSave = true)
        {
            try
            {
                if (Workspace.This.Files[tabIndex] != null)
                {
                    //                    Thread.Sleep(1000);
                    Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                    {
                        //Workspace.This.StartWaitAnimation("Saving...");
                        Workspace.This.SaveFile(Workspace.This.Files[tabIndex], false);

                        if (CloseAfterSave)
                        {
                            Workspace.This.Remove(Workspace.This.Files[tabIndex]);
                        }
                        //Workspace.This.StopWaitAnimation();
                    }));
                }
            }
            catch
            {
                // Rethrow to preserve stack details
                // Satisfies the rule. 
                throw;
            }
            finally
            {
                //Workspace.This.Owner.Dispatcher.Invoke((Action)delegate
                //{
                //    Workspace.This.StopWaitAnimation();
                //});
            }
        }

        private void WaitScanningEnded()
        {
            Workspace.This.ScannerVM.ScanProcessingThread.ThreadHandle.Join();
            if (_IsProcessCanceled)
            {
                return;
            }
            for (int temp = 0; temp < 500; temp++)
            {
                if (Workspace.This.Files.Count > 3)
                {
                    break;
                }
                Thread.Sleep(10);
            }
        }

        public bool IsNextBtnClicked
        {
            get
            {
                return _IsNextBtnClicked;
            }
            set
            {
                if (_IsNextBtnClicked != value)
                {
                    _IsNextBtnClicked = value;
                    RaisePropertyChanged("IsNextBtnClicked");
                }
            }
        }

        public bool IsNextStepBtnEnabled
        {
            get
            {
                return _IsNextStepBtnEnabled;
            }

            set
            {
                if (_IsNextStepBtnEnabled != value)
                {
                    _IsNextStepBtnEnabled = value;
                    RaisePropertyChanged("IsNextStepBtnEnabled");
                }
            }
        }

        public bool IsProcessing
        {
            get
            {
                return _IsProcessing;
            }
            set
            {
                if (_IsProcessing != value)
                {
                    _IsProcessing = value;
                    RaisePropertyChanged("IsProcessing");
                }
            }
        }

        public string SubProcessName
        {
            get
            {
                return _SubProcessName;
            }
            set
            {
                if (_SubProcessName != value)
                {
                    _SubProcessName = value;
                    RaisePropertyChanged("SubProcessName");
                }
            }
        }

        public int TotalProcessPercent
        {
            get
            {
                return _TotalProcessPercent;
            }
            set
            {
                if (_TotalProcessPercent != value)
                {
                    _TotalProcessPercent = value;
                    RaisePropertyChanged("TotalProcessPercent");
                }
            }
        }

        public string StoreFolder
        {
            get
            {
                return _StoreFolder;
            }
            set
            {
                if (_StoreFolder != value)
                {
                    _StoreFolder = value;
                    RaisePropertyChanged("StoreFolder");
                }
            }
        }

        public int SubProcessPercent
        {
            get
            {
                return _SubProcessPercent;
            }
            set
            {
                if (_SubProcessPercent != value)
                {
                    _SubProcessPercent = value;
                    RaisePropertyChanged("SubProcessPercent");
                }
            }
        }

        public string DeviceSerialNum
        {
            get
            {
                return _DeviceSerialNum;
            }
            set
            {
                if (_DeviceSerialNum != value)
                {
                    _DeviceSerialNum = value;
                    RaisePropertyChanged("DeviceSerialNum");
                }
            }
        }

        public int LoopTimes
        {
            get
            {
                return _LoopTimes;
            }
            set
            {
                if (_LoopTimes != value)
                {
                    _LoopTimes = value;
                    RaisePropertyChanged("LoopTimes");
                }
            }
        }
        private int _ConfigPower;
        public int ConfigPower
        {
            get
            {
                return _ConfigPower;
            }
            set
            {
                if (_ConfigPower != value)
                {
                    _ConfigPower = value;
                    RaisePropertyChanged("ConfigPower");
                }
            }
        }
        #endregion

        private void RemoveAllImages()
        {
            while (Workspace.This.Files.Count > 0)
            {
                Workspace.This.Owner.Dispatcher.Invoke(new Action(() =>
                {
                    Workspace.This.Remove(Workspace.This.Files[0]);
                }));
                Thread.Sleep(10);
            }
        }

    }
}
