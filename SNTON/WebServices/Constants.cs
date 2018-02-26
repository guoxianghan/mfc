using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SNTON.Constants;

namespace SNTON.WebServices
{
    public class Constants
    {
        /// <summary>
        /// Each of these messages are just string constants which will point to the
        /// appropriate language text in several supported languages (FedEx: English, Chinese).
        /// It is recommended to have a prefix "_P" and a number following the prefix specifying
        /// the number of arguments which have to be taken into consideration. It is good style to give 
        /// a hint of the number of arguments which are filled out as well for this constant:
        /// A value of "..._P2" means, that two  arguments have to be replaced in the string, like e.g.
        /// "This {0} a {1}". Argument[0] may be "is" and Argument[1] would be "Test" resulting
        /// in the final message to be displayed as "This is a test".
        /// Although the number of arguments can be counted in the array of arguments, it may make
        /// it easier to react on later changes to the errortext so that one can easily see that
        /// something is wrong by just looking into the stored texts.
        /// </summary>
        public class ErrorMessages
        {

            /// <summary>
            /// General Error Message with one parameter (exceptionname).
            /// This constant has to be implemented for each supported language in the BFW database 
            /// as a language text as well.
            /// 
            /// </summary>
            public const string ExceptionOccured_P1 = "ERROR.EXCEPTION.DETAILS_P1";
            
            /// <summary>
            /// General Error Message with two parameters (exceptionname and info).
            /// This constant has to be implemented for each supported language in the BFW database 
            /// as a language text as well.
            /// 
            /// </summary>
            public const string ExceptionOccured_P2 = "ERROR.EXCEPTION.DETAILS_P2";

            // Please add all other messages here and don't forget to add them as language texts

            /// <summary>
            /// Message: 'Request is missing;
            /// </summary>
            public const string RequestMissing = "IDS_REQUEST_MISSING";

            /// <summary>
            /// Message: 'Request is invalid;
            /// </summary>
            public const string RequestInvalid = "IDS_REQUEST_INVALID";

        }

        /// <summary>
        /// Same like ErrorMessages, but grouped by severity.
        /// </summary>
        public class WarningMessages
        {

            /// <summary>
            /// General Warning Message with one parameter.
            /// This constant has to be implemented for each supported language in the BFW database 
            /// as a language text as well.
            /// 
            /// </summary>
            public const string WarningOccured_P1 = "WARN.GENERAL_P1";

            /// <summary>
            /// General Warning Message with two parameters.
            /// This constant has to be implemented for each supported language in the BFW database 
            /// as a language text as well.
            /// 
            /// </summary>
            public const string WarningOccured_P2 = "WARN.GENERAL_P2";

            // Please add all other messages here and don't forget to add them as language texts

        }

        /// <summary>
        /// Constants for IDS operator actions used in FX_OPERATIONLOG database table in 
        /// IDS_ACTION field
        /// </summary>
        public class OperatorActionsIds
        {
            /// <summary>
            /// Message: "User login from IP address"
            /// </summary>
            public const string Login = "IDS_OPER_LOGIN";

            /// <summary>
            /// Message: "User logout from machine: {0}"
            /// </summary>
            public const string Logout = "IDS_OPER_LOGOUT";

            /// <summary>
            /// Message: "Interception list imported from file: {0}"
            /// </summary>
            public const string InterceptionImport = "IDS_OPER_INTERCEPTION_IMPORT";

            /// <summary>
            /// Message: "Self-pickup list imported"
            /// </summary>
            public const string SelfPickImport = "IDS_OPER_SELFPICKUP_IMPORT";

            /// <summary>
            /// Message: "Selection and hadling code priority tables imported"
            /// </summary>
            public const string SelHandlingImport = "IDS_OPER_SELHANDLING_CODE_IMPORT";

            /// <summary>
            /// Message: "Copy original sort plan {0} to new sort plan {1}"
            /// </summary>
            public const string CopySortplan = "IDS_OPER_COPY_SORTPLAN";

            /// <summary>
            /// Message: "Sortplan {0} was deleted"
            /// </summary>
            public const string DeleteSortplan = "IDS_OPER_DELETE_SORTPLAN";

            /// <summary>
            /// Message: "Delete sorting plan {0} rule code {1}"
            /// </summary>
            public const string DeleteSortplanRule = "IDS_OPER_DELETE_SORTPLAN_RULE";

