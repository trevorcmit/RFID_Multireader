using System;
using System.Collections.Generic;
using System.Text;


namespace CSLibrary.Barcode.Constants {
    public enum MessageType
    {
        DEC_MSG,
        ERR_MSG
    }

    public enum BarcodeState 
    {
        IDLE,
        BUSY,
        STOPPING,
    }

    public enum Result
    {
        INITIALIZE = -1,
        SUCCESS = 0,                
        ERR_BADFILENAME,
        ERR_BADPORT,
        ERR_BADREGION,
        ERR_DRIVER,              
        ERR_ENGINEBUSY,
        ERR_FILE,
        ERR_FILEINCOMPATIBLE,
        ERR_FILEINVALID,
        ERR_MEMORY,
        ERR_NODECODE,
        ERR_NODRIVER,
        ERR_NOIMAGE,
        ERR_NORESPONSE,
        ERR_NOTCONNECTED,
        ERR_PARAMETER,           
        ERR_UNSUPPORTED,         
        ERR_UPGRADE,
        ERR_MENUDECODE,
        ERR_REFLASH,
        ERR_NOTRIGGER,           
        ERR_BADSMARTIMAGE,
        ERR_SMARTIMAGETOOLARGE,
        ERR_BUFFER_TOO_SMALL,
        /// <summary>
        /// Undefined error.
        /// </summary>
        EOT = 256,
        /// <summary>
        /// Error starting asynchronous transfer thread.
        /// </summary>
        ERR_READTHREAD_START,
        /// <summary>
        /// Error stopping asynchronous transfer thread.
        /// </summary>
        ERR_READTHREAD_STOP,
        /// <summary>
        /// Error configuring transfer thread.
        /// </summary>
        ERR_POLLEVENT,
        /// <summary>
        /// Wrong structure passed in for the type specified.
        /// </summary>
        ERR_WRONGRESULTSTRUCT,
        /// <summary>
        /// User called hhpCancelIo to abort operation.
        /// </summary>
        ERR_USER_CANCEL,
        /// <summary>
        /// Received a NAK on response.
        /// </summary>
        ERR_NAK,
        /// <summary>
        /// Error compressing image data.
        /// </summary>
        ERR_COMPRESSION_FAILED,
        /// <summary>
        /// Error decompressing image data.
        /// </summary>
        ERR_DECOMPRESSION_FAILED,
        /// <summary>
        /// Imager failed to capture the image.
        /// </summary>
        ERR_CAPTURE_IMAGE_FAILED,
        /// <summary>
        /// Imager failed to ship captured image.
        /// </summary>
        ERR_SHIP_IMAGE_FAILED,
        /// <summary>
        /// Imager connected but is running in boot code.
        /// </summary>
        ERR_CONNECT_BOOT_CODE,
        /// <summary>
        /// Requested baud rate not supported by host port.
        /// </summary>
        ERR_BAUD_TO_HIGH,
        ERR_INVALID_COMM_PARAMS,
        ERR_UNICODE_UNSUPPORTED,
        ERR_INTERNAL_ERROR,
        ERR_NOINTELBARCODE,
        ERR_BADINTELIMAGE,
        ERR_NOINTELIMAGE,
        ERR_DLL_FILE,
        ERR_DECODER_INIT_FAILED,
        ERR_IMAGE_PROCESSING_FAILED,
        ERR_UNSUPPORTED_ENGINE,
        ERR_SYMBOLOGY_HAS_NO_RANGE,
        ERR_COMMUNICATIONS_ERROR,
        ERR_INVALID_PARAMETER
    }

    public enum SetupType
    {
        DEFAULT = 0,
        CURRENT,
    }

    public enum BaudRate
    {
        SERIAL_BAUD_300 = 300,
        SERIAL_BAUD_600 = 600,
        SERIAL_BAUD_1200 = 1200,
        SERIAL_BAUD_2400 = 2400,
        SERIAL_BAUD_4800 = 4800,
        SERIAL_BAUD_9600 = 9600,
        SERIAL_BAUD_19200 = 19200,
        /// <summary>
        /// 38400 BPS
        /// </summary>
        SERIAL_BAUD_38400 = 38400,
        /// <summary>
        /// 57600 BPS
        /// </summary>
        SERIAL_BAUD_57600 = 57600,
        /// <summary>
        /// 115200 BPS
        /// </summary>
        SERIAL_BAUD_115200 = 115200,
        SERIAL_BAUD_230400 = 230400,
        SERIAL_BAUD_460800 = 460800,
        /// <summary>
        /// 921600 BPS
        /// </summary>
        SERIAL_BAUD_921600 = 921600
    }

    public enum DataBit
    {
        BITS_7 = 7,
        BITS_8
    } 

    public enum Parity
    {
        NONE = 'N',
        ODD = 'O',
        EVEN = 'E',
        MARK = 'M',
        SPACE = 'S'
    }

    public enum StopBit
    {
        ONE = 1,
        TWO,
    } 

    public enum DecoderType
    {
        /// <summary>
        /// 1D Linear and stacked linear codes only.
        /// </summary>
        SDK_1D_CODES_ONLY=0,
        /// <summary>
        /// Same as above plus PDF417 and MicroPDF417.
        /// </summary>
        SDK_1D_AND_PDF_CODES,
        /// <summary>
        /// All symbologies.
        /// </summary>
        SDK_1D_AND_2D_CODES
    }

