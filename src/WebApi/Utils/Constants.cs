using System.Net.NetworkInformation;

namespace WebApi.Utils
{
    public class Constants
    {

        // Custom
        public static int VIDEO_FRAME_HEADER_LENGTH = 40;
        public static int SOCKET_READ_COUNT = 1024 * 1024 * 1;
        public static int SOCKET_RECV_BUFFER_SIZE = 1024 * 1024 * 1;

        public static int VIDEO_FILE_LIMIT = 1024 * 1024 * 10;

        public static ushort SMART_CODE = 28573;
        public static byte MEDIA_CLIENT_MOBILE = 4; /* Mobile Client */
        public static byte MAX_REQ_FIELD_VALUES = 6;
        public static byte CMD_SOCKET_TIMEOUT = 30;
        public static byte STREAM_SOCKET_TIMEOUT = 30;
        public static byte MAX_RSP_STR_BUF = 100;
        public static byte MAX_RSP_STR_LEN = 13; /* STRLEN of "{RPL_CMD&00&}" */
        public static ushort DVR_MAGIC_CODE = 511;

        /* MediaQueue Related Constants */
        public static byte FSH_LEN = 40;
        public static int MAX_QUEUE_SIZE = 7000000; /* Approx. 7 MB. */
        public static int MAX_QUEUE_DISCARD_SIZE = MAX_QUEUE_SIZE / 4;

        public static int MAX_QUEUE_UPPER_LIMIT = 5000000; /* Approx. 5 MB. */
        public static int MAX_QUEUE_LOWER_LIMIT = 2500000; /* Approx. 2.5 MB. */

        /* MxDataSource Related Constants */
        public static ushort MAX_NO_OF_RS_SESSIONS = 256;
        public static byte MIN_STRM_DELAY_SEC = 2;

        /* MxLiveEngine Related Constants */
        public static ushort MAX_MEDIA_DATA_SOURCE_SIZE = 256;

        /* MxRPBEngine Related Constants */
        public static byte MAX_NO_OF_RPB_SESSION = 1;

        /* Playback Pause Command States */
        public static byte RPB_PAUSE_CMD_NOT_SENT = 0;
        public static byte RPB_PAUSE_CMD_SENT = 1;
        public static byte RPB_PAUSE_CMD_SENT_AND_WAIT = 2;

        /* NEW Separators */
        public static char SOM = (char)0x01; /* Start of Message Transmission */
        public static char EOM = (char)0x04; /* End of Message Transmission */
        public static char SOT = (char)0x02; /* Start of Table Transmission */
        public static char EOT = (char)0x03; /* End of Table Transmission */
        public static char SOI = (char)0x1C; /* Start of Index Transmission */ //28
        public static char EOI = (char)0x1D; /* End of Index Transmission */ // 29
        public static char FSP = (char)0x1E; /* Field Separator */ //30
        public static char FVS = (char)0x1F; /* Field & Value Separator */ //31

        public enum ENGINE_TYPE_e
        {
            ENGINE_TYPE_NONE = -1,
            ENGINE_TYPE_ES = 0,
            ENGINE_TYPE_LIVE = 1,
            ENGINE_TYPE_RPB = 2,
        };

        public enum CHANNEL_TYPE_e
        {
            CHANNEL_DEFAULT_VALUE = -1,
            CONTROL_CHANNEL_LIVE_ASYNC = 0,
            CONTROL_CHANNEL_BLOCK_SYNC = 1,
            DATA_CHANNEL_LIVE_STRM = 2,
            DATA_CHANNEL_RPB_STRM = 3,
            EVENT_CHANNEL_LIVE = 4,
            CONTROL_CHANNEL_RPB_CMDS = 5,
        }

        /* Main-Command Id */
        /* NOTE : Must sync wih MxCmdBuilder cmdMainArray[] */
        public enum CMD_MAIN_ID_e
        {
            CMD_MAIN_REQ_LOG,
            CMD_MAIN_REQ_CHNL,
            CMD_MAIN_SET_CMD,
            CMD_MAIN_REQ_CON,
            CMD_MAIN_SET_CMD_SMART_CODE,
        }