            /// <summary>
            /// Message: "Sortplan {0} is activated"
            /// </summary>
            public const string ActivateSortplan = "IDS_OPER_ACTIVATE_SORTPLAN";

            /// <summary>
            /// Message: "Sortplan {0} is deactivated"
            /// </summary>
            public const string DeactivateSortplan = "IDS_OPER_DEACTIVATE_SORTPLAN";

            /// <summary>
            /// Message: "Sortplan {0} is created"
            /// </summary>
            public const string CreateSortplan = "IDS_OPER_CREATE_SORTPLAN";

            /// <summary>
            /// Message: "Sortplan rule chnaged"
            /// </summary>
            public const string ChangeSortplanRule = "IDS_OPER_CHANGE_SORTPLAN_RULE";

            /// <summary>
            /// Message: "Changed exception chute for sorter: '{0} and exception type: '{1}'."
            /// </summary>
            public const string ChangeExceptionChute = "IDS_OPER_CHANGE_EXCEPTION_CHUTE";

            /// <summary>
            /// Message: "User account:{0} created"
            /// </summary>
            public const string AddUserAccount = "IDS_OPER_ADD_USER_ACCOUNT";

            /// <summary>
            /// Message: "Deleted User account:{0} "
            /// </summary>
            public const string DeleteUserAccount = "IDS_OPER_DELETE_USER_ACCOUNT";

            /// <summary>
            /// Message: "User account:{0} updated "
            /// </summary>
            public const string UpdateUserAccount = "IDS_OPER_UPDATE_USER_ACCOUNT";

            /// <summary>
            /// Message: "Changed sorter {0} self pickup list {1} description from {2} to {3}"
            /// </summary>
            public const string ChangeSelfPickupDescription = "IDS_OPER_CHANGE_SELFPICKUP_DESCRIPTION";

            /// <summary>
            /// Message: "Changed sorter {0} self pickup list {1} chute from {2} to {3}"
            /// </summary>
            public const string ChangeSelfPickupChute = "IDS_OPER_CHANGE_SELFPICKUP_CHUTE";

            /// <summary>
            /// Message: "Edit sort plan {0} from {1} to {2}"
            /// </summary>
            public const string EditSortPlanName = "IDS_OPER_EDIT_SORTPLAN_NAME";

            /// <summary>
            /// Message: "Changed sort plan {0} rule code from {1} to {2}"
            /// </summary>
            public const string ChangeSortPlanRuleCode = "IDS_OPER_CHANGE_SORTPLAN_RULECODE";

            /// <summary>
            /// Message: "Changed sort plan {0} extra rule code from {1} to {2}"
            /// </summary>
            public const string ChangeSortPlanExtraRuleCode = "IDS_OPER_CHANGE_SORTPLAN_EXTRARULECODE";

            /// <summary>
            /// Message: "Changed sort plan {0} rule code {1} chute from {2} to {3}"
            /// </summary>
            public const string ChangeSortPlanRuleChute = "IDS_OPER_CHANGE_SORTPLAN_RULECHUTE";

            /// <summary>
            /// Message: "Create sort plan {0} rule code: {1}, extra rule code: {2}, assign chute: {3}"
            /// </summary>
            public const string CreateSortPlanRule = "IDS_OPER_CREATE_SORTPLAN_RULE";
        }

        /// <summary>
        /// Constants for IDS used in Interception import page webservice
        /// </summary>
        public class ImportPagesIds
        {
            /// <summary>
            /// Message: "Invalid line format"
            /// </summary>
            public const string InvalidLine = "IDS_INTIMPORT_INVALID_LINE";

            /// <summary>
            /// Message: "Invalid barcode"
            /// </summary>
            public const string InvalidBarcode = "IDS_INTIMPORT_INVALID_BARCODE";

            /// <summary>
            /// Message: "Invalid time format: {0}"
            /// </summary>
            public const string InvalidTimeFormat = "IDS_INTIMPORT_INVALID_TIME_FORMAT";

            /// <summary>
            /// Message: "End time in the past"
            /// </summary>
            public const string EndTimeInPast = "IDS_INTIMPORT_END_TIME_IN_PAST";

            /// <summary>
            /// Message: "End time too much in future"
            /// </summary>
            public const string EndTimeInFuture = "IDS_INTIMPORT_END_TIME_IN_FUTURE";