    public enum ImagerType
    {
        UNKNOWN_IMAGER = 0,
        DECODED_IMAGER_4080 = (EngineType.IT4000 * 10),
        DECODED_IMAGER_4080_USB = (ImagerType.DECODED_IMAGER_4080 + 1),
        NON_DECODED_IMAGER_4000 = (ImagerType.DECODED_IMAGER_4080 + 2),
        // Imager 5000 variants
        /// <summary>
        /// Serial (RS-232) 5080 VGA imager with internal decoder.
        /// </summary>
        DECODED_IMAGER_5080VGA = (EngineType.IT5X00VGA * 10),
        /// <summary>
        /// USB serial 5080 VGA imager with internal decoder.
        /// </summary>
        DECODED_IMAGER_5080VGA_USB = (ImagerType.DECODED_IMAGER_5080VGA + 1),
        /// <summary>
        /// Serial (RS-232) 5000 VGA imager with internal decoder.
        /// </summary>
        NON_DECODED_IMAGER_5000VGA = (ImagerType.DECODED_IMAGER_5080VGA + 2),
        // Imager 5000 VGA with PSOC variants
        /// <summary>
        /// Serial (RS-232) 5080 VGA imager with internal decoder and PSOC.
        /// </summary>
        DECODED_IMAGER_5080VGA_PSOC = (EngineType.IT5X00VGA_PSOC * 10),
        /// <summary>
        /// USB serial 5080 VGA  imager with internal decoder and PSOC.
        /// </summary>
        DECODED_IMAGER_5080VGA_PSOC_USB = (ImagerType.DECODED_IMAGER_5080VGA_PSOC + 1),
        /// <summary>
        /// Serial (RS-232) 5000 VGA imager with internal decoder and PSOC.
        /// </summary>
        NON_DECODED_IMAGER_5000VGA_PSOC = (ImagerType.DECODED_IMAGER_5080VGA_PSOC + 2),
        // Imager 5000  variants
        /// <summary>
        /// Serial (RS-232) 5080 imager with internal decoder.
        /// </summary>
        DECODED_IMAGER_5080 = (EngineType.IT5X00 * 10),
        /// <summary>
        /// USB serial 5080 imager with internal decoder.
        /// </summary>
        DECODED_IMAGER_5080_USB = (ImagerType.DECODED_IMAGER_5080 + 1),
        /// <summary>
        /// Serial (RS-232) 5000 imager with internal decoder.
        /// </summary>
        NON_DECODED_IMAGER_5000 = (ImagerType.DECODED_IMAGER_5080 + 2),
        // Imager 5000 with PSOC variants
        /// <summary>
        /// Serial (RS-232) 5080  imager with internal decoder and PSOC.
        /// </summary>
        DECODED_IMAGER_5080_PSOC = (EngineType.IT5X00_PSOC * 10),
        /// <summary>
        /// USB serial 5080  imager with internal decoder and PSOC.
        /// </summary>
        DECODED_IMAGER_5080_PSOC_USB = (ImagerType.DECODED_IMAGER_5080_PSOC + 1),
        /// <summary>
        /// Serial (RS-232) 5000  imager with internal decoder and PSOC.
        /// </summary>
        NON_DECODED_IMAGER_5000_PSOC = (ImagerType.DECODED_IMAGER_5080_PSOC + 2)

    }

