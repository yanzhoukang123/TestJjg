﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization; //XmlElement

namespace Azure.ImagingSystem
{
    public enum ImagerType
    {
        c600,
        c500,
        c400,
        c300,
        c200,
        GD1000,
        GDBL1000
    }

    public enum ChannelType
    {
        MONO = 0,
        RED = 1,
        GREEN = 2,
        BLUE = 3,
    };

    public enum TrayType
    {
        None = 0,
        ChemiTray = 1,
        OrangeTray = 2,
    }

    public class FilterWheelType : ICloneable
    {
        public int Position
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class LightingType
    {
        public int Position
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public int LightCode
        {
            get;
            set;
        }

        public int FilterPosition
        {
            get;
            set;
        }
    }

    public class ApertureType
    {
        public int Position
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public double FStop
        {
            get;
            set;
        }
    }

    public class BinningFactorType
    {
        public int Position
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public int VerticalBins
        {
            get;
            set;
        }

        public int HorizontalBins
        {
            get;
            set;
        }

        public BinningFactorType()
        {
        }

        // copy constructor
        public BinningFactorType(BinningFactorType binFactor)
        {
            this.Position = binFactor.Position;
            this.DisplayName = binFactor.DisplayName;
            this.VerticalBins = binFactor.VerticalBins;
            this.HorizontalBins = binFactor.HorizontalBins;
        }

        public BinningFactorType(int position, int horizontalBin, int verticalBin, string displayName)
        {
            this.Position = position;
            this.HorizontalBins = horizontalBin;
            this.VerticalBins = verticalBin;
            this.DisplayName = displayName;
        }
    }

    public class LensSettingType
    {
        public int FilterPosition
        {
            get;
            set;
        }

        //public int LightType
        //{
        //    get;
        //    set;
        //}

        //public int TrayType
        //{
        //    get;
        //    set;
        //}

        //public int ApertureType
        //{
        //    get;
        //    set;
        //}

        public bool IsShelf
        {
            get;
            set;
        }

        public int FocusPosition
        {
            get;
            set;
        }

    }

    public class FovType
    {
        public string AppType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ImageChannelSettings
    {
        #region Private members...

        private double _Exposure = 0.0;
        private bool _IsAutoExposure = false;
        private int _AutoExposureUpperCeiling = 50000;
        private ChannelType _Channel = ChannelType.MONO;
        private int _BinningMode = 1;
        private int _AdGain = 0;
        private int _ReadoutSpeed = 0;
        private int _FrameCount = 1;
        private int _LightType = 0;
        private List<double> _ExposureList = new List<double>();

        #endregion

        #region Public properties...

        public double Exposure
        {
            get { return _Exposure; }
            set { _Exposure = value; }
        }

        public bool IsAutoExposure
        {
            get { return _IsAutoExposure; }
            set
            {
                _IsAutoExposure = value;
                //OnPropertyChanged("IsAutoExposure");
            }
        }

        public int AutoExposureUpperCeiling
        {
            get { return _AutoExposureUpperCeiling; }
            set { _AutoExposureUpperCeiling = value; }
        }

        public ChannelType Channel
        {
            get { return _Channel; }
            set { _Channel = value; }
        }

        public int BinningMode
        {
            get { return _BinningMode; }
            set { _BinningMode = value; }
        }

        public int AdGain
        {
            get { return _AdGain; }
            set { _AdGain = value; }
        }

        public int ReadoutSpeed
        {
            get { return _ReadoutSpeed; }
            set { _ReadoutSpeed = value; }
        }

        public int FrameCount
        {
            get { return _FrameCount; }
            set { _FrameCount = value; }
        }

        public double MaxExposure { get; set; }

        public bool IsAutoExposureToBand { get; set; }

        public bool IsCaptureSelectedRoi { get; set; }

        public bool IsApplyFlatCorrection { get; set; }

        public bool IsApplyDarkCorrection { get; set; }

        public bool IsApplyDynamicDarkCorrection { get; set; }

        public bool IsEnableCcdBcRemoval { get; set; }

        public List<CCDBadColumn> CcdBadColumns { get; set; }

        public double BarrelParamA { get; set; }

        public double BarrelParamB { get; set; }

        public double BarrelParamC { get; set; }

        //public int LedIntensity { get; set; }

        public int LightType
        {
            get { return _LightType; }
            set { _LightType = value; }
        }

        [XmlElement("ExposureList")]
        public List<double> ExposureList
        {
            get { return _ExposureList; }
            set { _ExposureList = value; }
        }

        public List<RgbLedIntensity> RgbIntensities { get; set; }

        #endregion

        #region Constructors...

        public ImageChannelSettings(ChannelType channel = ChannelType.MONO)
        {
            _Channel = channel;
            BarrelParamB = 0.013;
            RgbIntensities = new List<RgbLedIntensity>();
        }

        #endregion

    }

}
