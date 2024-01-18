#ifndef  __ABIIMAGEPROC__
#define  __ABIIMAGEPROC__

#ifdef __cplusplus
#define ABIAPI extern "C" __declspec (dllexport)
#else
#define ABIAPI __declspec (dllexport)
#endif

#define P8uC1	0x01
#define P16uC1	0x02
#define P8uC3	0x03
#define P16uC3	0x04
#define P8uC3_1	0x05

#define FlipHorizontal	0x05
#define FlipVertical	0x06

BOOL SaveImgRGB(CString lFileName, PBYTE lBufPtr, int lWidth, int lHeight, BOOL lReverse);
//ABIAPI DWORD WINAPI MVC_BUF_Resize(PBYTE lSrcData, PBYTE lDstData ,int lDepth, int lWidth, int lHeight, int lReverse , double lRatioX, double lRatioY);
ABIAPI DWORD WINAPI MVC_IMG_ToByte(PBYTE lDstData, PCHAR lSrcPath);
ABIAPI DWORD WINAPI MVC_IMG_ToGray(PBYTE lDstData, PCHAR lSrcPath);
ABIAPI DWORD WINAPI MVC_IMG_Init(PCHAR lSrcPath, PBYTE lDstData, int *lDepth, int *lWidth, int *lHeight, int *lReverse);
ABIAPI DWORD WINAPI MVC_BUF_ToGray(PBYTE lSrcData, PBYTE lDstData, int lDepth, int lWidth, int lHeight, int lReverse);

/**
RGB2Gray ���Ҷ�ת��
flip : ��ת
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
iDstStep ��Ŀ�� ͼ��һ���ֽ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_RGB2Gray(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/, int iDstStep);
ABIAPI DWORD WINAPI MVC_BUF_Flip(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/, int iDstStep, int iFlipMode);
/**
Rotate : ͼ����ת
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
iDstStep ��Ŀ�� ͼ��һ���ֽ���
dAngle   : �Ƕȴ�С
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_Rotate(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/, int iDstWidth, int iDstHeight, int iDstStep, double dAngle);
/**
RGBCompose : ��ɫ�ϲ�
********************
lSrcDataR : ����ͼ��Rͨ��
lSrcDataG : ����ͼ��Gͨ��
lSrcDataB : ����ͼ��Bͨ��
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
iDstStep ��Ŀ�� ͼ��һ���ֽ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_RGBCompose(PBYTE lSrcDataR/*IN*/, PBYTE lSrcDataG/*IN*/, PBYTE lSrcDataB/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*out*/, int iDstStep);

/**
RGBExtract : ��ɫ��ȡ
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
iDstStep ��Ŀ�� ͼ��һ���ֽ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_RGBExtract(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstDataR/*OUT*/, 
	PBYTE lDstDataG/*OUT*/, PBYTE lDstDataB/*OUT*/, int iDstStep);
ABIAPI DWORD WINAPI MVC_BUF_RGBExtractSingleChannel(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstDataR/*OUT*/, 
	PBYTE lDstDataG/*OUT*/, PBYTE lDstDataB/*OUT*/, int iDstStep);

/**
Resize : ͼ������
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
iDstStep ��Ŀ�� ͼ��һ���ֽ���
dAngle   : �Ƕȴ�С
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_Resize(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/, int iDstStep, double dXFactor, double dYFactor);

//ABIAPI DWORD WINAPI MVC_BUF_CreateGammaC3LUT(double dMax, double dMin, double dGamma);
ABIAPI DWORD WINAPI MVC_BUF_CreateGammaLUT(double dMax, double dMin, double dGamma, bool bIsInvert);
/**
Scale : λ��任 ֻ֧��16uת8u
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
iMax     : ԭͼ�����ֵ
iMin     ��ԭͼ����Сֵ
lDstData ��Ŀ��ͼ������
iDstStep ��Ŀ�� ͼ��һ���ֽ���
iColorGradation : 0 : R�� �� 1 �� G �� �� 2 �� B �㣬 3 �� ����
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_Scale(
	PBYTE lSrcData/*IN*/,
	int iSrcType,
	int iSrcStep,
	int iWidth,
	int iHeight,
	int iMax,
	int iMin,
	double iGamma,
	PBYTE lDstData/*OUT*/,
	int iDstStep,
	PBYTE lSaturationData,
	int iSaturationStep,
	bool bSaturation,
	int iSaturatonValue,
	int iColorGradation,
	bool bIsInvert);