    /// <summary>
    /// Event type enum
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Barcode event
        /// </summary>
        BARCODE_EVENT=0,         // DECODE_MSG
        /// <summary>
        /// Image event
        /// </summary>
        IMAGE_EVENT,             // IMAGE
        /// <summary>
        /// Text message event
        /// </summary>
        TEXT_MSG_EVENT,          // TEXT_MSG
        /// <summary>
        /// Intelligent barcode event
        /// </summary>
        INTELIMG_BARCODE_EVENT,  // DECODE_MSG
        /// <summary>
        /// Intelligent image event
        /// </summary>
        INTELIMG_IMAGE_EVENT,    // IMAGE
        /// <summary>
        /// Not fully implemented.
        /// </summary>
        TRIGGER_EVENT            // BOOL pointer (on/off) Notification of
                                     // a hardware trigger when Trigger mode 5.
    } 

    /// <summary>
    /// Connection types
    /// </summary>
    enum ComPort
    {
        COM1=0,
        COM2,
        COM3,
        COM4,
        COM5,
        COM6,
        COM7,
        COM8,
        COM9,
        COM10,
        COM11,
        COM12,
        COM13,
        COM14,
        COM15,
        COM16,
        COM17,
        COM18,
        COM19,
        LAST_COMM_PORT = 255

    }
    /// <summary>
    /// Action Commands
    /// </summary>
    public enum Action
    {
        /// <summary>
        /// Aimer command (turn aimers on/off).
        /// </summary>
        AIMER_CMD = 0,
        /// <summary>
        /// No longer supported by imagers.
        /// </summary>
        ILLUMINATION_CMD,
        /// <summary>
        /// Beeper command (sound a single or double beep).
        /// </summary>
        BEEP_CMD

    }
    /// <summary>
    /// On/Off enum
    /// </summary>
    [Obsolete("This enum will not use in fature")]
    public enum OnOff
    {
        /// <summary>
        /// OFF
        /// </summary>
        OFF=0,
        /// <summary>
        /// ON
        /// </summary>
        ON=1

    };
    /// <summary>
    /// Control of Led on/off
    /// </summary>
    public enum Led
    {
        /// <summary>
        /// OFF
        /// </summary>
        OFF = 0,
        /// <summary>
        /// ON
        /// </summary>
        ON = 1
    }

    /// <summary>
    /// Beep Execute enum
    /// </summary>
    public enum Beep
    {
        /// <summary>
        /// Single Beep
        /// </summary>
        SINGLE_BEEP = 0,
        /// <summary>
        /// Double Beep
        /// </summary>
        DOUBLE_BEEP = 1
    };
    /// <summary>
    /// Plug and Play Command Enum
    /// </summary>
    public enum PlugAndPlay
    {
        /// <summary>
        /// Plug and play
        /// </summary>
        PNP_OCR_PASSPORT=0
    }

    #region Config constants

    ///<summary>
    /// Image formats
    ///</summary>
    public enum ImgFormat
    {
        /// <summary>
        /// 1 bit per pixel – Each row padded out to byte boundary.
        /// </summary>
        FF_RAW_BINARY = 0,
        /// <summary>
        /// 8 bits per pixel.
        /// </summary>
        FF_RAW_GRAY,
        /// <summary>
        /// TIFF bilevel uncompressed.
        /// </summary>
        FF_TIFF_BINARY,
        /// <summary>
        /// TIFF bilevel packbits compressed.
        /// </summary>
        FF_TIFF_BINARY_PACKBITS,
        /// <summary>
        /// TIFF 8 bits per pixel uncompressed.
        /// </summary>
        FF_TIFF_GRAY,
        /// <summary>
        /// JPEG lossy compression.
        /// </summary>
        FF_JPEG_GRAY,
        /// <summary>
        /// Windows BMP file uncompressed.
        /// </summary>
        FF_BMP_GRAY,
    }

    ///<summary>
    /// Compression mode formats
    ///</summary>
    public enum Compression
    {
        /// <summary>
        /// No compression.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Huffman lossless compression.
        /// </summary>
        LOSSLESS,
        /// <summary>
        /// JPEG lossy compression.
        /// </summary>
        LOSSY,
    }

    ///<summary>
    /// Capture Illumination Duty Cycle
    ///</summary>
    public enum DutyCycle
    {
        ///<summary>
        /// Keep off during image capture.
        ///</summary>
        DUTY_CYCLE_OFF = 0,
        ///<summary>
        /// Turn on for image capture.
        ///</summary>
        DUTY_CYCLE_ON,
    }

    ///<summary>
    /// Auto Exposure type
    ///</summary>
    public enum AutoExposure
    {
        ///<summary>
        /// Autoexposure for decode image (darker with less noise).
        ///</summary>
        BARCODE = 0,
        ///<summary>
        /// Autoexposure for pictures. (lighter, more pleasing image).
        ///</summary>
        PHOTO,
        ///<summary>
        /// No Autoexposure. User should supply Exposure, Gain, and 
        /// Frame Rate Values. 
        ///</summary>
        MANUAL,
    }

    ///<summary>
    /// Min and Max manual exposure
    /// Gain Values
    ///</summary>
    public enum Gain
    {
        ///<summary>
        /// Min and Max manual exposure
        /// Gain Values
        ///</summary>
        GAIN_1x = 1,
        ///<summary>
        /// Min and Max manual exposure
        /// Gain Values
        ///</summary>
        GAIN_2x,
        ///<summary>
        /// Min and Max manual exposure
        /// Gain Values
        ///</summary>
        GAIN_4x = 4,
    }

    ///<summary>
    /// Frame rate enum
    ///</summary>
    public enum FrameRate
    {
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_1_FRAMES_PER_SEC = 1,
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_2_FRAMES_PER_SEC,
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_3_FRAMES_PER_SEC,
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_4_FRAMES_PER_SEC,
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_5_FRAMES_PER_SEC,
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_6_FRAMES_PER_SEC,
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_10_FRAMES_PER_SEC = 10,
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_12_FRAMES_PER_SEC = 12,
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_15_FRAMES_PER_SEC = 15,
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_20_FRAMES_PER_SEC = 20,
        ///<summary>
        /// Frame rate enum
        ///</summary>
        R_30_FRAMES_PER_SEC = 30,
    }

    ///<summary>
    /// Beeper Volume enum
    ///</summary>
    public enum BeeperVolume
    {
        ///<summary>
        /// Don't sound beeper – no volume.
        ///</summary>
        OFF = 0,
        ///<summary>
        /// Low volume.
        ///</summary>
        LOW = 1,
        ///<summary>
        /// Medium volume.
        ///</summary>
        MEDIUM = 2,
        ///<summary>
        /// Loudest volume.
        ///</summary>
        HIGH = 3,
    }

    ///<summary>
    /// Trigger Modes type
    ///</summary>
    public enum TriggerMode
    {
        ///<summary>
        /// Manual trigger no low power mode.
        ///</summary>
        MANUAL_SERIAL = 0,
        ///<summary>
        /// Unused
        ///</summary>
        UNUSED,
        ///<summary>
        /// Manual trigger with low power mode on timeouts
        ///</summary>
        MANUAL_LOW_POWER,
        ///<summary>
        /// Looks for objects which can be barcodes and triggers if found
        ///</summary>
        PRESENTATION,
        ///<summary>
        /// Triggers if scanstand barcode not detected
        ///</summary>
        SCANSTAND,
        ///<summary>
        /// Unused
        ///</summary>
        HOST_NOTIFY,
        ///<summary>
        /// Imager Ships image on trigger.
        ///</summary>
        SNAP_AND_SHIP,
    }

    ///<summary>
    /// Sequence Modes type
    ///</summary>
    public enum SequenceMode
    {
        ///<summary>
        /// Disable Sequence Modes
        ///</summary>
        DISABLED = 0,
        ///<summary>
        /// Enable Sequence Modes
        ///</summary>
        ENABLED,
        ///<summary>
        /// Required Sequence Modes
        ///</summary>
        REQUIRED,
    }
    ///<summary>
    /// Decoder mode enum
    ///</summary>
    public enum DecoderMode
    {
        ///<summary>
        /// Normal decode mode (default)
        ///</summary>
        STANDARD = 0,
        ///<summary>
        /// Fast omni directional decoder
        ///</summary>
        QUICK_OMNI,
        ///<summary>
        /// Non omni Advanced Linear Decoder
        ///</summary>
        NONOMNI_ALD,
        ///<summary>
        /// Omni directional linear decoder
        ///</summary>
        OMNI_LINEAR,
    }

    ///<summary>
    /// Led Color
    ///</summary>
    public enum IllumLedColor
    {
        ///<summary>
        /// Secondary Color (Green/White/whatever) if supported
        ///</summary>
        SECONDARY_LEDS = 0,
        ///<summary>
        /// Default/Primary Illumination color (usually red)
        ///</summary>
        PRIMARY_LEDS,
    }

    ///<summary>
    /// Aimer Modes
    ///</summary>
    public enum AimerModes
    {
        ///<summary>
        /// No aimer LED's.
        ///</summary>
        ALWAYS_OFF = 0,
        ///<summary>
        /// Aimer LED's and Illumination LED's on simultaneously.
        ///</summary>
        ILLUM_AND_AIM,
        ///<summary>
        /// Aimer LED's always on.
        ///</summary>
        ALWAYS_ON,
    }

    ///<summary>
    /// System (MPU) clock speeds enum
    ///</summary>
    public enum SystemSpeed
    {
        /// <summary>
        /// 96 MHz System (MPU) clock speeds
        /// </summary>
        SPEED_96MHZ = 0,
        /// <summary>
        /// 48 MHz System (MPU) clock speeds
        /// </summary>
        SPEED_48MHZ = 1,
        /// <summary>
        /// 32 MHz System (MPU) clock speeds
        /// </summary>
        SPEED_32MHZ = 2,
    }
    ///<summary>
    /// Engine Id values from imager.
    ///</summary>
    public enum EngineType
    {
        ///<summary>
        /// Engine Id values from imager.
        ///</summary>
        NONE = 0,
        ///<summary>
        /// Engine Id values from imager.
        ///</summary>
        IT4200 = 1,
        ///<summary>
        /// Engine Id values from imager.
        ///</summary>
        IT4000 = 5,
        ///<summary>
        /// Engine Id values from imager.
        ///</summary>
        IT4100 = 6,
        ///<summary>
        /// Engine Id values from imager.
        ///</summary>
        IT4300 = 7,
        ///<summary>
        /// Engine Id values from imager.
        ///</summary>
        IT5X00VGA = 10,
        ///<summary>
        /// Engine Id values from imager.
        ///</summary>
        IT5X00VGA_PSOC = 11,
        ///<summary>
        /// Engine Id values from imager.
        ///</summary>
        IT5X00 = 12,
        ///<summary>
        /// Engine Id values from imager.
        ///</summary>
        IT5X00_PSOC = 13,
    }

    ///<summary>
    /// CONFIG Read\Write configuration item settings
    ///</summary>
    public enum ConfigItems
    {
        ///<summary>
        /// Beeper config
        ///</summary>
        BEEPER_CONFIG = 0,
        ///<summary>
        /// Trigger config
        ///</summary>
        TRIGGER_CONFIG,
        ///<summary>
        /// Image decoder config
        ///</summary>
        DECODER_CONFIG,
        ///<summary>
        /// Power config
        ///</summary>
        POWER_CONFIG,
        ///<summary>
        /// Version information
        ///</summary>
        VERSION_INFO,
        ///<summary>
        /// Symbology config
        ///</summary>
        SYMBOLOGY_CONFIG,
        ///<summary>
        /// Serial port config
        ///</summary>
        SERIAL_PORT_CONFIG,
        ///<summary>
        /// Image acquisition
        ///</summary>
        IMAGE_ACQUISITION,
        ///<summary>
        /// Image transfer
        ///</summary>
        IMAGE_TRANSFER,
        ///<summary>
        /// Sequence config
        ///</summary>
        SEQUENCE_CONFIG,
        ///<summary>
        /// All item config
        ///</summary>
        ALL_CONFIG,
    }
    /// <summary>
    /// CodePage for BarcodeData
    /// </summary>
    public enum CodePage : uint
    {
        /// <summary>
        /// default to ANSI code page
        /// </summary>
        CP_ACP = 0,
        /// <summary>
        /// default to OEM  code page
        /// </summary>
        CP_OEMCP = 1,
        /// <summary>
        /// default to MAC  code page
        /// </summary>
        CP_MACCP = 2,
        /// <summary>
        /// current thread's ANSI code page
        /// </summary>
        CP_THREAD_ACP = 3,
        /// <summary>
        /// SYMBOL translations
        /// </summary>
        CP_SYMBOL = 42,
        /// <summary>
        /// UTF-7 translation
        /// </summary>
        CP_UTF7 = 65000,
        /// <summary>
        /// UTF-8 translation
        /// </summary>
        CP_UTF8 = 65001,
    }

    #endregion

    /// <summary>
    /// Supported OCR Fonts
    /// </summary>
    public enum OCR
    {
        /// <summary>
        /// Disable OCR Codes.
        /// </summary>
        DISABLED = 0,
        /// <summary>
        /// Enable OCR-A Font Decoding.
        /// </summary>
        A,
        /// <summary>
        /// Enable OCR-B Font Decoding.
        /// </summary>
        B,
        /// <summary>
        /// Enable Money Font Decoding.
        /// </summary>
        MONEY,
        /// <summary>
        /// Not supported.
        /// </summary>
        MICR_UNSUPPORTED,
    }
    /// <summary>
    /// OCR directions
    /// </summary>
    public enum OCRDirection
    {
        /// <summary>
        /// LeftToRight
        /// </summary>
        LeftToRight = 0,
        /// <summary>
        /// TopToBottom
        /// </summary>
        TopToBottom,
        /// <summary>
        /// RightToLeft
        /// </summary>
        RightToLeft,
        /// <summary>
        /// BottomToTop
        /// </summary>
        BottomToTop,
    }

    #region Serial Port config
