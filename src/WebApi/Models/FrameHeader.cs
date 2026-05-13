using WebApi.Utils;

namespace WebApi.Models
{
    public class FrameHeader
    {
        public uint magicCode { get; set; }
        public byte headerVersion { get; set; }
        public byte productType { get; set; }
        public uint mediaFrmLen { get; set; }
        public uint timeStampSec { get; set; }
        public ushort timeStampMsec { get; set; }
        public byte streamType { get; set; }
        public byte codecType { get; set; }
        public byte fps { get; set; }
        public byte frmType { get; set; }
        public byte vidResolution { get; set; }
        public byte vidFormat { get; set; }
        public byte scanType { get; set; }
        public byte playBackStatus { get; set; }
        public byte vidLoss { get; set; }
        public ushort audSampleFrq { get; set; }
        public byte playbackSynNum { get; set; }
        public byte[] reserveByte { get; set; }
        public uint preReserveMediaLen { get; set; }
        public ushort cameraSeqNo { get; set; }

        public byte[] rawHeader { get; set; }

        public static ushort DVR_MAGIC_CODE { get; set; } = 511;

        /* MediaQueue Related Constants */
        public static byte FSH_LEN { get; set; } = 40;


        public FrameHeader()
        {
            int itr;

            magicCode = 0;
            headerVersion = 0;
            productType = 0;
            mediaFrmLen = 0;
            timeStampSec = 0;
            timeStampMsec = 0;
            streamType = 0;
            codecType = 0;
            fps = 0;
            frmType = 0;
            vidResolution = 0;
            vidFormat = 0;
            scanType = 0;
            playBackStatus = 0;
            vidLoss = 2;
            audSampleFrq = 0;
            playbackSynNum = 0;


            reserveByte = new byte[7];

            for (itr = 0; itr < 7; itr++)
            {
                reserveByte[itr] = 0;
            }

            preReserveMediaLen = 0;
        }

        public FrameHeader(FrameHeader obj)
        {
            CopyStruct(obj);
        }

        void CopyStruct(FrameHeader objFrameHeader)
        {
            int itr;

            magicCode = objFrameHeader.magicCode;
            headerVersion = objFrameHeader.headerVersion;
            productType = objFrameHeader.productType;
            mediaFrmLen = objFrameHeader.mediaFrmLen;
            timeStampSec = objFrameHeader.timeStampSec;
            timeStampMsec = objFrameHeader.timeStampMsec;
            streamType = objFrameHeader.streamType;
            codecType = objFrameHeader.codecType;
            fps = objFrameHeader.fps;
            frmType = objFrameHeader.frmType;
            vidResolution = objFrameHeader.vidResolution;
            vidFormat = objFrameHeader.vidFormat;
            scanType = objFrameHeader.scanType;
            playBackStatus = objFrameHeader.playBackStatus;
            vidLoss = objFrameHeader.vidLoss;
            audSampleFrq = objFrameHeader.audSampleFrq;
            playbackSynNum = objFrameHeader.playbackSynNum;
            cameraSeqNo = objFrameHeader.cameraSeqNo;

            for (itr = 0; itr < 7; itr++)
            {
                reserveByte[itr] = objFrameHeader.reserveByte[itr];
            }

            preReserveMediaLen = objFrameHeader.preReserveMediaLen;
        }

        public void reset()
        {
            int itr;

            magicCode = 0;
            headerVersion = 0;
            productType = 0;
            mediaFrmLen = 0;
            timeStampSec = 0;
            timeStampMsec = 0;
            streamType = 0;
            codecType = 0;
            fps = 0;
            frmType = 0;
            vidResolution = 0;
            vidFormat = 0;
            scanType = 0;
            playBackStatus = 0;
            vidLoss = 2;
            audSampleFrq = 0;
            playbackSynNum = 0;

            for (itr = 0; itr < 7; itr++)
            {
                reserveByte[itr] = 0;
            }

            preReserveMediaLen = 0;
        }

        /* FRAME_HEADER_t& operator = (const FRAME_HEADER_t& objFRAME_HEADER_t)
        {
          CopyStruct(objFRAME_HEADER_t);
         return *this;
      }  */


