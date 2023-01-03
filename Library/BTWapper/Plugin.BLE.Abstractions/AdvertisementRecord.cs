using Plugin.BLE.Abstractions.Extensions;


namespace Plugin.BLE.Abstractions {
    public enum AdvertisementRecordType {
        Flags = 0x01,

        UuidsIncomple16Bit = 0x02,

        UuidsComplete16Bit = 0x03,

        UuidsIncomplete32Bit = 0x04,

        UuidCom32Bit = 0x05,

        UuidsIncomplete128Bit = 0x06,

        UuidsComplete128Bit = 0x07,

        ShortLocalName = 0x08,

        CompleteLocalName = 0x09,

        TxPowerLevel = 0x0A,

        Deviceclass = 0x0D,

        SimplePairingHash = 0x0E,

        SimplePairingRandomizer = 0x0F,

        DeviceId = 0x10,

        SecurityManager = 0x11,

        SlaveConnectionInterval = 0x12,

        SsUuids16Bit = 0x14,

        SsUuids128Bit = 0x15,

        ServiceData = 0x16,

        PublicTargetAddress = 0x17,

        RandomTargetAddress = 0x18,

        Appearance = 0x19,

        DeviceAddress = 0x1B,

        LeRole = 0x1C,

        PairingHash = 0x1D,

        PairingRandomizer = 0x1E,

        SsUuids32Bit = 0x1F,

        ServiceDataUuid32Bit = 0x20,

        ServiceData128Bit = 0x21,

        SecureConnectionsConfirmationValue = 0x22,

        SecureConnectionsRandomValue = 0x23,

        Information3DData = 0x3D,

        ManufacturerSpecificData = 0xFF,

        IsConnectable = 0xAA
    }

    public class AdvertisementRecord {
        public AdvertisementRecordType Type { get; private set; }
        public byte[] Data { get; private set; }

        public AdvertisementRecord(AdvertisementRecordType type, byte[] data) {
            Type = type;
            Data = data;
        }

        public override string ToString() {
            return string.Format("Adv rec [Type {0}; Data {1}]", Type, Data.ToHexString());
        }

    }
}