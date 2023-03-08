using System;
using System.Collections.Generic;
using System.Text;
using CSLibrary.Barcode.Structures;
using CSLibrary.Barcode.Constants;


namespace CSLibrary.Barcode {
    public class BarcodeEventArgs : EventArgs
    {
        private MessageBase m_msg = null;
        private MessageType m_type = MessageType.ERR_MSG;
        private string m_error = String.Empty;

        /// <param name="type"></param>
        /// <param name="msg"></param>
        public BarcodeEventArgs(MessageType type, MessageBase msg) {
            m_type = type;
            m_msg = msg;
        }

        /// <param name="type"></param>
        /// <param name="error"></param>
        public BarcodeEventArgs(MessageType type, string error) {
            m_type = type;
            m_error = error;
        }

        public MessageBase Message {
            get { return m_msg; }
        }

        public MessageType MessageType {
            get { return m_type; }
        }

        public string ErrorMessage {
            get { return m_error; }
        }
    }

    public class BarcodeStateEventArgs : EventArgs
    {
        private BarcodeState m_state = BarcodeState.IDLE;

        /// <param name="state"></param>
        public BarcodeStateEventArgs(BarcodeState state)
        {
            m_state = state;
        }

        public BarcodeState State
        {
            get { return m_state; }
        }

    }
}
