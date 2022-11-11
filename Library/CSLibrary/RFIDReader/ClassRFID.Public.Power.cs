using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSLibrary.Constants;


namespace CSLibrary {
    public partial class RFIDReader {
        private uint m_oem_hipower = 0;

        public uint GetActiveMaxPowerLevel() {
            return 320;
        }

        public uint SelectedPowerLevel {
            get {
                uint pwrlvl = 0;
                GetPowerLevel(ref pwrlvl);
                return pwrlvl;
            }
        }

        public Result GetPowerLevel(ref uint pwrlvl) {
            MacWriteRegister(MACREGISTER.HST_ANT_DESC_SEL, 0);
            MacReadRegister(MACREGISTER.HST_ANT_DESC_RFPOWER, ref pwrlvl);
            return Result.OK;
        }

        public Result SetPowerLevel(uint pwrlevel, uint port = 0) {
            if (pwrlevel > 330) pwrlevel = 330;

            MacWriteRegister(MACREGISTER.HST_ANT_DESC_SEL, port);         // select antenna
            MacWriteRegister(MACREGISTER.HST_ANT_DESC_RFPOWER, pwrlevel);

            return Result.OK;
        }

        public Result SetPowerLevel(int pwrlevel, uint port = 0) {
            if (pwrlevel < 0)
                return Result.INVALID_PARAMETER;
            return SetPowerLevel((uint)pwrlevel, port);
        }

        public Result SetPowerLevel(UInt32 [] pwrlevel) {
            Result r;

            for (uint cnt = 0; cnt < pwrlevel.Length; cnt++)
                if ((r = SetPowerLevel(pwrlevel[cnt], cnt)) != Result.OK)
                    return r;

            return Result.OK;
        }

        private uint GetSoftwareMaxPowerLevel(RegionCode region) {
            // MAX Power 32dB
            if ((m_oem_hipower == 1) ||
                (m_oem_machine == Machine.CS468INT) ||
                (m_oem_machine == Machine.CS463) ||
                (m_oem_machine == Machine.CS469) ||
                (region == RegionCode.IN) ||
                (region == RegionCode.G800) ||
                (m_oem_machine == Machine.CS209) ||
                (m_oem_machine == Machine.CS103) ||
                (m_oem_machine == Machine.CS108)
                )
                return 320;

            // MAX Power 27.5dB
            if ((m_oem_machine == Machine.CS101 && region == RegionCode.ETSI) ||
                (m_oem_machine == Machine.CS203 && region == RegionCode.JP))
                return 275;

            return 300;
        }

        /// <param name="numberofPower"></param>
        /// <param name="power"></param>
        /// <param name="dwell"></param>
        /// <returns></returns>
        public Result SetPowerSequencing(int numberofPower, uint[] power = null, uint[] dwell = null) {
            if (numberofPower == 0) {
                try {
                    int i;
                    for (i = 0; i < m_AntennaList.Count; i++) {
                        if (m_AntennaList[i].PowerLevel > GetSoftwareMaxPowerLevel(m_save_region_code))
                            m_AntennaList[i].PowerLevel = GetSoftwareMaxPowerLevel(m_save_region_code);

                        SetAntennaPortStatus((uint)i, m_AntennaList[i].AntennaStatus);
                        SetAntennaPortConfiguration((uint)i, m_AntennaList[i].AntennaConfig);
                    }

                    for (; i < 16; i++) {
                        AntennaPortSetState((uint)i, AntennaPortState.DISABLED);
                    }
                }
                catch (Exception ex) {
                    CSLibrary.Debug.WriteLine("Set Antenna Configuration Fail : " + ex.Message);
                }
                return Result.OK;
            }

            // WILL NOT RUN WITH numberofPower=0
            if (power == null || dwell == null || power.Length < numberofPower || dwell.Length < numberofPower) {
                return Result.INVALID_PARAMETER;
            }

            {
                uint i;

                for (i = 0; i < numberofPower; i++) {
                    AntennaPortSetState((uint)i, AntennaPortState.ENABLED);
                    SetPowerLevel(power[i], i);
                    SetInventoryDuration(dwell[i], i);
                }

                for (; i < 16; i++) {
                    AntennaPortSetState((uint)i, AntennaPortState.DISABLED);
                }
                
                // SetInventoryDuration(dwell[0], 0);
                // SetInventoryDuration(dwell[1], 1);
            }

            return Result.OK;
        }

    }
}
