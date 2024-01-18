using Azure.Avocado.EthernetCommLib;
using Azure.ScannerEUI.ViewModel;
using System;
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

namespace Azure.ScannerEUI.View
{
    /// <summary>
    /// ScanWorkUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ScanWorkUserControl : UserControl
    {
        public ScanRegionUserControl OverAllRegion = null;
        ScanRegionUserControl SRUl = null;
        public ScanWorkUserControl()
        {
            InitializeComponent();
            //Workspace.This.ScannerVM.SetScanRegionEvent+=
        }
        int index = 0;
        private void CreateControl()
        {
            int Count = canvas1.Children.Count;
            if (Count > 9)
            {
                return;
            }
            index++;
            SRUl = new ScanRegionUserControl();
            SRUl.Width = 55;
            SRUl.Height = 55;
            if (canvas1.Children.Count > 0)
                SRUl.IndexCount = canvas1.Children.Count + 1;
            else
                SRUl.IndexCount = index;
            SRUl.SetLbNumber();
            SRUl.OnScanDataReceived += Bt_OnScanDataReceived;
            canvas1.Children.Add(SRUl);
        }
        private void RemoveControl()
        {
            if (canvas1.Children.Count > 0)
            {
                canvas1.Children.RemoveAt(canvas1.Children.Count - 1);
            }
            if (canvas1.Children.Count == 1)
            {
                index = 0;
            }
        }
        public void Bt_OnScanDataReceived(int Current)
        {
            foreach (ScanRegionUserControl uc in canvas1.Children)
            {
                if ((int)uc.Tag != Current)
                {
                    uc.SetBackGrondColor();
                }
                else
                {
                    OverAllRegion = uc;
                    GetRegion();
                }
            }
        }
        public void SetCurrentRegion(int Current)
        {
            foreach (ScanRegionUserControl uc in canvas1.Children)
            {
                if ((int)uc.Tag == Current)
                {
                    uc.CurretIndexCount = Current;
                    uc.GridPanel_MouseUp(null, null);
                }
            }
        }
        private void CreateControl_Click(object sender, RoutedEventArgs e)
        {
            CreateControl();
        }
        private void RemoveControl_Click(object sender, RoutedEventArgs e)
        {
            RemoveControl();
        }
        //down
        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            double ii = scrolls.VerticalOffset;
            scrolls.ScrollToVerticalOffset(ii + 1);
        }
        //up
        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            double ii = scrolls.VerticalOffset;
            if (ii > 0)
            {
                scrolls.ScrollToVerticalOffset(ii - 1);
            }
        }

        #region GetRegionParamete
        public void GetRegion()
        {
            if (OverAllRegion != null)
            {
                ScanRegionUserControl _Region = OverAllRegion;
                Workspace.This.ScannerVM.ScanX0 = _Region.X0;
                Workspace.This.ScannerVM.ScanDeltaX = _Region.Dx;
                Workspace.This.ScannerVM.ScanY0 = _Region.Y0;
                Workspace.This.ScannerVM.ScanDeltaY = _Region.Dy;
                Workspace.This.ScannerVM.ScanZ0 = _Region.Z0;
                Workspace.This.ScannerVM.ScanDeltaZ = _Region.Dz;
                for (int i = 0; i < Workspace.This.ScannerVM.ResolutionOptions.Count; i++) {
                    if (Workspace.This.ScannerVM.ResolutionOptions[i].Value == _Region.Reslution)
                    {
                        Workspace.This.ScannerVM.SelectedResolution = Workspace.This.ScannerVM.ResolutionOptions[i];
                    }
                }
                for (int i = 0; i < Workspace.This.ScannerVM.QualityOptions.Count; i++)
                {
                    if (Workspace.This.ScannerVM.QualityOptions[i].Value == _Region.Quality)
                    {
                        Workspace.This.ScannerVM.SelectedQuality = Workspace.This.ScannerVM.QualityOptions[i];
                    }
                }
                for (int i = 0; i < Workspace.This.IVVM.PGAOptionsModule.Count; i++) {

                    if (Workspace.This.IVVM.PGAOptionsModule[i].Value == _Region.LPag)
                    {
                        Workspace.This.IVVM.SelectedMModuleL1 = Workspace.This.IVVM.PGAOptionsModule[i];
                    }
                    if (Workspace.This.IVVM.PGAOptionsModule[i].Value == _Region.R1Pag)
                    {
                        Workspace.This.IVVM.SelectedMModuleR1 = Workspace.This.IVVM.PGAOptionsModule[i];
                    }
                    if (Workspace.This.IVVM.PGAOptionsModule[i].Value == _Region.R2Pag)
                    {
                        Workspace.This.IVVM.SelectedMModuleR2 = Workspace.This.IVVM.PGAOptionsModule[i];
                    }
                }
                for (int i = 0; i < Workspace.This.IVVM.GainComModule.Count; i++)
                {

                    if (Workspace.This.IVVM.GainComModule[i].Value == _Region.LApdGain)
                    {
                        Workspace.This.IVVM.SelectedGainComModuleL1 = Workspace.This.IVVM.GainComModule[i];
                    }
                    if (Workspace.This.IVVM.GainComModule[i].Value == _Region.R1ApdGain)
                    {
                        Workspace.This.IVVM.SelectedGainComModuleR1 = Workspace.This.IVVM.GainComModule[i];

                    }
                    if (Workspace.This.IVVM.GainComModule[i].Value == _Region.R2ApdGain)
                    {
                        Workspace.This.IVVM.SelectedGainComModuleR2 = Workspace.This.IVVM.GainComModule[i];
                    }
                }
                Workspace.This.IVVM.GainTxtModuleL1 = _Region.LPmtGain;
                Workspace.This.IVVM.GainTxtModuleR1 = _Region.R1PmtGain;
                Workspace.This.IVVM.GainTxtModuleR2 = _Region.R2PmtGain;
                Workspace.This.IVVM.LaserCPower = _Region.LPower;
                Workspace.This.IVVM.LaserAPower = _Region.R1Power;
                Workspace.This.IVVM.LaserBPower = _Region.R2Power;
                Workspace.This.IVVM.IsLaserL1Selected = _Region.LLaser;
                Workspace.This.IVVM.IsLaserR1Selected = _Region.R1Laser;
                Workspace.This.IVVM.IsLaserR2Selected = _Region.R2Laser;
            }
        }
        #endregion
    }
}