            /// <summary>
            /// Message: "End time is older then start time"
            /// </summary>
            public const string EndOlderThenStartTime = "IDS_INTIMPORT_END_OLDER_THEN_START";

            /// <summary>
            /// Message: "No records in the file"
            /// </summary>
            public const string NoRecords = "IDS_INTIMPORT_NO_RECORDS";

            /// <summary>
            /// Message: "Too many records. maximum allowed are: {0} records, there are: {1}"
            /// </summary>
            public const string TooManyRecords = "IDS_INTIMPORT_TOO_MANY_RECORDS";

            /// <summary>
            /// Message: "Time offset out of range"
            /// </summary>
            public const string TimeOffsetError = "IDS_INTIMPORT_TIME_OFFSET_ERROR";

            /// <summary>
            /// Message: "Invalid interception import file header: '{0}'"
            /// </summary>
            public const string InvalidHeader = "IDS_INTIMPORT_INVALID_HEADER";

            /// <summary>
            /// Message: "Duplicated barcode with line {0}"
            /// </summary>
            public const string DuplicatedBarcode = "IDS_INTIMPORT_DUPLICATED_BARCODE";

            /// <summary>
            /// Message: "No valid records to import"
            /// </summary>
            public const string NoValidRecords = "IDS_INTIMPORT_NO_VALID_RECORDS";

            /// <summary>
            /// Message: "Error to delete active interception list"
            /// </summary>
            public const string DeleteError = "IDS_INTIMPORT_DELETE_ERROR";

            /// <summary>
            /// Message: "Invalid sorter"
            /// </summary>
            public const string InvalidSorter = "IDS_SELFPICKUP_INVALID_SORTER";

            /// <summary>
            /// Message: "Invalid self-pickup list"
            /// </summary>
            public const string InvalidSelfPickupList = "IDS_SELFPICKUP_INVALID_SELF_PICKUP_LIST";

            /// <summary>
            /// Message: "No selected chutes."
            /// </summary>
            public const string NoSelectedChutes = "IDS_SELFPICKUP_NO_SEL_CHUTES";

            /// <summary>
            /// Message: "Not found selected chute with ID:{0}."
            /// </summary>
            public const string SelectedChutesNotFound = "IDS_SELFPICKUP_SEL_CHUTE_NOT_FOUND";

            /// <summary>
            /// Message: "Exceed the max pickup chute count {0}"
            /// </summary>
            public const string ExceedChuteCount = "IDS_EXCEED_CHUTECOUNT";
        }

        /// <summary>
        /// Constants for IDS used in report pages webservices
        /// </summary>
        public class ReportsPageIds
        {
            /// <summary>
            /// Message: "End time is older then start time"
            /// </summary>
            public const string EndOlderThenStartTime = "IDS_REPORTS_END_OLDER_THEN_START";
        }


        /// <summary>
        /// Constants for IDS used in Selection & Handling import page webservice
        /// </summary>
        public class SelHandleImportPageIds
        {
            /// <summary>
            /// Message: "No records in the file"
            /// </summary>
            public const string NoRecords = "IDS_SELHAND_NO_RECORDS";

            /// <summary>
            /// Message: "Invalid interception import file header: '{0}'"
            /// </summary>
            public const string InvalidHeader = "IDS_SELHAND_INVALID_HEADER";

            /// <summary>
            /// Message: "No valid records to import"
            /// </summary>
            public const string NoValidRecords = "IDS_SELHAND_NO_VALID_RECORDS";

            /// <summary>
            /// Message: "Unsupported code type: {0}"
            /// </summary>
            public const string UnsupportedCodeType = "IDS_SELHAND_UNSUPPORTED_CODE_TYPE";

            /// <summary>
            /// Message: "Invalid line format"
            /// </summary>
            public const string InvalidLine = "IDS_SELHAND_INVALID_LINE";

            /// <summary>
            /// Message: "Invalid priority format"
            /// </summary>
            public const string InvalidPriority = "IDS_SELHAND_INVALID_PRIORITY";

            /// <summary>
            /// Message: "Invalid code format"
            /// </summary>
            public const string InvalidCode = "IDS_SELHAND_INVALID_CODE";

            /// <summary>
            /// Message: "Invalid code type format"
            /// </summary>
            public const string InvalidCodeType = "IDS_SELHAND_INVALID_CODE_TYPE";

