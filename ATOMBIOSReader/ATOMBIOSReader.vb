Public Class ATOMBIOSReader
    'Define offset To location Of ROM header. 

    Dim OFFSET_TO_POINTER_TO_ATOM_ROM_HEADER As Integer = 72 '0x00000048L
    Dim OFFSET_TO_ATOM_ROM_IMAGE_SIZE As Integer = 2 '0x00000002L
    Dim INDEX_COMMAND_TABLE() As String = New String() {
        "ASIC_Init", "GetDisplaySurfaceSize", "ASIC_RegistersInit",
        "VRAM_BlockVenderDetection", "SetClocksRatio/DIGxEncoderControl", "MemoryControllerInit",
        "EnableCRTCMemReq", "MemoryParamAdjust", "DVOEncoderControl",
        "GPIOPinControl", "SetEngineClock", "SetMemoryClock",
        "SetPixelClock", "DynamicClockGating", "ResetMemoryDLL",
        "ResetMemoryDevice", "MemoryPLLInit", "AdjustDisplayPll",
        "AdjustMemoryController", "EnableASIC_StaticPwrMgt", "ASIC_StaticPwrMgtStatusChange/SetUniphyInstance",
        "DAC_LoadDetection", "LVTMAEncoderControl", "LCD1OutputControl",
        "DAC1EncoderControl", "DAC2EncoderControl", "DVOOutputControl",
        "CV1OutputControl", "GetConditionalGoldenSetting/SetCRTC_DPM_State", "TVEncoderControl",
        "TMDSAEncoderControl", "LVDSEncoderControl", "TV1OutputControl",
        "EnableScaler", "BlankCRTC", "EnableCRTC",
        "GetPixelClock", "EnableVGA_Render", "EnableVGA_Access/GetSCLKOverMCLKRatio",
        "SetCRTC_Timing", "SetCRTC_OverScan", "SetCRTC_Replication",
        "SelectCRTC_Source", "EnableGraphSurfaces", "UpdateCRTC_DoubleBufferRegisters",
        "LUT_AutoFill", "EnableHW_IconCursor", "GetMemoryClock",
        "GetEngineClock", "SetCRTC_UsingDTDTiming", "ExternalEncoderControl",
        "LVTMAOutputControl", "VRAM_BlockDetectionByStrap", "MemoryCleanUp",
        "ReadEDIDFromHWAssistedI2C/ProcessI2cChannelTransaction",
        "WriteOneByteToHWAssistedI2C", "ReadHWAssistedI2CStatus/HPDInterruptService",
        "SpeedFanControl", "PowerConnectorDetection", "MC_Synchronization",
        "ComputeMemoryEnginePLL", "MemoryRefreshConversion", "VRAM_GetCurrentInfoBlock",
        "DynamicMemorySettings", "MemoryTraining", "EnableSpreadSpectrumOnPPLL",
        "TMDSAOutputControl", "SetVoltage", "DAC1OutputControl",
        "DAC2OutputControl", "SetupHWAssistedI2CStatus", "ClockSource",
        "MemoryDeviceInit", "EnableYUV", "DIG1EncoderControl",
        "DIG2EncoderControl", "DIG1TransmitterControl/UNIPHYTransmitterControl",
        "DIG2TransmitterControl/LVTMATransmitterControl",
        "ProcessAuxChannelTransaction", "DPEncoderService"}
    Dim INDEX_DATA_TABLE() As String = New String() {
        "UtilityPipeLine", "MultimediaCapabilityInfo", "MultimediaConfigInfo",
        "StandardVESA_Timing", "FirmwareInfo", "DAC_Info",
        "LVDS_Info", "TMDS_Info", "AnalogTV_Info",
        "SupportedDevicesInfo", "GPIO_I2C_Info", "VRAM_UsageByFirmware",
        "GPIO_Pin_LUT", "VESA_ToInternalModeLUT", "ComponentVideoInfo",
        "PowerPlayInfo", "CompassionateData", "SaveRestoreInfo/DispDevicePriorityInfo",
        "PPLL_SS_Info/SS_Info", "OemInfo", "XTMDS_Info",
        "MclkSS_Info", "Object_Info/Object_Header", "IndirectIOAccess",
        "MC_InitParameter/AdjustARB_SEQ", "ASIC_VDDC_Info", "ASIC_InternalSS_Info/ASIC_MVDDC_Info",
        "TV_VideoMode/DispOutInfo", "VRAM_Info", "MemoryTrainingInfo/ASIC_MVDDQ_Info",
        "IntegratedSystemInfo", "ASIC_ProfilingInfo/ASIC_VDDCI_Info",
        "VoltageObjectInfo/VRAM_GPIO_DetectionInfo",
        "PowerSourceInfo"}

    ' Common header for all ROM Data tables.
    'Every table pointed  _ATOM_MASTER_DATA_TABLE has this common header. 
    'And the pointer actually points to this header. 

    Public Structure _ATOM_COMMON_TABLE_HEADER
        Dim usStructureSize As UShort
        Dim ucTableFormatRevision As Byte  'Change it When the Parser Is Not backward compatible 
        Dim ucTableContentRevision As Byte  'Change it only When the table needs To change but the firmware 
        'Image can't be updated, while Driver needs to carry the new table! 

        'Return usStructureSize & ucTableFormatRevision & ucTableContentRevision
    End Structure '} ATOM_COMMON_TABLE_HEADER


    '/****************************************************************************/	
    '// Structure stores the ROM header.
    '/****************************************************************************/	
    Public Structure _ATOM_ROM_HEADER
        Dim sHeader As _ATOM_COMMON_TABLE_HEADER
        Dim uaFirmWareSignature() As Byte '/*Signature To distinguish between Atombios And non-atombios, 
        'atombios should init it As "ATOM", don't change the position */
        Dim usBiosRuntimeSegmentAddress As UShort
        Dim usProtectedModeInfoOffset As UShort
        Dim usConfigFilenameOffset As UShort
        Dim usCRC_BlockOffset As UShort
        Dim usBIOS_BootupMessageOffset As UShort
        Dim usInt10Offset As UShort
        Dim usPciBusDevInitCode As UShort
        Dim usIoBaseAddress As UShort
        Dim usSubsystemVendorID As UShort
        Dim usSubsystemID As UShort
        Dim usPCI_InfoOffset As UShort
        Dim usMasterCommandTableOffset As UShort '/*Offset For SW To Get all command table offsets, Don't change the position */
        Dim usMasterDataTableOffset As UShort   '/*Offset For SW To Get all data table offsets, Don't change the position */
        Dim ucExtendedFunctionCode As Byte
        Dim ucReserved As Byte

        'Return ATOM_COMMON_TABLE_HEADER & uaFirmWareSignature(4) & usBiosRuntimeSegmentAddress & usProtectedModeInfoOffset & usConfigFilenameOffset &
        'usCRC_BlockOffset & usBIOS_BootupMessageOffset & usInt10Offset & usPciBusDevInitCode & usIoBaseAddress & usSubsystemVendorID & usSubsystemID &
        'usPCI_InfoOffset & usMasterCommandTableOffset & usMasterDataTableOffset & ucExtendedFunctionCode & ucReserved
    End Structure '}ATOM_ROM_HEADER;

    '/*==============================Command Table Portion==================================== */

    '/****************************************************************************/	
    '// Structures used in Command.mtb 
    '/****************************************************************************/	
    Public Structure _ATOM_MASTER_LIST_OF_COMMAND_TABLES
        Dim ASIC_Init As UShort                              '//Function Table, used by various SW components, latest version 1.1
        Dim GetDisplaySurfaceSize As UShort                  '//Atomic Table, Used by Bios when enabling HW ICON
        Dim ASIC_RegistersInit As UShort                     '//Atomic Table, indirectly used by various SW components, called from ASIC_Init
        Dim VRAM_BlockVenderDetection As UShort              '//Atomic Table, used only by Bios
        Dim DIGxEncoderControl As UShort                                         '//Only used by Bios
        Dim MemoryControllerInit As UShort                   '//Atomic Table, indirectly used by various SW components, called from ASIC_Init
        Dim EnableCRTCMemReq As UShort                       '//Function Table, directly used by various SW components, latest version 2.1
        Dim MemoryParamAdjust As UShort                                          '//Atomic Table, indirectly used by various SW components, called from SetMemoryClock if needed
        Dim DVOEncoderControl As UShort                      '//Function Table, directly used by various SW components, latest version 1.2
        Dim GPIOPinControl As UShort                                                 '//Atomic Table, only used by Bios
        Dim SetEngineClock As UShort                         '//Function Table, directly used by various SW components, latest version 1.1
        Dim SetMemoryClock As UShort                         '//Function Table, directly used by various SW components, latest version 1.1
        Dim SetPixelClock As UShort                          '//Function Table, directly used by various SW components, latest version 1.2  
        Dim EnableDispPowerGating As UShort                  '//Atomic Table, indirectly used by various SW components, called from ASIC_Init
        Dim ResetMemoryDLL As UShort                         '//Atomic Table, indirectly used by various SW components, called from SetMemoryClock
        Dim ResetMemoryDevice As UShort                      '//Atomic Table, indirectly used by various SW components, called from SetMemoryClock
        Dim MemoryPLLInit As UShort                          '//Atomic Table, used only by Bios
        Dim AdjustDisplayPll As UShort                                           '//Atomic Table, used by various SW componentes. 
        Dim AdjustMemoryController As UShort                 '//Atomic Table, indirectly used by various SW components, called from SetMemoryClock                
        Dim EnableASIC_StaticPwrMgt As UShort                '//Atomic Table, only used by Bios
        Dim SetUniphyInstance As UShort                      '//Atomic Table, only used by Bios   
        Dim DAC_LoadDetection As UShort                      '//Atomic Table, directly used by various SW components, latest version 1.2  
        Dim LVTMAEncoderControl As UShort                    '//Atomic Table, directly used by various SW components, latest version 1.3
        Dim HW_Misc_Operation As UShort                      '//Atomic Table, directly used by various SW components, latest version 1.1 
        Dim DAC1EncoderControl As UShort                     '//Atomic Table, directly used by various SW components, latest version 1.1  
        Dim DAC2EncoderControl As UShort                     '//Atomic Table, directly used by various SW components, latest version 1.1 
        Dim DVOOutputControl As UShort                       '//Atomic Table, directly used by various SW components, latest version 1.1 
        Dim CV1OutputControl As UShort                       '//Atomic Table, Atomic Table, Obsolete from Ry6xx, use DAC2 Output instead 
        Dim GetConditionalGoldenSetting As UShort            '//Only used by Bios
        Dim TVEncoderControl As UShort                       '//Function Table, directly used by various SW components, latest version 1.1
        Dim PatchMCSetting As UShort                         '//only used by BIOS
        Dim MC_SEQ_Control As UShort                         '//only used by BIOS
        Dim Gfx_Harvesting As UShort                         '//Atomic Table, Obsolete from Ry6xx, Now only used by BIOS for GFX harvesting
        Dim EnableScaler As UShort                           '//Atomic Table, used only by Bios
        Dim BlankCRTC As UShort                              '//Atomic Table, directly used by various SW components, latest version 1.1 
        Dim EnableCRTC As UShort                             '//Atomic Table, directly used by various SW components, latest version 1.1 
        Dim GetPixelClock As UShort                          '//Atomic Table, directly used by various SW components, latest version 1.1 
        Dim EnableVGA_Render As UShort                       '//Function Table, directly used by various SW components, latest version 1.1
        Dim GetSCLKOverMCLKRatio As UShort                   '//Atomic Table, only used by Bios
        Dim SetCRTC_Timing As UShort                         '//Atomic Table, directly used by various SW components, latest version 1.1
        Dim SetCRTC_OverScan As UShort                       '//Atomic Table, used by various SW components, latest version 1.1 
        Dim SetCRTC_Replication As UShort                    '//Atomic Table, used only by Bios
        Dim SelectCRTC_Source As UShort                      '//Atomic Table, directly used by various SW components, latest version 1.1 
        Dim EnableGraphSurfaces As UShort                    '//Atomic Table, used only by Bios
        Dim UpdateCRTC_DoubleBufferRegisters As UShort            '//Atomic Table, used only by Bios
        Dim LUT_AutoFill As UShort                           '//Atomic Table, only used by Bios
        Dim EnableHW_IconCursor As UShort                    '//Atomic Table, only used by Bios
        Dim GetMemoryClock As UShort                         '//Atomic Table, directly used by various SW components, latest version 1.1 
        Dim GetEngineClock As UShort                         '//Atomic Table, directly used by various SW components, latest version 1.1 
        Dim SetCRTC_UsingDTDTiming As UShort                 '//Atomic Table, directly used by various SW components, latest version 1.1
        Dim ExternalEncoderControl As UShort                 '//Atomic Table, directly used by various SW components, latest version 2.1
        Dim LVTMAOutputControl As UShort                     '//Atomic Table, directly used by various SW components, latest version 1.1
        Dim VRAM_BlockDetectionByStrap As UShort             '//Atomic Table, used only by Bios
        Dim MemoryCleanUp As UShort                          '//Atomic Table, only used by Bios    
        Dim ProcessI2cChannelTransaction As UShort           '//Function Table, only used by Bios
        Dim WriteOneByteToHWAssistedI2C As UShort            '//Function Table, indirectly used by various SW components 
        Dim ReadHWAssistedI2CStatus As UShort                '//Atomic Table, indirectly used by various SW components
        Dim SpeedFanControl As UShort                        '//Function Table, indirectly used by various SW components, called from ASIC_Init
        Dim PowerConnectorDetection As UShort                '//Atomic Table, directly used by various SW components, latest version 1.1
        Dim MC_Synchronization As UShort                     '//Atomic Table, indirectly used by various SW components, called from SetMemoryClock
        Dim ComputeMemoryEnginePLL As UShort                 '//Atomic Table, indirectly used by various SW components, called from SetMemory/EngineClock
        Dim MemoryRefreshConversion As UShort                '//Atomic Table, indirectly used by various SW components, called from SetMemory Or SetEngineClock
        Dim VRAM_GetCurrentInfoBlock As UShort               '//Atomic Table, used only by Bios
        Dim DynamicMemorySettings As UShort                  '//Atomic Table, indirectly used by various SW components, called from SetMemoryClock
        Dim MemoryTraining As UShort                         '//Atomic Table, used only by Bios
        Dim EnableSpreadSpectrumOnPPLL As UShort             '//Atomic Table, directly used by various SW components, latest version 1.2
        Dim TMDSAOutputControl As UShort                     '//Atomic Table, directly used by various SW components, latest version 1.1
        Dim SetVoltage As UShort                             '//Function Table, directly And/Or indirectly used by various SW components, latest version 1.1
        Dim DAC1OutputControl As UShort                      '//Atomic Table, directly used by various SW components, latest version 1.1
        Dim DAC2OutputControl As UShort                      '//Atomic Table, directly used by various SW components, latest version 1.1
        Dim ComputeMemoryClockParam As UShort                '//Function Table, only used by Bios, obsolete soon.Switch to use "ReadEDIDFromHWAssistedI2C"
        Dim ClockSource As UShort                            '//Atomic Table, indirectly used by various SW components, called from ASIC_Init
        Dim MemoryDeviceInit As UShort                       '//Atomic Table, indirectly used by various SW components, called from SetMemoryClock
        Dim GetDispObjectInfo As UShort                      '//Atomic Table, indirectly used by various SW components, called from EnableVGARender
        Dim DIG1EncoderControl As UShort                     '//Atomic Table, directly used by various SW components, latest version 1.1
        Dim DIG2EncoderControl As UShort                     '//Atomic Table, directly used by various SW components, latest version 1.1
        Dim DIG1TransmitterControl As UShort                 '//Atomic Table, directly used by various SW components, latest version 1.1
        Dim DIG2TransmitterControl As UShort                '//Atomic Table, directly used by various SW components, latest version 1.1 
        Dim ProcessAuxChannelTransaction As UShort                    '//Function Table, only used by Bios
        Dim DPEncoderService As UShort                                            '//Function Table, only used by Bios
        Dim GetVoltageInfo As UShort                         '//Function Table, only used by Bios since SI
    End Structure '}ATOM_MASTER_LIST_OF_COMMAND_TABLES;   

    '    // For backward compatible 
    Dim ATOM_MASTER_LIST_OF_COMMAND_TABLES As _ATOM_MASTER_LIST_OF_COMMAND_TABLES
    Dim ReadEDIDFromHWAssistedI2C = ATOM_MASTER_LIST_OF_COMMAND_TABLES.ProcessI2cChannelTransaction
    Dim DPTranslatorControl = ATOM_MASTER_LIST_OF_COMMAND_TABLES.DIG2EncoderControl
    Dim UNIPHYTransmitterControl = ATOM_MASTER_LIST_OF_COMMAND_TABLES.DIG1TransmitterControl
    Dim LVTMATransmitterControl = ATOM_MASTER_LIST_OF_COMMAND_TABLES.DIG2TransmitterControl
    Dim SetCRTC_DPM_State = ATOM_MASTER_LIST_OF_COMMAND_TABLES.GetConditionalGoldenSetting
    Dim ASIC_StaticPwrMgtStatusChange = ATOM_MASTER_LIST_OF_COMMAND_TABLES.SetUniphyInstance
    Dim HPDInterruptService = ATOM_MASTER_LIST_OF_COMMAND_TABLES.ReadHWAssistedI2CStatus
    Dim EnableVGA_Access = ATOM_MASTER_LIST_OF_COMMAND_TABLES.GetSCLKOverMCLKRatio
    Dim EnableYUV = ATOM_MASTER_LIST_OF_COMMAND_TABLES.GetDispObjectInfo
    Dim DynamicClockGating = ATOM_MASTER_LIST_OF_COMMAND_TABLES.EnableDispPowerGating
    Dim SetupHWAssistedI2CStatus = ATOM_MASTER_LIST_OF_COMMAND_TABLES.ComputeMemoryClockParam

    Dim TMDSAEncoderControl = ATOM_MASTER_LIST_OF_COMMAND_TABLES.PatchMCSetting
    Dim LVDSEncoderControl = ATOM_MASTER_LIST_OF_COMMAND_TABLES.MC_SEQ_Control
    Dim LCD1OutputControl = ATOM_MASTER_LIST_OF_COMMAND_TABLES.HW_Misc_Operation
    Dim TV1OutputControl = ATOM_MASTER_LIST_OF_COMMAND_TABLES.Gfx_Harvesting

    Public Structure _ATOM_MASTER_COMMAND_TABLE
        Dim sHeader As _ATOM_COMMON_TABLE_HEADER
        Dim ListOfCommandTables As _ATOM_MASTER_LIST_OF_COMMAND_TABLES
    End Structure '}ATOM_MASTER_COMMAND_TABLE;

    '//==============================Data Table Portion====================================

    '/****************************************************************************/	
    '// Structure used in Data.mtb
    '/****************************************************************************/	
    Public Structure _ATOM_MASTER_LIST_OF_DATA_TABLES
        Dim UtilityPipeLine As UShort           '// Offest For the utility To Get parser info, Don't change this position!
        Dim MultimediaCapabilityInfo As UShort '// Only used by MM Lib, latest version 1.1, Not configuable from Bios, need to include the table to build Bios 
        Dim MultimediaConfigInfo As UShort     '// Only used by MM Lib, latest version 2.1, Not configuable from Bios, need to include the table to build Bios
        Dim StandardVESA_Timing As UShort      '// Only used by Bios
        Dim FirmwareInfo As UShort             '// Shared by various SW components, latest version 1.4
        Dim PaletteData As UShort              '// Only used by BIOS
        Dim LCD_Info As UShort                 '// Shared by various SW components, latest version 1.3, was called LVDS_Info 
        Dim DIGTransmitterInfo As UShort       '// Internal used by VBIOS only version 3.1
        Dim AnalogTV_Info As UShort            '// Shared by various SW components, latest version 1.1 
        Dim SupportedDevicesInfo As UShort     '// Will be obsolete from R600
        Dim GPIO_I2C_Info As UShort            '// Shared by various SW components, latest version 1.2 will be used from R600           
        Dim VRAM_UsageByFirmware As UShort     '// Shared by various SW components, latest version 1.3 will be used from R600
        Dim GPIO_Pin_LUT As UShort             '// Shared by various SW components, latest version 1.1
        Dim VESA_ToInternalModeLUT As UShort   '// Only used by Bios
        Dim ComponentVideoInfo As UShort       '// Shared by various SW components, latest version 2.1 will be used from R600
        Dim PowerPlayInfo As UShort            '// Shared by various SW components, latest version 2.1, New Design from R600
        Dim CompassionateData As UShort        '// Will be obsolete from R600
        Dim SaveRestoreInfo As UShort          '// Only used by Bios
        Dim PPLL_SS_Info As UShort             '// Shared by various SW components, latest version 1.2, used To Call SS_Info, change to New name because of int ASIC SS info
        Dim OemInfo As UShort                  '// Defined And used by external SW, should be obsolete soon
        Dim XTMDS_Info As UShort               '// Will be obsolete from R600
        Dim MclkSS_Info As UShort              '// Shared by various SW components, latest version 1.1, only enabled when ext SS chip Is used
        Dim Object_Header As UShort            '// Shared by various SW components, latest version 1.1
        Dim IndirectIOAccess As UShort         '// Only used by Bios, this table position can't change at all!!
        Dim MC_InitParameter As UShort         '// Only used by command table
        Dim ASIC_VDDC_Info As UShort                        '// Will be obsolete from R600
        Dim ASIC_InternalSS_Info As UShort          '// New tabel name from R600, used to be called "ASIC_MVDDC_Info"
        Dim TV_VideoMode As UShort                          '// Only used by command table
        Dim VRAM_Info As UShort                             '// Only used by command table, latest version 1.3
        Dim MemoryTrainingInfo As UShort                '// Used for VBIOS And Diag utility for memory training purpose since R600. the New table rev start from 2.1
        Dim IntegratedSystemInfo As UShort          '// Shared by various SW components
        Dim ASIC_ProfilingInfo As UShort                '// New table name from R600, used to be called "ASIC_VDDCI_Info" for pre-R600
        Dim VoltageObjectInfo As UShort             '// Shared by various SW components, latest version 1.1
        Dim PowerSourceInfo As UShort                   '// Shared by various SW components, latest versoin 1.1
    End Structure '}ATOM_MASTER_LIST_OF_DATA_TABLES;

    '// For backward compatible 
    Dim ATOM_MASTER_LIST_OF_DATA_TABLES As _ATOM_MASTER_LIST_OF_DATA_TABLES
    Dim LVDS_Info = ATOM_MASTER_LIST_OF_DATA_TABLES.LCD_Info
    Dim DAC_Info = ATOM_MASTER_LIST_OF_DATA_TABLES.PaletteData
    Dim TMDS_Info = ATOM_MASTER_LIST_OF_DATA_TABLES.DIGTransmitterInfo

    Public Structure _ATOM_MASTER_DATA_TABLE
        Dim sHeader As _ATOM_COMMON_TABLE_HEADER
        Dim ListOfDataTables As _ATOM_MASTER_LIST_OF_DATA_TABLES
    End Structure '}ATOM_MASTER_DATA_TABLE;

    Public Function ReadingBytes(ByVal Offset As UShort, ByVal NoOfBytes As Integer, ByRef LittleEndian As UShort) As UShort
        Dim ENCODE As New System.Text.UnicodeEncoding(True, True, False)
        Dim HexValue() As Byte
        'Dim ATOM_ROM_HEADER As _ATOM_ROM_HEADER
        'ReDim ATOM_ROM_HEADER.uaFirmWareSignature(4)
        'Dim ATOM_MASTER_COMMAND_TABLES As _ATOM_MASTER_COMMAND_TABLE
        'Dim ATOM_MASTER_DATA_TABLES As _ATOM_MASTER_DATA_TABLE

        Using FS As New IO.FileStream(txtFileName.Text, IO.FileMode.Open, IO.FileAccess.Read),
            BR As New IO.BinaryReader(FS, ENCODE)
            FS.Position = Offset
            If NoOfBytes = 2 Then
                Dim startIdx As Integer = 0
                HexValue = BR.ReadBytes(NoOfBytes)
                LittleEndian = BitConverter.ToUInt16(HexValue, startIdx)
            ElseIf NoOfBytes = 1 Then
                LittleEndian = FS.ReadByte
            End If

            Return LittleEndian
        End Using

        ''debug
        'Debug.WriteLine("LittleEndian = " & LittleEndian & " (0x" & Hex(LittleEndian) & ")")
    End Function

    Public Function WritingReport(ByVal i As Integer, ByVal Offset As UShort, ByVal Tbls As String) As String
        Dim REPO As System.IO.StreamWriter
        Dim LittleEndian As UShort
        Dim DebugOutput As String = ""

        REPO = My.Computer.FileSystem.OpenTextFileWriter(txtFileName.Text + ".txt", True)

        If i = 0 And Tbls = "COMMAND" Then
            REPO.WriteLine("")
            REPO.WriteLine("Command Tables:")
        ElseIf i = 0 And Tbls = "DATA" Then
            REPO.WriteLine("")
            REPO.WriteLine("Data Tables:")
        End If

        If Tbls = "COMMAND" Then
            If Offset = 0 Then
                DebugOutput = "  " & Hex(i).ToString.PadLeft(4, "0").ToLower & ":   -               " & "(" & INDEX_COMMAND_TABLE(i) & ")"
            Else
                DebugOutput = "  " & Hex(i).ToString.PadLeft(4, "0").ToLower & ":   " & Hex(Offset).ToString.PadLeft(4, "0").ToLower &
                               "  Len " & Hex(ReadingBytes(Offset, 2, LittleEndian)).ToString.PadLeft(4, "0").ToLower & "  (" & INDEX_COMMAND_TABLE(i) & ")"
            End If
        ElseIf Tbls = "DATA" Then
            If Offset = 0 Then
                DebugOutput = "  " & Hex(i).ToString.PadLeft(4, "0").ToLower & ":   -                          " & "(" & INDEX_DATA_TABLE(i) & ")"
            Else
                DebugOutput = "  " & Hex(i).ToString.PadLeft(4, "0").ToLower & ":   " & Hex(Offset).ToString.PadLeft(4, "0").ToLower &
                               "  Len " & Hex(ReadingBytes(Offset, 2, LittleEndian)).ToString.PadLeft(4, "0").ToLower &
                               "  Rev " & Hex(ReadingBytes(Offset + 2, 1, LittleEndian)).ToString.PadLeft(2, "0").ToLower &
                               ":" & Hex(ReadingBytes(Offset + 3, 1, LittleEndian)).ToString.PadLeft(2, "0").ToLower & "  (" & INDEX_DATA_TABLE(i) & ")"
            End If
        End If

        REPO.WriteLine(DebugOutput)
        Debug.WriteLine(DebugOutput)
        REPO.Close()

        Return DebugOutput
    End Function

    Public Function ATOMROMFILE(ByVal sender As String)
        Dim ENCODE As New System.Text.UnicodeEncoding(True, True, False)
        Dim ATOMROMHeaderOffset As UShort
        Dim ATOMMasterCommandTableOffset As UShort
        Dim ATOMMasterDataTableOffset As UShort
        Dim ATOM_ROM_HEADER As _ATOM_ROM_HEADER
        ReDim ATOM_ROM_HEADER.uaFirmWareSignature(4)
        Dim ATOM_MASTER_COMMAND_TABLES As _ATOM_MASTER_COMMAND_TABLE
        Dim ATOM_MASTER_DATA_TABLES As _ATOM_MASTER_DATA_TABLE
        Dim LittleEndian As UShort
        Dim ATOMBIOSSignature As String

        Using FS As New IO.FileStream(txtFileName.Text, IO.FileMode.Open, IO.FileAccess.Read)

            '##########################################################################
            'write a report
            Dim REPO As System.IO.StreamWriter
            REPO = My.Computer.FileSystem.OpenTextFileWriter(FS.Name + ".txt", False)
            REPO.WriteLine("Read " & Hex(FS.Length).ToLower & " bytes of data from " & FS.Name)
            REPO.Close()
            '##########################################################################

            '#################################################################################################################################################################
            'ATOM_ROM_HEADER

            'debug
            Debug.WriteLine("")
            Debug.WriteLine("######################################################################################################################")
            Debug.WriteLine("ATOM_ROM_HEADER Section")
            Debug.WriteLine("")
            Dim startIdx As Integer = 0
            ATOMROMHeaderOffset = ReadingBytes(OFFSET_TO_POINTER_TO_ATOM_ROM_HEADER, 2, LittleEndian)
            With ATOM_ROM_HEADER
                .sHeader.usStructureSize = ReadingBytes(ATOMROMHeaderOffset, 2, LittleEndian)
                .sHeader.ucTableFormatRevision = ReadingBytes(ATOMROMHeaderOffset + 2, 1, LittleEndian)
                .sHeader.ucTableContentRevision = ReadingBytes(ATOMROMHeaderOffset + 3, 1, LittleEndian)
                .uaFirmWareSignature(0) = ReadingBytes(ATOMROMHeaderOffset + 4, 1, LittleEndian)
                .uaFirmWareSignature(1) = ReadingBytes(ATOMROMHeaderOffset + 5, 1, LittleEndian)
                .uaFirmWareSignature(2) = ReadingBytes(ATOMROMHeaderOffset + 6, 1, LittleEndian)
                .uaFirmWareSignature(3) = ReadingBytes(ATOMROMHeaderOffset + 7, 1, LittleEndian)
                .usBiosRuntimeSegmentAddress = ReadingBytes(ATOMROMHeaderOffset + 8, 2, LittleEndian)
                .usProtectedModeInfoOffset = ReadingBytes(ATOMROMHeaderOffset + 10, 2, LittleEndian)
                .usConfigFilenameOffset = ReadingBytes(ATOMROMHeaderOffset + 12, 2, LittleEndian)
                .usCRC_BlockOffset = ReadingBytes(ATOMROMHeaderOffset + 14, 2, LittleEndian)
                .usBIOS_BootupMessageOffset = ReadingBytes(ATOMROMHeaderOffset + 16, 2, LittleEndian)
                .usInt10Offset = ReadingBytes(ATOMROMHeaderOffset + 18, 2, LittleEndian)
                .usPciBusDevInitCode = ReadingBytes(ATOMROMHeaderOffset + 20, 2, LittleEndian)
                .usIoBaseAddress = ReadingBytes(ATOMROMHeaderOffset + 22, 2, LittleEndian)
                .usSubsystemVendorID = ReadingBytes(ATOMROMHeaderOffset + 24, 2, LittleEndian)
                .usSubsystemID = ReadingBytes(ATOMROMHeaderOffset + 26, 2, LittleEndian)
                .usPCI_InfoOffset = ReadingBytes(ATOMROMHeaderOffset + 28, 2, LittleEndian)
                .usMasterCommandTableOffset = ReadingBytes(ATOMROMHeaderOffset + 30, 2, LittleEndian)
                .usMasterDataTableOffset = ReadingBytes(ATOMROMHeaderOffset + 32, 2, LittleEndian)
                .ucExtendedFunctionCode = ReadingBytes(ATOMROMHeaderOffset + 34, 1, LittleEndian)
                .ucReserved = ReadingBytes(ATOMROMHeaderOffset + 35, 1, LittleEndian)
            End With

            'run check whether the file is valid ATOM BIOS file
            ATOMBIOSSignature = Convert.ToChar(ATOM_ROM_HEADER.uaFirmWareSignature(0)) & Convert.ToChar(ATOM_ROM_HEADER.uaFirmWareSignature(1)) &
                Convert.ToChar(ATOM_ROM_HEADER.uaFirmWareSignature(2)) & Convert.ToChar(ATOM_ROM_HEADER.uaFirmWareSignature(3))
            Debug.WriteLine("ATOMBIOSSignature = " & ATOMBIOSSignature)
            Debug.WriteLine("")
            If ATOMBIOSSignature <> "ATOM" Then
                lblStatus.Text = "Error loading BIOS file!"
                MsgBox("Not a valid ATOM BIOS file!", MsgBoxStyle.Critical, "Error")
                Exit Function
            End If

            'debug
            Debug.WriteLine("ATOM_ROM_HEADER offset = " & ATOMROMHeaderOffset & " (0x" & Hex(ATOMROMHeaderOffset) & ")")
            Debug.WriteLine("usStructureSize = " & ATOM_ROM_HEADER.sHeader.usStructureSize & " (0x" & Hex(ATOM_ROM_HEADER.sHeader.usStructureSize) & ")")
            Debug.WriteLine("ucTableFormatRevision = " & ATOM_ROM_HEADER.sHeader.ucTableFormatRevision & " (0x" & Hex(ATOM_ROM_HEADER.sHeader.ucTableFormatRevision) & ")")
            Debug.WriteLine("ucTableContentRevision = " & ATOM_ROM_HEADER.sHeader.ucTableContentRevision & " (0x" & Hex(ATOM_ROM_HEADER.sHeader.ucTableContentRevision) & ")")
            Debug.WriteLine("uaFirmWareSignature = " & ATOM_ROM_HEADER.uaFirmWareSignature(0) & " " & ATOM_ROM_HEADER.uaFirmWareSignature(1) &
                        " " & ATOM_ROM_HEADER.uaFirmWareSignature(2) & " " & ATOM_ROM_HEADER.uaFirmWareSignature(3) & " (0x" & Hex(ATOM_ROM_HEADER.uaFirmWareSignature(0)) &
                        " 0x" & Hex(ATOM_ROM_HEADER.uaFirmWareSignature(1)) & " 0x" & Hex(ATOM_ROM_HEADER.uaFirmWareSignature(2)) & " 0x" & Hex(ATOM_ROM_HEADER.uaFirmWareSignature(3)) &
                        " : " & Convert.ToChar(ATOM_ROM_HEADER.uaFirmWareSignature(0)) & " " & Convert.ToChar(ATOM_ROM_HEADER.uaFirmWareSignature(1)) &
                        " " & Convert.ToChar(ATOM_ROM_HEADER.uaFirmWareSignature(2)) & " " & Convert.ToChar(ATOM_ROM_HEADER.uaFirmWareSignature(3)) & ")")
            Debug.WriteLine("usBiosRuntimeSegmentAddress = " & ATOM_ROM_HEADER.usBiosRuntimeSegmentAddress & " (0x" & Hex(ATOM_ROM_HEADER.usBiosRuntimeSegmentAddress) & ")")
            Debug.WriteLine("usProtectedModeInfoOffset = " & ATOM_ROM_HEADER.usProtectedModeInfoOffset & " (0x" & Hex(ATOM_ROM_HEADER.usProtectedModeInfoOffset) & ")")
            Debug.WriteLine("usConfigFilenameOffset = " & ATOM_ROM_HEADER.usConfigFilenameOffset & " (0x" & Hex(ATOM_ROM_HEADER.usConfigFilenameOffset) & ")")
            Debug.WriteLine("usCRC_BlockOffset = " & ATOM_ROM_HEADER.usCRC_BlockOffset & " (0x" & Hex(ATOM_ROM_HEADER.usCRC_BlockOffset) & ")")
            Debug.WriteLine("usBIOS_BootupMessageOffset = " & ATOM_ROM_HEADER.usBIOS_BootupMessageOffset & " (0x" & Hex(ATOM_ROM_HEADER.usBIOS_BootupMessageOffset) & ")")
            Debug.WriteLine("usInt10Offset = " & ATOM_ROM_HEADER.usInt10Offset & " (0x" & Hex(ATOM_ROM_HEADER.usInt10Offset) & ")")
            Debug.WriteLine("usPciBusDevInitCode = " & ATOM_ROM_HEADER.usPciBusDevInitCode & " (0x" & Hex(ATOM_ROM_HEADER.usPciBusDevInitCode) & ")")
            Debug.WriteLine("usIoBaseAddress = " & ATOM_ROM_HEADER.usIoBaseAddress & " (0x" & Hex(ATOM_ROM_HEADER.usIoBaseAddress) & ")")
            Debug.WriteLine("usSubsystemVendorID = " & ATOM_ROM_HEADER.usSubsystemVendorID & " (0x" & Hex(ATOM_ROM_HEADER.usSubsystemVendorID) & ")")
            Debug.WriteLine("usSubsystemID = " & ATOM_ROM_HEADER.usSubsystemID & " (0x" & Hex(ATOM_ROM_HEADER.usSubsystemID) & ")")
            Debug.WriteLine("usPCI_InfoOffset = " & ATOM_ROM_HEADER.usPCI_InfoOffset & " (0x" & Hex(ATOM_ROM_HEADER.usPCI_InfoOffset) & ")")
            Debug.WriteLine("usMasterCommandTableOffset = " & ATOM_ROM_HEADER.usMasterCommandTableOffset & " (0x" & Hex(ATOM_ROM_HEADER.usMasterCommandTableOffset) & ")")
            Debug.WriteLine("usMasterDataTableOffset = " & ATOM_ROM_HEADER.usMasterDataTableOffset & " (0x" & Hex(ATOM_ROM_HEADER.usMasterDataTableOffset) & ")")
            Debug.WriteLine("ucExtendedFunctionCode = " & ATOM_ROM_HEADER.ucExtendedFunctionCode & " (0x" & Hex(ATOM_ROM_HEADER.ucExtendedFunctionCode) & ")")
            Debug.WriteLine("ucReserved = " & ATOM_ROM_HEADER.ucReserved & " (0x" & Hex(ATOM_ROM_HEADER.ucReserved) & ")")


            '#################################################################################################################################################################
            'ATOM_MASTER_COMMAND_TABLES

            'debug
            Debug.WriteLine("")
            Debug.WriteLine("######################################################################################################################")
            Debug.WriteLine("ATOM_MASTER_COMMAND_TABLES Section")
            Debug.WriteLine("")
            ATOMMasterCommandTableOffset = ATOM_ROM_HEADER.usMasterCommandTableOffset
            With ATOM_MASTER_COMMAND_TABLES
                .sHeader.usStructureSize = ReadingBytes(ATOMMasterCommandTableOffset, 2, LittleEndian)
                .sHeader.ucTableFormatRevision = ReadingBytes(ATOMMasterCommandTableOffset + 2, 1, LittleEndian)
                .sHeader.ucTableContentRevision = ReadingBytes(ATOMMasterCommandTableOffset + 3, 1, LittleEndian)
                .ListOfCommandTables.ASIC_Init = ReadingBytes(ATOMMasterCommandTableOffset + 4, 2, LittleEndian)
                .ListOfCommandTables.GetDisplaySurfaceSize = ReadingBytes(ATOMMasterCommandTableOffset + 6, 2, LittleEndian)
                .ListOfCommandTables.ASIC_RegistersInit = ReadingBytes(ATOMMasterCommandTableOffset + 8, 2, LittleEndian)
                .ListOfCommandTables.VRAM_BlockVenderDetection = ReadingBytes(ATOMMasterCommandTableOffset + 10, 2, LittleEndian)
                .ListOfCommandTables.DIGxEncoderControl = ReadingBytes(ATOMMasterCommandTableOffset + 12, 2, LittleEndian)
                .ListOfCommandTables.MemoryControllerInit = ReadingBytes(ATOMMasterCommandTableOffset + 14, 2, LittleEndian)
                .ListOfCommandTables.EnableCRTCMemReq = ReadingBytes(ATOMMasterCommandTableOffset + 16, 2, LittleEndian)
                .ListOfCommandTables.MemoryParamAdjust = ReadingBytes(ATOMMasterCommandTableOffset + 18, 2, LittleEndian)
                .ListOfCommandTables.DVOEncoderControl = ReadingBytes(ATOMMasterCommandTableOffset + 20, 2, LittleEndian)
                .ListOfCommandTables.GPIOPinControl = ReadingBytes(ATOMMasterCommandTableOffset + 22, 2, LittleEndian)
                .ListOfCommandTables.SetEngineClock = ReadingBytes(ATOMMasterCommandTableOffset + 24, 2, LittleEndian)
                .ListOfCommandTables.SetMemoryClock = ReadingBytes(ATOMMasterCommandTableOffset + 26, 2, LittleEndian)
                .ListOfCommandTables.SetPixelClock = ReadingBytes(ATOMMasterCommandTableOffset + 28, 2, LittleEndian)
                .ListOfCommandTables.EnableDispPowerGating = ReadingBytes(ATOMMasterCommandTableOffset + 30, 2, LittleEndian)
                .ListOfCommandTables.ResetMemoryDLL = ReadingBytes(ATOMMasterCommandTableOffset + 32, 2, LittleEndian)
                .ListOfCommandTables.ResetMemoryDevice = ReadingBytes(ATOMMasterCommandTableOffset + 34, 2, LittleEndian)
                .ListOfCommandTables.MemoryPLLInit = ReadingBytes(ATOMMasterCommandTableOffset + 36, 2, LittleEndian)
                .ListOfCommandTables.AdjustDisplayPll = ReadingBytes(ATOMMasterCommandTableOffset + 38, 2, LittleEndian)
                .ListOfCommandTables.AdjustMemoryController = ReadingBytes(ATOMMasterCommandTableOffset + 40, 2, LittleEndian)
                .ListOfCommandTables.EnableASIC_StaticPwrMgt = ReadingBytes(ATOMMasterCommandTableOffset + 42, 2, LittleEndian)
                .ListOfCommandTables.SetUniphyInstance = ReadingBytes(ATOMMasterCommandTableOffset + 44, 2, LittleEndian)
                .ListOfCommandTables.DAC_LoadDetection = ReadingBytes(ATOMMasterCommandTableOffset + 46, 2, LittleEndian)
                .ListOfCommandTables.LVTMAEncoderControl = ReadingBytes(ATOMMasterCommandTableOffset + 48, 2, LittleEndian)
                .ListOfCommandTables.HW_Misc_Operation = ReadingBytes(ATOMMasterCommandTableOffset + 50, 2, LittleEndian)
                .ListOfCommandTables.DAC1EncoderControl = ReadingBytes(ATOMMasterCommandTableOffset + 52, 2, LittleEndian)
                .ListOfCommandTables.DAC2EncoderControl = ReadingBytes(ATOMMasterCommandTableOffset + 54, 2, LittleEndian)
                .ListOfCommandTables.DVOOutputControl = ReadingBytes(ATOMMasterCommandTableOffset + 56, 2, LittleEndian)
                .ListOfCommandTables.CV1OutputControl = ReadingBytes(ATOMMasterCommandTableOffset + 58, 2, LittleEndian)
                .ListOfCommandTables.GetConditionalGoldenSetting = ReadingBytes(ATOMMasterCommandTableOffset + 60, 2, LittleEndian)
                .ListOfCommandTables.TVEncoderControl = ReadingBytes(ATOMMasterCommandTableOffset + 62, 2, LittleEndian)
                .ListOfCommandTables.PatchMCSetting = ReadingBytes(ATOMMasterCommandTableOffset + 64, 2, LittleEndian)
                .ListOfCommandTables.MC_SEQ_Control = ReadingBytes(ATOMMasterCommandTableOffset + 66, 2, LittleEndian)
                .ListOfCommandTables.Gfx_Harvesting = ReadingBytes(ATOMMasterCommandTableOffset + 68, 2, LittleEndian)
                .ListOfCommandTables.EnableScaler = ReadingBytes(ATOMMasterCommandTableOffset + 70, 2, LittleEndian)
                .ListOfCommandTables.BlankCRTC = ReadingBytes(ATOMMasterCommandTableOffset + 72, 2, LittleEndian)
                .ListOfCommandTables.EnableCRTC = ReadingBytes(ATOMMasterCommandTableOffset + 74, 2, LittleEndian)
                .ListOfCommandTables.GetPixelClock = ReadingBytes(ATOMMasterCommandTableOffset + 76, 2, LittleEndian)
                .ListOfCommandTables.EnableVGA_Render = ReadingBytes(ATOMMasterCommandTableOffset + 78, 2, LittleEndian)
                .ListOfCommandTables.GetSCLKOverMCLKRatio = ReadingBytes(ATOMMasterCommandTableOffset + 80, 2, LittleEndian)
                .ListOfCommandTables.SetCRTC_Timing = ReadingBytes(ATOMMasterCommandTableOffset + 82, 2, LittleEndian)
                .ListOfCommandTables.SetCRTC_OverScan = ReadingBytes(ATOMMasterCommandTableOffset + 84, 2, LittleEndian)
                .ListOfCommandTables.SetCRTC_Replication = ReadingBytes(ATOMMasterCommandTableOffset + 86, 2, LittleEndian)
                .ListOfCommandTables.SelectCRTC_Source = ReadingBytes(ATOMMasterCommandTableOffset + 88, 2, LittleEndian)
                .ListOfCommandTables.EnableGraphSurfaces = ReadingBytes(ATOMMasterCommandTableOffset + 90, 2, LittleEndian)
                .ListOfCommandTables.UpdateCRTC_DoubleBufferRegisters = ReadingBytes(ATOMMasterCommandTableOffset + 92, 2, LittleEndian)
                .ListOfCommandTables.LUT_AutoFill = ReadingBytes(ATOMMasterCommandTableOffset + 94, 2, LittleEndian)
                .ListOfCommandTables.EnableHW_IconCursor = ReadingBytes(ATOMMasterCommandTableOffset + 96, 2, LittleEndian)
                .ListOfCommandTables.GetMemoryClock = ReadingBytes(ATOMMasterCommandTableOffset + 98, 2, LittleEndian)
                .ListOfCommandTables.GetEngineClock = ReadingBytes(ATOMMasterCommandTableOffset + 100, 2, LittleEndian)
                .ListOfCommandTables.SetCRTC_UsingDTDTiming = ReadingBytes(ATOMMasterCommandTableOffset + 102, 2, LittleEndian)
                .ListOfCommandTables.ExternalEncoderControl = ReadingBytes(ATOMMasterCommandTableOffset + 104, 2, LittleEndian)
                .ListOfCommandTables.LVTMAOutputControl = ReadingBytes(ATOMMasterCommandTableOffset + 106, 2, LittleEndian)
                .ListOfCommandTables.VRAM_BlockDetectionByStrap = ReadingBytes(ATOMMasterCommandTableOffset + 108, 2, LittleEndian)
                .ListOfCommandTables.MemoryCleanUp = ReadingBytes(ATOMMasterCommandTableOffset + 110, 2, LittleEndian)
                .ListOfCommandTables.ProcessI2cChannelTransaction = ReadingBytes(ATOMMasterCommandTableOffset + 112, 2, LittleEndian)
                .ListOfCommandTables.WriteOneByteToHWAssistedI2C = ReadingBytes(ATOMMasterCommandTableOffset + 114, 2, LittleEndian)
                .ListOfCommandTables.ReadHWAssistedI2CStatus = ReadingBytes(ATOMMasterCommandTableOffset + 116, 2, LittleEndian)
                .ListOfCommandTables.SpeedFanControl = ReadingBytes(ATOMMasterCommandTableOffset + 118, 2, LittleEndian)
                .ListOfCommandTables.PowerConnectorDetection = ReadingBytes(ATOMMasterCommandTableOffset + 120, 2, LittleEndian)
                .ListOfCommandTables.MC_Synchronization = ReadingBytes(ATOMMasterCommandTableOffset + 122, 2, LittleEndian)
                .ListOfCommandTables.ComputeMemoryEnginePLL = ReadingBytes(ATOMMasterCommandTableOffset + 124, 2, LittleEndian)
                .ListOfCommandTables.MemoryRefreshConversion = ReadingBytes(ATOMMasterCommandTableOffset + 126, 2, LittleEndian)
                .ListOfCommandTables.VRAM_GetCurrentInfoBlock = ReadingBytes(ATOMMasterCommandTableOffset + 128, 2, LittleEndian)
                .ListOfCommandTables.DynamicMemorySettings = ReadingBytes(ATOMMasterCommandTableOffset + 130, 2, LittleEndian)
                .ListOfCommandTables.MemoryTraining = ReadingBytes(ATOMMasterCommandTableOffset + 132, 2, LittleEndian)
                .ListOfCommandTables.EnableSpreadSpectrumOnPPLL = ReadingBytes(ATOMMasterCommandTableOffset + 134, 2, LittleEndian)
                .ListOfCommandTables.TMDSAOutputControl = ReadingBytes(ATOMMasterCommandTableOffset + 136, 2, LittleEndian)
                .ListOfCommandTables.SetVoltage = ReadingBytes(ATOMMasterCommandTableOffset + 138, 2, LittleEndian)
                .ListOfCommandTables.DAC1OutputControl = ReadingBytes(ATOMMasterCommandTableOffset + 140, 2, LittleEndian)
                .ListOfCommandTables.DAC2OutputControl = ReadingBytes(ATOMMasterCommandTableOffset + 142, 2, LittleEndian)
                .ListOfCommandTables.ComputeMemoryClockParam = ReadingBytes(ATOMMasterCommandTableOffset + 144, 2, LittleEndian)
                .ListOfCommandTables.ClockSource = ReadingBytes(ATOMMasterCommandTableOffset + 146, 2, LittleEndian)
                .ListOfCommandTables.MemoryDeviceInit = ReadingBytes(ATOMMasterCommandTableOffset + 148, 2, LittleEndian)
                .ListOfCommandTables.GetDispObjectInfo = ReadingBytes(ATOMMasterCommandTableOffset + 150, 2, LittleEndian)
                .ListOfCommandTables.DIG1EncoderControl = ReadingBytes(ATOMMasterCommandTableOffset + 152, 2, LittleEndian)
                .ListOfCommandTables.DIG2EncoderControl = ReadingBytes(ATOMMasterCommandTableOffset + 154, 2, LittleEndian)
                .ListOfCommandTables.DIG1TransmitterControl = ReadingBytes(ATOMMasterCommandTableOffset + 156, 2, LittleEndian)
                .ListOfCommandTables.DIG2TransmitterControl = ReadingBytes(ATOMMasterCommandTableOffset + 158, 2, LittleEndian)
                .ListOfCommandTables.ProcessAuxChannelTransaction = ReadingBytes(ATOMMasterCommandTableOffset + 160, 2, LittleEndian)
                .ListOfCommandTables.DPEncoderService = ReadingBytes(ATOMMasterCommandTableOffset + 162, 2, LittleEndian)
                .ListOfCommandTables.GetVoltageInfo = ReadingBytes(ATOMMasterCommandTableOffset + 164, 2, LittleEndian)
            End With

            'debug
            Debug.WriteLine("ATOM_MASTER_COMMAND_TABLES offset = " & ATOM_ROM_HEADER.usMasterCommandTableOffset & " (0x" & Hex(ATOM_ROM_HEADER.usMasterCommandTableOffset) & ")")

            'debug
            Debug.WriteLine("usStructureSize = " & ATOM_MASTER_COMMAND_TABLES.sHeader.usStructureSize & " (0x" & Hex(ATOM_MASTER_COMMAND_TABLES.sHeader.usStructureSize) & ")")

            'debug
            Debug.WriteLine("ucTableFormatRevision = " & ATOM_MASTER_COMMAND_TABLES.sHeader.ucTableFormatRevision & " (0x" & Hex(ATOM_MASTER_COMMAND_TABLES.sHeader.ucTableFormatRevision) & ")")

            'debug
            Debug.WriteLine("ucTableContentRevision = " & ATOM_MASTER_COMMAND_TABLES.sHeader.ucTableContentRevision & " (0x" & Hex(ATOM_MASTER_COMMAND_TABLES.sHeader.ucTableContentRevision) & ")")

            '##########################################################################
            'write a report
            WritingReport(0, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ASIC_Init, "COMMAND")
            WritingReport(1, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.GetDisplaySurfaceSize, "COMMAND")
            WritingReport(2, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ASIC_RegistersInit, "COMMAND")
            WritingReport(3, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.VRAM_BlockVenderDetection, "COMMAND")
            WritingReport(4, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DIGxEncoderControl, "COMMAND")
            WritingReport(5, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.MemoryControllerInit, "COMMAND")
            WritingReport(6, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.EnableCRTCMemReq, "COMMAND")
            WritingReport(7, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.MemoryParamAdjust, "COMMAND")
            WritingReport(8, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DVOEncoderControl, "COMMAND")
            WritingReport(9, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.GPIOPinControl, "COMMAND")
            WritingReport(10, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SetEngineClock, "COMMAND")
            WritingReport(11, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SetMemoryClock, "COMMAND")
            WritingReport(12, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SetPixelClock, "COMMAND")
            WritingReport(13, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.EnableDispPowerGating, "COMMAND")
            WritingReport(14, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ResetMemoryDLL, "COMMAND")
            WritingReport(15, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ResetMemoryDevice, "COMMAND")
            WritingReport(16, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.MemoryPLLInit, "COMMAND")
            WritingReport(17, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.AdjustDisplayPll, "COMMAND")
            WritingReport(18, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.AdjustMemoryController, "COMMAND")
            WritingReport(19, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.EnableASIC_StaticPwrMgt, "COMMAND")
            WritingReport(20, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SetUniphyInstance, "COMMAND")
            WritingReport(21, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DAC_LoadDetection, "COMMAND")
            WritingReport(22, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.LVTMAEncoderControl, "COMMAND")
            WritingReport(23, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.HW_Misc_Operation, "COMMAND")
            WritingReport(24, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DAC1EncoderControl, "COMMAND")
            WritingReport(25, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DAC2EncoderControl, "COMMAND")
            WritingReport(26, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DVOOutputControl, "COMMAND")
            WritingReport(27, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.CV1OutputControl, "COMMAND")
            WritingReport(28, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.GetConditionalGoldenSetting, "COMMAND")
            WritingReport(29, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.TVEncoderControl, "COMMAND")
            WritingReport(30, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.PatchMCSetting, "COMMAND")
            WritingReport(31, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.MC_SEQ_Control, "COMMAND")
            WritingReport(32, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.Gfx_Harvesting, "COMMAND")
            WritingReport(33, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.EnableScaler, "COMMAND")
            WritingReport(34, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.BlankCRTC, "COMMAND")
            WritingReport(35, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.EnableCRTC, "COMMAND")
            WritingReport(36, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.GetPixelClock, "COMMAND")
            WritingReport(37, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.EnableVGA_Render, "COMMAND")
            WritingReport(38, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.GetSCLKOverMCLKRatio, "COMMAND")
            WritingReport(39, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SetCRTC_Timing, "COMMAND")
            WritingReport(40, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SetCRTC_OverScan, "COMMAND")
            WritingReport(41, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SetCRTC_Replication, "COMMAND")
            WritingReport(42, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SelectCRTC_Source, "COMMAND")
            WritingReport(43, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.EnableGraphSurfaces, "COMMAND")
            WritingReport(44, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.UpdateCRTC_DoubleBufferRegisters, "COMMAND")
            WritingReport(45, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.LUT_AutoFill, "COMMAND")
            WritingReport(46, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.EnableHW_IconCursor, "COMMAND")
            WritingReport(47, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.GetMemoryClock, "COMMAND")
            WritingReport(48, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.GetEngineClock, "COMMAND")
            WritingReport(49, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SetCRTC_UsingDTDTiming, "COMMAND")
            WritingReport(50, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ExternalEncoderControl, "COMMAND")
            WritingReport(51, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.LVTMAOutputControl, "COMMAND")
            WritingReport(52, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.VRAM_BlockDetectionByStrap, "COMMAND")
            WritingReport(53, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.MemoryCleanUp, "COMMAND")
            WritingReport(54, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ProcessI2cChannelTransaction, "COMMAND")
            WritingReport(55, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.WriteOneByteToHWAssistedI2C, "COMMAND")
            WritingReport(56, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ReadHWAssistedI2CStatus, "COMMAND")
            WritingReport(57, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SpeedFanControl, "COMMAND")
            WritingReport(58, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.PowerConnectorDetection, "COMMAND")
            WritingReport(59, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.MC_Synchronization, "COMMAND")
            WritingReport(60, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ComputeMemoryEnginePLL, "COMMAND")
            WritingReport(61, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.MemoryRefreshConversion, "COMMAND")
            WritingReport(62, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.VRAM_GetCurrentInfoBlock, "COMMAND")
            WritingReport(63, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DynamicMemorySettings, "COMMAND")
            WritingReport(64, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.MemoryTraining, "COMMAND")
            WritingReport(65, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.EnableSpreadSpectrumOnPPLL, "COMMAND")
            WritingReport(66, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.TMDSAOutputControl, "COMMAND")
            WritingReport(67, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.SetVoltage, "COMMAND")
            WritingReport(68, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DAC1OutputControl, "COMMAND")
            WritingReport(69, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DAC2OutputControl, "COMMAND")
            WritingReport(70, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ComputeMemoryClockParam, "COMMAND")
            WritingReport(71, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ClockSource, "COMMAND")
            WritingReport(72, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.MemoryDeviceInit, "COMMAND")
            WritingReport(73, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.GetDispObjectInfo, "COMMAND")
            WritingReport(74, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DIG1EncoderControl, "COMMAND")
            WritingReport(75, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DIG2EncoderControl, "COMMAND")
            WritingReport(76, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DIG1TransmitterControl, "COMMAND")
            WritingReport(77, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DIG2TransmitterControl, "COMMAND")
            WritingReport(78, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.ProcessAuxChannelTransaction, "COMMAND")
            WritingReport(79, ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.DPEncoderService, "COMMAND")
            '##########################################################################

            'debug unused table
            Debug.WriteLine("GetVoltageInfo = " & ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.GetVoltageInfo & " (0x" & Hex(ATOM_MASTER_COMMAND_TABLES.ListOfCommandTables.GetVoltageInfo) & ")")


            '#################################################################################################################################################################
            'ATOM_MASTER_DATA_TABLES

            'debug
            Debug.WriteLine("")
            Debug.WriteLine("######################################################################################################################")
            Debug.WriteLine("ATOM_MASTER_DATA_TABLES Section")
            Debug.WriteLine("")
            ATOMMasterDataTableOffset = ATOM_ROM_HEADER.usMasterDataTableOffset
            With ATOM_MASTER_DATA_TABLES
                .sHeader.usStructureSize = ReadingBytes(ATOMMasterDataTableOffset, 2, LittleEndian)
                .sHeader.ucTableFormatRevision = ReadingBytes(ATOMMasterDataTableOffset + 2, 1, LittleEndian)
                .sHeader.ucTableContentRevision = ReadingBytes(ATOMMasterDataTableOffset + 3, 1, LittleEndian)
                .ListOfDataTables.UtilityPipeLine = ReadingBytes(ATOMMasterDataTableOffset + 4, 2, LittleEndian)
                .ListOfDataTables.MultimediaCapabilityInfo = ReadingBytes(ATOMMasterDataTableOffset + 6, 2, LittleEndian)
                .ListOfDataTables.MultimediaConfigInfo = ReadingBytes(ATOMMasterDataTableOffset + 8, 2, LittleEndian)
                .ListOfDataTables.StandardVESA_Timing = ReadingBytes(ATOMMasterDataTableOffset + 10, 2, LittleEndian)
                .ListOfDataTables.FirmwareInfo = ReadingBytes(ATOMMasterDataTableOffset + 12, 2, LittleEndian)
                .ListOfDataTables.PaletteData = ReadingBytes(ATOMMasterDataTableOffset + 14, 2, LittleEndian)
                .ListOfDataTables.LCD_Info = ReadingBytes(ATOMMasterDataTableOffset + 16, 2, LittleEndian)
                .ListOfDataTables.DIGTransmitterInfo = ReadingBytes(ATOMMasterDataTableOffset + 18, 2, LittleEndian)
                .ListOfDataTables.AnalogTV_Info = ReadingBytes(ATOMMasterDataTableOffset + 20, 2, LittleEndian)
                .ListOfDataTables.SupportedDevicesInfo = ReadingBytes(ATOMMasterDataTableOffset + 22, 2, LittleEndian)
                .ListOfDataTables.GPIO_I2C_Info = ReadingBytes(ATOMMasterDataTableOffset + 24, 2, LittleEndian)
                .ListOfDataTables.VRAM_UsageByFirmware = ReadingBytes(ATOMMasterDataTableOffset + 26, 2, LittleEndian)
                .ListOfDataTables.GPIO_Pin_LUT = ReadingBytes(ATOMMasterDataTableOffset + 28, 2, LittleEndian)
                .ListOfDataTables.VESA_ToInternalModeLUT = ReadingBytes(ATOMMasterDataTableOffset + 30, 2, LittleEndian)
                .ListOfDataTables.ComponentVideoInfo = ReadingBytes(ATOMMasterDataTableOffset + 32, 2, LittleEndian)
                .ListOfDataTables.PowerPlayInfo = ReadingBytes(ATOMMasterDataTableOffset + 34, 2, LittleEndian)
                .ListOfDataTables.CompassionateData = ReadingBytes(ATOMMasterDataTableOffset + 36, 2, LittleEndian)
                .ListOfDataTables.SaveRestoreInfo = ReadingBytes(ATOMMasterDataTableOffset + 38, 2, LittleEndian)
                .ListOfDataTables.PPLL_SS_Info = ReadingBytes(ATOMMasterDataTableOffset + 40, 2, LittleEndian)
                .ListOfDataTables.OemInfo = ReadingBytes(ATOMMasterDataTableOffset + 42, 2, LittleEndian)
                .ListOfDataTables.XTMDS_Info = ReadingBytes(ATOMMasterDataTableOffset + 44, 2, LittleEndian)
                .ListOfDataTables.MclkSS_Info = ReadingBytes(ATOMMasterDataTableOffset + 46, 2, LittleEndian)
                .ListOfDataTables.Object_Header = ReadingBytes(ATOMMasterDataTableOffset + 48, 2, LittleEndian)
                .ListOfDataTables.IndirectIOAccess = ReadingBytes(ATOMMasterDataTableOffset + 50, 2, LittleEndian)
                .ListOfDataTables.MC_InitParameter = ReadingBytes(ATOMMasterDataTableOffset + 52, 2, LittleEndian)
                .ListOfDataTables.ASIC_VDDC_Info = ReadingBytes(ATOMMasterDataTableOffset + 54, 2, LittleEndian)
                .ListOfDataTables.ASIC_InternalSS_Info = ReadingBytes(ATOMMasterDataTableOffset + 56, 2, LittleEndian)
                .ListOfDataTables.TV_VideoMode = ReadingBytes(ATOMMasterDataTableOffset + 58, 2, LittleEndian)
                .ListOfDataTables.VRAM_Info = ReadingBytes(ATOMMasterDataTableOffset + 60, 2, LittleEndian)
                .ListOfDataTables.MemoryTrainingInfo = ReadingBytes(ATOMMasterDataTableOffset + 62, 2, LittleEndian)
                .ListOfDataTables.IntegratedSystemInfo = ReadingBytes(ATOMMasterDataTableOffset + 64, 2, LittleEndian)
                .ListOfDataTables.ASIC_ProfilingInfo = ReadingBytes(ATOMMasterDataTableOffset + 66, 2, LittleEndian)
                .ListOfDataTables.VoltageObjectInfo = ReadingBytes(ATOMMasterDataTableOffset + 68, 2, LittleEndian)
                .ListOfDataTables.PowerSourceInfo = ReadingBytes(ATOMMasterDataTableOffset + 70, 2, LittleEndian)
            End With

            'debug
            Debug.WriteLine("ATOM_MASTER_DATA_TABLES offset = " & ATOMMasterDataTableOffset & " (0x" & Hex(ATOMMasterDataTableOffset) & ")")
            Debug.WriteLine("usStructureSize = " & ATOM_MASTER_DATA_TABLES.sHeader.usStructureSize & " (0x" & Hex(ATOM_MASTER_DATA_TABLES.sHeader.usStructureSize) & ")")
            Debug.WriteLine("ucTableFormatRevision = " & ATOM_MASTER_DATA_TABLES.sHeader.ucTableFormatRevision & " (0x" & Hex(ATOM_MASTER_DATA_TABLES.sHeader.ucTableFormatRevision) & ")")
            Debug.WriteLine("ucTableContentRevision = " & ATOM_MASTER_DATA_TABLES.sHeader.ucTableContentRevision & " (0x" & Hex(ATOM_MASTER_DATA_TABLES.sHeader.ucTableContentRevision) & ")")

            '##########################################################################
            'write a report
            WritingReport(0, ATOM_MASTER_DATA_TABLES.ListOfDataTables.UtilityPipeLine, "DATA")
            WritingReport(1, ATOM_MASTER_DATA_TABLES.ListOfDataTables.MultimediaCapabilityInfo, "DATA")
            WritingReport(2, ATOM_MASTER_DATA_TABLES.ListOfDataTables.MultimediaConfigInfo, "DATA")
            WritingReport(3, ATOM_MASTER_DATA_TABLES.ListOfDataTables.StandardVESA_Timing, "DATA")
            WritingReport(4, ATOM_MASTER_DATA_TABLES.ListOfDataTables.FirmwareInfo, "DATA")
            WritingReport(5, ATOM_MASTER_DATA_TABLES.ListOfDataTables.PaletteData, "DATA")
            WritingReport(6, ATOM_MASTER_DATA_TABLES.ListOfDataTables.LCD_Info, "DATA")
            WritingReport(7, ATOM_MASTER_DATA_TABLES.ListOfDataTables.DIGTransmitterInfo, "DATA")
            WritingReport(8, ATOM_MASTER_DATA_TABLES.ListOfDataTables.AnalogTV_Info, "DATA")
            WritingReport(9, ATOM_MASTER_DATA_TABLES.ListOfDataTables.SupportedDevicesInfo, "DATA")
            WritingReport(10, ATOM_MASTER_DATA_TABLES.ListOfDataTables.GPIO_I2C_Info, "DATA")
            WritingReport(11, ATOM_MASTER_DATA_TABLES.ListOfDataTables.VRAM_UsageByFirmware, "DATA")
            WritingReport(12, ATOM_MASTER_DATA_TABLES.ListOfDataTables.GPIO_Pin_LUT, "DATA")
            WritingReport(13, ATOM_MASTER_DATA_TABLES.ListOfDataTables.VESA_ToInternalModeLUT, "DATA")
            WritingReport(14, ATOM_MASTER_DATA_TABLES.ListOfDataTables.ComponentVideoInfo, "DATA")
            WritingReport(15, ATOM_MASTER_DATA_TABLES.ListOfDataTables.PowerPlayInfo, "DATA")
            WritingReport(16, ATOM_MASTER_DATA_TABLES.ListOfDataTables.CompassionateData, "DATA")
            WritingReport(17, ATOM_MASTER_DATA_TABLES.ListOfDataTables.SaveRestoreInfo, "DATA")
            WritingReport(18, ATOM_MASTER_DATA_TABLES.ListOfDataTables.PPLL_SS_Info, "DATA")
            WritingReport(19, ATOM_MASTER_DATA_TABLES.ListOfDataTables.OemInfo, "DATA")
            WritingReport(20, ATOM_MASTER_DATA_TABLES.ListOfDataTables.XTMDS_Info, "DATA")
            WritingReport(21, ATOM_MASTER_DATA_TABLES.ListOfDataTables.MclkSS_Info, "DATA")
            WritingReport(22, ATOM_MASTER_DATA_TABLES.ListOfDataTables.Object_Header, "DATA")
            WritingReport(23, ATOM_MASTER_DATA_TABLES.ListOfDataTables.IndirectIOAccess, "DATA")
            WritingReport(24, ATOM_MASTER_DATA_TABLES.ListOfDataTables.MC_InitParameter, "DATA")
            WritingReport(25, ATOM_MASTER_DATA_TABLES.ListOfDataTables.ASIC_VDDC_Info, "DATA")
            WritingReport(26, ATOM_MASTER_DATA_TABLES.ListOfDataTables.ASIC_InternalSS_Info, "DATA")
            WritingReport(27, ATOM_MASTER_DATA_TABLES.ListOfDataTables.TV_VideoMode, "DATA")
            WritingReport(28, ATOM_MASTER_DATA_TABLES.ListOfDataTables.VRAM_Info, "DATA")
            WritingReport(29, ATOM_MASTER_DATA_TABLES.ListOfDataTables.MemoryTrainingInfo, "DATA")
            WritingReport(30, ATOM_MASTER_DATA_TABLES.ListOfDataTables.IntegratedSystemInfo, "DATA")
            WritingReport(31, ATOM_MASTER_DATA_TABLES.ListOfDataTables.ASIC_ProfilingInfo, "DATA")
            WritingReport(32, ATOM_MASTER_DATA_TABLES.ListOfDataTables.VoltageObjectInfo, "DATA")
            WritingReport(33, ATOM_MASTER_DATA_TABLES.ListOfDataTables.PowerSourceInfo, "DATA")
            '##########################################################################


        End Using

        lblStatus.Text = "BIOS successfully loaded! Please check generated report file of master list tables in BIOS folder."
        Return lblStatus.Text
    End Function

    Private Sub btnOpenFile_Click(sender As Object, e As EventArgs) Handles btnOpenFile.Click
        Dim myStream As IO.FileStream = Nothing
        Dim openFileDialog1 As New OpenFileDialog()

        openFileDialog1.InitialDirectory = ""
        openFileDialog1.Filter = "ROM files (*.rom)|*.rom|BIN files (*.bin)|*.bin|All files (*.*)|*.*"
        openFileDialog1.FilterIndex = 1
        openFileDialog1.RestoreDirectory = False

        If openFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Try
                myStream = openFileDialog1.OpenFile()
                If (myStream IsNot Nothing) Then
                    txtFileName.Text = myStream.Name
                    ATOMROMFILE(txtFileName.Text)
                End If
            Catch Ex As Exception
                MessageBox.Show("Cannot read file from disk. Original error: " & Ex.Message)
            Finally
                ' Check this again, since we need to make sure we didn't throw an exception on open.
                If (myStream IsNot Nothing) Then
                    myStream.Close()
                End If
            End Try
        End If
    End Sub
End Class