#if NOUSE
    /// <summary>
    /// Serial Port Baud Rates
    /// </summary>
    public enum OEM_BAUD_RATE
    {
        OEM_BAUD_300 = 300,
        OEM_BAUD_600 = 600,
        OEM_BAUD_1200 = 1200,
        OEM_BAUD_2400 = 2400,
        OEM_BAUD_4800 = 4800,
        OEM_BAUD_9600 = 9600,
        OEM_BAUD_19200 = 19200,
        OEM_BAUD_38400 = 38400,
        OEM_BAUD_57600 = 57600,
        OEM_BAUD_115200 = 115200,
        OEM_BAUD_230400 = 230400,
        OEM_BAUD_460800 = 460800,
        OEM_BAUD_921600 = 921600

    }

    ///<summary>
    /// Serial Port Stop Bits
    ///</summary>
    public enum OEM_STOP_BITS
    {
        ///<summary>
        /// 1 Stop Bits
        ///</summary>
        STOP_BITS_ONE = 0,
        ///<summary>
        /// 2 Stop Bits
        ///</summary>
        STOP_BITS_TWO = 2,
    }

    ///<summary>
    /// Used by a caller for data bits configuration.
    ///</summary>
    public enum OEM_DATA_BITS
    {
        ///<summary>
        /// Used by a caller for data bits configuration.
        ///</summary>
        DATA_BITS_SEVEN = 7,
        ///<summary>
        /// Used by a caller for data bits configuration.
        ///</summary>
        DATA_BITS_EIGHT,
    }

    ///<summary>
    /// Used by a caller for parity configuration.
    ///</summary>
    public enum OEM_PARITY
    {
        ///<summary>
        /// Used by a caller for parity configuration.
        ///</summary>
        NOPAR = 0,
        ///<summary>
        /// Used by a caller for parity configuration.
        ///</summary>
        ODDPAR,
        ///<summary>
        /// Used by a caller for parity configuration.
        ///</summary>
        EVENPAR,
        ///<summary>
        /// Used by a caller for parity configuration.
        ///</summary>
        MARKPAR,
        ///<summary>
        /// Used by a caller for parity configuration.
        ///</summary>
        SPACEPAR,
    }

    ///<summary>
    /// Used by a caller for RTS level.
    ///</summary>
    public enum OEM_RTS_CONTROL
    {
        ///<summary>
        /// Used by a caller for RTS level.
        ///</summary>
        RTS_LINE_HIGH = 1,
        ///<summary>
        /// Used by a caller for RTS level.
        ///</summary>
        RTS_LINE_HANDSHAKE,
    }