            /// <summary>
            /// Message: "Duplicated code"
            /// </summary>
            public const string DuplicatedCode = "IDS_SELHAND_DUPLICATED_CODE";

            /// <summary>
            /// Message: "Invalid priority order"
            /// </summary>
            public const string InvalidPriorityOrder = "IDS_SELHAND_INVALID_PRIORITY_ORDER";

            /// <summary>
            /// Message: "Exceeded maximum number of selection codes. Allowed {0}, there are {1}."
            /// </summary>
            public const string ExceededSelection = "IDS_SELHAND_EXCEED_SEL";

            /// <summary>
            /// Message: "Exceeded maximum number of handling codes. Allowed {0}, there are {1}."
            /// </summary>
            public const string ExceededHandling = "IDS_SELHAND_EXCEED_HAND";

            /// <summary>
            /// Message: "The code {0} is existing in active sort plan
            /// </summary>
            public const string ExistCodeInActivePlan = "IDS_EXISTCODE_IN_ACTIVEPLAN";
        }

        /// <summary>
        /// Constants for IDS used in webservices for Sortingplans pages 
        /// </summary>
        public class SortingplanPageIds
        {
            /// <summary>
            /// Message: "Sortingplan does not exist"
            /// </summary>
            public const string InvalidSortplan = "IDS_SORTPLAN_INVALID";

            /// <summary>
            /// Message: "Sortingplan is active"
            /// </summary>
            public const string SortplanIsActive = "IDS_SORTPLAN_IS_ACTIVE";

            /// <summary>
            /// Message: "Sortingplan is not active"
            /// </summary>
            public const string SortplanNotActive = "IDS_SORTPLAN_NOT_ACTIVE";

            /// <summary>
            /// Message: "Sortingplan is not deleted"
            /// </summary>
            public const string SortplanNotDeleted = "IDS_SORTPLAN_NOT_DELETED";

            /// <summary>
            /// Message: "Exists sortingplan with same name"
            /// </summary>
            public const string SortplanWithSameName = "IDS_SORTPLAN_WITH_SAME_NAME";

            /// <summary>
            /// Message: "Sorter with ID: {0} does not exist"
            /// </summary>
            public const string InvalidSorterId = "IDS_SORTERID_INVALID";

            /// <summary>
            /// Message: "Ruletype is missing"
            /// </summary>
            public const string RuleTypeMissing = "IDS_RULETYPE_MISSING";

            /// <summary>
            /// Message: "The sum of sortingplan count is larger than {0}"
            /// </summary>
            public const string SortplanExceedSel = "IDS_SORTPLAN_EXCEED_SEL";

            /// <summary>
            /// Message: "The sum of rule chute count is larger than {0}"
            /// </summary>
            public const string RuleChuteExceedSel = "IDS_RULECHUTE_EXCEED_SEL";

            /// <summary>
            /// Message: "Invalid sorting plan rule"
            /// </summary>
            public const string InvalidSortplanRule = "IDS_SORTPLANRULE_INVALID";

            /// <summary>
            /// Message: "Failed to delete Sorting Plan Rule"
            /// </summary>
            public const string DeleteSortplanRuleFailed = "IDS_DELETE_SORTPLANRULE_FAILED";

            /// <summary>
            /// Message: "Sorter chute side must keep same"
            /// </summary>
            public const string SorterChuteSideKeepSame = "IDS_SORTER_CHUTE_KEEPSAME";

            /// <summary>
            /// Rule chute count is at least {0}
            /// </summary>
            public const string RuleChuteCountMin = "IDS_RULECHUTE_COUNT_MIN";


            /// <summary>
            /// The destination key pair {0}, {1} is not unique, it already exists for sorter {2}
            /// </summary>
            public const string DestKeyPairNotUnique = "IDS_DEST_KEY_PAIR_NNIQUE";

            /// <summary>
            /// The destination code {0} has already existed for sorter {1}.
            /// </summary>
            public const string DestCodeHasExisting = "IDS_DESTCODE_EXISTING";

            /// <summary>
            /// Message: "ExtraRuleCode is missing"
            /// </summary>
            public const string ExtraRuleCodeMissing = "IDS_EXTRA_RULECODE_MISSING";

