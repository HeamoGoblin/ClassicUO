﻿#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ClassicUO.Data;
using ClassicUO.Game;
using ClassicUO.Game.Data;
using ClassicUO.Renderer;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;

namespace ClassicUO.IO.Resources
{
    internal unsafe class AnimationsLoader : UOFileLoader
    {
        private static AnimationsLoader _instance;

        private readonly AnimationCache _animationCache = new AnimationCache();
        private readonly Dictionary<ushort, Rectangle> _animDimensionCache = new Dictionary<ushort, Rectangle>();
        private IntPtr _bufferCachePtr = Marshal.AllocHGlobal(0x800000);
        private readonly Dictionary<ushort, Dictionary<ushort, EquipConvData>> _equipConv = new Dictionary<ushort, Dictionary<ushort, EquipConvData>>();
        private readonly UOFileMul[] _files = new UOFileMul[5];
        private readonly UOFileUop[] _filesUop = new UOFileUop[4];

        private readonly DataReader _reader = new DataReader();
        private readonly UOPFrameData[] _uop_frame_pixels_offsets = new UOPFrameData[1000];
        private readonly LinkedList<(ushort, byte, byte, bool)> _usedTextures = new LinkedList<(ushort, byte, byte, bool)>();

        private AnimationsLoader()
        {
        }

        public static AnimationsLoader Instance => _instance ?? (_instance = new AnimationsLoader());

        //public IndexAnimation[] DataIndex { get; } = new IndexAnimation[Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT];

        public IReadOnlyDictionary<ushort, Dictionary<ushort, EquipConvData>> EquipConversions => _equipConv;

        public List<Tuple<ushort, byte>>[] GroupReplaces { get; } = new List<Tuple<ushort, byte>>[2]
        {
            new List<Tuple<ushort, byte>>(), new List<Tuple<ushort, byte>>()
        };