#endif
    #endregion

    #region MASK

    /// <summary>
    /// Image Transfer bit masks
    /// </summary>
    [Flags]
    public enum ImgTransferMask : uint
    {
        /// <summary>
        /// Number of bits per pixel transferred (1 or 8).
        /// </summary>
        BITS_PER_PIXEL_MASK = 0x00001,
        /// <summary>
        ///  Subsample value (1-10).
        /// </summary>
        SUBSAMPLE_VALUE_MASK = 0x00002,
        /// <summary>
        ///  Rectangular region within image to send.
        /// </summary>
        BOUNDING_RECTANGLE_MASK = 0x00004,
        /// <summary>
        ///  No Compression, Lossless or Lossy.
        /// </summary>
        COMPRESSION_MODE_MASK = 0x00008,
        /// <summary>
        ///  Range -> 0 - 255.
        /// </summary>
        HISTOGRAM_STRETCH_MASK = 0x00010,
        /// <summary>
        /// If lossy compression, image quality percent.
        /// </summary>
        COMPRESSION_FACTOR_MASK = 0x00020,
        /// <summary>
        /// Edge sharpening filter.
        /// </summary>
        EDGE_ENHANCEMENT_MASK = 0x00100,
        /// <summary>
        /// Gamma correction.
        /// </summary>
        GAMMA_CORRECTION_MASK = 0x00200,
        /// <summary>
        /// Text sharpening filter.
        /// </summary>
        TEXT_ENHANCEMENT_MASK = 0x00400,
        /// <summary>
        /// Sharpening for image beyond normal focus.
        /// </summary>
        INFINITY_FILTER_MASK = 0x00800,
        /// <summary>
        /// Rotate the image 180 degrees.
        /// </summary>
        FLIP_IMAGE_MASK = 0x01000,
        /// <summary>
        /// Smoothing (fly spec) filter.
        /// </summary>
        NOISE_FILTER_MASK = 0x02000,
        /// <summary>
        /// Transfer progress message window.
        /// </summary>
        TRANSFER_UPDATE_HWND = 0x00040,
        /// <summary>
        /// Pointer to DWORD for percent of transfer complete.
        /// </summary>
        TRANSFER_UPDATE_DWORD = 0x00080,
        /// <summary>
        /// Mask to select all configuration items in structure.
        /// </summary>
        TRANSFER_MASK_ALL = 0x03fff,
        /// <summary>
        /// Mask to select all structure members.
        /// </summary>
        TRANSFER_MASK_ALL_NO_NOTIFY = 0x03f3f,
    }
    /// <summary>
    /// Image Acquisition bit masks
    /// </summary>
    [Flags]
    public enum ImgAcquisitionMask : uint
    {
        /// <summary>
        /// Target value (0-255) for the “white” pixel.
        /// </summary>
        WHITE_VALUE_MASK = 0x00001,
        /// <summary>
        /// Acceptable delta from target white.
        /// </summary>
        WHITE_WINDOW_MASK = 0x00002,
        /// <summary>
        /// Max # of frames to try to get white value.
        /// </summary>
        MAX_CAPTURE_RETRIES_MASK = 0x00004,
        /// <summary>
        /// How LEDs behave during image capture.
        /// </summary>
        ILLUMINATION_DUTY_CYCLE_MASK = 0x00008,
        /// <summary>
        /// Duplicate of above mask.
        /// </summary>
        LIGHTS_DUTY_CYCLE_MASK = 0x00008,
        /// <summary>
        /// How aimers behave during image capture.
        /// </summary>
        AIMER_DUTY_CYCLE_MASK = 0x00010,
        /// <summary>
        /// If manual mode, gain value to use.
        /// </summary>
        FIXED_GAIN_MASK = 0x00020,
        /// <summary>
        /// If manual mode, exposure value to use.
        /// </summary>
        FIXED_EXPOSURE_MASK = 0x00040,
        /// <summary>
        /// If manual mode, frame rate to use.
        /// </summary>
        FRAME_RATE_MASK = 0x00080,
        /// <summary>
        /// Barcode, Photo, or manual AGC mode.
        /// </summary>
        AUTOEXPOSURE_MODE_MASK = 0x00100,
        /// <summary>
        /// Same as above mask.
        /// </summary>
        IMAGE_CAPTURE_MODE_MASK = 0x00100,
        /// <summary>
        /// Wait for trigger before capture.
        /// </summary>
        WAIT_FOR_TRIGGER_MASK = 0x00200,
        /// <summary>
        /// Capture a preview image (214x160x8 JPEG).
        /// </summary>
        PREVIEW_MODE_IMAGE_MASK = 0x00400,
        /// <summary>
        /// VGA Compatible image mode
        /// </summary>
        VGA_COMPATIBLE_IMAGE_MASK = 0x00800,
        /// <summary>
        /// Mask for all configuration items.
        /// </summary>
        CAPTURE_MASK_CONFIG_ALL = 0x009ff,
        /// <summary>
        /// Mask for all manual exposure mode items.
        /// </summary>
        CAPTURE_MASK_FIXED_AGC = 0x00980,
        /// <summary>
        /// Mask for all structure members.
        /// </summary>
        CAPTURE_MASK_ALL = 0x00fff,
    }
    /// <summary>
    /// Beeper bit masks
    /// </summary>
    [Flags]
    public enum BeeperMask : uint
    {
        /// <summary>
        /// Beep on successful decode.
        /// </summary>
        ON_DECODE = 0x00001,
        /// <summary>
        /// Beep on imager reset.
        /// </summary>
        SHORT_BEEP = 0x00002,
        /// <summary>
        /// Beep on receive menu command.
        /// </summary>
        MENU_CMD_BEEP = 0x00004,
        /// <summary>
        /// Set the beeper volume.
        /// </summary>
        BEEP_VOLUME = 0x00008,
        /// <summary>
        /// Mask for all members valid.
        /// </summary>
        ALL = 0x0000f,
    }
    /// <summary>
    /// Trigger bit masks
    /// </summary>
    [Flags]
    public enum TriggerMask : uint
    {
        /// <summary>
        /// Enable/disable manual trigger mode.
        /// </summary>
        TRIG_MODE = 0x00001,
        /// <summary>
        /// Sanity timeout on trigger event.
        /// </summary>
        TIMEOUT = 0x00002,
        /// <summary>
        /// All members valid mask.
        /// </summary>
        ALL = 0x00003,
    }
    /// <summary>
    /// Sequence bit masks
    /// </summary>
    [Flags]
    public enum SequenceMask : uint
    {
        /// <summary>
        /// sequenceMode
        /// </summary>
        MODE = 0x00001,
        /// <summary>
        /// dwNumBarCodes and seqBarCodes[]
        /// </summary>
        BARCODES = 0x00002,
        /// <summary>
        /// Everything
        /// </summary>
        ALL = 0x00003,
    }
    /// <summary>
    /// Decoder bit masks
    /// </summary>
    [Flags]
    public enum DecoderMask : uint
    {
        /// <summary>
        /// Maximum length of decoded string. This item is Read Only.
        /// </summary>
        MAX_MESSAGE_LENGTH = 0x00001,
        /// <summary>
        /// Look for and report all barcodes in captured frame.
        /// </summary>
        DECODE_MULTIPLE = 0x00002,
        /// <summary>
        /// Use aimers when capturing barcodes.
        /// </summary>
        USE_AIMERS = 0x00004,
        /// <summary>
        /// Relative contrast between barcode and background (0-9).
        /// </summary>
        PRINT_WEIGHT = 0x00008,
        /// <summary>
        /// Normal, linear codes only.  Fastest (may miss codes at edges of image).
        /// </summary>
        DECODE_METHOD = 0x00010,
        /// <summary>
        /// Only accept barcodes whose boundaries intersect center window.
        /// </summary>
        CENTER_ENABLE = 0x00020,
        /// <summary>
        /// Rectangle about center of image for previous mask.
        /// </summary>
        CENTER_WINDOW = 0x00040,
        /// <summary>
        /// Illumination LED color to use.
        /// </summary>
        ILLUMINAT_LED_COLOR = 0x00080,
        /// <summary>
        /// 
        /// </summary>
        UPC_ADDENDA_DELAY = 0x00100,
        /// <summary>
        /// 
        /// </summary>
        COMPOSITE_DELAY = 0x00200,
        /// <summary>
        /// 
        /// </summary>
        CONCATENATE_DELAY = 0x00400,
        /// <summary>
        /// All structure members are active.
        /// </summary>
        ALL = 0x007ff,
    }
    /// <summary>
    /// Power setting bit masks
    /// </summary>
    [Flags]
    public enum PowerMask : uint
    {
        // Power setting item masks
        /// <summary>
        /// 
        /// </summary>
        TRIGGER_MODE = 0x00001,	//TRGMOD
        /// <summary>
        /// 
        /// </summary>
        SERIAL_TRIGGER_TIMEOUT = 0x00002,	//TRGSTO
        /// <summary>
        /// 
        /// </summary>
        STOP_MODE_TIMEOUT = 0x00004,   //TRGLPT
        /// <summary>
        /// 
        /// </summary>
        ILLUM_LED_INTENSITY = 0x00008,	//PWRLDC
        /// <summary>
        /// 
        /// </summary>
        SYS_SPEED = 0x00010,	//PWRSPD
        /// <summary>
        /// 
        /// </summary>
        AIMER_MODE = 0x00020,	//SCNAIM
        /// <summary>
        /// 
        /// </summary>
        AIMER_DURATION = 0x00040,	//SCNADR
        /// <summary>
        /// 
        /// </summary>
        AIMER_DELAY = 0x00080,	//SCNDLY
        /// <summary>
        /// 
        /// </summary>
        IMAGER_IDLE_TIMEOUT = 0x00100,	//SDRTIM
        /// <summary>
        /// 
        /// </summary>
        SLEEP_MODE_TIMEOUT = 0x00200,	//232LPT
        /// <summary>
        /// 
        /// </summary>
        POWER_OFF_HANDLE = 0x00400,
        /// <summary>
        /// 
        /// </summary>
        POWER_OFF_HWND = 0x00800,
        /// <summary>
        /// 
        /// </summary>
        AUTO_AIMER_TIMEOUT = 0x01000,   //SCNAAM
        /// <summary>
        /// All structure members are active.
        /// </summary>
        ALL = 0x01FFF,
    }
    /// <summary>
    /// Version information bit masks
    /// </summary>
    [Flags]
    public enum VersionMask : uint
    {
        // Versions setting item masks
        /// <summary>
        /// SDK version number.
        /// </summary>
        SDK_API = 0x00001,
        /// <summary>
        /// Imager firmware version.
        /// </summary>
        IMAGER_FIRMWARE = 0x00002,
        /// <summary>
        /// Imager part number.
        /// </summary>
        IMAGER_PART_NUM = 0x00004,
        /// <summary>
        /// Imager boot code version.
        /// </summary>
        IMAGER_BOOT_CODE = 0x00008,
        /// <summary>
        /// Device type of which imager is part.
        /// </summary>
        IMAGER_DEVICE_TYPE = 0x00010,
        /// <summary>
        /// Manufactures ID
        /// </summary>
        MANUFACTURERS_ID = 0x00020,
        /// <summary>
        /// Decoder Revision
        /// </summary>
        DECODER_REV = 0x00040,
        /// <summary>
        /// Scan driver revision
        /// </summary>
        SCAN_DRIVER_REV = 0x00080,
        /// <summary>
        /// All version info mask.
        /// </summary>
        ALL = 0x000ff,
    }
    /// <summary>
    /// Symbology bit masks
    /// </summary>
    [Flags]
    public enum SymbolMask : uint
    {
        /// <summary>
        /// Flags are valid.
        /// </summary>
        FLAGS = 0x00000001,
        /// <summary>
        /// Min Length valid.
        /// </summary>
        MIN_LEN = 0x00000002,
        /// <summary>
        /// Max Length valid.
        /// </summary>
        MAX_LEN = 0x00000004,
        /// <summary>
        /// OCR mode valid.
        /// </summary>
        OCR_MODE = 0x00000008,
        /// <summary>
        /// OCR direction valid.
        /// </summary>
        DIRECTION = 0x00000010,
        /// <summary>
        /// OCR template valid.
        /// </summary>
        TEMPLATE = 0x00000020,
        /// <summary>
        /// OCR group H valid.
        /// </summary>
        GROUP_H = 0x00000040,
        /// <summary>
        /// OCR group H valid.
        /// </summary>
        GROUP_G = 0x00000080,
        /// <summary>
        /// OCR check char valid.
        /// </summary>
        CHECK_CHAR = 0x00000100,
        /// <summary>
        /// Generic all mask.
        /// </summary>
        ALL = 0xffffffff,
    }

    #endregion

    #region Symbol flags
    /// <summary>
    /// Symbology flag
    /// </summary>
    public enum SymbolFlags
    {
        /// <summary>
        /// Enable Symbology bit
        /// </summary>
        ENABLE = 0x00000001,
        /// <summary>
        /// Enable usage of check character
        /// </summary>
        CHECK_ENABLE = 0x00000002,
        /// <summary>
        /// Send check character
        /// </summary>
        CHECK_TRANSMIT = 0x00000004,
        /// <summary>
        /// Include the start and stop characters in the decoded result string
        /// </summary>
        START_STOP_XMIT = 0x00000008,
        /// <summary>
        /// Code39 append mode
        /// </summary>
        ENABLE_APPEND_MODE = 0x00000010,
        /// <summary>
        /// Enable Code39 Full ASCII
        /// </summary>
        ENABLE_FULLASCII = 0x00000020,
        /// <summary>
        /// UPC-A/UPC-e Send Num Sys
        /// </summary>
        NUM_SYS_TRANSMIT = 0x00000040,
        /// <summary>
        /// Enable 2 digit Addenda (UPC and EAN)
        /// </summary>
        EN_2_DIGIT_ADDENDA = 0x00000080,
        /// <summary>
        /// Enable 5 digit Addenda (UPC and EAN)
        /// </summary>
        EN_5_DIGIT_ADDENDA = 0x00000100,
        /// <summary>
        /// Only allow codes with addenda (UPC and EAN)
        /// </summary>
        ADDENDA_REQUIRED = 0x00000200,
        /// <summary>
        /// Include Addenda separator space in returned string.
        /// </summary>
        ADDENDA_SEPARATOR = 0x00000400,
        /// <summary>
        /// Extended UPC-E
        /// </summary>
        EXPANDED_UPCE = 0x00000800,
        /// <summary>
        /// UPC-E1 enable (use SYMBOLOGY_ENABLE for UPC-E0)
        /// </summary>
        UPCE1_ENABLE = 0x00001000,
        /// <summary>
        /// Enable UPC composite codes
        /// </summary>
        COMPOSITE_UPC = 0x00002000,
        /// <summary>
        /// Enable Aztec Run
        /// </summary>
        AZTEC_RUNE = 0x00004000,
        /// <summary>
        /// Include australian postal bar data in string
        /// </summary>
        AUSTRALIAN_BAR_WIDTH = 0x00010000,
        /// <summary>
        /// Customer fields as numeric data using the N Table.
        /// </summary>
        AUS_CUST_FIELD_NUM = 0x00020000,
        /// <summary>
        /// Customer fields as alphanumeric data using the C Table.
        /// </summary>
        AUS_CUST_FIELD_AlPHA = 0x00040000,
        // We reuse flags for MESA since MESA has no addenda, Extended UPC-E or UPC-E1 enable flags
        /// <summary>
        /// Mesa IMS enable
        /// </summary>
        ENABLE_MESA_IMS = 0x00020000,
        /// <summary>
        /// Mesa 1MS enable
        /// </summary>
        ENABLE_MESA_1MS = 0x00040000,
        /// <summary>
        /// Mesa 3MS enable
        /// </summary>
        ENABLE_MESA_3MS = 0x00080000,
        /// <summary>
        /// Mesa 9MS enable
        /// </summary>
        ENABLE_MESA_9MS = 0x00100000,
        /// <summary>
        /// Mesa UMS enable
        /// </summary>
        ENABLE_MESA_UMS = 0x00200000,
        /// <summary>
        /// Mesa EMS enable
        /// </summary>
        ENABLE_MESA_EMS = 0x00400000,
        /// <summary>
        /// Mesa EMS Mask
        /// </summary>
        ENABLE_MESA_MASK = 0x007E0000,
        // For RSE,RSL,RSS there is only one symbology ID so we use 3 flags for enable
        /// <summary>
        /// Enable RSE Symbology bit
        /// </summary>
        RSE_ENABLE = 0x00800000,
        /// <summary>
        /// Enable RSL Symbology bit
        /// </summary>
        RSL_ENABLE = 0x01000000,
        /// <summary>
        /// Enable RSS Symbology bit
        /// </summary>
        RSS_ENABLE = 0x02000000,
        /// <summary>
        /// Enable RSS Mask
        /// </summary>
        RSX_ENABLE_MASK = 0x03800000,
        // Telepen and PosiCode
        /// <summary>
        /// Telepen Old Style mode.
        /// </summary>
        TELEPEN_OLD_STYLE = 0x04000000,
        /// <summary>
        /// PosiCode Limited of 1
        /// </summary>
        POSICODE_LIMITED_1 = 0x08000000,
        /// <summary>
        /// PosiCode Limited of 2
        /// </summary>
        POSICODE_LIMITED_2 = 0x10000000,
        /// <summary>
        /// Codabar concatenate.
        /// </summary>
        CODABAR_CONCATENATE = 0x20000000,
        /*// For OCR we reuse flags since none of the other flags apply to OCR
        /// <summary>
        /// OCR-A enable.
        /// </summary>
        ENABLE_OCR_A = 0x00000001,
        /// <summary>
        /// OCR-B enable.
        /// </summary>
        ENABLE_OCR_B = 0x00000002,
        /// <summary>
        /// OCR-Money enable.
        /// </summary>
        ENABLE_OCR_MONEY = 0x00000004,
        /// <summary>
        /// OCR-Micr enable.
        /// </summary>
        ENABLE_OCR_MICR = 0x00000008,*/
    }
    #endregion

    #region Mis
    /// <summary>
    /// Windows message IDs
    /// </summary>
    public enum WM_ID
    {
        /// <summary>
        /// User Define Message Starting Point
        /// </summary>
        WM_USER = 0x0400,
        /// <summary>
        /// Barcode Event
        /// </summary>
        WM_SDK_EVENT_HWND_MSG = (WM_USER + 14534),
        /// <summary>
        /// Image progress event
        /// </summary>
        WM_SDK_PROGRESS_HWND_MSG = (WM_USER + 14535),
        /// <summary>
        /// Imager firmware flashing event
        /// </summary>
        WM_SDK_IMAGER_FLASHING = (WM_USER + 14536),
        /// <summary>
        /// Barcode Power event
        /// </summary>
        WM_SDK_POWER_EVENT = (WM_USER + 14537),
        /// <summary>
        /// Barcode Sequential read event
        /// </summary>
        WM_SDK_SEQ_BARCODE_READ = (WM_USER + 14538),
    }
    #endregion

    /// <summary>
    /// Constant value
    /// </summary>
    class Constants
    {
        /// <summary>
        /// DWORD -1 (bad value)
        /// </summary>
        public static readonly uint BAD_DWORD_VALUE = uint.MaxValue;
        /// <summary>
        /// Universal Mask
        /// </summary>
        public static readonly uint ALL_MASK = 0xffffffff;

        public static readonly uint MIN_COMPRESSION_FACTOR = 0;
        public static readonly uint MAX_COMPRESSION_FACTOR = 100;
        public static readonly uint MIN_EDGE_ENHANCEMENT = 0;
        public static readonly uint MAX_EDGE_ENHANCEMENT = 23;
        public static readonly uint MIN_GAMMA_CORRECTION = 0;
        public static readonly uint MAX_GAMMA_CORRECTION = 1000;
        public static readonly uint MIN_TEXT_ENHANCEMENT = 0;
        public static readonly uint MAX_TEXT_ENHANCEMENT = 255;

        /// <summary>
        /// Data structure that holds the decoded barcode message.
        /// </summary>
        public static readonly uint MAX_MESAGE_LENGTH = 4096;

        /// <summary>
        /// Trigger Timeout Range-Infinite
        /// </summary>
        public static readonly uint MIN_TRIGGER_TIMEOUT_VAL = 0;
        /// <summary>
        /// Trigger Timeout Range-5 Minutes.
        /// </summary>
        public static readonly uint MAX_TRIGGER_TIMEOUT_VAL = 300000; 


        public static readonly uint MAX_SEQ_BARCODES = 12;
        public static readonly uint MAX_NUM_START_CHARS = 32;
        public static readonly uint SEQ_ALL_LENGTH = 9999;


        // Min/Max for delays
        public static readonly uint MIN_CODE_DELAY = 10;
        public static readonly uint MAX_CONCAT_DELAY = 1000;
        public static readonly uint MAX_COMPOSITE_DELAY = 1000;
        public static readonly uint MAX_ADDENDA_DELAY = 500;
        
        // Revision information
        public static readonly uint MAX_VERSION_STRING_LEN = 128;
        public static readonly uint MAX_DEVICE_TYPE_STRING_LEN = 64;
        public static readonly uint MAX_MANUFACTURERS_ID_LEN = 16;

        /// Defines for Engine Information Structure
        public static readonly uint MAX_SHORT_VERSION_LEN = 32;
        public static readonly uint MAX_SERIAL_NUMBER_LEN = 16;
        public static readonly uint MAX_CHECKSUM_LEN = 8;
        public static readonly uint ENGINE_ID_DIGITS = 4;
    }

    /*public class Symbol
    {
        public static readonly SYMBOL SYM_CODE25 = SYMBOL.STRT25;
        //-----------------------------------------------------------------------------
        //  AIM Symbology ID characters
        //-----------------------------------------------------------------------------
        public static readonly char AIMID_AZTEC = 'z';
        public static readonly char AIMID_MESA = 'z';
        public static readonly char AIMID_CODABAR = 'F';
        public static readonly char AIMID_CODE11 = 'H';
        public static readonly char AIMID_CODE128 = 'C';
        public static readonly char AIMID_CODE39 = 'A';
        public static readonly char AIMID_CODE49 = 'T';
        public static readonly char AIMID_CODE93 = 'G';
        public static readonly char AIMID_COMPOSITE = 'e';
        public static readonly char AIMID_DATAMATRIX = 'd';
        public static readonly char AIMID_EAN = 'E';
        public static readonly char AIMID_INT25 = 'I';
        public static readonly char AIMID_MAXICODE = 'U';
        public static readonly char AIMID_MICROPDF = 'L';
        public static readonly char AIMID_PDF417 = 'L';
        public static readonly char AIMID_OCR = 'o';
        public static readonly char AIMID_QR = 'Q';
        public static readonly char AIMID_RSS = 'e';
        public static readonly char AIMID_UPC = 'E';
        public static readonly char AIMID_POSTNET = 'X';
        public static readonly char AIMID_ISBT = 'C';
        public static readonly char AIMID_BPO = 'X';
        public static readonly char AIMID_CANPOST = 'X';
        public static readonly char AIMID_AUSPOST = 'X';
        public static readonly char AIMID_IATA25 = 'R';
        public static readonly char AIMID_CODABLOCK = 'O';
        public static readonly char AIMID_JAPOST = 'X';
        public static readonly char AIMID_PLANET = 'X';
        public static readonly char AIMID_DUTCHPOST = 'X';
        public static readonly char AIMID_MSI = 'M';
        public static readonly char AIMID_TLC39 = 'L';
        public static readonly char AIMID_MATRIX25 = 'X';
        public static readonly char AIMID_KORPOST = 'X';
        public static readonly char AIMID_TRIOPTIC = 'X';
        public static readonly char AIMID_CODE32 = 'X';
        public static readonly char AIMID_STRT25 = 'S';
        public static readonly char AIMID_PLESSEY = 'P';
        public static readonly char AIMID_CHINAPOST = 'X';
        public static readonly char AIMID_TELEPEN = 'B';
        public static readonly char AIMID_CODE16K = 'K';
        public static readonly char AIMID_POSICODE = 'p';
        public static readonly char AIMID_COUPONCODE = 'E';
        public static readonly char AIMID_UPUIDTAG = 'X';
        public static readonly char AIMID_CODE4CB = 'X';
        //-----------------------------------------------------------------------------
        //  HHP Symbology ID characters
        //-----------------------------------------------------------------------------
        public static readonly char SYMID_AZTEC = 'z';
        public static readonly char SYMID_MESA = 'z';
        public static readonly char SYMID_CODABAR = 'a';
        public static readonly char SYMID_CODE11 = 'h';
        public static readonly char SYMID_CODE128 = 'j';
        public static readonly char SYMID_CODE39 = 'b';
        public static readonly char SYMID_CODE49 = 'l';
        public static readonly char SYMID_CODE93 = 'i';
        public static readonly char SYMID_COMPOSITE = 'y';
        public static readonly char SYMID_DATAMATRIX = 'w';
        public static readonly char SYMID_EAN = 'd';
        public static readonly char SYMID_INT25 = 'e';
        public static readonly char SYMID_MAXICODE = 'x';
        public static readonly char SYMID_MICROPDF = 'R';
        public static readonly char SYMID_PDF417 = 'r';
        public static readonly char SYMID_POSTNET = 'P';
        public static readonly char SYMID_OCR = 'O';
        public static readonly char SYMID_QR = 's';
        public static readonly char SYMID_RSS = 'y';
        public static readonly char SYMID_UPC = 'c';
        public static readonly char SYMID_ISBT = 'j';
        public static readonly char SYMID_BPO = 'B';
        public static readonly char SYMID_CANPOST = 'C';
        public static readonly char SYMID_AUSPOST = 'A';
        public static readonly char SYMID_IATA25 = 'f';
        public static readonly char SYMID_CODABLOCK = 'q';
        public static readonly char SYMID_JAPOST = 'J';
        public static readonly char SYMID_PLANET = 'L';
        public static readonly char SYMID_DUTCHPOST = 'K';
        public static readonly char SYMID_MSI = 'g';
        public static readonly char SYMID_TLC39 = 'T';
        public static readonly char SYMID_MATRIX25 = 'm';
        public static readonly char SYMID_KORPOST = '?';
        public static readonly char SYMID_TRIOPTIC = '=';
        public static readonly char SYMID_CODE32 = '<';
        public static readonly char SYMID_STRT25 = 'f';
        public static readonly char SYMID_PLESSEY = 'n';
        public static readonly char SYMID_CHINAPOST = 'Q';
        public static readonly char SYMID_TELEPEN = 't';
        public static readonly char SYMID_CODE16K = 'o';
        public static readonly char SYMID_POSICODE = 'W';
        public static readonly char SYMID_COUPONCODE = 'c';
        public static readonly char SYMID_UPUIDTAG = 'N';
        public static readonly char SYMID_CODE4CB = 'M';
        
        // Decoder configuration definitions for each symbology

        // Structure for oddball OCR
        public static readonly int MAX_TEMPLATE_LEN = 256;
        public static readonly int MAX_GROUP_H_LEN = 256;
        public static readonly int MAX_GROUP_G_LEN = 256;
        public static readonly int MAX_CHECK_CHAR_LEN = 64;
    }*/


    /// <summary>
    /// AIM Symbology ID characters
    /// </summary>
    public enum CodeID : ushort
    {
        CODE128,
        GS1_128,
        AIM128,
        EAN,
        EAN_13,
        UPC_E,
        UPC_A,
        INT25,
        ITF_6,
        ITF_14,
        MATRIX25,
        CODE39,
        CODABAR,
        CODE93,
        CODE11,
        GDS1Databar,
        COMPOSITE,
        ISBN,
        ISSN,
        EUROMATRIX2,
        Industrial25,
        Standard25,
        PLESSEY,
        MSIPLESSEY,
        PDF417,
        QR,
        AZTEC,
        DATAMATRIX,
        MAXICODE,
        ChineseSensibleCode
    }


}