ABIAPI DWORD WINAPI MVC_BUF_ScaleSingleChannel(
	PBYTE lSrcData/*IN*/, 
	int iSrcType, 
	int iSrcStep, 
	int iWidth, 
	int iHeight, 
	int iMax, 
	int iMin, 
	double iGamma,  
	PBYTE lDstData/*OUT*/, 
	int iDstStep,
	bool bIsInvert
	);
/**
AutoScale : λ��任 ֻ֧��16uת8u
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
iDstStep ��Ŀ�� ͼ��һ���ֽ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_AutoScale(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/, int iDstStep, int* iMaxValue, int* iMinValue, PBYTE lSaturationData, int iSaturationStep, bool bSaturation, int iSaturatonValue, int iColorGradation);

ABIAPI DWORD WINAPI WINAPI MVC_BUF_GetAutoScaleValues(
	PBYTE lSrcData/*IN*/, 
	int iSrcType, 
	int iSrcStep, 
	int iWidth, 
	int iHeight, 
	int* iMaxValue, 
	int* iMinValue
	);

/**
SaturationScale : λ��任 ֻ֧��16uת8u
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
iDstStep ��Ŀ�� ͼ��һ���ֽ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_SaturationScale(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, int iMax, int iMin, double iGamma, PBYTE lDstData/*OUT*/, int iDstStep, int iColor);
ABIAPI DWORD WINAPI MVC_BUF_SaturationMask(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/, int iDstStep, int iMax);
/**
Max : ȡ��ǰͼ�����ֵ
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
iMaxValue: �������ֵ
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_Max(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, unsigned short* iMaxValue/*OUT*/);

/**
Scale : λ��任 ֻ֧��16uת8u
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
iMax     : ԭͼ�����ֵ
iMin     ��ԭͼ����Сֵ
lDstData ��Ŀ��ͼ������
iDstStep ��Ŀ�� ͼ��һ���ֽ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_ScaleSaturation(
	PBYTE lSrcData/*IN*/,
	int iSrcType,
	int iSrcStep,
	int iWidth,
	int iHeight,
	int iMax,
	int iMin,
	double iGamma,
	PBYTE lDstData/*OUT*/,
	int iDstStep,
	int *pColors);


/**
Min : ȡ��ǰͼ����Сֵ
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
iMinValue: �������ֵ
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_Min(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, unsigned short* iMinValue/*OUT*/);

/**
GetPixel : ȡ��ǰͼ��ֵ
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
iXPos, iYPos ������λ��
iValue   : ����ֵ
**/
ABIAPI DWORD WINAPI MVC_BUF_GetPixel(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, int iXPos, int iYPos, int* iValue/*OUT*/);

/**
SetPixel : ���õ�ǰͼ��ֵ
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
iXPos, iYPos ������λ��
iValue   : ����ֵ
**/
ABIAPI DWORD WINAPI MVC_BUF_SetPixel(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, int iXPos, int iYPos, int* iValue/*IN*/);

/**
Invert	 : �ڰ�ת��
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_Invert(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/);

/**
Despeckle: ȥ���˲�
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
iDstStep : Դͼ��һ���ֽ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_Despeckle(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/, int iDstStep);

ABIAPI DWORD WINAPI MVC_BUF_MedianFilter(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/, int iDstStep);

/**
Crop     : �ü�
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
iDstStep : Դͼ��һ���ֽ���
pRect    : �ĸ�ֵ�����飬�ֱ�Ϊx, y, width, height
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_Crop(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, PBYTE lDstData/*OUT*/, int iDstStep, int* pRect);

/**
Despeckle: ȥ���˲�
********************
lSrcData : ����ͼ������
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
lDstData ��Ŀ��ͼ������
iDstStep : Դͼ��һ���ֽ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_TextOverlay(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, TCHAR* pText, int iTextLen, int* pRect, PBYTE lDstData/*OUT*/);


ABIAPI DWORD WINAPI MVC_BUF_TextMask(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lSrcDstData/*OUT*/, int iMaskStep);

ABIAPI DWORD WINAPI MVC_BUF_Animation(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lMaskData/*OUT*/, int iMaskStep, int* pRect, PBYTE lDstData );

/**
ImageAdd : ����ͼ��ӷ����� ������ͼ���ʽ��ͬ��
********************
lSrcDataDst : ����ͼ��,Ҳ��Ϊ���ͼ��
lSrcData ������ͼ��
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_ImageAdd(PBYTE lSrcDataDst/*IN*/, PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight);
/**
MVC_BUF_ImageDivided : ����ͼ��ӷ����� ������ͼ���ʽ��ͬ��
********************
lSrcDstData : ����ͼ��,Ҳ��Ϊ���ͼ��
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/

ABIAPI DWORD WINAPI MVC_BUF_ImageSubC(PBYTE lSrcDstData/*IN*//*OUT*/, int iValue, int iSrcType, int iSrcStep, int iWidth, int iHeight);

