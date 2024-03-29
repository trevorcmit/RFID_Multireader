﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSLibrary.Constants;


namespace CSLibrary {
    public partial class RFIDReader {
        private Operation CurrentOperation;

        public void StopOperation() {
            //HighLevelInterface._debugBLEHold = false;
            byte[] cmd = { 0x40, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            _deviceHandler.SendAsync(0, 0, DOWNLINKCMD.RFIDCMD, cmd, HighLevelInterface.BTWAITCOMMANDRESPONSETYPE.WAIT_BTAPIRESPONSE_DATA2);
        }

        public Result StartOperation(Operation opertion) {
            CurrentOperation = opertion;

            // Clear inventory compatmode
            if (opertion != Operation.TAG_EXERANGING)
            {
                UInt32 Value = 0;

                // Clear inventory compatmode
                MacReadRegister(MACREGISTER.HST_INV_CFG, ref Value);
                Value &= ~(1U << 26); // bit 26
                Value &= ~(3U << 16); // bit 16,17
                MacWriteRegister(MACREGISTER.INV_CYCLE_DELAY, 0);
                MacWriteRegister(MACREGISTER.HST_INV_CFG, Value);
            }

            InventoryDebug.Clear();
            switch (opertion)
            {
                case Operation.TAG_RANGING: // Enable get battery level by interrupt
                    _deviceHandler.battery.EnableAutoBatteryLevel();
                    TagRangingThreadProc();
                    break;

                case Operation.TAG_PRERANGING: // Enable get battery level by interrupt
                    PreTagRangingThreadProc();
                    break;

                case Operation.TAG_EXERANGING: // Enable get battery level by interrupt
                    CurrentOperation = Operation.TAG_RANGING;
                    _deviceHandler.battery.EnableAutoBatteryLevel();
                    ExeTagRangingThreadProc();
                    break;

                case Operation.TAG_SEARCHING: // Enable get battery level by interrupt
                    _deviceHandler.battery.EnableAutoBatteryLevel();
                    TagSearchOneTagThreadProc();
                    break;

                case Operation.TAG_PRESEARCHING:
                    PreTagSearchOneTagThreadProc();
                    break;

                case Operation.TAG_EXESEARCHING: // Enable get battery level by interrupt
                    CurrentOperation = Operation.TAG_SEARCHING;
                    _deviceHandler.battery.EnableAutoBatteryLevel();
                    ExeTagSearchOneTagThreadProc();
                    break;

                case Operation.TAG_SELECTED:
                    TagSelected();
                    break;

                case Operation.TAG_SELECTEDDYNQ:
                    TagSelectedDYNQ();
                    break;

                case Operation.TAG_FASTSELECTED:
                    FastTagSelected();
                    break;

                case Operation.TAG_GENERALSELECTED:
                    SetMaskThreadProc();
                    break;

                case Operation.TAG_PREFILTER:
                    PreFilter();
                    break;

                case Operation.TAG_READ:
                    ReadThreadProc();
                    break;

                case Operation.TAG_READ_PC:
                    TagReadPCThreadProc();
                    break;

                case Operation.TAG_READ_EPC:
                    TagReadEPCThreadProc();
                    break;

                case Operation.TAG_READ_ACC_PWD:
                    TagReadAccPwdThreadProc();
                    break;

                case Operation.TAG_READ_KILL_PWD:
                    TagReadKillPwdThreadProc();
                    break;

                case Operation.TAG_READ_TID:
                    TagReadTidThreadProc();
                    break;

                case Operation.TAG_READ_USER:
                    TagReadUsrMemThreadProc();
                    break;

                case Operation.TAG_WRITE:
                    WriteThreadProc();
                    break;

                case Operation.TAG_WRITE_PC:
                    TagWritePCThreadProc();
                    break;

                case Operation.TAG_WRITE_EPC:
                    TagWriteEPCThreadProc();
                    break;

                case Operation.TAG_WRITE_ACC_PWD:
                    TagWriteAccPwdThreadProc();
                    break;

                case Operation.TAG_WRITE_KILL_PWD:
                    TagWriteKillPwdThreadProc();
                    break;

                case Operation.TAG_WRITE_USER:
                    TagWriteUsrMemThreadProc();
                    break;

                case Operation.TAG_BLOCK_WRITE:
                    BlockWriteThreadProc();
                    break;

                case Operation.TAG_LOCK:
                    TagLockThreadProc();
                    break;

                case Operation.TAG_BLOCK_PERMALOCK:
                    TagBlockLockThreadProc();
                    break;

                case Operation.TAG_KILL:
                    TagKillThreadProc();
                    break;

                case Operation.TAG_AUTHENTICATE:
                    TagAuthenticateThreadProc();
                    break;

                case Operation.TAG_READBUFFER:
                    TagReadBufferThreadProc();
                    break;

                case Operation.TAG_UNTRACEABLE:
                    TagUntraceableThreadProc();
                    break;

                case Operation.FM13DT_READMEMORY:
                    FM13DTReadMemoryThreadProc();
                    break;

                case Operation.FM13DT_WRITEMEMORY:
                    FM13DTWriteMemoryThreadProc();
                    break;

                case Operation.FM13DT_READREGISTER:
                    FM13DTReadRegThreadProc();
                    break;

                case Operation.FM13DT_WRITEREGISTER:
                    FM13DTWriteRegThreadProc();
                    break;

               case Operation.FM13DT_AUTH:
                    FM13DTAuthThreadProc();
                    break;

                case Operation.FM13DT_GETTEMP:
                    FM13DTGetTempThreadProc();
                    break;

                case Operation.FM13DT_STARTLOG:
                    FM13DTStartLogThreadProc();
                    break;

                case Operation.FM13DT_STOPLOG:
                    FM13DTStopLogChkThreadProc();
                    break;

                case Operation.FM13DT_DEEPSLEEP:
                    FM13DTDeepSleepThreadProc();
                    break;

                case Operation.FM13DT_OPMODECHK:
                    FM13DTOpModeChkThreadProc();
                    break;

                case Operation.FM13DT_INITIALREGFILE:
                    FM13DTInitialRegFileThreadProc();
                    break;

                case Operation.FM13DT_LEDCTRL:
                    FM13DTLedCtrlThreadProc();
                    break;

                case Operation.QT_COMMAND:
                    QT_CommandProc();
                    break;

                default:
                    return Result.NOT_SUPPORTED;
            }
            return Result.OK;
        }
    }
}