        public enum CMD_SUB_ID_e
        {
            CMD_SUB_GET_CNFG = 1,
            CMD_SUB_REQ_POL = 7,
            CMD_SUB_REG_FOR_EVT = 8,
            CMD_SUB_REC_EVT = 9,
            CMD_SUB_ES_HEALTH = 20,
            CMD_SUB_SET_VIEW = 18,
            CMD_SUB_SRT_LV_STRM = 55,
            CMD_SUB_SRT_MULT_LV_STRM = 51,
            CMD_SUB_STP_LV_STRM = 56,
            CMD_SUB_LOGOUT = 10,
            CMD_SUB_PLYBCK_SRCH_MNTH = 80,
            CMD_SUB_PLYBCK_SRCH_DAY = 81,
            CMD_SUB_INC_LV_AUD = 60,
            CMD_SUB_EXC_LV_AUD = 61,
            CMD_SUB_SETPANTILT = 64,
            CMD_SUB_SETZOOM = 65,
            CMD_SUB_SETFOCUS = 66,
            CMD_SUB_SETIRIS = 67,
            CMD_SUB_PTZLOCKUNLOCK = 75,
            CMD_SUB_PTZHEALTH = 77,
            CMD_SUB_CALLPRESET = 68,
            CMD_SUB_CNG_LV_STRM = 59,
            CMD_SUB_GET_PLY_SESSION_ID = 82,
            CMD_SUB_PLY_RCD_STRM = 71,
            CMD_SUB_PAUSE_RCD_STRM = 72,
            CMD_SUB_RESUME_RCD_STRM = 73,
            CMD_SUB_STOP_RCD_STRM = 75,
            CMD_SUB_CLR_PLY_STRM_ID = 76,
            CMD_SUB_GET_PHONE_NO = 12,
            CMD_SUB_SEND_AUDIO_ID = 108,
            CMD_SUB_STOP_AUDIO_ID = 132,
            CMD_SUB_USER_EVENT = 3, // MANUAL TRIGGER
            CMD_SUB_USER_DETAILS = 226, // Get User details...
            CMD_SET_USER_PWD = 109,
            CMD_SUB_SEND_OTP = 231,
            CMD_SUB_VERIFY_OTP = 232,
            CMD_SUB_GET_CAM_STAUTS = 87,
            CMD_SUB_GET_ONE_FRAME = 83,
            CMD_SUB_SRT_MAN_REC = 62,
            CMD_SUB_STP_MAN_REC = 63,

            // ES Servers' Health Commands...
            CMD_SUB_RS_HEALTH = 21,
            CMD_SUB_FOS_HEALTH = 216,
            CMD_SUB_IVA_HEALTH = 138,
            CMD_SUB_TRANSCODING_HEALTH = 220,
            CMD_SUB_ONVIF_HEALTH = 208,
        }

        public enum ePlaybackSpeed
        {
            PLAYBACK_SPEED_1X = 0,
            PLAYBACK_SPEED_2X = 1,
            PLAYBACK_SPEED_4X = 2,
            PLAYBACK_SPEED_8X = 3,
            PLAYBACK_SPEED_16X = 4,
        }

        /* CMD VALUE RELATED CONSTANTS */
        public static byte CST_FEATURE_CNFG_CMD_VALUE = 1;
        public static byte CST_RS_CNFG_CMD_VALUE = 2;
        public static byte CST_DEVICE_CNFG_CMD_VALUE = 3;
        public static byte CST_CAMERA_CNFG_CMD_VALUE = 4;
        public static byte CST_VIEWS_CNFG_CMD_VALUE = 10;
        public static byte CST_COMPANY_LOGO_CNFG_CMD_VALUE = 31;
        public static byte CST_MY_VIEW_ADD_CMD_VALUE = 1;
        public static byte CST_MY_VIEW_EDIT_CMD_VALUE = 2;
        public static byte CST_MY_VIEW_DELETE_CMD_VALUE = 3;
        public static byte CST_MANUAL_TRIGGER_CMD_VALUE = 12;
        public static byte CST_MS_USER_LIST_CMD_VALUE = 32;
        public static byte CST_FOS_CNFG_CMD_VALUE = 19;

        /* POLL TIME OUT */
        public static byte CST_HEALTH_POLL_TIME_VALUE = 30;
        public static byte CST_AUTO_SYNC_GET_CNFG_TIME_VALUE = 1;

        /* Queue Status */
        public static byte QUEUE_STATE_NORMAL = 0;
        public static byte QUEUE_STATE_RELEASE = 1;
        public static byte QUEUE_STATE_RESET = 2;

        /* SURFACE REGISTER OPERATION CONSTANTS */
        public static byte SURFACE_REGISTER_FOR_VIDEO = 1;
        public static byte SURFACE_REGISTER_FOR_AUDIO = 2;


        /** Base delay time for playback */
        public static int RPB_ForwardDir_BaseTime = 1000;  // 1 SEC
        public static int RPB_ReversDir_BaseTime = 4000;     // 4 SEC

