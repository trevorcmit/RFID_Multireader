using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSLibrary.Constants;


namespace CSLibrary {
    public partial class RFIDReader {
        public bool IsHoppingChannelOnly {
            get { return m_oem_freq_modification_flag != 0x00; }
        }

        public bool IsFixedChannelOnly {
            get { return (m_save_country_code == 1 | m_save_country_code == 3 | m_save_country_code == 8 | m_save_country_code == 9); }
        }

        public bool IsFixedChannel {
            get { { return m_save_fixed_channel; } }
        }

        /// <returns>Result</returns>
        public Result GetCountryCode(ref uint code) {
            code = m_save_country_code;

            if (code < 0 || code > 8) return Result.INVALID_OEM_COUNTRY_CODE;

            return Result.OK;
        }

        public List<RegionCode> GetActiveRegionCode() {
            return m_save_country_list;
        }

        public RegionCode SelectedRegionCode {
            get { return m_save_region_code; }
        }

    }
}
