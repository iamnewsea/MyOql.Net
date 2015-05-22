using System;
using System.Text;
using System.Web;
using System.IO;

namespace MyCmn
{
    /// <summary>
    /// 修复 IE 下Post数据乱码的Bug
    /// </summary>
    public partial class RequestFilter : Stream
    {
        private Stream m_sink;
        public RequestFilter(Stream sink)
        {
            m_sink = sink;
        }

        // The following members of Stream must be overriden.
        public override bool CanRead
        { get { return m_sink.CanRead; } }

        public override bool CanSeek
        { get { return m_sink.CanSeek; } }

        public override bool CanWrite
        { get { return m_sink.CanWrite; } }

        public override long Length
        { get { return m_sink.Length; } }

        public override long Position
        {
            get { return m_sink.Position; }
            set { m_sink.Position = value; ; }
        }

        public override long Seek(long offset, SeekOrigin direction)
        {
            return m_sink.Seek(offset, direction);
        }

        public override void Flush()
        {
            m_sink.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset >= m_sink.Length) return 0;
            int begin = this.Position.AsInt() + offset;
            var ret = m_sink.Read(buffer, begin, count);
            if (ret > 0 && HttpContext.Current.Request.Headers["Content-Type"] == "application/x-www-form-urlencoded")
            {
                Encoding encode = HttpContext.Current.Response.ContentEncoding;
                var DecodeUrl = HttpUtility.UrlDecode(buffer, begin, ret, HttpContext.Current.Request.ContentEncoding);

                buffer = encode.GetBytes(DecodeUrl);
                return buffer.Length;
                //var ret = m_sink.Read(buffer, offset, Math.Min(count, m_sink.Length.GetInt()));
            }
            return ret;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