        public enum CMD_STATUS_e
        {
            CMD_STATUS_NONE = 0,        /* cmd not send yet */
            CMD_STATUS_SENT_SUCCESS = 1,        /* cmd sent successfully */
            CMD_STATUS_SENT_FAIL = 2,       /* cmd sent fail */
            CMD_STATUS_RECV_SUCCESS = 3,        /* cmd recv successfully.this status not useful now bcoz on success we deleting node */
            CMD_STATUS_RECV_FAIL = 4,       /* cmd recv fail */
        }

        public enum eAudioDecoderType
        {
            AUDIO_DECODER_G711_ULAW = 1,
            AUDIO_DECODER_G726_8 = 2,
            AUDIO_DECODER_G726_16 = 3,
            AUDIO_DECODER_G726_24 = 4,
            AUDIO_DECODER_G726_32 = 5,
            AUDIO_DECODER_G726_40 = 6,
            AUDIO_DECODER_AAC = 7,
            AUDIO_DECODER_RAW_PCM_LE = 8,
            AUDIO_DECODER_RAW_PCM_BE = 9,
            AUDIO_DECODER_G711_ALAW = 10,
        }

        public static byte TRUE = 1;
        public static byte FALSE = 0;
        public static byte RESET = 2;

        public enum enumLivePlaybackStatus
        {
            enumLivePlaybackStatus_Normal = 0,
            enumLivePlaybackStatus_FileIOErr = 1,
            enumLivePlaybackStatus_StartOfSyncPlayback = 2,
            enumLivePlaybackStatus_HDDFormatStart = 5,
            enumLivePlaybackStatus_ConfigChange = 6,
            enumLivePlaybackStatus_PlaybackOverLiveStrmStop = 7,
            enumLivePlaybackStatus_SyncLastFrame = 8,
            enumLivePlaybackStatus_NoFrameNow = 9,
        }

        public enum eOperationalStatus
        {
            START = 0,
            STARTING = 1,
            STARTED = 2,
            STOP = 3,
            STOPPING = 5,
            STOPPED = 6
        }


        public enum MxPlaybackType_e : int
        {
            PLAYBACK_TYPE_NORMAL = 0,
            PLAYBACK_TYPE_DEVICE = 1,
            PLAYBACK_TYPE_MERGED = 2,
            PLAYBACK_TYPE_CAMERA = 3,
            PLAYBACK_TYPE_SUMMARY = 4,
            PLAYBACK_TYPE_FROM_ALL = 5 // RS+ DEVICE+ BACK-UP
        }


        /** Get configuration commandid. */
        public static class GetConfigCmdId
        {
            public const int GET_CONFIG_FEATURE_LIST = 0;
            public const int GET_CONFIG_RS = 1;
            public const int GET_CONFIG_DEV = 2;
            public const int GET_CONFIG_CAM = 3;
            public const int GET_CONFIG_MY_VIEWS = 4;

        }

        /** Get userright commandid. */
        public static class UserRightId
        {

            public const int LIVE_MONITORING = 0;
            public const int CAN_VIEW_MY_VIEW = 1;
            public const int CAN_ADD_DELETE_MY_VIEW = 1;
            public const int CAN_EDIT_MY_VIEW = 1;
            public const int PTZ_OPERATIONS = 2;
            public const int SNAPSHOT = 3;
            public const int PLAYBACK = 9;

            //Not available in SAD.
            public const int MANUAL_RECORDING = 4;
            public const int MANUAL_TRIGGER = 5;
            public const int EVENT_MONITORING = 6;
            public const int EMAP_MONITORING = 7;
            public const int ALARM_MONITORING = 8;
            public const int INVESTIGATOR = 10;
            public const int EXPORT_OPTIONS = 11;
            public const int SYSTEM_MONITORING = 12;
            public const int COSEC = 13;
            public const int MICROPHONE = 22;
        }


        /** Get userright commandid. */
        public static class NewUserRightId
        {

            public const int LIVE_MONITORING = 0;
            public const int CAN_VIEW_MY_VIEW = 23;
            public const int CAN_ADD_DELETE_MY_VIEW = 24;
            public const int CAN_EDIT_MY_VIEW = 25;
            public const int PTZ_OPERATIONS = 2;
            public const int SNAPSHOT = 3;
            public const int PLAYBACK = 9;

            //Not available in SAD.
            public const int MANUAL_RECORDING = 4;
            public const int MANUAL_TRIGGER = 5;
            public const int EVENT_MONITORING = 6;
            public const int EMAP_MONITORING = 7;
            public const int ALARM_MONITORING = 8;
            public const int INVESTIGATOR = 10;
            public const int EXPORT_OPTIONS = 11;
            public const int SYSTEM_MONITORING = 12;
            public const int COSEC = 13;
            public const int MICROPHONE = 22;
        }

    }
}