ABIAPI DWORD WINAPI MVC_BUF_ImageDivC(PBYTE lSrcDstData/*IN*//*OUT*/, int iDivisor, int iSrcType, int iSrcStep, int iWidth, int iHeight);

/**
MVC_BUF_ImageMulti : ����ͼ��ӷ����� ������ͼ���ʽ��ͬ��
********************
lSrcDstData : ����ͼ��,Ҳ��Ϊ���ͼ��
iSrcType ������ͼ������
iSrcStep ��Դͼ��һ���ֽ���
iWidth   ��ͼ���
iHeight  ��ͼ���
********************
error return ippStsNoErr; no error return !ippStsNoErr
**/
ABIAPI DWORD WINAPI MVC_BUF_ImageMulC(PBYTE lSrcDstData/*IN*//*OUT*/,int iMultiplier, int iSrcType, int iSrcStep, int iWidth, int iHeight);


/********************************************
 * Divided and Add algorithm
 * Src1/10 + Src2/10 = lDstData
 * lSrcData1, lSrcData2 is BYTE type
 * lDstData is Float Type
********************************************/
ABIAPI DWORD WINAPI MVC_BUF_AddDivided16uTo32f(PBYTE lSrcData1, int iSrcStep, float* lSrcData2,  float* lDstData, int iDstStep, int iWidth, int iHeight, int iDivisorConstant);

ABIAPI DWORD WINAPI MVC_BUF_Add16uTo32f(PBYTE srcData, int iSrcStep, float* dstData, int iDstStep, int iWidth, int iHeight);

ABIAPI DWORD WINAPI MVC_BUF_DivC_32f_C1R(float* srcImageBuf, int iDivisor, float* dstImageBuf, int iWidth, int iHeight);

/********************************************
 * convert 32f image to 8u image
********************************************/
ABIAPI DWORD WINAPI MVC_BUF_Scale32fTo16u(float* lSrcData, int iSrcStep, PBYTE lDstData, int iDstStep, int iWidth, int iHeight);


/********************************************
 * sub lSrcData2 from lSrcData1 to lDstData
 * lSrcData1 - lSrcData2 = lDstData
********************************************/
ABIAPI DWORD WINAPI MVC_BUF_Sub16u(PBYTE lSrc, PBYTE lSrcDst, int iSrcStep, int iDstStep, int iWidth, int iHeight);

ABIAPI DWORD WINAPI MVC_BUF_Histogram(PBYTE lSrc, int iSrcStep, int iSrcType, int iWidth, int iHeight, int** pHist, int** pLevels, int nLevels[3]);

ABIAPI DWORD WINAPI MVC_BUF_HistogramEven_16u_C1R(PBYTE lSrc, int iSrcStep, int iWidth, int iHeight, int* pHist, int* pLevels, int nLevels, int nLowerLevel, int nUpperLevel);

ABIAPI DWORD WINAPI MVC_BUF_HistogramEven(PBYTE pSrcData, int iSrcStep, int iSrcType, int iWidth, int iHeight, int** pHist, int** pLevels, int nLevels[3], int nLowerLevel[3], int nUpperLevel[3]);

ABIAPI DWORD WINAPI MVC_BUF_DivAndScale(PBYTE srcImageData, int iSrcStep, PBYTE oprImageData, int iOprStep, float* resImageData, int iDstStep, int iWidth, int iHeight, int iUpperLimit);

ABIAPI DWORD WINAPI MVC_BUF_DivAndScaleWithROI(PBYTE srcImageData, int iSrcStep, PBYTE oprImageData, int iOprStep, float* resImageData, int iDstStep, int iWidth, int iHeight, int iUpperLimit, int iOffset);

ABIAPI DWORD WINAPI MVC_BUF_CopyReplicateBorder(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/, int iDstStep, int iBorderSize);

ABIAPI DWORD WINAPI MVC_BUF_CopyConstBorder(PBYTE lSrcData/*IN*/, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE lDstData/*OUT*/, int iDstStep, int iBorderSize, int constVal);

ABIAPI DWORD WINAPI MVC_BUF_Convert8bppTo16bpp(PBYTE srcData, int iSrcType, int iSrcStep, int iWidth, int iHeight, PBYTE dstData, int iDstStep);

ABIAPI DWORD WINAPI MVC_BUF_MeanStdDev(PBYTE srcData, int iSrcType, int iSrcStep, int iWidth, int iHeight, double* pMean, double* pStdDev);
#endif