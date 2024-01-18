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
    /// ScanRegionUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ScanRegionUserControl : UserControl
    {
        #region pri
        BrushConverter bc = new BrushConverter();
        private string _numberStr;
        private double _x0=0;
        private double _dx=0;
        private double _y0=0;
        private double _dy=0;
        private double _z0=0;
        private double _dz=0;
        private int _Reslution = 10;
        private int _Quality=1;
        private int _lPag=3;
        private int _r1Pag=3;
        private int _r2Pag=3;
        private int _lApdGain=300;
        private int _r1ApdGain=300;
        private int _r2ApdGain=300;
        private int _lPmtGain=4000;
        private int _r1PmtGain=4000;
        private int _r2PmtGain=4000;
        private int _lPower=0;
        private int _r1Power=0;
        private int _r2Power=0;
        private bool _lLaser = false;
        private bool _r1Laser= false;
        private bool _r2Laser= false;
        #endregion

        #region pub
        public delegate void ReceivedScanDataHandle(int currentIndex);
        public event ReceivedScanDataHandle OnScanDataReceived;
        public int IndexCount = 0;
        public int CurretIndexCount = 0;
        private string backColor = "#FFEFEFEF";
        public string BackColor { get => backColor; set => backColor = value; }
        public double X0 { get => _x0; set => _x0 = value; }
        public double Dx { get => _dx; set => _dx = value; }
        public double Y0 { get => _y0; set => _y0 = value; }
        public double Dy { get => _dy; set => _dy = value; }
        public double Z0 { get => _z0; set => _z0 = value; }
        public double Dz { get => _dz; set => _dz = value; }
        public int Reslution { get => _Reslution; set => _Reslution = value; }
        public int Quality { get => _Quality; set => _Quality = value; }
        public int LPag { get => _lPag; set => _lPag = value; }
        public int R1Pag { get => _r1Pag; set => _r1Pag = value; }
        public int R2Pag { get => _r2Pag; set => _r2Pag = value; }
        public int LApdGain { get => _lApdGain; set => _lApdGain = value; }
        public int R1ApdGain { get => _r1ApdGain; set => _r1ApdGain = value; }
        public int R2ApdGain { get => _r2ApdGain; set => _r2ApdGain = value; }
        public int LPmtGain { get => _lPmtGain; set => _lPmtGain = value; }
        public int R1PmtGain { get => _r1PmtGain; set => _r1PmtGain = value; }
        public int R2PmtGain { get => _r2PmtGain; set => _r2PmtGain = value; }
        public int LPower { get => _lPower; set => _lPower = value; }
        public int R1Power { get => _r1Power; set => _r1Power = value; }
        public int R2Power { get => _r2Power; set => _r2Power = value; }
        public bool LLaser { get => _lLaser; set => _lLaser = value; }
        public bool R1Laser { get => _r1Laser; set => _r1Laser = value; }
        public bool R2Laser { get => _r2Laser; set => _r2Laser = value; }
        public string NumberStr { get => _numberStr; set => _numberStr = value; }

        #endregion

        public ScanRegionUserControl()
        {
            InitializeComponent();
        }
        public void GridPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BackColor = "#FFFFFF";
            GridPanel.Background = (SolidColorBrush)bc.ConvertFrom(BackColor);
            CurretIndexCount = (int)this.Tag;
            OnScanDataReceived(CurretIndexCount);
        }
        public void SetLbNumber()
        {
            this.Tag = IndexCount;
            _LbNumber.Content = "#" + IndexCount;
            _numberStr = _ScanStr.Content + " "+_Region.Content + " "+_LbNumber.Content;
        }
        public void SetBackGrondColor()
        {
            BackColor = "#FFEFEFEF";
            GridPanel.Background = (SolidColorBrush)bc.ConvertFrom(BackColor);
        }
    }
}
