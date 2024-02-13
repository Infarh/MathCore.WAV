using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathCore.WAV.Infrastructure;

public class WriteOnlyStreamWrapper(Stream DataStream) : Stream
{
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush() => DataStream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => DataStream.Write(buffer, offset, count);

    public override Task FlushAsync(CancellationToken Cancel) => DataStream.FlushAsync(Cancel);

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken Cancel) => throw new NotSupportedException();

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken Cancel) => throw new NotSupportedException();

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken Cancel) => DataStream.WriteAsync(buffer, offset, count, Cancel);
}