        public bool DecodeFrameHeader(byte[] headerData)
        {
            bool result = false;

            rawHeader = new byte[headerData.Length];

            Array.Copy(headerData, rawHeader, headerData.Length);

            //first get start code  (0-3)              4 byte
            int iIndex = 4;
            var byteArrayTemp = new byte[iIndex];
            Array.Copy(headerData, 0, byteArrayTemp, 0, iIndex);
            magicCode = BitConverter.ToUInt32(byteArrayTemp, 0);

            if (magicCode != DVR_MAGIC_CODE)
                return false;

            //Device version                1 byte  (4)
            headerVersion = headerData[iIndex++];

            //Product type               1 byte (5)
            productType = headerData[iIndex++]; //0-DVR,1-NVR

            //Media frame length                4 byte  (6-9)
            byteArrayTemp = [headerData[iIndex++], headerData[iIndex++], headerData[iIndex++], headerData[iIndex++]];
            mediaFrmLen = BitConverter.ToUInt32(byteArrayTemp, 0); //Includes Header Length (i.e. 40 bytes)
            if (mediaFrmLen < FSH_LEN)
                return false;

            //Date time total                6 byte  (10-13)

            //First 4 bytes will be total seconds
            byteArrayTemp = [headerData[iIndex++], headerData[iIndex++], headerData[iIndex++], headerData[iIndex++]];

            uint iTotalSeconds = BitConverter.ToUInt32(byteArrayTemp, 0);

            //  cGlobalException.WriteLog("Total Seconds : "+iTotalSeconds);

            timeStampSec = iTotalSeconds;

            //frameTimeStamp = new DateTime( 1970 , 1 , 1 , 0 , 0 , 0 );

            //frameTimeStamp = frameTimeStamp.AddSeconds( iTotalSeconds );

            //Second 2 bytes miliseconds  (14,15)
            byteArrayTemp = [headerData[iIndex++], headerData[iIndex++]];
            ushort iTotalMiliSeconds = BitConverter.ToUInt16(byteArrayTemp, 0);
            //frameTimeStamp = frameTimeStamp.AddMilliseconds( iTotalMiliSeconds );
            timeStampMsec = iTotalMiliSeconds;

            iIndex++;

            //Camera sequence no received at 32nd and 33rd index in which 2048 camera support added
            byteArrayTemp = [(byte)headerData[32], (byte)headerData[33]];
            ushort cameraSequenceNo = BitConverter.ToUInt16(byteArrayTemp, 0);
            cameraSeqNo = cameraSequenceNo;

            //camera sequence no not received at 32 and 33 index means it does not support 2048 camera and need to take cameraSeqNo from 16th index
            if (cameraSeqNo == 0)
            {
                cameraSeqNo = headerData[16];
            }

            //Stream Type               1 byte (17)
            streamType = headerData[iIndex++]; //0:Video Frame 1: Audio Frame

            //Codec Type               1 byte  (18)
            //Media Type : Video 0:JPEG, 1:H.264, 2:MPEG
            // Media Type : Audio 
            // 0:G711,1:G726-8,2:G726-16,3:G726-24,4:G726-32,5:G726-40,6:AAC,7:RAW_PCM
            codecType = headerData[iIndex++];

            //    if(_streamType == (byte)MediaStreamType.Video && _codecType == (byte)VideoCodec.H264 

            //Fps               1 byte  (19)
            fps = headerData[iIndex++];



            //Frame type             (20)  1 byte

            frmType = headerData[iIndex++]; //0:I-Frame ,1:P-Frame, 2:B-Frame


            //Video Resolution           (21)   1 byte
            //_videoResolution = MediaFrameHeader[iIndex++];//0:QCIF, 1:CIF, 2: 2CIF,3:HD1,4:4CIF, 5:D1            
            vidResolution = headerData[iIndex++];//0:QCIF, 1:CIF, 2: 2CIF,3:HD1,4:4CIF, 5:D1     

            //Video Format             (22) 1 byte
            vidFormat = headerData[iIndex++]; //0:NTSC, 1:PAL

            //Scan Type              1 byte
            scanType = headerData[iIndex++]; //1:Interlace Scan 0: Deinterlace Scan

            //Playback Status              1 byte
            playBackStatus = headerData[iIndex++]; //0: Normal, 1:File I/O Error 6: Configuration Change 7:Playback over

            //Video loss              1 byte
            vidLoss = headerData[iIndex++]; //0: No Video Loss, 1: Video Loss

            //if VideoLoss is true then consider FrameType as Iframe
            //              if ( videoLoss == ( byte )VideoLossStatus.VideoLoss )
            //                  frameType = ( byte )headerData.IFrame;

            //Second 2 bytes sample audio freq
            byteArrayTemp = [headerData[iIndex++], headerData[iIndex++]];
            ushort shtAudioSampleFreq = BitConverter.ToUInt16(byteArrayTemp, 0);
            audSampleFrq = shtAudioSampleFreq;  //Audio Frequency in Hz


            playbackSynNum = headerData[iIndex++]; //Playback Sync Number - 1 Byte - 0 To 127 [index - 28]

            //7 reservered bytes
            iIndex += 7;

            //previous frame length
            byteArrayTemp = [headerData[iIndex++], headerData[iIndex++], headerData[iIndex++], headerData[iIndex++]];

            preReserveMediaLen = BitConverter.ToUInt32(byteArrayTemp, 0);


            result = true;

            return result;
        }


