﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azure.ImagingSystem
{
    public static class XMotorSpeedCalibration
    {
        /// <summary>
        /// get the calibrated speed according to detaX and Quality settings
        /// </summary>
        /// <param name="detaX">unit of mm, range from 1 to 304</param>
        /// <param name="quality">1, 2, 4 or 8</param>
        /// <returns></returns>
        public static int GetSpeed(int detaX, int quality)
        {
            int speed = 0;
            if (detaX > 304 || detaX < 1)
            {
                throw new System.ArgumentOutOfRangeException("detaX");
            }
            if (quality != 1 && quality != 2 && quality != 4 && quality != 8)
            {
                throw new System.ArgumentException("quality");
            }
            speed = _CalibratedSpeed[(detaX - 1) * 4 + (int)(Math.Log(quality, 2))];
            return speed;
        }


        public static int GetSpeed(int acc, int startSpeed, int distance, double singleTripTime)
        {
            int topSpeed = 0;
            if(distance <= 0) { return 0; }
            // here we derived a formula A * TopSpeed^2 + B * TopSpeed + C = 0
            double a = 1;
            double b = -1.0 * (2.0 * startSpeed + singleTripTime * acc);
            double c = 1.0 * startSpeed * startSpeed + 1.0 * acc * distance;
            double deta = Math.Sqrt(b * b - 4 * a * c);
            double x1 = (-1.0 * b + deta) / (2 * a);
            double x2 = (-1.0 * b - deta) / (2 * a);

            double t0 = (x1 - startSpeed) / acc;
            double t1 = singleTripTime - (x1 - startSpeed) / acc;
            if (t0 < t1) { return (int)Math.Round(x1); }
            else { return (int)Math.Round(x2); }
        }

        private static int[] _CalibratedSpeed =
        {
            1678,	939,	498,	247,
            3732,	1947,	999,	498,
            5786,	2954,	1500,	748,
            7840,	3961,	2001,	998,
            9894,	4968,	2502,	1248,
            11948,	5975,	3003,	1498,
            14002,	6983,	3504,	1749,
            16056,	7990,	4005,	1999,
            18110,	8997,	4506,	2249,
            20164,	10004,	5007,	2499,
            22218,	11011,	5508,	2749,
            24272,	12019,	6009,	3000,
            26326,	13026,	6510,	3250,
            28380,	14033,	7011,	3500,
            30434,	15040,	7512,	3750,
            32488,	16047,	8013,	4000,
            34542,	17055,	8514,	4251,
            36596,	18062,	9015,	4501,
            38650,	19069,	9516,	4751,
            40671,	20086,	10017,	5001,
            42692,	21103,	10518,	5251,
            44714,	22119,	11018,	5502,
            46735,	23136,	11519,	5752,
            48756,	24153,	12020,	6002,
            50847,	25164,	12522,	6253,
            52938,	26176,	13023,	6504,
            55028,	27187,	13525,	6754,
            57119,	28199,	14026,	7005,
            59210,	29210,	14528,	7256,
            61301,	30208,	15029,	7506,
            63391,	31205,	15531,	7756,
            65482,	32203,	16032,	8006,
            67572,	33200,	16534,	8256,
            69663,	34198,	17035,	8506,
            71757,	35223,	17537,	8757,
            73851,	36248,	18038,	9007,
            75946,	37273,	18540,	9258,
            78040,	38298,	19041,	9508,
            80134,	39323,	19543,	9759,
            82221,	40334,	20044,	10009,
            84308,	41346,	20546,	10259,
            86396,	42357,	21047,	10510,
            88483,	43369,	21549,	10760,
            90570,	44380,	22050,	11010,
            92695,	45398,	22554,	11260,
            94821,	46416,	23058,	11510,
            96946,	47434,	23562,	11760,
            99072,	48452,	24066,	12010,
            101197,	49470,	24570,	12260,
            103322,	50488,	25074,	12510,
            105447,	51506,	25578,	12760,
            107573,	52525,	26082,	13010,
            109698,	53543,	26586,	13260,
            111823,	54561,	27090,	13510,
            113948,	55579,	27594,	13760,
            116074,	56597,	28098,	14010,
            118199,	57615,	28602,	14260,
            120325,	58633,	29106,	14510,
            122450,	59651,	29610,	14760,
            124575,	60669,	30114,	15010,
            126700,	61687,	30618,	15260,
            128826,	62705,	31122,	15510,
            130951,	63723,	31626,	15760,
            133076,	64741,	32130,	16010,
            135296,	65755,	32625,	16261,
            137515,	66769,	33119,	16511,
            139735,	67784,	33614,	16762,
            141954,	68798,	34108,	17012,
            144174,	69812,	34603,	17263,
            146331,	70826,	35111,	17514,
            148488,	71840,	35619,	17764,
            150646,	72854,	36127,	18015,
            152803,	73868,	36635,	18265,
            154960,	74882,	37143,	18516,
            157148,	75896,	37644,	18767,
            159337,	76910,	38145,	19017,
            161525,	77925,	38647,	19268,
            163714,	78939,	39148,	19518,
            165902,	79953,	39649,	19769,
            168090,	80967,	40150,	20020,
            170279,	81981,	40652,	20270,
            172467,	82996,	41153,	20521,
            174656,	84010,	41655,	20771,
            176844,	85024,	42156,	21022,
            179031,	86092,	42660,	21273,
            181217,	87159,	43164,	21524,
            183404,	88227,	43667,	21774,
            185590,	89294,	44171,	22025,
            187777,	90362,	44675,	22276,
            190077,	91357,	45179,	22527,
            192377,	92353,	45683,	22777,
            194677,	93348,	46186,	23028,
            196977,	94344,	46690,	23278,
            199277,	95339,	47194,	23529,
            201520,	96371,	47700,	23781,
            203764,	97402,	48206,	24033,
            206007,	98434,	48711,	24285,
            208251,	99465,	49217,	24537,
            210494,	100497,	49723,	24789,
            212737,	101528,	50225,	25038,
            214980,	102560,	50727,	25288,
            217224,	103591,	51228,	25537,
            219467,	104623,	51730,	25787,
            221710,	105654,	52232,	26036,
            224015,	106683,	52736,	26287,
            226321,	107712,	53241,	26539,
            228626,	108741,	53745,	26790,
            230932,	109770,	54250,	27042,
            233237,	110799,	54754,	27293,
            235529,	111828,	55260,	27545,
            237821,	112857,	55767,	27796,
            240114,	113886,	56273,	28048,
            242406,	114915,	56780,	28299,
            244698,	115944,	57286,	28551,
            247016,	116973,	57788,	28801,
            249335,	118002,	58291,	29051,
            251653,	119031,	58793,	29302,
            253972,	120060,	59296,	29552,
            256290,	121089,	59798,	29802,
            258595,	122118,	60302,	30055,
            260901,	123147,	60807,	30307,
            263206,	124176,	61311,	30560,
            265512,	125205,	61816,	30812,
            267817,	126234,	62320,	31065,
            270184,	127282,	62824,	31312,
            272551,	128331,	63329,	31559,
            274919,	129379,	63833,	31806,
            277286,	130428,	64338,	32053,
            279653,	131476,	64842,	32300,
            282020,	132463,	65347,	32552,
            284387,	133449,	65851,	32804,
            286754,	134436,	66356,	33056,
            289121,	135422,	66860,	33308,
            291488,	136409,	67365,	33560,
            293855,	137519,	67870,	33808,
            296222,	138629,	68374,	34057,
            298590,	139740,	68879,	34305,
            300957,	140850,	69383,	34554,
            303324,	141960,	69888,	34802,
            305691,	143008,	70392,	35053,
            308058,	144057,	70897,	35304,
            310426,	145105,	71401,	35554,
            312793,	146154,	71906,	35805,
            315160,	147202,	72410,	36056,
            317591,	148244,	72917,	36307,
            320022,	149286,	73424,	36558,
            322453,	150328,	73931,	36809,
            324884,	151370,	74438,	37060,
            327315,	152412,	74945,	37311,
            329491,	153454,	75452,	37562,
            331668,	154496,	75959,	37813,
            333844,	155538,	76466,	38063,
            336021,	156580,	76973,	38314,
            338197,	157622,	77480,	38565,
            340883,	158664,	77987,	38816,
            343568,	159706,	78494,	39067,
            346254,	160747,	79001,	39318,
            348939,	161789,	79508,	39569,
            351625,	162831,	80015,	39820,
            354056,	163873,	80522,	40071,
            356487,	164915,	81029,	40322,
            358918,	165957,	81536,	40573,
            361349,	166999,	82043,	40824,
            363780,	168041,	82550,	41075,
            366271,	169088,	83055,	41327,
            368762,	170134,	83560,	41579,
            371253,	171181,	84065,	41830,
            373744,	172227,	84570,	42082,
            376235,	173274,	85075,	42334,
            378434,	174254,	85580,	42586,
            380634,	175233,	86085,	42838,
            382833,	176213,	86590,	43089,
            385033,	177192,	87095,	43341,
            387232,	178172,	87600,	43593,
            390015,	179286,	88100,	43845,
            392797,	180399,	88601,	44097,
            395580,	181513,	89101,	44349,
            398362,	182626,	89602,	44601,
            401145,	183740,	90102,	44853,
            403636,	184787,	90612,	45104,
            406127,	185833,	91121,	45354,
            408618,	186880,	91631,	45605,
            411109,	187926,	92140,	45855,
            413600,	188973,	92650,	46106,
            416180,	190041,	93157,	46356,
            418760,	191109,	93664,	46606,
            421340,	192177,	94171,	46855,
            423920,	193245,	94678,	47105,
            426500,	194313,	95185,	47355,
            429080,	195381,	95692,	47607,
            431660,	196449,	96199,	47859,
            434240,	197517,	96706,	48110,
            436820,	198585,	97213,	48362,
            439400,	199653,	97720,	48614,
            441980,	200721,	98227,	48864,
            444560,	201789,	98734,	49114,
            447140,	202856,	99241,	49364,
            449720,	203924,	99748,	49614,
            452300,	204992,	100255,	49864,
            454880,	206060,	100762,	50114,
            457460,	207128,	101269,	50364,
            460040,	208196,	101776,	50615,
            462620,	209264,	102283,	50865,
            465200,	210332,	102790,	51115,
            467884,	211372,	103301,	51367,
            470568,	212412,	103811,	51619,
            473253,	213451,	104322,	51870,
            475937,	214491,	104832,	52122,
            478621,	215531,	105343,	52374,
            481305,	216571,	105853,	52626,
            483989,	217610,	106364,	52878,
            486673,	218650,	106874,	53131,
            489357,	219689,	107385,	53383,
            492041,	220729,	107895,	53635,
            494725,	221769,	108406,	53886,
            497409,	222809,	108916,	54137,
            500094,	223848,	109427,	54389,
            502778,	224888,	109937,	54640,
            505462,	225928,	110448,	54891,
            508146,	226968,	110958,	55143,
            510830,	228007,	111469,	55395,
            513514,	229047,	111979,	55646,
            516198,	230086,	112490,	55898,
            518882,	231126,	113000,	56150,
            521661,	232202,	113506,	56400,
            524439,	233278,	114012,	56651,
            527218,	234355,	114518,	56901,
            529996,	235431,	115024,	57152,
            532775,	236507,	115530,	57402,
            535553,	237583,	116038,	57652,
            538332,	238659,	116546,	57903,
            541110,	239735,	117054,	58153,
            543889,	240811,	117562,	58404,
            546667,	241887,	118070,	58654,
            549446,	242963,	118577,	58904,
            552224,	244039,	119084,	59154,
            555003,	245116,	119591,	59405,
            557781,	246192,	120098,	59655,
            560560,	247268,	120605,	59905,
            563338,	248344,	121112,	60155,
            566117,	249420,	121619,	60406,
            568895,	250496,	122126,	60656,
            571674,	251572,	122633,	60907,
            574452,	252648,	123140,	61157,
            577327,	253784,	123655,	61408,
            580203,	254920,	124171,	61660,
            583078,	256057,	124686,	61911,
            585954,	257193,	125202,	62163,
            588829,	258329,	125717,	62414,
            591704,	259388,	126214,	62666,
            594580,	260447,	126710,	62919,
            597455,	261507,	127207,	63171,
            600331,	262566,	127703,	63424,
            603206,	263625,	128200,	63676,
            606081,	264683,	128706,	63926,
            608957,	265741,	129212,	64176,
            611832,	266800,	129718,	64427,
            614708,	267858,	130224,	64677,
            617583,	268916,	130730,	64927,
            620458,	270001,	131236,	65178,
            623334,	271085,	131742,	65430,
            626209,	272170,	132248,	65681,
            629085,	273254,	132754,	65933,
            631960,	274339,	133260,	66184,
            634929,	275417,	133772,	66435,
            637897,	276495,	134283,	66686,
            640866,	277573,	134795,	66938,
            643834,	278651,	135306,	67189,
            646803,	279729,	135818,	67440,
            649771,	280807,	136329,	67690,
            652740,	281885,	136841,	67939,
            655708,	282964,	137352,	68189,
            658677,	284042,	137864,	68438,
            661645,	285120,	138375,	68688,
            664614,	286198,	138887,	68941,
            667582,	287276,	139398,	69193,
            670551,	288354,	139910,	69446,
            673519,	289432,	140421,	69698,
            676488,	290510,	140933,	69951,
            679456,	291588,	141444,	70202,
            682425,	292666,	141956,	70453,
            685393,	293744,	142467,	70704,
            688362,	294822,	142979,	70955,
            691330,	295900,	143490,	71206,
            694298, 296978, 144000, 71458,
            697267, 298056, 144510, 71710,
            700235, 299134, 145020, 71961,
            703204, 300212, 145530, 72213,
            706172, 301290, 146040, 72465,
            709140, 302368, 146567, 72715,
            712107, 303446, 147094, 72965,
            715075, 304524, 147621, 73216,
            718042, 305602, 148148, 73466,
            721010, 306680, 148675, 73716,
            724531, 307758, 149172, 73971,
            728052, 308836, 149669, 74226,
            731573, 309914, 150167, 74480,
            735094, 310992, 150664, 74735,
            738615, 312070, 151161, 74990,
            742273, 313148, 151672, 75237,
            745932, 314226, 152184, 75484,
            749590, 315304, 152695, 75732,
            753249, 316382, 153207, 75979,
            756907, 317460, 153718, 76226,
        };

        /// <summary>
        /// 
        /// </summary>
        private static int[,] _AvocadoCalibratedSpeed =
        {
            { 1678,   939,    498,    247 },
            { 3732,   1947,   999,    498 },
            { 5786,   2954,   1500,   748 },
            { 7840,   3961,   2001,   998 },
            { 9894,   4968,   2502,   1248 },
            { 11948,  5975,   3003,   1498 },
            { 14002,  6983,   3504,   1749 },
            { 16056,  7990,   4005,   1999 },
            { 18110,  8997,   4506,   2249 },
            { 20164,  10004,  5007,   2499 },
            { 22218,  11011,  5508,   2749 },
            { 24272,  12019,  6009,   3000 },
            { 26326,  13026,  6510,   3250 },
            { 28380,  14033,  7011,   3500 },
            { 30434,  15040,  7512,   3750 },
            { 32488,  16047,  8013,   4000 },
            { 34542,  17055,  8514,   4251 },
            { 36596,  18062,  9015,   4501 },
            { 38650,  19069,  9516,   4751 },
            { 40671,  20086,  10017,  5001 },
            { 42692,  21103,  10518,  5251 },
            { 44714,  22119,  11018,  5502 },
            { 46735,  23136,  11519,  5752 },
            { 48756,  24153,  12020,  6002 },
            { 50847,  25164,  12522,  6253 },
            { 52938,  26176,  13023,  6504 },
            { 55028,  27187,  13525,  6754 },
            { 57119,  28199,  14026,  7005 },
            { 59210,  29210,  14528,  7256 },
            { 61301,  30208,  15029,  7506 },
            { 63391,  31205,  15531,  7756 },
            { 65482,  32203,  16032,  8006 },
            { 67572,  33200,  16534,  8256 },
            { 69663,  34198,  17035,  8506 },
            { 71757,  35223,  17537,  8757 },
            { 73851,  36248,  18038,  9007 },
            { 75946,  37273,  18540,  9258 },
            { 78040,  38298,  19041,  9508 },
            { 80134,  39323,  19543,  9759 },
            { 82221,  40334,  20044,  10009 },
            { 84308,  41346,  20546,  10259 },
            { 86396,  42357,  21047,  10510 },
            { 88483,  43369,  21549,  10760 },
            { 90570,  44380,  22050,  11010 },
            { 92695,  45398,  22554,  11260 },
            { 94821,  46416,  23058,  11510 },
            { 96946,  47434,  23562,  11760 },
            { 99072,  48452,  24066,  12010 },
            { 101197, 49470,  24570,  12260 },
            { 103322, 50488,  25074,  12510 },
            { 105447, 51506,  25578,  12760 },
            { 107573, 52525,  26082,  13010 },
            { 109698, 53543,  26586,  13260 },
            { 111823, 54561,  27090,  13510 },
            { 113948, 55579,  27594,  13760 },
            { 116074, 56597,  28098,  14010 },
            { 118199, 57615,  28602,  14260 },
            { 120325, 58633,  29106,  14510 },
            { 122450, 59651,  29610,  14760 },
            { 124575, 60669,  30114,  15010 },
            { 126700, 61687,  30618,  15260 },
            { 128826, 62705,  31122,  15510 },
            { 130951, 63723,  31626,  15760 },
            { 133076, 64741,  32130,  16010 },
            { 135296, 65755,  32625,  16261 },
            { 137515, 66769,  33119,  16511 },
            { 139735, 67784,  33614,  16762 },
            { 141954, 68798,  34108,  17012 },
            { 144174, 69812,  34603,  17263 },
            { 146331, 70826,  35111,  17514 },
            { 148488, 71840,  35619,  17764 },
            { 150646, 72854,  36127,  18015 },
            { 152803, 73868,  36635,  18265 },
            { 154960, 74882,  37143,  18516 },
            { 157148, 75896,  37644,  18767 },
            { 159337, 76910,  38145,  19017 },
            { 161525, 77925,  38647,  19268 },
            { 163714, 78939,  39148,  19518 },
            { 165902, 79953,  39649,  19769 },
            { 168090, 80967,  40150,  20020 },
            { 170279, 81981,  40652,  20270 },
            { 172467, 82996,  41153,  20521 },
            { 174656, 84010,  41655,  20771 },
            { 176844, 85024,  42156,  21022 },
            { 179031, 86092,  42660,  21273 },
            { 181217, 87159,  43164,  21524 },
            { 183404, 88227,  43667,  21774 },
            { 185590, 89294,  44171,  22025 },
            { 187777, 90362,  44675,  22276 },
            { 190077, 91357,  45179,  22527 },
            { 192377, 92353,  45683,  22777 },
            { 194677, 93348,  46186,  23028 },
            { 196977, 94344,  46690,  23278 },
            { 199277, 95339,  47194,  23529 },
            { 201520, 96371,  47700,  23781 },
            { 203764, 97402,  48206,  24033 },
            { 206007, 98434,  48711,  24285 },
            { 208251, 99465,  49217,  24537 },
            { 210494, 100497, 49723,  24789 },
            { 212737, 101528, 50225,  25038 },
            { 214980, 102560, 50727,  25288 },
            { 217224, 103591, 51228,  25537 },
            { 219467, 104623, 51730,  25787 },
            { 221710, 105654, 52232,  26036 },
            { 224015, 106683, 52736,  26287 },
            { 226321, 107712, 53241,  26539 },
            { 228626, 108741, 53745,  26790 },
            { 230932, 109770, 54250,  27042 },
            { 233237, 110799, 54754,  27293 },
            { 235529, 111828, 55260,  27545 },
            { 237821, 112857, 55767,  27796 },
            { 240114, 113886, 56273,  28048 },
            { 242406, 114915, 56780,  28299 },
            { 244698, 115944, 57286,  28551 },
            { 247016, 116973, 57788,  28801 },
            { 249335, 118002, 58291,  29051 },
            { 251653, 119031, 58793,  29302 },
            { 253972, 120060, 59296,  29552 },
            { 256290, 121089, 59798,  29802 },
            { 258595, 122118, 60302,  30055 },
            { 260901, 123147, 60807,  30307 },
            { 263206, 124176, 61311,  30560 },
            { 265512, 125205, 61816,  30812 },
            { 267817, 126234, 62320,  31065 },
            { 270184, 127282, 62824,  31312 },
            { 272551, 128331, 63329,  31559 },
            { 274919, 129379, 63833,  31806 },
            { 277286, 130428, 64338,  32053 },
            { 279653, 131476, 64842,  32300 },
            { 282020, 132463, 65347,  32552 },
            { 284387, 133449, 65851,  32804 },
            { 286754, 134436, 66356,  33056 },
            { 289121, 135422, 66860,  33308 },
            { 291488, 136409, 67365,  33560 },
            { 293855, 137519, 67870,  33808 },
            { 296222, 138629, 68374,  34057 },
            { 298590, 139740, 68879,  34305 },
            { 300957, 140850, 69383,  34554 },
            { 303324, 141960, 69888,  34802 },
            { 305691, 143008, 70392,  35053 },
            { 308058, 144057, 70897,  35304 },
            { 310426, 145105, 71401,  35554 },
            { 312793, 146154, 71906,  35805 },
            { 315160, 147202, 72410,  36056 },
            { 317591, 148244, 72917,  36307 },
            { 320022, 149286, 73424,  36558 },
            { 322453, 150328, 73931,  36809 },
            { 324884, 151370, 74438,  37060 },
            { 327315, 152412, 74945,  37311 },
            { 329491, 153454, 75452,  37562 },
            { 331668, 154496, 75959,  37813 },
            { 333844, 155538, 76466,  38063 },
            { 336021, 156580, 76973,  38314 },
            { 338197, 157622, 77480,  38565 },
            { 340883, 158664, 77987,  38816 },
            { 343568, 159706, 78494,  39067 },
            { 346254, 160747, 79001,  39318 },
            { 348939, 161789, 79508,  39569 },
            { 351625, 162831, 80015,  39820 },
            { 354056, 163873, 80522,  40071 },
            { 356487, 164915, 81029,  40322 },
            { 358918, 165957, 81536,  40573 },
            { 361349, 166999, 82043,  40824 },
            { 363780, 168041, 82550,  41075 },
            { 366271, 169088, 83055,  41327 },
            { 368762, 170134, 83560,  41579 },
            { 371253, 171181, 84065,  41830 },
            { 373744, 172227, 84570,  42082 },
            { 376235, 173274, 85075,  42334 },
            { 378434, 174254, 85580,  42586 },
            { 380634, 175233, 86085,  42838 },
            { 382833, 176213, 86590,  43089 },
            { 385033, 177192, 87095,  43341 },
            { 387232, 178172, 87600,  43593 },
            { 390015, 179286, 88100,  43845 },
            { 392797, 180399, 88601,  44097 },
            { 395580, 181513, 89101,  44349 },
            { 398362, 182626, 89602,  44601 },
            { 401145, 183740, 90102,  44853 },
            { 403636, 184787, 90612,  45104 },
            { 406127, 185833, 91121,  45354 },
            { 408618, 186880, 91631,  45605 },
            { 411109, 187926, 92140,  45855 },
            { 413600, 188973, 92650,  46106 },
            { 416180, 190041, 93157,  46356 },
            { 418760, 191109, 93664,  46606 },
            { 421340, 192177, 94171,  46855 },
            { 423920, 193245, 94678,  47105 },
            { 426500, 194313, 95185,  47355 },
            { 429080, 195381, 95692,  47607 },
            { 431660, 196449, 96199,  47859 },
            { 434240, 197517, 96706,  48110 },
            { 436820, 198585, 97213,  48362 },
            { 439400, 199653, 97720,  48614 },
            { 441980, 200721, 98227,  48864 },
            { 444560, 201789, 98734,  49114 },
            { 447140, 202856, 99241,  49364 },
            { 449720, 203924, 99748,  49614 },
            { 452300, 204992, 100255, 49864 },
            { 454880, 206060, 100762, 50114 },
            { 457460, 207128, 101269, 50364 },
            { 460040, 208196, 101776, 50615 },
            { 462620, 209264, 102283, 50865 },
            { 465200, 210332, 102790, 51115 },
            { 467884, 211372, 103301, 51367 },
            { 470568, 212412, 103811, 51619 },
            { 473253, 213451, 104322, 51870 },
            { 475937, 214491, 104832, 52122 },
            { 478621, 215531, 105343, 52374 },
            { 481305, 216571, 105853, 52626 },
            { 483989, 217610, 106364, 52878 },
            { 486673, 218650, 106874, 53131 },
            { 489357, 219689, 107385, 53383 },
            { 492041, 220729, 107895, 53635 },
            { 494725, 221769, 108406, 53886 },
            { 497409, 222809, 108916, 54137 },
            { 500094, 223848, 109427, 54389 },
            { 502778, 224888, 109937, 54640 },
            { 505462, 225928, 110448, 54891 },
            { 508146, 226968, 110958, 55143 },
            { 510830, 228007, 111469, 55395 },
            { 513514, 229047, 111979, 55646 },
            { 516198, 230086, 112490, 55898 },
            { 518882, 231126, 113000, 56150 },
            { 521661, 232202, 113506, 56400 },
            { 524439, 233278, 114012, 56651 },
            { 527218, 234355, 114518, 56901 },
            { 529996, 235431, 115024, 57152 },
            { 532775, 236507, 115530, 57402 },
            { 535553, 237583, 116038, 57652 },
            { 538332, 238659, 116546, 57903 },
            { 541110, 239735, 117054, 58153 },
            { 543889, 240811, 117562, 58404 },
            { 546667, 241887, 118070, 58654 },
            { 549446, 242963, 118577, 58904 },
            { 552224, 244039, 119084, 59154 },
            { 555003, 245116, 119591, 59405 },
            { 557781, 246192, 120098, 59655 },
            { 560560, 247268, 120605, 59905 },
            { 563338, 248344, 121112, 60155 },
            { 566117, 249420, 121619, 60406 },
            { 568895, 250496, 122126, 60656 },
            { 571674, 251572, 122633, 60907 },
            { 574452, 252648, 123140, 61157 },
            { 577327, 253784, 123655, 61408 },
            { 580203, 254920, 124171, 61660 },
            { 583078, 256057, 124686, 61911 },
            { 585954, 257193, 125202, 62163 },
            { 588829, 258329, 125717, 62414 },
            { 591704, 259388, 126214, 62666 },
            { 594580, 260447, 126710, 62919 },
            { 597455, 261507, 127207, 63171 },
            { 600331, 262566, 127703, 63424 },
            { 603206, 263625, 128200, 63676 },
            { 606081, 264683, 128706, 63926 },
            { 608957, 265741, 129212, 64176 },
            { 611832, 266800, 129718, 64427 },
            { 614708, 267858, 130224, 64677 },
            { 617583, 268916, 130730, 64927 },
            { 620458, 270001, 131236, 65178 },
            { 623334, 271085, 131742, 65430 },
            { 626209, 272170, 132248, 65681 },
            { 629085, 273254, 132754, 65933 },
            { 631960, 274339, 133260, 66184 },
            { 634929, 275417, 133772, 66435 },
            { 637897, 276495, 134283, 66686 },
            { 640866, 277573, 134795, 66938 },
            { 643834, 278651, 135306, 67189 },
            { 646803, 279729, 135818, 67440 },
            { 649771, 280807, 136329, 67690 },
            { 652740, 281885, 136841, 67939 },
            { 655708, 282964, 137352, 68189 },
            { 658677, 284042, 137864, 68438 },
            { 661645, 285120, 138375, 68688 },
            { 664614, 286198, 138887, 68941 },
            { 667582, 287276, 139398, 69193 },
            { 670551, 288354, 139910, 69446 },
            { 673519, 289432, 140421, 69698 },
            { 676488, 290510, 140933, 69951 },
            { 679456, 291588, 141444, 70202 },
            { 682425, 292666, 141956, 70453 },
            { 685393, 293744, 142467, 70704 },
            { 688362, 294822, 142979, 70955 },
            { 691330, 295900, 143490, 71206 },
            { 694298, 296978, 144000, 71458 },
            { 697267, 298056, 144510, 71710 },
            { 700235, 299134, 145020, 71961 },
            { 703204, 300212, 145530, 72213 },
            { 706172, 301290, 146040, 72465 },
            { 709140, 302368, 146567, 72715 },
            { 712107, 303446, 147094, 72965 },
            { 715075, 304524, 147621, 73216 },
            { 718042, 305602, 148148, 73466 },
            { 721010, 306680, 148675, 73716 },
            { 724531, 307758, 149172, 73971 },
            { 728052, 308836, 149669, 74226 },
            { 731573, 309914, 150167, 74480 },
            { 735094, 310992, 150664, 74735 },
            { 738615, 312070, 151161, 74990 },
            { 742273, 313148, 151672, 75237 },
            { 745932, 314226, 152184, 75484 },
            { 749590, 315304, 152695, 75732 },
            { 753249, 316382, 153207, 75979 },
            { 756907, 317460, 153718, 76226 },
        };

    }
}
