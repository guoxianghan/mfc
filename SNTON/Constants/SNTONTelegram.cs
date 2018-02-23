using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SNTON.Constants
{
    /// <summary>
    /// static class containing standard telegram
    /// </summary>
    public static class SNTONTelegram
    {
        /// <summary>
        /// FedEx PLC telegram ID
        /// </summary>
        public class FedExTelegramID
        {

            /// <summary>
            /// Destination Request
            /// </summary>
            public const string DestinationRequest = "01";

            /// <summary>
            /// Destination Reply
            /// </summary>
            public const string DestinationReply = "51";

            /// <summary>
            /// Final Sort Report
            /// </summary>
            public const string SortReport = "11";

            /// <summary>
            /// MFC state Report
            /// </summary>
            public const string MfcStateReport = "92";


            /// <summary>
            /// PLC sent us the watchdog reply
            /// </summary>
            public const string WatchdogReplyFromPLC = "98";

        }
        /// <summary>
        /// Please make sure to use the same names as specified in the .XML!
        /// </summary>
        public class TelegramFields
        {
            /// <summary>
            /// PIC
            /// </summary>
            public const string TelegramId = "TELEGRAMID";

            /// <summary>
            /// PIC
            /// </summary>
            public const string Pic = "PIC";

            /// <summary>
            /// HostPIC 
            /// </summary>
            public const string HostPic = "HostPIC";

            /// <summary>
            /// Barcode
            /// </summary>
            public const string Barcode = "Barcode";

            /// <summary>
            /// BarcodeScannerDataState 
            /// </summary>
            public const string BarcodeType = "BarcodeType";

            /// <summary>
            /// Additional Barcode 01
            /// </summary>
            public const string Barcode01 = "AdditionalBarcode01";

            /// <summary>
            /// Additional Barcode 01 type 
            /// </summary>
            public const string BarcodeType01 = "AdditionalBarcodeType01";

            /// <summary>
            /// Additional Barcode 02
            /// </summary>
            public const string Barcode02 = "AdditionalBarcode02";

            /// <summary>
            /// Additional Barcode 01 type 
            /// </summary>
            public const string BarcodeType02 = "AdditionalBarcodeType02";

            /// <summary>
            /// SourceLocation 
            /// </summary>
            public const string ILoc = "SourceLocation";

            /// <summary>
            /// Equipment Id
            /// </summary>
            public const string EquipmentId = "EquipmentId";

            /// <summary>
            /// the location which request came from
            /// </summary>
            public const string RequestedDestination = "RequestedDestination";

            /// <summary>
            /// the main destination which MFC reply to PLC
            /// </summary>
            public const string DestinationLocation = "DestinationLocation";

            /// <summary>
            /// the real destination to which the parcel has/will be sorted.
            /// </summary>
            public const string ActualDestination = "ActualDestination";

            /// <summary>
            /// the Divert Code for ActualDestination .
            /// </summary>
            public const string DivertCode = "DivertCode";

            /// <summary>
            /// Length field
            /// </summary>
            public const string Length = "Length";

            /// <summary>
            /// Width field
            /// </summary>
            public const string Width = "Width";

            /// <summary>
            /// Height field
            /// </summary>
            public const string Height = "Height";

            /// <summary>
            /// the unit of length,Width and Height
            /// </summary>
            public const string LwhUnit = "LWHUnit";

            /// <summary>
            /// 4 numeric characters. numbers of the Carrier 
            /// </summary>
            public const string CarrierId = "CarrierID";

            /// <summary>
            /// In case the Original Destination could not be reached for whatever reason, the
            /// PLC is told to use this destination next.
            /// </summary>
            public const string AltDestination1 = "AlternativeDestination01";

            /// <summary>
            /// the Divert Code for AltDestination1 .
            /// </summary>
            public const string AltDivertCode01 = "SR_DivertCode01";

            /// <summary>
            /// In case the AltDestination1 could not be reached for whatever reason, the
            /// PLC is told to use this destination next.
            /// </summary>
            public const string AltDestination2 = "AlternativeDestination02";

            /// <summary>
            /// the Divert Code for AltDestination02 .
            /// </summary>
            public const string AltDivertCode02 = "SR_DivertCode02";

            /// <summary>
            /// In case the previous AltDestination could not be reached for whatever reason, the
            /// PLC is told to use this destination next.
            /// </summary>
            public const string AltDestination3 = "AlternativeDestination03";

            /// <summary>
            /// the Divert Code for AltDestination03 .
            /// </summary>
            public const string AltDivertCode03 = "SR_DivertCode03";

            /// <summary>
            /// In case the previous AltDestination could not be reached for whatever reason, the
            /// PLC is told to use this destination next.
            /// </summary>
            public const string AltDestination4 = "AlternativeDestination04";

            /// <summary>
            /// the Divert Code for AltDestination04 .
            /// </summary>
            public const string AltDivertCode04 = "SR_DivertCode04";


            /// <summary>
            /// In case the Original Destination could not be reached for whatever reason, the
            /// PLC is told to use this destination next.
            /// </summary>
            public const string SortReportAltDestination1 = "SR_AlternativeDestination01";

            /// <summary>
            /// In case the Original Destination could not be reached for whatever reason, the
            /// PLC is told to use this destination next.
            /// </summary>
            public const string SortReportAltDestination2 = "SR_AlternativeDestination02";

            /// <summary>
            /// In case the Original Destination could not be reached for whatever reason, the
            /// PLC is told to use this destination next.
            /// </summary>
            public const string SortReportAltDestination3 = "SR_AlternativeDestination03";

            /// <summary>
            /// In case the Original Destination could not be reached for whatever reason, the
            /// PLC is told to use this destination next.
            /// </summary>
            public const string SortReportAltDestination4 = "SR_AlternativeDestination04";

            /// <summary>
            /// operation or Intercept
            /// </summary>
            public const string OperIntercept = "CusResReq";

            /// <summary>
            /// MFC status report to PLC
            /// </summary>
            public const string StateReportedToPLC = "StateReportedToPLC";

          

        }

        public enum SourceZone
        {
            /// <summary>
            /// Sss
            /// </summary>
            [EnumMember]
            Host
        }

        /// <summary>
        /// Determines the status of host
        /// </summary>
        [DataContract]
        public enum HostConnStatus
        {
            /// <summary>
            /// It's off
            /// </summary>
            [EnumMember]
            Off = 0,

            /// <summary>
            /// It's on
            /// </summary>
            [EnumMember]
            On,
        }
    }
}
