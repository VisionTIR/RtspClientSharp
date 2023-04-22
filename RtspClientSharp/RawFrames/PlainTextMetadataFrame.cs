using RtspClientSharp.RawFrames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detectors.Libraries.RtspClientSharp.RtspClientSharp.RawFrames
{
    internal class PlainTextMetadataFrame : RawFrame
    {
        public PlainTextMetadataFrame(DateTime timestamp, ArraySegment<byte> frameSegment) : base(timestamp, frameSegment)
        {
        }

        public override FrameType Type => FrameType.PlainText;
    }
}
