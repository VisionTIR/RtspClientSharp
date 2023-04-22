using Detectors.Libraries.RtspClientSharp.RtspClientSharp.RawFrames;
using RtspClientSharp.MediaParsers;
using RtspClientSharp.Sdp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detectors.Libraries.RtspClientSharp.RtspClientSharp.MediaParsers
{
    internal class PlainTextMetadataInfoParser : MediaPayloadParser
    {
        public static byte[] GetBer(byte[] data)
        {
            int barLenghtMask = 0x01111111;

            int a = 1 & barLenghtMask;

            int barLenghtCodified = data[ULKEY.Length];

            //int barlenght = barLenghtCodified & (int)barLenghtMask;
            int barlenght = 130 - 128;

            byte[] result = new byte[barlenght + 1];

            Array.Copy(data, ULKEY.Length, result, 0, result.Length);

            return result;
        }

        public static byte[] GetData(byte[] all)
        {
            byte[] ber = GetBer(all);

            Int64 lenght = ValueLenghtFromBer(ber);

            byte[] result = new byte[lenght];

            Array.Copy(all, ULKEY.Length + ber.Length, result, 0, lenght);

            return result;
        }

        public override void Parse(TimeSpan timeOffset, ArraySegment<byte> byteSegment, bool markerBit)
        {
            //byte[] all = new byte[byteSegment.Array.Length - byteSegment.Offset];
            //Array.Copy(byteSegment.Array, byteSegment.Offset, all, 0, all.Length);
            //byte[] result = GetData(all);

            //int finalIndex = byteSegment.Offset + ULKEY.Length +

            //byte[] all = new byte[byteSegment.Count];
            //Array.Copy(byteSegment.Array, byteSegment.Offset, all, 0, all.Length);

            //byte[] result = GetData(all);

            //DateTime timestamp = GetFrameTimestamp(timeOffset);

            //OnFrameGenerated(new MetadatoFrame(timestamp, new ArraySegment<byte>(result, 0, result.Length)));

            int barLenght = GetBarLenght(byteSegment);
            int finalIndex = byteSegment.Offset + ULKEY.Length + barLenght;

            int valueLenght = (int)GetValueLenght(new ArraySegment<byte>(byteSegment.Array, byteSegment.Offset + ULKEY.Length, barLenght));
            int finalLenght = valueLenght; // byteSegment.Array.Length - finalIndex;

            DateTime timestamp = GetFrameTimestamp(timeOffset);

            OnFrameGenerated(new PlainTextMetadataFrame(timestamp, new ArraySegment<byte>(byteSegment.Array, finalIndex, finalLenght)));
        }

        public override void ResetState()
        {
        }

        private static readonly byte[] ULKEY =
        {
            0x06, 0x0e, 0x2b, 0x34, 0x02, 0x0b, 0x01, 0x01,
            0x0e, 0x01, 0x03, 0x01, 0x01, 0x00, 0x00, 0x00,
        };

        private static int GetBarLenght(ArraySegment<byte> allKlv)
        {
            int barLenghtCodified = allKlv.Array[allKlv.Offset + ULKEY.Length];
            return barLenghtCodified - 128 + 1;
        }

        private static Int64 GetValueLenght(ArraySegment<byte> ber)
        {
            byte[] lenght = new byte[ber.Count - 1];

            Array.Copy(ber.Array, ber.Offset + 1, lenght, 0, lenght.Length);

            try
            {
                //byte[] result = new byte[4];
                //for (int i = 0; i < result.Length; i++)
                //{
                //    int inverIndex = result.Length - 1 - i;
                //    if (inverIndex < lenght.Length)
                //        result[i] = lenght[inverIndex];
                //    else result[i] = 0;
                //}

                byte[] result = new byte[4];
                for (int i = 0; i < lenght.Length; i++)
                {
                    result[i] = lenght[lenght.Length - 1 - i];
                }

                return BitConverter.ToInt32(result, 0);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static Int64 ValueLenghtFromBer(byte[] ber)
        {
            try
            {
                if (ber.Length < 2) return ber[0];
                return BitConverter.ToInt64(ber, 1);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