        public SittingInfoData[] SittingInfos { get; } =
        {
            new SittingInfoData
            (
                0x0459,
                0,
                -1,
                4,
                -1,
                2,
                2,
                false
            ),
            new SittingInfoData
            (
                0x045A,
                -1,
                2,
                -1,
                6,
                2,
                2,
                false
            ),
            new SittingInfoData
            (
                0x045B,
                0,
                -1,
                4,
                -1,
                2,
                2,
                false
            ),
            new SittingInfoData
            (
                0x045C,
                -1,
                2,
                -1,
                6,
                2,
                2,
                false
            ),
            new SittingInfoData
            (
                0x0A2A,
                0,
                2,
                4,
                6,
                -4,
                -4,
                false
            ),
            new SittingInfoData
            (
                0x0A2B,
                0,
                2,
                4,
                6,
                -8,
                -8,
                false
            ),
            new SittingInfoData
            (
                0x0B2C,
                -1,
                2,
                -1,
                6,
                2,
                2,
                false
            ),
            new SittingInfoData
            (
                0x0B2D,
                0,
                -1,
                4,
                -1,
                2,
                2,
                false
            ),
            new SittingInfoData
            (
                0x0B2E,
                4,
                4,
                4,
                4,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x0B2F,
                2,
                2,
                2,
                2,
                6,
                6,
                false
            ),
            new SittingInfoData
            (
                0x0B30,
                6,
                6,
                6,
                6,
                -8,
                8,
                true
            ),
            new SittingInfoData
            (
                0x0B31,
                0,
                0,
                0,
                0,
                0,
                4,
                true
            ),
            new SittingInfoData
            (
                0x0B32,
                4,
                4,
                4,
                4,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x0B33,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x0B4E,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x0B4F,
                4,
                4,
                4,
                4,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x0B50,
                0,
                0,
                0,
                0,
                0,
                0,
                true
            ),
            new SittingInfoData
            (
                0x0B51,
                6,
                6,
                6,
                6,
                0,
                0,
                true
            ),
            new SittingInfoData
            (
                0x0B52,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x0B53,
                4,
                4,
                4,
                4,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x0B54,
                0,
                0,
                0,
                0,
                0,
                0,
                true
            ),
            new SittingInfoData
            (
                0x0B55,
                6,
                6,
                6,
                6,
                0,
                0,
                true
            ),
            new SittingInfoData
            (
                0x0B56,
                2,
                2,
                2,
                2,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x0B57,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x0B58,
                6,
                6,
                6,
                6,
                0,
                8,
                true
            ),
            new SittingInfoData
            (
                0x0B59,
                0,
                0,
                0,
                0,
                0,
                8,
                true
            ),
            new SittingInfoData
            (
                0x0B5A,
                2,
                2,
                2,
                2,
                8,
                8,
                false
            ),
            new SittingInfoData
            (
                0x0B5B,
                4,
                4,
                4,
                4,
                8,
                8,
                false
            ),
            new SittingInfoData
            (
                0x0B5C,
                0,
                0,
                0,
                0,
                0,
                8,
                true
            ),
            new SittingInfoData
            (
                0x0B5D,
                6,
                6,
                6,
                6,
                0,
                8,
                true
            ),
            new SittingInfoData
            (
                0x0B5E,
                0,
                2,
                4,
                6,
                -8,
                -8,
                false
            ),
            new SittingInfoData
            (
                0x0B5F,
                -1,
                2,
                -1,
                6,
                3,
                14,
                false
            ),
            new SittingInfoData
            (
                0x0B60,
                -1,
                2,
                -1,
                6,
                3,
                14,
                false
            ),
            new SittingInfoData
            (
                0x0B61,
                -1,
                2,
                -1,
                6,
                3,
                14,
                false
            ),
            new SittingInfoData
            (
                0x0B62,
                -1,
                2,
                -1,
                6,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x0B63,
                -1,
                2,
                -1,
                6,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x0B64,
                -1,
                2,
                -1,
                6,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x0B65,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x0B66,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x0B67,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x0B68,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x0B69,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x0B6A,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x0B91,
                4,
                4,
                4,
                4,
                6,
                6,
                false
            ),
            new SittingInfoData
            (
                0x0B92,
                4,
                4,
                4,
                4,
                6,
                6,
                false
            ),
            new SittingInfoData
            (
                0x0B93,
                2,
                2,
                2,
                2,
                6,
                6,
                false
            ),
            new SittingInfoData
            (
                0x0B94,
                2,
                2,
                2,
                2,
                6,
                6,
                false
            ),
            new SittingInfoData
            (
                0x0CF3,
                -1,
                2,
                -1,
                6,
                2,
                8,
                false
            ),
            new SittingInfoData
            (
                0x0CF4,
                -1,
                2,
                -1,
                6,
                2,
                8,
                false
            ),
            new SittingInfoData
            (
                0x0CF6,
                0,
                -1,
                4,
                -1,
                2,
                8,
                false
            ),
            new SittingInfoData
            (
                0x0CF7,
                0,
                -1,
                4,
                -1,
                2,
                8,
                false
            ),
            new SittingInfoData
            (
                0x11FC,
                0,
                2,
                4,
                6,
                2,
                7,
                false
            ),
            new SittingInfoData
            (
                0x1218,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x1219,
                2,
                2,
                2,
                2,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x121A,
                0,
                0,
                0,
                0,
                0,
                8,
                true
            ),
            new SittingInfoData
            (
                0x121B,
                6,
                6,
                6,
                6,
                0,
                8,
                true
            ),
            new SittingInfoData
            (
                0x1527,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x1771,
                0,
                2,
                4,
                6,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x1776,
                0,
                2,
                4,
                6,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x1779,
                0,
                2,
                4,
                6,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x1DC7,
                -1,
                2,
                -1,
                6,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DC8,
                -1,
                2,
                -1,
                6,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DC9,
                -1,
                2,
                -1,
                6,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DCA,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DCB,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DCC,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DCD,
                -1,
                2,
                -1,
                6,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DCE,
                -1,
                2,
                -1,
                6,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DCF,
                -1,
                2,
                -1,
                6,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DD0,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DD1,
                0,
                -1,
                4,
                -1,
                3,
                10,
                false
            ),
            new SittingInfoData
            (
                0x1DD2,
                -1,
                2,
                -1,
                6,
                3,
                10,
                false
            ),

            new SittingInfoData
            (
                0x2A58,
                4,
                4,
                4,
                4,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x2A59,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x2A5A,
                0,
                2,
                4,
                6,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x2A5B,
                0,
                2,
                4,
                6,
                10,
                10,
                false
            ),
            new SittingInfoData
            (
                0x2A7F,
                0,
                2,
                4,
                6,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x2A80,
                0,
                2,
                4,
                6,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x2DDF,
                0,
                2,
                4,
                6,
                2,
                2,
                false
            ),
            new SittingInfoData
            (
                0x2DE0,
                0,
                2,
                4,
                6,
                2,
                2,
                false
            ),
            new SittingInfoData
            (
                0x2DE3,
                2,
                2,
                2,
                2,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x2DE4,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x2DE5,
                6,
                6,
                6,
                6,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x2DE6,
                0,
                0,
                0,
                0,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x2DEB,
                0,
                0,
                0,
                0,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x2DEC,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x2DED,
                2,
                2,
                2,
                2,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x2DEE,
                6,
                6,
                6,
                6,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x2DF5,
                0,
                2,
                4,
                6,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x2DF6,
                0,
                2,
                4,
                6,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x3088,
                0,
                2,
                4,
                6,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x3089,
                0,
                2,
                4,
                6,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x308A,
                0,
                2,
                4,
                6,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x308B,
                0,
                2,
                4,
                6,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x35ED,
                0,
                2,
                4,
                6,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x35EE,
                0,
                2,
                4,
                6,
                0,
                0,
                false
            ),

            new SittingInfoData
            (
                0x3DFF,
                0,
                -1,
                4,
                -1,
                2,
                2,
                false
            ),
            new SittingInfoData
            (
                0x3E00,
                -1,
                2,
                -1,
                6,
                2,
                2,
                false
            ),

            // TODO: new expansion
            new SittingInfoData
            (
                0x4C8D,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C8E,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C8F,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),

            new SittingInfoData
            (
                0x4C1E,
                2,
                2,
                2,
                2,
                6,
                6,
                false
            ),
            //new SittingInfoData(0x4C1F, 2, 2, 2, 2, 6, 6, false),

            new SittingInfoData
            (
                0x4C8B,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x4C8C,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x4C8D,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C8E,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C8F,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),

            new SittingInfoData
            (
                0x4C1E,
                2,
                2,
                2,
                2,
                6,
                6,
                false
            ),
            new SittingInfoData
            (
                0x4C80,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C81,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x4C82,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C83,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C84,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x4C85,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x4C86,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C87,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C88,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x4C89,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x4C8A,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x4C8B,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x4C8C,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ),
            new SittingInfoData
            (
                0x4C8D,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C8E,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            new SittingInfoData
            (
                0x4C8F,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ),
            //new SittingInfoData(0x4C1F, 2, 2, 2, 2, 6, 6, false),

            // thanks Tony!
            new SittingInfoData
            (
                0xA05F,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ), // EAST ONLY
            new SittingInfoData
            (
                0xA05E,
                0,
                0,
                0,
                0,
                4,
                4,
                false
            ), // NORTH ONLY
            new SittingInfoData
            (
                0xA05D,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ), // SOUTH ONLY
            new SittingInfoData
            (
                0xA05C,
                6,
                6,
                6,
                6,
                6,
                4,
                false
            ), // WEST ONLY

            new SittingInfoData
            (
                0x9EA2,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ), // EAST ONLY
            new SittingInfoData
            (
                0x9EA1,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ), // SOUTH ONLY
            new SittingInfoData
            (
                0x9E9F,
                0,
                0,
                0,
                0,
                4,
                4,
                false
            ), // NORTH ONLY
            new SittingInfoData
            (
                0x9EA0,
                6,
                6,
                6,
                6,
                6,
                4,
                false
            ), // WEST ONLY

            new SittingInfoData
            (
                0x9E91,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ), // SOUTH ONLY
            new SittingInfoData
            (
                0x9E90,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ), // EAST ONLY
            new SittingInfoData
            (
                0x9E8F,
                6,
                6,
                6,
                6,
                6,
                4,
                false
            ), // WEST ONLY
            new SittingInfoData
            (
                0x9E8E,
                0,
                0,
                0,
                0,
                4,
                4,
                false
            ), // NORTH ONLY

            new SittingInfoData
            (
                0x9C62,
                0,
                0,
                0,
                0,
                4,
                4,
                false
            ), // NORTH ONLY
            new SittingInfoData
            (
                0x9C61,
                0,
                0,
                0,
                0,
                4,
                4,
                false
            ), // NORTH ONLY
            new SittingInfoData
            (
                0x9C60,
                0,
                0,
                0,
                0,
                4,
                4,
                false
            ), // NORTH ONLY

            new SittingInfoData
            (
                0x9C5F,
                6,
                6,
                6,
                6,
                6,
                4,
                false
            ), // WEST ONLY
            new SittingInfoData
            (
                0x9C5E,
                6,
                6,
                6,
                6,
                6,
                4,
                false
            ), // WEST ONLY
            new SittingInfoData
            (
                0x9C5D,
                6,
                6,
                6,
                6,
                6,
                4,
                false
            ), // WEST ONLY

            new SittingInfoData
            (
                0x9C5A,
                0,
                0,
                0,
                0,
                4,
                4,
                false
            ), // NORTH ONLY
            new SittingInfoData
            (
                0x9C59,
                0,
                0,
                0,
                0,
                4,
                4,
                false
            ), // NORTH ONLY

            new SittingInfoData
            (
                0x9C58,
                6,
                6,
                6,
                6,
                6,
                4,
                false
            ), // WEST ONLY
            new SittingInfoData
            (
                0x9C57,
                6,
                6,
                6,
                6,
                6,
                4,
                false
            ), // WEST ONLY

            new SittingInfoData
            (
                0x402A,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ), // EAST ONLY
            new SittingInfoData
            (
                0x4029,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ), // EAST ONLY

            new SittingInfoData
            (
                0x4028,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ), // SOUTH ONLY
            new SittingInfoData
            (
                0x4027,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ), // SOUTH ONLY
            new SittingInfoData
            (
                0x4023,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ), // SOUTH ONLY
            new SittingInfoData
            (
                0x4024,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ), // EAST ONLY
            new SittingInfoData
            (
                0x4C1E,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ), // EAST ONLY
            new SittingInfoData
            (
                0x4C1B,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ), // SOUTH ONLY
            new SittingInfoData
            (
                0x7132,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ), // EAST ONLY
            new SittingInfoData
            (
                0x71C2,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ), // SOUTH ONLY

            new SittingInfoData
            (
                0x9977,
                2,
                2,
                2,
                2,
                0,
                0,
                false
            ), // EAST ONLY
            new SittingInfoData
            (
                0x996C,
                4,
                4,
                4,
                4,
                4,
                4,
                false
            ) // SOUTH ONLY
        };


        public override unsafe Task Load()
        {
            return Task.Run
            (
                () =>
                {
                    bool loaduop = false;
                    int[] un = { 0x40000, 0x10000, 0x20000, 0x20000, 0x20000 };

                    for (int i = 0; i < 5; i++)
                    {
                        string pathmul = UOFileManager.GetUOFilePath("anim" + (i == 0 ? string.Empty : (i + 1).ToString()) + ".mul");

                        string pathidx = UOFileManager.GetUOFilePath("anim" + (i == 0 ? string.Empty : (i + 1).ToString()) + ".idx");

                        if (File.Exists(pathmul) && File.Exists(pathidx))
                        {
                            _files[i] = new UOFileMul(pathmul, pathidx, un[i], i == 0 ? 6 : -1);
                        }

                        if (i > 0 && Client.IsUOPInstallation)
                        {
                            string pathuop = UOFileManager.GetUOFilePath($"AnimationFrame{i}.uop");

                            if (File.Exists(pathuop))
                            {
                                _filesUop[i - 1] = new UOFileUop(pathuop, "build/animationlegacyframe/{0:D6}/{0:D2}.bin");

                                if (!loaduop)
                                {
                                    loaduop = true;
                                }
                            }
                        }
                    }

                    if (loaduop)
                    {
                        LoadUop();
                    }

                    int animIdxBlockSize = sizeof(AnimIdxBlock);

                    UOFile idxfile0 = _files[0]?.IdxFile;

                    long? maxAddress0 = (long?) idxfile0?.StartAddress + idxfile0?.Length;

                    UOFile idxfile2 = _files[1]?.IdxFile;

                    long? maxAddress2 = (long?) idxfile2?.StartAddress + idxfile2?.Length;

                    UOFile idxfile3 = _files[2]?.IdxFile;

                    long? maxAddress3 = (long?) idxfile3?.StartAddress + idxfile3?.Length;

                    UOFile idxfile4 = _files[3]?.IdxFile;

                    long? maxAddress4 = (long?) idxfile4?.StartAddress + idxfile4?.Length;

                    UOFile idxfile5 = _files[4]?.IdxFile;

                    long? maxAddress5 = (long?) idxfile5?.StartAddress + idxfile5?.Length;

                    if (Client.Version >= ClientVersion.CV_500A)
                    {
                        string path = UOFileManager.GetUOFilePath("mobtypes.txt");

                        if (File.Exists(path))
                        {
                            string[] typeNames = new string[5]
                            {
                                "monster", "sea_monster", "animal", "human", "equipment"
                            };

                            using (StreamReader reader = new StreamReader(File.OpenRead(path)))
                            {
                                string line;

                                while ((line = reader.ReadLine()) != null)
                                {
                                    line = line.Trim();

                                    if (line.Length == 0 || line[0] == '#' || !char.IsNumber(line[0]))
                                    {
                                        continue;
                                    }

                                    string[] parts = line.Split
                                    (
                                        new[]
                                        {
                                            '\t', ' '
                                        },
                                        StringSplitOptions.RemoveEmptyEntries
                                    );

                                    if (parts.Length < 3)
                                    {
                                        continue;
                                    }

                                    int id = int.Parse(parts[0]);

                                    if (id >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
                                    {
                                        continue;
                                    }

                                    string testType = parts[1].ToLower();

                                    int commentIdx = parts[2].IndexOf('#');

                                    if (commentIdx > 0)
                                    {
                                        parts[2] = parts[2].Substring(0, commentIdx - 1);
                                    }
                                    else if (commentIdx == 0)
                                    {
                                        continue;
                                    }

                                    uint number = uint.Parse(parts[2], NumberStyles.HexNumber);

                                    for (int i = 0; i < 5; i++)
                                    {
                                        if (testType == typeNames[i])
                                        {
                                            ref var index = ref _animationCache.GetEntry(id);

                                            index.Type = (ANIMATION_GROUPS_TYPE) (i + 1);
                                            index.Flags = (ANIMATION_FLAGS) (0x80000000 | number);

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (ushort i = 0; i < Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT; i++)
                    {
                        ref var entry = ref _animationCache.GetEntry(i);

                        if (entry.Type == ANIMATION_GROUPS_TYPE.UNKNOWN)
                        {
                            entry.Type = CalculateTypeByGraphic(i);
                        }

                        entry.Graphic = i;
                        entry.CorpseGraphic = i;
                        entry.GraphicConversion = 0x8000;

                        long offsetToData = CalculateOffset(i, entry.Flags, entry.Type, out int count);

                        if (offsetToData >= idxfile0.Length)
                        {
                            continue;
                        }

                        bool isValid = false;

                        long address = _files[0].IdxFile.StartAddress.ToInt64() + offsetToData;

                        int offset = 0;

                        for (byte j = 0; j < count; j++)
                        {
                            for (byte d = 0; d < 5; d++)
                            {
                                AnimIdxBlock* aidx = (AnimIdxBlock*) (address + offset * animIdxBlockSize);
                                ++offset;

                                if ((long) aidx < maxAddress0 && aidx->Size != 0 && aidx->Position != 0xFFFFFFFF && aidx->Size != 0xFFFFFFFF)
                                {
                                    ref var dirEntry = ref _animationCache.GetDirectionEntry(i, j, d);

                                    dirEntry.Address = (IntPtr) aidx->Position;
                                    dirEntry.Size = aidx->Size;

                                    isValid = true;
                                }
                            }
                        }

                        entry.IsValidMUL = isValid;
                    }

                    string file = UOFileManager.GetUOFilePath("Anim1.def");

                    if (File.Exists(file))
                    {
                        using (DefReader defReader = new DefReader(file))
                        {
                            while (defReader.Next())
                            {
                                ushort group = (ushort) defReader.ReadInt();

                                if (group == 0xFFFF)
                                {
                                    continue;
                                }

                                int replace = defReader.ReadGroupInt();

                                GroupReplaces[0].Add(new Tuple<ushort, byte>(group, (byte) replace));
                            }
                        }
                    }

                    file = UOFileManager.GetUOFilePath("Anim2.def");

                    if (File.Exists(file))
                    {
                        using (DefReader defReader = new DefReader(file))
                        {
                            while (defReader.Next())
                            {
                                ushort group = (ushort) defReader.ReadInt();

                                if (group == 0xFFFF)
                                {
                                    continue;
                                }

                                int replace = defReader.ReadGroupInt();

                                GroupReplaces[1].Add(new Tuple<ushort, byte>(group, (byte) replace));
                            }
                        }
                    }

                    if (Client.Version < ClientVersion.CV_305D)
                    {
                        return;
                    }

                    file = UOFileManager.GetUOFilePath("Equipconv.def");

                    if (File.Exists(file))
                    {
                        using (DefReader defReader = new DefReader(file, 5))
                        {
                            while (defReader.Next())
                            {
                                ushort body = (ushort) defReader.ReadInt();

                                if (body >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
                                {
                                    continue;
                                }

                                ushort graphic = (ushort) defReader.ReadInt();

                                if (graphic >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
                                {
                                    continue;
                                }

                                ushort newGraphic = (ushort) defReader.ReadInt();

                                if (newGraphic >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
                                {
                                    continue;
                                }

                                int gump = defReader.ReadInt();

                                if (gump > ushort.MaxValue)
                                {
                                    continue;
                                }

                                if (gump == 0)
                                {
                                    gump = graphic;
                                }
                                else if (gump == 0xFFFF || gump == -1)
                                {
                                    gump = newGraphic;
                                }

                                ushort color = (ushort) defReader.ReadInt();

                                if (!_equipConv.TryGetValue(body, out Dictionary<ushort, EquipConvData> dict))
                                {
                                    _equipConv.Add(body, new Dictionary<ushort, EquipConvData>());

                                    if (!_equipConv.TryGetValue(body, out dict))
                                    {
                                        continue;
                                    }
                                }

                                dict[graphic] = new EquipConvData(newGraphic, (ushort) gump, color);
                            }
                        }
                    }

                    file = UOFileManager.GetUOFilePath("Bodyconv.def");

                    if (File.Exists(file))
                    {
                        using (DefReader defReader = new DefReader(file))
                        {
                            while (defReader.Next())
                            {
                                ushort index = (ushort) defReader.ReadInt();

                                if (index >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
                                {
                                    continue;
                                }

                                int[] anim =
                                {
                                    defReader.ReadInt(), -1, -1, -1
                                };

                                if (defReader.PartsCount >= 3)
                                {
                                    anim[1] = defReader.ReadInt();

                                    if (defReader.PartsCount >= 4)
                                    {
                                        anim[2] = defReader.ReadInt();

                                        if (defReader.PartsCount >= 5)
                                        {
                                            anim[3] = defReader.ReadInt();
                                        }
                                    }
                                }

                                int animFile = 0;
                                ushort realAnimID = 0xFFFF;
                                sbyte mountedHeightOffset = 0;

                                if (anim[0] != -1 && maxAddress2.HasValue && maxAddress2 != 0)
                                {
                                    animFile = 1;
                                    realAnimID = (ushort) anim[0];

                                    if (index == 0x00C0 || index == 793)
                                    {
                                        mountedHeightOffset = -9;
                                    }
                                }
                                else if (anim[1] != -1 && maxAddress3.HasValue && maxAddress3 != 0)
                                {
                                    animFile = 2;
                                    realAnimID = (ushort) anim[1];

                                    if (index == 0x0579)
                                    {
                                        mountedHeightOffset = 9;
                                    }
                                }
                                else if (anim[2] != -1 && maxAddress4.HasValue && maxAddress4 != 0)
                                {
                                    animFile = 3;
                                    realAnimID = (ushort) anim[2];
                                }
                                else if (anim[3] != -1 && maxAddress5.HasValue && maxAddress5 != 0)
                                {
                                    animFile = 4;
                                    realAnimID = (ushort) anim[3];
                                    mountedHeightOffset = -9;

                                    if (index == 0x0115 || index == 0x00C0)
                                    {
                                        mountedHeightOffset = 0;
                                    }
                                    else if (index == 0x042D)
                                    {
                                        mountedHeightOffset = 3;
                                    }
                                }


                                if (realAnimID != 0xFFFF && animFile != 0)
                                {
                                    UOFile currentIdxFile = _files[animFile].IdxFile;

                                    ref var animIndex = ref _animationCache.GetEntry(index);

                                    ANIMATION_GROUPS_TYPE realType = Client.Version < ClientVersion.CV_500A ?
                                        CalculateTypeByGraphic(realAnimID) :
                                        animIndex.Type;

                                    long addressOffset = CalculateOffset(realAnimID, animIndex.Flags, realType, out int count);

                                    if (addressOffset < currentIdxFile.Length)
                                    {
                                        animIndex.Type = realType;

                                        if (animIndex.MountOffsetY == 0)
                                        {
                                            animIndex.MountOffsetY = mountedHeightOffset;
                                        }

                                        animIndex.GraphicConversion = (ushort) (realAnimID | 0x8000);
                                        animIndex.FileIndex = (byte) animFile;

                                        addressOffset += currentIdxFile.StartAddress.ToInt64();
                                        long maxaddress = currentIdxFile.StartAddress.ToInt64() + currentIdxFile.Length;

                                        int offset = 0;

                                        for (byte j = 0; j < count; j++)
                                        {  
                                            for (byte d = 0; d < 5; d++)
                                            {
                                                AnimIdxBlock* aidx = (AnimIdxBlock*) (addressOffset + offset * animIdxBlockSize);

                                                ++offset;

                                                if ((long) aidx < maxaddress /*&& aidx->Size != 0*/ && aidx->Position != 0xFFFFFFFF &&
                                                    aidx->Size != 0xFFFFFFFF)
                                                {               
                                                    ref var convertedEntry = ref _animationCache.GetConvertedDirectionEntry(index, j, d);

                                                    convertedEntry.Address = (IntPtr) aidx->Position;
                                                    convertedEntry.Size = Math.Min(1, aidx->Size);
                                                    convertedEntry.FileIndex = (byte) animFile;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    file = UOFileManager.GetUOFilePath("Body.def");
                    Dictionary<int, bool> filter = new Dictionary<int, bool>();

                    if (File.Exists(file))
                    {
                        using (DefReader defReader = new DefReader(file, 1))
                        {
                            while (defReader.Next())
                            {
                                int index = defReader.ReadInt();

                                if (index >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
                                {
                                    continue;
                                }

                                if (filter.TryGetValue(index, out bool b) && b)
                                {
                                    continue;
                                }

                                int[] group = defReader.ReadGroup();

                                if (group == null)
                                {
                                    continue;
                                }

                                int color = defReader.ReadInt();

                                int checkIndex;

                                //Yes, this is actually how this is supposed to work.
                                if (group.Length >= 3)
                                {
                                    checkIndex = group[2];
                                }
                                else
                                {
                                    checkIndex = group[0];
                                }

                                if (checkIndex >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
                                {
                                    continue;
                                }

                                ref var entry = ref _animationCache.GetEntry(index);

                                entry.Graphic = (ushort) checkIndex;
                                entry.Color = (ushort) color;
                                entry.IsValidMUL = true;
                                
                                filter[index] = true;
                            }
                        }
                    }

                    file = UOFileManager.GetUOFilePath("Corpse.def");
                    filter.Clear();

                    if (File.Exists(file))
                    {
                        using (DefReader defReader = new DefReader(file, 1))
                        {
                            while (defReader.Next())
                            {
                                int index = defReader.ReadInt();

                                if (index >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
                                {
                                    continue;
                                }

                                if (filter.TryGetValue(index, out bool b) && b)
                                {
                                    continue;
                                }

                                int[] group = defReader.ReadGroup();

                                if (group == null)
                                {
                                    continue;
                                }

                                int color = defReader.ReadInt();

                                int checkIndex;

                                if (group.Length >= 3)
                                {
                                    checkIndex = group[2];
                                }
                                else
                                {
                                    checkIndex = group[0];
                                }

                                if (checkIndex >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
                                {
                                    continue;
                                }

                                ref var entry = ref _animationCache.GetEntry(index);

                                entry.CorpseGraphic = (ushort) checkIndex;
                                entry.CorpseColor = (ushort) color;
                                entry.IsValidMUL = true;

                                filter[index] = true;
                            }
                        }
                    }
                }
            );
        }


        public override void Dispose()
        {
            base.Dispose();

            if (_bufferCachePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_bufferCachePtr);
            }
        }

        private void LoadUop()
        {
            if (Client.Version <= ClientVersion.CV_60144)
            {
                return;
            }

            for (ushort animID = 0; animID < Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT; animID++)
            {
                for (byte grpID = 0; grpID < 100; grpID++)
                {
                    string hashstring = $"build/animationlegacyframe/{animID:D6}/{grpID:D2}.bin";
                    ulong hash = UOFileUop.CreateHash(hashstring);

                    for (byte i = 0; i < _filesUop.Length; i++)
                    {
                        UOFileUop uopFile = _filesUop[i];

                        if (uopFile != null && uopFile.TryGetUOPData(hash, out UOFileIndex data))
                        {
                            _animationCache.StoreUopHash(hash, data, i);
                            _animationCache.ReplaceUopGroup(animID, grpID, grpID);

                            ref var entry = ref _animationCache.GetEntry(animID);
                            entry.Graphic = animID;
                            entry.CorpseGraphic = animID;
                            entry.GraphicConversion = 0x8000;
                            entry.FileIndex = i;
                            entry.Flags |= ANIMATION_FLAGS.AF_USE_UOP_ANIMATION | ANIMATION_FLAGS.AF_FOUND;

                            for (byte d = 0; d < 5; d++)
                            {
                                ref var dirEntry = ref _animationCache.GetDirectionEntry(animID, grpID, d, true);
                                dirEntry.IsUOP = true;
                            }
                        }
                    }
                }
            }


            for (int i = 0; i < _filesUop.Length; i++)
            {
                _filesUop[i]?.ClearHashes();
            }

            string animationSequencePath = UOFileManager.GetUOFilePath("AnimationSequence.uop");

            if (!File.Exists(animationSequencePath))
            {
                Log.Warn("AnimationSequence.uop not found");

                return;
            }

            UOFileUop animSeq = new UOFileUop(animationSequencePath, "build/animationsequence/{0:D8}.bin");
            UOFileIndex[] animseqEntries = new UOFileIndex[Math.Max(animSeq.TotalEntriesCount, Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)];
            animSeq.FillEntries(ref animseqEntries);
            DataReader reader = new DataReader();

            for (int i = 0; i < animseqEntries.Length; i++)
            {
                ref UOFileIndex entry = ref animseqEntries[i];

                if (entry.Offset == 0)
                {
                    continue;
                }

                animSeq.Seek(entry.Offset);

                byte[] decbuffer = animSeq.GetData(entry.Length, entry.DecompressedLength);

                reader.SetData(decbuffer, decbuffer.Length);
                uint animID = reader.ReadUInt();
                reader.Skip(48);
                int replaces = reader.ReadInt();

                if (replaces == 48 || replaces == 68)
                {
                    continue;
                }

                ref var animEntry = ref _animationCache.GetEntry((int)animID);

                if ((animEntry.Flags & ANIMATION_FLAGS.AF_FOUND) != 0 && (animEntry.Flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) == 0)
                {
                    animEntry.Flags |= ANIMATION_FLAGS.AF_USE_UOP_ANIMATION;
                }

                for (int k = 0; k < replaces; k++)
                {
                    int oldGroup = reader.ReadInt();
                    uint frameCount = reader.ReadUInt();
                    int newGroup = reader.ReadInt();

                    if (frameCount == 0 && animEntry.Graphic != 0)
                    {
                        _animationCache.ReplaceUopGroup((ushort) animID, (byte) oldGroup, (byte) newGroup);
                    }

                    reader.Skip(60);
                }

                if (animEntry.Graphic != 0)
                {
                    if (animID == 0x04E7 || animID == 0x042D || animID == 0x04E6 || animID == 0x05F7)
                    {
                        animEntry.MountOffsetY = 18;
                    }
                    else if (animID == 0x01B0 || animID == 0x0579 || animID == 0x05F6 || animID == 0x05A0)
                    {
                        animEntry.MountOffsetY = 9;
                    }
                }
            }

            animSeq.Dispose();
            reader.ReleaseData();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint CalculatePeopleGroupOffset(ushort graphic)
        {
            return (uint) (((graphic - 400) * 175 + 35000) * sizeof(AnimIdxBlock));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint CalculateHighGroupOffset(ushort graphic)
        {
            return (uint) (graphic * 110 * sizeof(AnimIdxBlock));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint CalculateLowGroupOffset(ushort graphic)
        {
            return (uint) (((graphic - 200) * 65 + 22000) * sizeof(AnimIdxBlock));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ANIMATION_GROUPS_TYPE CalculateTypeByGraphic(ushort graphic)
        {
            return graphic < 200 ? ANIMATION_GROUPS_TYPE.MONSTER : graphic < 400 ? ANIMATION_GROUPS_TYPE.ANIMAL : ANIMATION_GROUPS_TYPE.HUMAN;
        }

        public ref AnimationEntry GetAnimationEntry(ushort graphic) => ref _animationCache.GetEntry(graphic);

        private ref AnimationDirectionEntry GetAnimationDirectionEntry(ushort graphic, byte group, byte direction, bool isUop = false)
        {
            ref AnimationEntry entry = ref _animationCache.GetEntry(graphic);

            if (!isUop && (entry.Flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0)
            {
                isUop = true;
            }

            bool conv = HasBodyConversion(ref entry);

            //ref AnimationDirectionEntry dirEntry = ref _animationCache.GetConvertedDirectionEntry(graphic, group, direction, isUop);

            //if (!conv || dirEntry.Address == IntPtr.Zero || dirEntry.IsUOP)
            //{
            //    dirEntry = ref _animationCache.GetDirectionEntry(graphic, group, direction, isUop);
            //}

            ref AnimationDirectionEntry dirEntry = ref
               (conv ?
                   ref _animationCache.GetConvertedDirectionEntry(graphic, group, direction, isUop)
                   :
                   ref _animationCache.GetDirectionEntry(graphic, group, direction, isUop)
               );

            if (dirEntry.Address == IntPtr.Zero)
            {
                if (conv)
                {
                    dirEntry = ref _animationCache.GetDirectionEntry(graphic, group, direction, isUop);
                }
                //else
                //{
                //    dirEntry = ref _animationCache.GetConvertedDirectionEntry(graphic, group, direction, isUop);
                //}
            }

            if (dirEntry.FramesCount == 0 && (dirEntry.Address != IntPtr.Zero || dirEntry.IsUOP))
            {
                if (LoadAnimationFrames(graphic, group, direction, ref dirEntry))
                {

                }
            }

            return ref dirEntry;
        }

        private static bool HasBodyConversion(ref AnimationEntry entry)
        {
            return (entry.GraphicConversion & 0x8000) == 0;
        }

        public byte GetFrameInfo(ushort graphic, byte group, byte direction, bool forceUop = false)
        {
            ref AnimationDirectionEntry dirEntry = ref GetAnimationDirectionEntry(graphic, group, direction, forceUop);

            if (dirEntry.FramesCount != 0)
            {
                dirEntry.LastAccessTime = Time.Ticks;
                return dirEntry.FramesCount;
            }

            return 0;
        }

        public AnimationFrameTexture GetBodyFrame(ref ushort graphic, ref ushort hue, byte group, byte direction, int frame, bool isParent = false, bool forceUOP = false)
        {
            return GetFrame
            (
                ref graphic,
                ref hue,
                group,
                direction,
                frame,
                false,
                isParent,
                forceUOP
            );
        }

        public AnimationFrameTexture GetCorpseFrame(ref ushort graphic, ref ushort hue, byte group, byte direction, int frame, bool forceUOP = false)
        {
            return GetFrame
            (
                ref graphic,
                ref hue,
                group,
                direction,
                frame,
                true,
                false,
                forceUOP
            );
        }

        public void FixAnimationGraphicAndHue(ref ushort graphic, ref ushort hue, bool isCorpse, bool isParent, bool forceUOP, out bool isUop)
        {
            ref var entry = ref _animationCache.GetEntry(graphic);

            isUop = forceUOP || ((entry.Flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0 && (isParent || !entry.IsValidMUL));

            if (!isUop)
            {
                ushort newGraphic = isCorpse ? entry.CorpseGraphic : entry.Graphic;

                do
                {
                    ref var newEntry = ref _animationCache.GetEntry(newGraphic);

                    bool convert = (HasBodyConversion(ref newEntry) || !HasBodyConversion(ref entry))
                                   &&
                                   !(HasBodyConversion(ref newEntry) && HasBodyConversion(ref entry));

                    if (convert)
                    {
                        if (graphic != newGraphic)
                        {
                            graphic = newGraphic;

                            ref var ent = ref _animationCache.GetEntry(graphic);

                            newGraphic = isCorpse ? entry.CorpseGraphic : ent.Graphic;

                            if (hue == 0)
                            {
                                hue = isCorpse ? entry.CorpseColor : entry.Color;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                } while (graphic != newGraphic);
            }
        }

        private AnimationFrameTexture GetFrame
        (
            ref ushort graphic,
            ref ushort hue,
            byte group,
            byte direction,
            int frame,
            bool isCorpse,
            bool isParent,
            bool forceUOP
        )
        {
            FixAnimationGraphicAndHue
            (
                ref graphic,
                ref hue,
                isCorpse,
                isParent,
                forceUOP,
                out bool isUop
            );

            ref AnimationDirectionEntry dirEntry = ref GetAnimationDirectionEntry(graphic, group, direction, isUop);

            if (dirEntry.FramesCount != 0)
            {
                dirEntry.LastAccessTime = Time.Ticks;
            }

            return _animationCache.GetFrame(graphic, group, direction, frame, isUop);
        }


        public override void ClearResources()
        {
            LinkedListNode<(ushort, byte, byte, bool)> first = _usedTextures.First;

            while (first != null)
            {
                var next = first.Next;

                ref var entry = ref GetAnimationDirectionEntry(first.Value.Item1,
                                                               first.Value.Item2,
                                                               first.Value.Item3,
                                                               first.Value.Item4);

                if (entry.LastAccessTime != 0)
                {
                    for (int j = 0; j < entry.FramesCount; j++)
                    {
                        _animationCache.Remove(first.Value.Item1, first.Value.Item2, first.Value.Item3, j, first.Value.Item4);
                    }

                    entry.FramesCount = 0;
                    entry.LastAccessTime = 0;

                    _usedTextures.Remove(first);
                }

                first = next;
            }

            if (_usedTextures.Count != 0)
            {
                _usedTextures.Clear();
            }
        }

        public void UpdateAnimationTable(uint flags)
        {
            for (ushort i = 0; i < Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT; i++)
            {
                ref var entry = ref _animationCache.GetEntry(i);

                bool replace = entry.FileIndex >= 3;

                if (entry.FileIndex == 1)
                {
                    replace = (World.ClientLockedFeatures.Flags & LockedFeatureFlags.LordBlackthornsRevenge) != 0;
                }
                else if (entry.FileIndex == 2)
                {
                    replace = (World.ClientLockedFeatures.Flags & LockedFeatureFlags.AgeOfShadows) != 0;
                }

                if (replace)
                {
                    if (!HasBodyConversion(ref entry))
                    {
                        entry.GraphicConversion = (ushort) (entry.GraphicConversion & ~0x8000);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAnimDirection(ref byte dir, ref bool mirror)
        {
            switch (dir)
            {
                case 2:
                case 4:
                    mirror = dir == 2;
                    dir = 1;

                    break;

                case 1:
                case 5:
                    mirror = dir == 1;
                    dir = 2;

                    break;

                case 0:
                case 6:
                    mirror = dir == 0;
                    dir = 3;

                    break;

                case 3:
                    dir = 0;

                    break;

                case 7:
                    dir = 4;

                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetSittingAnimDirection(ref byte dir, ref bool mirror, ref int x, ref int y)
        {
            switch (dir)
            {
                case 0:
                    mirror = true;
                    dir = 3;

                    break;

                case 2:
                    mirror = true;
                    dir = 1;

                    break;

                case 4:
                    mirror = false;
                    dir = 1;

                    break;

                case 6:
                    mirror = false;
                    dir = 3;

                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FixSittingDirection(ref byte direction, ref bool mirror, ref int x, ref int y, int sittingIndex)
        {
            ref SittingInfoData data = ref SittingInfos[sittingIndex - 1];

            switch (direction)
            {
                case 7:
                case 0:
                {
                    if (data.Direction1 == -1)
                    {
                        if (direction == 7)
                        {
                            direction = (byte) data.Direction4;
                        }
                        else
                        {
                            direction = (byte) data.Direction2;
                        }
                    }
                    else
                    {
                        direction = (byte) data.Direction1;
                    }

                    break;
                }

                case 1:
                case 2:
                {
                    if (data.Direction2 == -1)
                    {
                        if (direction == 1)
                        {
                            direction = (byte) data.Direction1;
                        }
                        else
                        {
                            direction = (byte) data.Direction3;
                        }
                    }
                    else
                    {
                        direction = (byte) data.Direction2;
                    }

                    break;
                }

                case 3:
                case 4:
                {
                    if (data.Direction3 == -1)
                    {
                        if (direction == 3)
                        {
                            direction = (byte) data.Direction2;
                        }
                        else
                        {
                            direction = (byte) data.Direction4;
                        }
                    }
                    else
                    {
                        direction = (byte) data.Direction3;
                    }

                    break;
                }

                case 5:
                case 6:
                {
                    if (data.Direction4 == -1)
                    {
                        if (direction == 5)
                        {
                            direction = (byte) data.Direction3;
                        }
                        else
                        {
                            direction = (byte) data.Direction1;
                        }
                    }
                    else
                    {
                        direction = (byte) data.Direction4;
                    }

                    break;
                }
            }

            GetSittingAnimDirection(ref direction, ref mirror, ref x, ref y);

            const int SITTING_OFFSET_X = 8;

            int offsX = SITTING_OFFSET_X;

            if (mirror)
            {
                if (direction == 3)
                {
                    y += 25 + data.MirrorOffsetY;
                    x += offsX - 4;
                }
                else
                {
                    y += data.OffsetY + 9;
                }
            }
            else
            {
                if (direction == 3)
                {
                    y += 23 + data.MirrorOffsetY;
                    x -= 3;
                }
                else
                {
                    y += 10 + data.OffsetY;
                    x -= offsX + 1;
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ANIMATION_GROUPS GetGroupIndex(ushort graphic)
        {
            if (graphic >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
            {
                return ANIMATION_GROUPS.AG_HIGHT;
            }

            ref var entry = ref _animationCache.GetEntry(graphic);

            switch (entry.Type)
            {
                case ANIMATION_GROUPS_TYPE.ANIMAL: return ANIMATION_GROUPS.AG_LOW;

                case ANIMATION_GROUPS_TYPE.MONSTER:
                case ANIMATION_GROUPS_TYPE.SEA_MONSTER: return ANIMATION_GROUPS.AG_HIGHT;

                case ANIMATION_GROUPS_TYPE.HUMAN:
                case ANIMATION_GROUPS_TYPE.EQUIPMENT: return ANIMATION_GROUPS.AG_PEOPLE;
            }

            return ANIMATION_GROUPS.AG_HIGHT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetDieGroupIndex(ushort id, bool second, bool isRunning = false)
        {
            if (id >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
            {
                return 0;
            }

            ref var entry = ref _animationCache.GetEntry(id);

            switch (entry.Type)
            {
                case ANIMATION_GROUPS_TYPE.ANIMAL:

                    if ((entry.Flags & ANIMATION_FLAGS.AF_USE_2_IF_HITTED_WHILE_RUNNING) != 0 ||
                        (entry.Flags & ANIMATION_FLAGS.AF_CAN_FLYING) != 0)
                    {
                        return 2;
                    }

                    if ((entry.Flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0)
                    {
                        return (byte) (second ? 3 : 2);
                    }

                    return (byte) (second ? LOW_ANIMATION_GROUP.LAG_DIE_2 : LOW_ANIMATION_GROUP.LAG_DIE_1);

                case ANIMATION_GROUPS_TYPE.SEA_MONSTER:

                {
                    if (!isRunning)
                    {
                        return 8;
                    }

                    goto case ANIMATION_GROUPS_TYPE.MONSTER;
                }

                case ANIMATION_GROUPS_TYPE.MONSTER:

                    if ((entry.Flags & ANIMATION_FLAGS.AF_USE_UOP_ANIMATION) != 0)
                    {
                        return (byte) (second ? 3 : 2);
                    }

                    return (byte) (second ? HIGHT_ANIMATION_GROUP.HAG_DIE_2 : HIGHT_ANIMATION_GROUP.HAG_DIE_1);

                case ANIMATION_GROUPS_TYPE.HUMAN:
                case ANIMATION_GROUPS_TYPE.EQUIPMENT: return (byte) (second ? PEOPLE_ANIMATION_GROUP.PAG_DIE_2 : PEOPLE_ANIMATION_GROUP.PAG_DIE_1);
            }

            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAnimationExists(ushort graphic, byte group, bool isCorpse = false)
        {
            if (graphic < Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT && group < 100)
            {
                ref var dirEntry = ref GetAnimationDirectionEntry(graphic, group, 0);

                //AnimationDirection direction = isCorpse ?
                //    GetCorpseAnimationGroup(ref graphic, ref group, ref hue)?.Direction[0] :
                //    GetBodyAnimationGroup(ref graphic, ref group, ref hue, true)?.Direction[0];
                
                return (dirEntry.Address != IntPtr.Zero && dirEntry.Size != 0) || dirEntry.IsUOP;
            }

            return false;
        }

        private bool LoadAnimationFrames(ushort animID, byte animGroup, byte direction, ref AnimationDirectionEntry animDir)
        {
            if (/*animDir.FileIndex == -1 &&*/ animDir.Address == (IntPtr) (-1))
            {
                return false;
            }

            if (animDir.FileIndex >= _files.Length || animID >= Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
            {
                return false;
            }

            if (animDir.IsUOP || animDir.Address == IntPtr.Zero && animDir.Size == 0)
            {
                return ReadUOPAnimationFrame(animID, animGroup, direction, ref animDir);
            }

            if (animDir.Address == IntPtr.Zero && animDir.Size == 0)
            {
                return false;
            }

            UOFileMul file = _files[animDir.FileIndex];
            file.Seek(animDir.Address.ToInt64());
            ReadMULAnimationFrame(animID, animGroup, direction, ref animDir, file);

            return true;
        }

        private bool ReadUOPAnimationFrame(ushort animID, byte animGroup, byte direction, ref AnimationDirectionEntry animDirection)
        {
            if (!_animationCache.TryGetUopHash(animID, animGroup, out var index))
            {
                return false;
            }

            byte fileNum = index.Item1;
            UOFileIndex fileIndex = index.Item2;

            if (fileNum < 0 || fileNum >= _filesUop.Length)
            {
                return false;
            }

            if (fileNum == 0 && fileIndex.Length == 0 && fileIndex.DecompressedLength == 0 && fileIndex.Offset == 0)
            {
                Log.Warn("uop animData is null");

                return false;
            }

            animDirection.LastAccessTime = Time.Ticks;
            int decLen = (int)fileIndex.DecompressedLength;
            UOFileUop file = _filesUop[fileNum];
            file.Seek(fileIndex.Offset);

            ZLib.Decompress
            (
                file.PositionAddress,
                fileIndex.Length,
                0,
                _bufferCachePtr,
                decLen
            );

            _reader.SetData(_bufferCachePtr, decLen);
            _reader.Skip(32);

            int frameCount = _reader.ReadInt();
            int dataStart = _reader.ReadInt();
            _reader.Seek(dataStart);

            for (int i = 0; i < frameCount; i++)
            {
                uint start = (uint) _reader.Position;
                ushort group = _reader.ReadUShort();
                short frameID = _reader.ReadShort();
                _reader.Skip(8);
                uint pixelOffset = _reader.ReadUInt();
                //int vsize = pixelDataOffsets.Count;

                ref UOPFrameData data = ref _uop_frame_pixels_offsets[i];
                data.DataStart = start;
                data.PixelDataOffset = pixelOffset;

                //if (vsize + 1 < data.FrameID)
                //{
                //    while (vsize + 1 != data.FrameID)
                //    {
                //        pixelDataOffsets.Add(new UOPFrameData());
                //        vsize++;
                //    }
                //}

                //pixelDataOffsets.Add(data);
            }

            //int vectorSize = pixelDataOffsets.Count;
            //if (vectorSize < 50)
            //{
            //    while (vectorSize != 50)
            //    {
            //        pixelDataOffsets.Add(new UOPFrameData());
            //        vectorSize++;
            //    }
            //}

            animDirection.FramesCount = (byte)(frameCount / 5);
            int dirFrameStartIdx = animDirection.FramesCount * direction;

            long end = (long)_reader.StartAddress + _reader.Length;

            unchecked
            {
                for (int i = 0, count = animDirection.FramesCount; i < count; ++i)
                {
                    ref UOPFrameData frameData = ref _uop_frame_pixels_offsets[i + dirFrameStartIdx];

                    if (frameData.DataStart == 0)
                    {
                        continue;
                    }

                    _reader.Seek((int) (frameData.DataStart + frameData.PixelDataOffset));
                    ushort* palette = (ushort*) _reader.PositionAddress;
                    _reader.Skip(512);
                    short imageCenterX = _reader.ReadShort();
                    short imageCenterY = _reader.ReadShort();
                    short imageWidth = _reader.ReadShort();
                    short imageHeight = _reader.ReadShort();

                    if (imageWidth == 0 || imageHeight == 0)
                    {
                        Log.Warn("frame size is null");

                        continue;
                    }

                    uint[] data = new uint[imageWidth * imageHeight];

                    uint header = _reader.ReadUInt();

                    long pos = _reader.Position;

                    int sum = imageCenterY + imageHeight;

                    while (header != 0x7FFF7FFF && pos < end)
                    {
                        ushort runLength = (ushort) (header & 0x0FFF);
                        int x = (int) ((header >> 22) & 0x03FF);

                        if ((x & 0x0200) > 0)
                        {
                            x |= (int) 0xFFFFFE00;
                        }

                        int y = (int) ((header >> 12) & 0x3FF);

                        if ((y & 0x0200) > 0)
                        {
                            y |= (int) 0xFFFFFE00;
                        }

                        x += imageCenterX;
                        y += sum;

                        int block = y * imageWidth + x;

                        for (int k = 0; k < runLength; ++k)
                        {
                            ushort val = palette[_reader.ReadByte()];

                            // FIXME: same of MUL ? Keep it as original for the moment
                            if (val != 0)
                            {
                                data[block] = HuesHelper.Color16To32(val) | 0xFF_00_00_00;
                            }

                            block++;
                        }

                        header = _reader.ReadUInt();
                    }


                    AnimationFrameTexture f = new AnimationFrameTexture(imageWidth, imageHeight)
                    {
                        CenterX = imageCenterX,
                        CenterY = imageCenterY
                    };

                    f.PushData(data);
                    //animDirection.Frames[i] = f;

                    _animationCache.Push(animID, animGroup, direction, i, animDirection.FramesCount, f, true);
                }
            }

            _usedTextures.AddLast((animID, animGroup, direction, true));

            _reader.ReleaseData();

            return true;
        }

        private unsafe void ReadMULAnimationFrame(ushort animID, byte animGroup, byte direction, ref AnimationDirectionEntry animDir, UOFile reader)
        {
            animDir.LastAccessTime = Time.Ticks;

            ushort* palette = (ushort*) reader.PositionAddress;
            reader.Skip(512);

            long dataStart = reader.Position;
            uint frameCount = reader.ReadUInt();
            animDir.FramesCount = (byte) frameCount;
            uint* frameOffset = (uint*) reader.PositionAddress;


            //if (animDir.FrameIndexStart >= 0)
            //{
            //    Log.Panic("MEMORY LEAK MUL ANIM");
            //}


            long end = (long) reader.StartAddress + reader.Length;

            for (int i = 0; i < frameCount; i++)
            {
                /*if (animDir.Frames[i] != null)
                {
                    continue;
                }
                */

                reader.Seek(dataStart + frameOffset[i]);

                short imageCenterX = reader.ReadShort();
                short imageCenterY = reader.ReadShort();
                short imageWidth = reader.ReadShort();
                short imageHeight = reader.ReadShort();

                if (imageWidth == 0 || imageHeight == 0)
                {
                    continue;
                }

                uint[] data = new uint[imageWidth * imageHeight];

                uint header = reader.ReadUInt();

                long pos = reader.Position;

                while (header != 0x7FFF7FFF && pos < end)
                {
                    ushort runLength = (ushort) (header & 0x0FFF);
                    int x = (int) ((header >> 22) & 0x03FF);

                    if ((x & 0x0200) > 0)
                    {
                        x |= unchecked((int) 0xFFFFFE00);
                    }

                    int y = (int) ((header >> 12) & 0x3FF);

                    if ((y & 0x0200) > 0)
                    {
                        y |= unchecked((int) 0xFFFFFE00);
                    }

                    x += imageCenterX;
                    y += imageCenterY + imageHeight;

                    int block = y * imageWidth + x;

                    for (int k = 0; k < runLength; k++)
                    {
                        data[block++] = HuesHelper.Color16To32(palette[reader.ReadByte()]) | 0xFF_00_00_00;
                    }

                    header = reader.ReadUInt();
                }


                AnimationFrameTexture f = new AnimationFrameTexture(imageWidth, imageHeight)
                {
                    CenterX = imageCenterX,
                    CenterY = imageCenterY
                };

                f.PushData(data);

                _animationCache.Push(animID, animGroup, direction, i, (int) frameCount, f);
            }

            _usedTextures.AddLast((animID, animGroup, direction, false));
        }

        public void GetAnimationDimensions
        (
            sbyte animIndex,
            ushort graphic,
            byte dir,
            byte animGroup,
            bool ismounted,
            byte frameIndex,
            out int centerX,
            out int centerY,
            out int width,
            out int height
        )
        {
            dir &= 0x7F;
            bool mirror = false;
            GetAnimDirection(ref dir, ref mirror);

            if (frameIndex == 0xFF)
            {
                frameIndex = (byte) animIndex;
            }

            GetAnimationDimensions
            (
                frameIndex,
                graphic,
                dir,
                animGroup,
                out centerX,
                out centerY,
                out width,
                out height
            );

            if (centerX == 0 && centerY == 0 && width == 0 && height == 0)
            {
                height = ismounted ? 100 : 60;
            }
        }

        public void GetAnimationDimensions
        (
            byte frameIndex,
            ushort id,
            byte dir,
            byte animGroup,
            out int x,
            out int y,
            out int w,
            out int h
        )
        {
            if (id < Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT)
            {
                if (_animDimensionCache.TryGetValue(id, out Rectangle rect))
                {
                    x = rect.X;
                    y = rect.Y;
                    w = rect.Width;
                    h = rect.Height;

                    return;
                }

                ushort hue = 0;

                if (dir < 5)
                {
                    AnimationFrameTexture frame = GetBodyFrame
                    (
                        ref id,
                        ref hue,
                        animGroup,
                        dir,
                        frameIndex,
                        true,
                        false
                    );


                    if (frame != null)
                    {
                        x = frame.CenterX;
                        y = frame.CenterY;
                        w = frame.Width;
                        h = frame.Height;
                        _animDimensionCache[id] = new Rectangle(x, y, w, h);

                        return;
                    }
                }
            }

            x = 0;
            y = 0;
            w = 0;
            h = 0;
        }

        public void ClearUnusedResources(int maxCount)
        {
            int count = 0;
            long ticks = Time.Ticks - Constants.CLEAR_TEXTURES_DELAY;

            LinkedListNode<(ushort, byte, byte, bool)> first = _usedTextures.First;

            while (first != null)
            {
                LinkedListNode<(ushort, byte, byte, bool)> next = first.Next;

                ref var entry = ref GetAnimationDirectionEntry(first.Value.Item1,
                                                                first.Value.Item2,
                                                                first.Value.Item3,
                                                                first.Value.Item4);


                if (entry.LastAccessTime != 0 && entry.LastAccessTime < ticks)
                {
                    for (int j = 0; j < entry.FramesCount; j++)
                    {
                        _animationCache.Remove(first.Value.Item1, first.Value.Item2, first.Value.Item3, j, first.Value.Item4);
                    }

                    entry.FramesCount = 0;
                    entry.LastAccessTime = 0;

                    _usedTextures.Remove(first);

                    if (++count >= maxCount)
                    {
                        break;
                    }
                }

                first = next;
            }
        }

        private long CalculateOffset(ushort graphic, ANIMATION_FLAGS flags, ANIMATION_GROUPS_TYPE type, out int groupCount)
        {
            long result = 0;
            groupCount = 0;

            ANIMATION_GROUPS group = ANIMATION_GROUPS.AG_NONE;

            switch (type)
            {
                case ANIMATION_GROUPS_TYPE.MONSTER:

                    if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_PEOPLE_GROUP) != 0)
                    {
                        group = ANIMATION_GROUPS.AG_PEOPLE;
                    }
                    else if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_LOW_GROUP) != 0)
                    {
                        group = ANIMATION_GROUPS.AG_LOW;
                    }
                    else
                    {
                        group = ANIMATION_GROUPS.AG_HIGHT;
                    }

                    break;

                case ANIMATION_GROUPS_TYPE.SEA_MONSTER:
                    result = AnimationsLoader.CalculateHighGroupOffset(graphic);
                    groupCount = (int)LOW_ANIMATION_GROUP.LAG_ANIMATION_COUNT;

                    break;

                case ANIMATION_GROUPS_TYPE.ANIMAL:

                    if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_LOW_GROUP_EXTENDED) != 0)
                    {
                        if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_PEOPLE_GROUP) != 0)
                        {
                            group = ANIMATION_GROUPS.AG_PEOPLE;
                        }
                        else if ((flags & ANIMATION_FLAGS.AF_CALCULATE_OFFSET_BY_LOW_GROUP) != 0)
                        {
                            group = ANIMATION_GROUPS.AG_LOW;
                        }
                        else
                        {
                            group = ANIMATION_GROUPS.AG_HIGHT;
                        }
                    }
                    else
                    {
                        group = ANIMATION_GROUPS.AG_LOW;
                    }

                    break;

                default:
                    group = ANIMATION_GROUPS.AG_PEOPLE;

                    break;
            }

            switch (group)
            {
                case ANIMATION_GROUPS.AG_LOW:
                    result = CalculateLowGroupOffset(graphic);
                    groupCount = (int)LOW_ANIMATION_GROUP.LAG_ANIMATION_COUNT;

                    break;

                case ANIMATION_GROUPS.AG_HIGHT:
                    result = CalculateHighGroupOffset(graphic);
                    groupCount = (int)HIGHT_ANIMATION_GROUP.HAG_ANIMATION_COUNT;

                    break;

                case ANIMATION_GROUPS.AG_PEOPLE:
                    result = CalculatePeopleGroupOffset(graphic);
                    groupCount = (int)PEOPLE_ANIMATION_GROUP.PAG_ANIMATION_COUNT;

                    break;
            }

            return result;
        }


        public struct SittingInfoData
        {
            public SittingInfoData
            (
                ushort graphic,
                sbyte d1,
                sbyte d2,
                sbyte d3,
                sbyte d4,
                sbyte offsetY,
                sbyte mirrorOffsetY,
                bool drawback
            )
            {
                Graphic = graphic;
                Direction1 = d1;
                Direction2 = d2;
                Direction3 = d3;
                Direction4 = d4;
                OffsetY = offsetY;
                MirrorOffsetY = mirrorOffsetY;
                DrawBack = drawback;
            }

            public readonly ushort Graphic;
            public readonly sbyte Direction1, Direction2, Direction3, Direction4;
            public readonly sbyte OffsetY, MirrorOffsetY;
            public readonly bool DrawBack;
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct UOPFrameData
        {
            public uint DataStart;
            public uint PixelDataOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private ref struct AnimIdxBlock
        {
            public readonly uint Position;
            public readonly uint Size;
            public readonly uint Unknown;
        }
    }

    internal enum ANIMATION_GROUPS : byte
    {
        AG_NONE = 0,
        AG_LOW,
        AG_HIGHT,
        AG_PEOPLE
    }

    internal enum ANIMATION_GROUPS_TYPE : byte
    {
        UNKNOWN = 0,
        MONSTER,
        SEA_MONSTER,
        ANIMAL,
        HUMAN,
        EQUIPMENT,
    }

    internal enum HIGHT_ANIMATION_GROUP : byte
    {
        HAG_WALK = 0,
        HAG_STAND,
        HAG_DIE_1,
        HAG_DIE_2,
        HAG_ATTACK_1,
        HAG_ATTACK_2,
        HAG_ATTACK_3,
        HAG_MISC_1,
        HAG_MISC_2,
        HAG_MISC_3,
        HAG_STUMBLE,
        HAG_SLAP_GROUND,
        HAG_CAST,
        HAG_GET_HIT_1,
        HAG_MISC_4,
        HAG_GET_HIT_2,
        HAG_GET_HIT_3,
        HAG_FIDGET_1,
        HAG_FIDGET_2,
        HAG_FLY,
        HAG_LAND,
        HAG_DIE_IN_FLIGHT,
        HAG_ANIMATION_COUNT
    }

    internal enum PEOPLE_ANIMATION_GROUP : byte
    {
        PAG_WALK_UNARMED = 0,
        PAG_WALK_ARMED,
        PAG_RUN_UNARMED,
        PAG_RUN_ARMED,
        PAG_STAND,
        PAG_FIDGET_1,
        PAG_FIDGET_2,
        PAG_STAND_ONEHANDED_ATTACK,
        PAG_STAND_TWOHANDED_ATTACK,
        PAG_ATTACK_ONEHANDED,
        PAG_ATTACK_UNARMED_1,
        PAG_ATTACK_UNARMED_2,
        PAG_ATTACK_TWOHANDED_DOWN,
        PAG_ATTACK_TWOHANDED_WIDE,
        PAG_ATTACK_TWOHANDED_JAB,
        PAG_WALK_WARMODE,
        PAG_CAST_DIRECTED,
        PAG_CAST_AREA,
        PAG_ATTACK_BOW,
        PAG_ATTACK_CROSSBOW,
        PAG_GET_HIT,
        PAG_DIE_1,
        PAG_DIE_2,
        PAG_ONMOUNT_RIDE_SLOW,
        PAG_ONMOUNT_RIDE_FAST,
        PAG_ONMOUNT_STAND,
        PAG_ONMOUNT_ATTACK,
        PAG_ONMOUNT_ATTACK_BOW,
        PAG_ONMOUNT_ATTACK_CROSSBOW,
        PAG_ONMOUNT_SLAP_HORSE,
        PAG_TURN,
        PAG_ATTACK_UNARMED_AND_WALK,
        PAG_EMOTE_BOW,
        PAG_EMOTE_SALUTE,
        PAG_FIDGET_3,
        PAG_ANIMATION_COUNT
    }

    internal enum LOW_ANIMATION_GROUP : byte
    {
        LAG_WALK = 0,
        LAG_RUN,
        LAG_STAND,
        LAG_EAT,
        LAG_UNKNOWN,
        LAG_ATTACK_1,
        LAG_ATTACK_2,
        LAG_ATTACK_3,
        LAG_DIE_1,
        LAG_FIDGET_1,
        LAG_FIDGET_2,
        LAG_LIE_DOWN,
        LAG_DIE_2,
        LAG_ANIMATION_COUNT
    }

    [Flags]
    internal enum ANIMATION_FLAGS : uint
    {
        AF_NONE = 0x00000,
        AF_UNKNOWN_1 = 0x00001,
        AF_USE_2_IF_HITTED_WHILE_RUNNING = 0x00002,
        AF_IDLE_AT_8_FRAME = 0x00004,
        AF_CAN_FLYING = 0x00008,
        AF_UNKNOWN_10 = 0x00010,
        AF_CALCULATE_OFFSET_LOW_GROUP_EXTENDED = 0x00020,
        AF_CALCULATE_OFFSET_BY_LOW_GROUP = 0x00040,
        AF_UNKNOWN_80 = 0x00080,
        AF_UNKNOWN_100 = 0x00100,
        AF_UNKNOWN_200 = 0x00200,
        AF_CALCULATE_OFFSET_BY_PEOPLE_GROUP = 0x00400,
        AF_UNKNOWN_800 = 0x00800,
        AF_UNKNOWN_1000 = 0x01000,
        AF_UNKNOWN_2000 = 0x02000,
        AF_UNKNOWN_4000 = 0x04000,
        AF_UNKNOWN_8000 = 0x08000,
        AF_USE_UOP_ANIMATION = 0x10000,
        AF_UNKNOWN_20000 = 0x20000,
        AF_UNKNOWN_40000 = 0x40000,
        AF_UNKNOWN_80000 = 0x80000,
        AF_FOUND = 0x80000000
    }

    struct AnimationEntry
    {
        public ANIMATION_FLAGS Flags;

        public ushort Graphic;
        public ushort GraphicConversion;

        public ushort CorpseGraphic;

        public ushort Color;
        public ushort CorpseColor;

        public ANIMATION_GROUPS_TYPE Type;

        public byte FileIndex;
        public sbyte MountOffsetY;
        internal bool IsValidMUL;

        //public AnimationEntry()
        //{
        //    Flags = 0;
        //    Graphic = 0;
        //    GraphicConversion = 0x8000;
        //    CorpseGraphic = 0;
        //    Color = 0;
        //    CorpseColor = 0;
        //    Type = ANIMATION_GROUPS_TYPE.UNKNOWN;
        //    FileIndex = 0;
        //    MountOffsetY = 0;
        //}
    }

    struct AnimationDirectionEntry
    {
        public IntPtr Address;
        public uint Size;

        public uint LastAccessTime;

        public byte FramesCount;
        public byte FileIndex;

        public bool IsUOP;
    }

    class AnimationCache
    {
        private readonly AnimationEntry[] _indexEntries = new AnimationEntry[Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT];
        private readonly AnimationFrameTexture[,,][] _cache = new AnimationFrameTexture[Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT, 100 * 2 /* uop */, 5][];
        private readonly AnimationDirectionEntry[,,] _indexCache = new AnimationDirectionEntry[Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT, 100 * 2 /* uop */, 5];
        private readonly AnimationDirectionEntry[,,] _indexBodyConversionCache = new AnimationDirectionEntry[Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT, 100 * 2 /* uop */, 5];
        private readonly byte[,] _uopIndexConvertionCache = new byte[Constants.MAX_ANIMATIONS_DATA_INDEX_COUNT, 100];
        private readonly Dictionary<ulong, (byte, UOFileIndex)> _uopHashses = new Dictionary<ulong, (byte, UOFileIndex)>();


        public ref AnimationEntry this[int graphic] => ref _indexEntries[graphic];


        public ref AnimationEntry GetEntry(int graphic)
        {
            ref AnimationEntry entry = ref _indexEntries[graphic];

            return ref entry;
    }
    
        public ref AnimationDirectionEntry GetDirectionEntry(ushort animID, byte group, byte direction, bool uop = false)
        {
            if (uop)
            {
                group = _uopIndexConvertionCache[animID, group];
                group += 100;
            }

            ref AnimationDirectionEntry entry = ref _indexCache[animID, group, direction];

            return ref entry;
        }

        public ref AnimationDirectionEntry GetConvertedDirectionEntry(ushort animID, byte group, byte direction, bool uop = false)
        {
            if (uop)
            {
                group = _uopIndexConvertionCache[animID, group];
                group += 100;
            }

            ref AnimationDirectionEntry entry = ref _indexBodyConversionCache[animID, group, direction];

            return ref entry;
        }
        
        
        public bool Push(ushort animID, byte group, byte direction, int frame, int totalFrames, AnimationFrameTexture texture, bool uop = false)
        {            
            if (frame >= totalFrames || totalFrames <= 0)
            {
                return false;
            }

            if (uop)
            {
                group = _uopIndexConvertionCache[animID, group];
                group += 100;
            }

            ref var frames = ref _cache[animID, group, direction];

            if (frames == null)
            {
                frames = new AnimationFrameTexture[totalFrames];
            }

            if (frame < frames.Length)
            {
                if (frames[frame] != null)
                {
                    Log.Error($"animation 0x{animID:X4} already exists");

                    return false;
                }
            }

            frames[frame] = texture;

            return true;
        }

        public bool Remove(ushort animID, byte group, byte direction, int frame, bool uop = false)
        {
            if (uop)
            {
                group = _uopIndexConvertionCache[animID, group];
                group += 100;
            }

            AnimationFrameTexture[] frames = _cache[animID, group, direction];

            if (frames != null && frame < frames.Length)
            {
                ref AnimationFrameTexture texture = ref frames[frame];

                if (texture != null)
                {
                    texture.Dispose();
                    texture = null;

                    return true;
                }
            }

            return false;
        }

        public void ReplaceUopGroup(ushort animID, byte oldGroup, byte newGroup)
        {
            _uopIndexConvertionCache[animID, oldGroup] = newGroup;

            //ulong oldHash = UOFileUop.CreateHash($"build/animationlegacyframe/{animID:D6}/{oldGroup:D2}.bin");
            //ulong newHash = UOFileUop.CreateHash($"build/animationlegacyframe/{animID:D6}/{newGroup:D2}.bin");

            //if (_uopHashses.TryGetValue(oldHash, out var d))
            //{

            //}

            //if (_uopHashses.TryGetValue(newHash, out var d2))
            //{

            //}


            ////_uopHashses[oldHash] = d2;
            ////_uopHashses[newHash] = d;

            //for (byte i = 0; i < 5; ++i)
            //{
            //    ref var oldDir = ref GetDirectionEntry(animID, oldGroup, i, true);
            //    ref var newDir = ref GetDirectionEntry(animID, newGroup, i, true);

            //    //IntPtr address = oldDir.Address;
            //    //byte fileIndex = oldDir.FileIndex;
            //    //uint size = oldDir.Size;

            //    //oldDir.Address = newDir.Address;
            //    //oldDir.FileIndex = newDir.FileIndex;      
            //    //oldDir.Size = newDir.Size;

            //    //// invert ?
            //    //newDir.Address = address;
            //    //newDir.FileIndex = fileIndex;
            //    //newDir.Size = size;

            //    oldDir.FramesCount = newDir.FramesCount = 0;
            //    oldDir.IsUOP = newDir.IsUOP = true;
            //}       
        }

        public void StoreUopHash(ulong hash, UOFileIndex index, byte fileNum)
        {
            _uopHashses[hash] = (fileNum, index);
        }

        public bool TryGetUopHash(ushort animID, byte group, out (byte, UOFileIndex) index)
        {
            group = _uopIndexConvertionCache[animID, group];

            ulong hash = UOFileUop.CreateHash($"build/animationlegacyframe/{animID:D6}/{group:D2}.bin");

            return TryGetUopHash(hash, out index);
        }

        public bool TryGetUopHash(ulong hash, out (byte, UOFileIndex) index)
        {
            return _uopHashses.TryGetValue(hash, out index);
        }

        public AnimationFrameTexture GetFrame(ushort animID, byte group, byte direction, int frame, bool uop = false)
        {
            if (uop)
            {
                group = _uopIndexConvertionCache[animID, group];
                group += 100;
            }

            AnimationFrameTexture[] frames = _cache[animID, group, direction];

            if (frames != null)
            {
                if (frame < 0 || frame >= frames.Length)
                {
                    frame = 0;
                }

                return _cache[animID, group, direction][frame];
            }

            return null;
        }
    }

    internal struct EquipConvData : IEquatable<EquipConvData>
    {
        public EquipConvData(ushort graphic, ushort gump, ushort color)
        {
            Graphic = graphic;
            Gump = gump;
            Color = color;
        }

        public ushort Graphic;
        public ushort Gump;
        public ushort Color;


        public override int GetHashCode()
        {
            return (Graphic, Gump, Color).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is EquipConvData v && Equals(v);
        }

        public bool Equals(EquipConvData other)
        {
            return (Graphic, Gump, Color) == (other.Graphic, other.Gump, other.Color);
        }
    }
}