        public bool DecodeFrameHeader_V(byte[] headerData)
        {
            bool result = false;


            rawHeader = new byte[headerData.Length];

            System.Array.Copy(headerData, rawHeader, headerData.Length);

            //first get start code  (0-3)              4 byte
            int iIndex = 4;
            var byteArrayTemp = new byte[iIndex];
            Array.Copy(headerData, 0, byteArrayTemp, 0, iIndex);
            magicCode = BitConverter.ToUInt32(byteArrayTemp, 0);

            if (magicCode != Constants.DVR_MAGIC_CODE)
                return false;

            //Device version                1 byte  (4)
            headerVersion = headerData[iIndex++];

            //Product type               1 byte (5)
            productType = headerData[iIndex++]; //0-DVR,1-NVR

            //Media frame length                4 byte  (6-9)
            byteArrayTemp = [headerData[iIndex++], headerData[iIndex++], headerData[iIndex++], headerData[iIndex++]];
            mediaFrmLen = BitConverter.ToUInt32(byteArrayTemp, 0); //Includes Header Length (i.e. 40 bytes)
            if (mediaFrmLen < Constants.FSH_LEN)
                return false;

            //Date time total                6 byte  (10-13)

            //First 4 bytes will be total seconds
            byteArrayTemp = [headerData[iIndex++], headerData[iIndex++], headerData[iIndex++], headerData[iIndex++]];

            uint iTotalSeconds = BitConverter.ToUInt32(byteArrayTemp, 0);

            //  cGlobalException.WriteLog("Total Seconds : "+iTotalSeconds);

            timeStampSec = iTotalSeconds;

            //frameTimeStamp = new DateTime( 1970 , 1 , 1 , 0 , 0 , 0 );

            //frameTimeStamp = frameTimeStamp.AddSeconds( iTotalSeconds );

            //Second 2 bytes miliseconds  (14,15)
            byteArrayTemp = [headerData[iIndex++], headerData[iIndex++]];
            ushort iTotalMiliSeconds = BitConverter.ToUInt16(byteArrayTemp, 0);
            //frameTimeStamp = frameTimeStamp.AddMilliseconds( iTotalMiliSeconds );
            timeStampMsec = iTotalMiliSeconds;

            iIndex++;

            //Camera sequence no received at 32nd and 33rd index in which 2048 camera support added
            byteArrayTemp = [(byte)headerData[32], (byte)headerData[33]];
            ushort cameraSequenceNo = BitConverter.ToUInt16(byteArrayTemp, 0);
            cameraSeqNo = cameraSequenceNo;

            //camera sequence no not received at 32 and 33 index means it does not support 2048 camera and need to take cameraSeqNo from 16th index
            if (cameraSeqNo == 0)
            {
                cameraSeqNo = headerData[16];
            }

            //Stream Type               1 byte (17)
            streamType = headerData[iIndex++]; //0:Video Frame 1: Audio Frame

            //Codec Type               1 byte  (18)
            //Media Type : Video 0:JPEG, 1:H.264, 2:MPEG
            // Media Type : Audio 
            // 0:G711,1:G726-8,2:G726-16,3:G726-24,4:G726-32,5:G726-40,6:AAC,7:RAW_PCM
            codecType = headerData[iIndex++];

            //    if(_streamType == (byte)MediaStreamType.Video && _codecType == (byte)VideoCodec.H264 

            //Fps               1 byte  (19)
            fps = headerData[iIndex++];



            //Frame type             (20)  1 byte

            frmType = headerData[iIndex++]; //0:I-Frame ,1:P-Frame, 2:B-Frame


            //Video Resolution           (21)   1 byte
            //_videoResolution = MediaFrameHeader[iIndex++];//0:QCIF, 1:CIF, 2: 2CIF,3:HD1,4:4CIF, 5:D1            
            vidResolution = headerData[iIndex++];//0:QCIF, 1:CIF, 2: 2CIF,3:HD1,4:4CIF, 5:D1     

            //Video Format             (22) 1 byte
            vidFormat = headerData[iIndex++]; //0:NTSC, 1:PAL

            //Scan Type              1 byte
            scanType = headerData[iIndex++]; //1:Interlace Scan 0: Deinterlace Scan

            //Playback Status              1 byte
            playBackStatus = headerData[iIndex++]; //0: Normal, 1:File I/O Error 6: Configuration Change 7:Playback over

            //Video loss              1 byte
            vidLoss = headerData[iIndex++]; //0: No Video Loss, 1: Video Loss

            //if VideoLoss is true then consider FrameType as Iframe
            //              if ( videoLoss == ( byte )VideoLossStatus.VideoLoss )
            //                  frameType = ( byte )headerData.IFrame;

            //Second 2 bytes sample audio freq
            byteArrayTemp = [headerData[iIndex++], headerData[iIndex++]];
            ushort shtAudioSampleFreq = BitConverter.ToUInt16(byteArrayTemp, 0);
            audSampleFrq = shtAudioSampleFreq;  //Audio Frequency in Hz


            playbackSynNum = headerData[iIndex++]; //Playback Sync Number - 1 Byte - 0 To 127

            //7 reservered bytes
            iIndex += 7;

            //previous frame length
            byteArrayTemp = [headerData[iIndex++], headerData[iIndex++], headerData[iIndex++], headerData[iIndex++]];

            preReserveMediaLen = BitConverter.ToUInt32(byteArrayTemp, 0);


            result = true;


            return result;
        }

    }

}