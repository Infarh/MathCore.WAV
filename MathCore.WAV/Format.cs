
// ReSharper disable UnusedMember.Global

namespace MathCore.WAV
{
    public enum Format : short
    {
        ///<sumary>Microsoft Corporation</sumary>
        Unknown = 0x0000,
        ///<sumary>Microsoft Corporation</sumary>
        PCM = 0x0001,
        ///<sumary>Microsoft Corporation</sumary>
        AdPCM = 0x0002,
        ///<sumary>Microsoft Corporation</sumary>
        IEEEFloat = 0x0003,
        ///<sumary>Compaq Computer Corp.</sumary>
        Vselp = 0x0004,
        ///<sumary>IBM Corporation</sumary>
        IbmCvsd = 0x0005,
        ///<sumary>Microsoft Corporation</sumary>
        Alaw = 0x0006,
        ///<sumary>Microsoft Corporation</sumary>
        Mulaw = 0x0007,
        ///<sumary>Microsoft Corporation</sumary>
        DTS = 0x0008,
        ///<sumary>Microsoft Corporation</sumary>
        DRM = 0x0009,
        ///<sumary>OKI</sumary>
        OkiAdPCM = 0x0010,
        ///<sumary>Intel Corporation</sumary>
        DviAdPCM = 0x0011,
        /// <summary>Intel Corporation</summary>
        ImaAdPCM = DviAdPCM,
        ///<sumary>Video logic</sumary>
        MediaSpaceAdPCM = 0x0012,
        ///<sumary>Sierra Semiconductor Corp</sumary>
        SierraAdPCM = 0x0013,
        ///<sumary>Antex Electronics Corporation</sumary>
        G723AdPCM = 0x0014,
        ///<sumary>DSP Solutions, Inc.</sumary>
        DigiSTD = 0x0015,
        ///<sumary>DSP Solutions, Inc.</sumary>
        Digifix = 0x0016,
        ///<sumary>Dialogic Corporation</sumary>
        DialogicOkiAdPCM = 0x0017,
        ///<sumary>Media Vision, Inc.</sumary>
        MediaVisionAdPCM = 0x0018,
        ///<sumary>Hewlett-Packard Company</sumary>
        CuCodec = 0x0019,
        ///<sumary>Yamaha Corporation of America</sumary>
        YamahaAdPCM = 0x0020,
        ///<sumary>Speech Compression</sumary>
        Sonarc = 0x0021,
        ///<sumary>DSP Group, Inc</sumary>
        DSPgroupTrueSpeech = 0x0022,
        ///<sumary>Echo Speech Corporation</sumary>
        EchoSC1 = 0x0023,
        ///<sumary>Virtual Music, Inc.</sumary>
        AudioFileAF36 = 0x0024,
        ///<sumary>Audio Processing Technology</sumary>
        Aptx = 0x0025,
        ///<sumary>Virtual Music, Inc.</sumary>
        AudioFileAF10 = 0x0026,
        ///<sumary>Aculab plc</sumary>
        Prosody1612 = 0x0027,
        ///<sumary>Merging Technologies S.A.</sumary>
        LRC = 0x0028,
        ///<sumary>Dolby Laboratories</sumary>
        DolbyAC2 = 0x0030,
        ///<sumary>Microsoft Corporation</sumary>
        GSM610 = 0x0031,
        ///<sumary>Microsoft Corporation</sumary>
        MSNAudio = 0x0032,
        ///<sumary>Antex Electronics Corporation</sumary>
        AntexAdPCMe = 0x0033,
        ///<sumary>Control Resources Limited</sumary>
        ControlResVqLPC = 0x0034,
        ///<sumary>DSP Solutions, Inc.</sumary>
        DigiReal = 0x0035,
        ///<sumary>DSP Solutions, Inc.</sumary>
        DigiAdPCM = 0x0036,
        ///<sumary>Control Resources Limited</sumary>
        ControlResCR10 = 0x0037,
        ///<sumary>Natural MicroSystems</sumary>
        NmsVbxAdPCM = 0x0038,
        ///<sumary>Crystal Semiconductor IMA ADPCM</sumary>
        CsImaAdPCM = 0x0039,
        /// <summary>Echo Speech Corporation</summary>
        EchoSC3 = 0x003A,
        /// <summary>Rockwell International</summary>
        RockwellAdPCM = 0x003B,
        /// <summary>Rockwell International</summary>
        RockwellDigitalK = 0x003C,
        /// <summary>Xebec Multimedia Solutions Limited</summary>
        Xebec = 0x003D,
        ///<sumary>Antex Electronics Corporation</sumary>
        G721AdPCM = 0x0040,
        ///<sumary>Antex Electronics Corporation</sumary>
        G728Celp = 0x0041,
        ///<sumary>Microsoft Corporation</sumary>
        MSG723 = 0x0042,
        ///<sumary>Microsoft Corporation</sumary>
        MPEG = 0x0050,
        ///<sumary>InSoft, Inc.</sumary>
        RT24 = 0x0052,
        ///<sumary>InSoft, Inc.</sumary>
        PAC = 0x0053,
        ///<sumary>ISO/MPEG Layer3 Format Tag</sumary>
        MpegLayer3 = 0x0055,
        ///<sumary>Lucent Technologies</sumary>
        LucentG723 = 0x0059,
        ///<sumary>Cirrus Logic</sumary>
        Cirrus = 0x0060,
        ///<sumary>ESS Technology</sumary>
        EsPCM = 0x0061,
        ///<sumary>VOXWare Inc</sumary>
        VOXWare = 0x0062,
        ///<sumary>Canopus, co., Ltd.</sumary>
        CanopusAtrac = 0x0063,
        ///<sumary>APICOM</sumary>
        G726AdPCM = 0x0064,
        ///<sumary>APICOM</sumary>
        G722AdPCM = 0x0065,
        ///<sumary>Microsoft Corporation</sumary>
        DsatDisplay = 0x0067,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareByteAligned = 0x0069,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareAC8 = 0x0070,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareAC10 = 0x0071,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareAC16 = 0x0072,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareAC20 = 0x0073,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareRT24 = 0x0074,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareRT29 = 0x0075,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareRT29HW = 0x0076,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareVR12 = 0x0077,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareVR18 = 0x0078,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareTQ40 = 0x0079,
        ///<sumary>Softsound, Ltd.</sumary>
        SoftSound = 0x0080,
        ///<sumary>VOXWare Inc</sumary>
        VOXWareTQ60 = 0x0081,
        ///<sumary>Microsoft Corporation</sumary>
        Msrt24 = 0x0082,
        ///<sumary>AT&amp;T Labs, Inc.</sumary>
        G729A = 0x0083,
        ///<sumary>Motion Pixels</sumary>
        MviMvi2 = 0x0084,
        ///<sumary>DataFusion Systems (Pty) (Ltd)</sumary>
        DfG726 = 0x0085,
        ///<sumary>DataFusion Systems (Pty) (Ltd)</sumary>
        DfGSM610 = 0x0086,
        ///<sumary>Iterated Systems, Inc.</sumary>
        IsiAudio = 0x0088,
        ///<sumary>OnLive! Technologies, Inc.</sumary>
        OnLive = 0x0089,
        ///<sumary>Siemens Business Communications Sys</sumary>
        Sbc24 = 0x0091,
        ///<sumary>Sonic Foundry</sumary>
        DolbyAC3Spdif = 0x0092,
        ///<sumary>MediaSonic</sumary>
        MediaSonicG723 = 0x0093,
        ///<sumary>Aculab plc</sumary>
        Prosody_8Kbps = 0x0094,
        ///<sumary>ZyXEL Communications, Inc.</sumary>
        ZyxelAdPCM = 0x0097,
        ///<sumary>Philips Speech Processing</sumary>
        PhilipsLpcbb = 0x0098,
        ///<sumary>Studer Professional Audio AG</sumary>
        Packed = 0x0099,
        /// <summary>Malden Electronics Ltd.</summary>
        MaldenPhonyTalk = 0x00A0,
        ///<sumary>Rhetorex Inc.</sumary>
        RhetorexAdPCM = 0x0100,
        ///<sumary>BeCubed Software Inc.</sumary>
        Irat = 0x0101,
        ///<sumary>Vivo Software</sumary>
        VivoG723 = 0x0111,
        ///<sumary>Vivo Software</sumary>
        VivoSiren = 0x0112,
        ///<sumary>Digital Equipment Corporation</sumary>
        DigitalG723 = 0x0123,
        ///<sumary>Sanyo Electric Co., Ltd.</sumary>
        SanyoLdAdPCM = 0x0125,
        ///<sumary>Sipro Lab Telecom Inc.</sumary>
        SiprolabAceplnet = 0x0130,
        ///<sumary>Sipro Lab Telecom Inc.</sumary>
        SiprolabAcelp4800 = 0x0131,
        ///<sumary>Sipro Lab Telecom Inc.</sumary>
        SiprolabAcelp8V3 = 0x0132,
        ///<sumary>Sipro Lab Telecom Inc.</sumary>
        SiprolabG729 = 0x0133,
        ///<sumary>Sipro Lab Telecom Inc.</sumary>
        SiprolabG729A = 0x0134,
        ///<sumary>Sipro Lab Telecom Inc.</sumary>
        SiprolabKelvin = 0x0135,
        ///<sumary>Dictaphone Corporation</sumary>
        G726ADPCM = 0x0140,
        ///<sumary>Qualcomm, Inc.</sumary>
        QualcommPurevoice = 0x0150,
        ///<sumary>Qualcomm, Inc.</sumary>
        QualcommHalfRate = 0x0151,
        ///<sumary>Ring Zero Systems, Inc.</sumary>
        TubGSM = 0x0155,
        ///<sumary>Microsoft Corporation</sumary>
        Msaudio1 = 0x0160,
        ///<sumary>Unisys Corp.</sumary>
        UnisysNapAdPCM = 0x0170,
        ///<sumary>Unisys Corp.</sumary>
        UnisysNapUlaw = 0x0171,
        ///<sumary>Unisys Corp.</sumary>
        UnisysNapAlaw = 0x0172,
        ///<sumary>Unisys Corp.</sumary>
        UnisysNap16K = 0x0173,
        ///<sumary>Creative Labs, Inc</sumary>
        CreativeAdPCM = 0x0200,
        ///<sumary>Creative Labs, Inc</sumary>
        CreativeFastspeech8 = 0x0202,
        ///<sumary>Creative Labs, Inc</sumary>
        CreativeFastspeech10 = 0x0203,
        ///<sumary>UHER informatic GmbH</sumary>
        UherAdPCM = 0x0210,
        ///<sumary>Quarterdeck Corporation</sumary>
        Quarterdeck = 0x0220,
        ///<sumary>I-link Worldwide</sumary>
        IlinkVc = 0x0230,
        ///<sumary>Aureal Semiconductor</sumary>
        RawSport = 0x0240,
        ///<sumary>ESS Technology, Inc.</sumary>
        EsstAc3 = 0x0241,
        ///<sumary>Interactive Products, Inc.</sumary>
        IpiHsx = 0x0250,
        ///<sumary>Interactive Products, Inc.</sumary>
        IpiRpelp = 0x0251,
        ///<sumary>Consistent Software</sumary>
        Cs2 = 0x0260,
        ///<sumary>Sony Corp.</sumary>
        SonyScx = 0x0270,
        ///<sumary>Fujitsu Corp.</sumary>
        FmTownsSnd = 0x0300,
        ///<sumary>Brooktree Corporation</sumary>
        BtvDigital = 0x0400,
        ///<sumary>QDesign Corporation</sumary>
        QdesignMusic = 0x0450,
        ///<sumary>AT&amp;T Labs, Inc.</sumary>
        VmeVmpcm = 0x0680,
        ///<sumary>AT&amp;T Labs, Inc.</sumary>
        TMC = 0x0681,
        ///<sumary>Ing C. Olivetti &amp; C., S.p.A.</sumary>
        OliGSM = 0x1000,
        ///<sumary>Ing C. Olivetti &amp; C., S.p.A.</sumary>
        OliAdPCM = 0x1001,
        ///<sumary>Ing C. Olivetti &amp; C., S.p.A.</sumary>
        OliCelp = 0x1002,
        ///<sumary>Ing C. Olivetti &amp; C., S.p.A.</sumary>
        OliSBC = 0x1003,
        ///<sumary>Ing C. Olivetti &amp; C., S.p.A.</sumary>
        OliOPR = 0x1004,
        ///<sumary>Lernout &amp; Hauspie</sumary>
        LhCodec = 0x1100,
        ///<sumary>Norris Communications, Inc.</sumary>
        Norris = 0x1400,
        ///<sumary>AT&amp;T Labs, Inc.</sumary>
        SoundSpaceMusiCompress = 0x1500,
        ///<sumary>FAST Multimedia AG</sumary>
        DVM = 0x2000,
    }
}