            /// <summary>
            /// Message: "Special parcale rule is missing"
            /// </summary>
            public const string SpecialParcaleRuleMissing = "IDS_SPEPICALPARCEL_RULE_MISSING";

            /// <summary>
            /// Message: "Chute {0} has been used as exception chute"
            /// </summary>
            public const string ChuteAsException = "IDS_CHUTE_ASEXCEPTION";

            /// <summary>
            /// Message: "Sort plan name is missing"
            /// </summary>
            public const string SortPlanNameMissing = "IDS_SORTPLAN_NAME_MISSING";

            /// <summary>
            /// Message: "Chute {0} has been used in other custom clearance status
            /// </summary>
            public const string ChuteUsedInCustomClearStatus = "IDS_CHUTE_USED_CUSTOME_CLEARSTATUS";

            /// <summary>
            /// Message: "Chute {0} has been used as self pickup chute"
            /// </summary>
            public const string ChuteAsSelfPickup = "IDS_CHUTE_ASSELFPICKUP";

            /// <summary>
            /// Message: "SelCodes ({0})/HandCode ({1}) are missing in priority table."
            /// </summary>
            public const string SelHandCodesMiss = "IDS_SELHANDCODES_MISS_IN_PRIORITY";

            /// <summary>
            /// Message: "SelCodes ({0}) are missing in priority table."
            /// </summary>
            public const string SelCodesMiss = "IDS_SELCODES_MISS_IN_PRIORITY";

            /// <summary>
            /// Message: "HandCodes ({0}) are missing in priority table."
            /// </summary>
            public const string HandCodesMiss = "IDS_HANDCODES_MISS_IN_PRIORITY";
        }

        public class ExceptionChutesIds
        {
            /// <summary>
            /// Message: "Invalid chute no: {0}."
            /// </summary>
            public const string InvalidChute = "IDS_INVALID_CHUTE";

            /// <summary>
            /// Message: "The chute {0} has been used as sorter's acitve sort plan rule chute."
            /// </summary>
            public const string ChuteAsRuleChute = "IDS_CHUTE_ASRULECHUTE";

            /// <summary>
            /// Message: "Invalid exception type: '{0}'."
            /// </summary>
            public const string InvalidExceptionType = "IDS_INVALID_EXCEPTION_TYPE";

            /// <summary>
            /// Message: "Chute {0} has been used as self pickup chute"
            /// </summary>
            public const string ChuteAsSelfPickup = "IDS_CHUTE_ASSELFPICKUP";
        }

        /// <summary>
        /// Constants for IDS used in webservices for Selfpickup pages 
        /// </summary>
        public class SelfPickupPageIds
        {
            /// <summary>
            /// Message: "Pick up list description is missing"
            /// </summary>
            public const string DescriptionMissing = "IDS_DESCRIPTION_MISSING";

            /// <summary>
            /// Message: "Chute {0} has been used as exception chute"
            /// </summary>
            public const string ChuteAsException = "IDS_CHUTE_ASEXCEPTION";

            /// <summary>
            /// Message: "The chute {0} has been used as sorter's acitve sort plan rule chute."
            /// </summary>
            public const string ChuteAsRuleChute = "IDS_CHUTE_ASRULECHUTE";
        }

        public class ExceptionTypeIds
        {
            public const string NoValidBarcode = "IDS_NO_VALID_BARCODE";
            public const string NoParcelInformation = "IDS_NO_PARCEL_INFORMATION";
            public const string NoHandOrSelecAllocNotClr = "IDS_NO_HAND_OR_SELEC_ALLOC_NOT_CLR";
            public const string NoHandOrSelecAllocClr = "IDS_NO_HAND_OR_SELEC_ALLOC_CLR";
            public const string NoAllocationNotClr = "IDS_NO_ALLOCATION_NOT_CLR";
            public const string NoAllocationClr = "IDS_NO_ALLOCATION_CLR";
            public const string OutboundDiplomatic = "IDS_OUTBOUND_DIPLOMATIC";
            public const string Interception = "IDS_INTERCEPTION";
            public const string MaxRecirculation = "IDS_MAX_RECIRCULATION";
            public const string LlcFailDivert = "IDS_LLC_FAIL_DIVERT";
        }

        /// <summary>
        /// Connect exception type with IDS
        /// </summary>
        public static readonly Dictionary<string, string> ExceptionTypesIds = new Dictionary<string, string>()
        {
            
        }; 

    }
}
