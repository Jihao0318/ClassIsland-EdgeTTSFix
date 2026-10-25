using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace EdgeTTSFix.Services.EdgeTts;

/// <summary>
/// 生成 Edge「大声朗读」接口所需的 Sec-MS-GEC 令牌。
/// V2 令牌格式为 &lt;16 位 CS64 摘要&gt;-&lt;64 位 SHA256 摘要&gt;。
/// </summary>
public static class EdgeTtsTokenGenerator
{
    public const string TrustedClientToken = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";

    // Microsoft Edge 在计算令牌前，会先把这段版权声明拼在时间戳之前。
    private const string CopyrightNotice = "Copyright (C) Microsoft Corporation. All rights reserved.";

    private const long WindowsEpochSeconds = 11_644_473_600L;
    private const long TokenWindowSeconds = 300L;
    private const long TicksPerSecond = 10_000_000L;

    /// <summary>
    /// 生成 V2 令牌：&lt;16 hex&gt;-&lt;64 hex&gt;。
    /// </summary>
    public static string GenerateV2(DateTimeOffset now)
    {
        var payload = GenerateCopyrightPayload(now);
        var digest = SHA256.HashData(payload);

        var dataWords = ToWordsLe4Even(payload);
        var keyWords = ToWordsLe4Even(digest);

        var (swapLast, swapAcc) = Cs64WordSwap(dataWords, keyWords);
        var (chainState, chainAcc) = Cs64Reversible(dataWords, keyWords);

        Span<byte> cs64 = stackalloc byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(cs64[..4], swapLast ^ chainState);
        BinaryPrimitives.WriteUInt32LittleEndian(cs64[4..], swapAcc ^ chainAcc);

        return string.Concat(Convert.ToHexString(cs64), "-", Convert.ToHexString(digest));
    }

    /// <summary>
    /// 生成旧版 V1 令牌：对「时间戳 + 受信任客户端令牌」取 SHA256。
    /// </summary>
    public static string GenerateV1(DateTimeOffset now)
    {
        var ticks = GetBucketedFileTimeTicks(now);
        var payload = Encoding.ASCII.GetBytes(string.Concat(
            ticks.ToString(CultureInfo.InvariantCulture),
            TrustedClientToken));
        return Convert.ToHexString(SHA256.HashData(payload));
    }

    /// <summary>
    /// 取 Windows FileTime ticks（100ns），并向下取整到最近的 5 分钟窗口。
    /// </summary>
    public static long GetBucketedFileTimeTicks(DateTimeOffset now)
    {
        var fileTimeSeconds = now.ToUnixTimeSeconds() + WindowsEpochSeconds;
        var bucketedSeconds = fileTimeSeconds - (fileTimeSeconds % TokenWindowSeconds);
        return bucketedSeconds * TicksPerSecond;
    }

    private static byte[] GenerateCopyrightPayload(DateTimeOffset now) =>
        Encoding.ASCII.GetBytes(string.Concat(
            CopyrightNotice,
            GetBucketedFileTimeTicks(now).ToString(CultureInfo.InvariantCulture),
            TrustedClientToken));

    private static uint[] ToWordsLe4Even(ReadOnlySpan<byte> data)
    {
        var padded = new byte[(data.Length + 3) & ~3];
        data.CopyTo(padded);

        var wordCount = padded.Length / 4;
        if (wordCount == 0)
        {
            return [];
        }

        if ((wordCount & 1) != 0)
        {
            wordCount++;
        }

        var words = new uint[wordCount];
        for (var i = 0; i < padded.Length / 4; i++)
        {
            words[i] = BinaryPrimitives.ReadUInt32LittleEndian(padded.AsSpan(i * 4, 4));
        }

        return words;
    }

    private static (uint Last, uint Acc) Cs64WordSwap(uint[] words, uint[] keyWords)
    {
        var a = keyWords[0] | 1u;
        var b = keyWords[1] | 1u;
        uint acc = 0;
        uint last = 0;

        for (var i = 0; i < words.Length; i += 2)
        {
            uint x = words[i];
            uint y = words[i + 1];

            var t = unchecked((uint)((ulong)Rotate16(x) * 0xEF0569FBu + (ulong)x * a));
            t = Rotate16(t);
            var q = unchecked(t * 0x76F50F34u);
            q = unchecked(Rotate16(q) * 0xC3EFEA97u + q);
            acc = unchecked(acc + q);

            t = unchecked((uint)((ulong)Rotate16(y) * 0xC31713DBu + (ulong)y * b));
            t = Rotate16(t);
            q = unchecked(t * 0x1881CE3Cu);
            last = unchecked(Rotate16(q) * 0x35BD1EC9u + q);
            acc = unchecked(acc + last);
        }

        return (last, acc);
    }

    private static (uint State, uint Acc) Cs64Reversible(uint[] words, uint[] keyWords)
    {
        var a = keyWords[0] | 1u;
        var b = keyWords[1] | 1u;
        uint state = 0;
        uint acc = 0;

        for (var i = 0; i < words.Length; i += 2)
        {
            uint x = words[i];
            uint y = words[i + 1];

            state = unchecked((uint)(((ulong)state + x) * a));
            state = unchecked(Rotate16(state) * 0xCF98B111u);
            state = unchecked(Rotate16(state) * 0x87085B9Fu);
            state = unchecked(Rotate16(state) * 0x12CEB96Du);
            state = unchecked(Rotate16(state) * 0x257E1D83u);

            var t = unchecked((uint)(((ulong)y + state) * b));
            t = unchecked(Rotate16(t) * 0xA27416F5u);
            t = unchecked(Rotate16(t) * 0xD38396FFu);
            t = Rotate16(t);

            acc = unchecked(acc + state);
            state = unchecked(Rotate16(unchecked(t * 0x7C932B89u)) * 0xBFA49F69u);
            acc = unchecked(acc + state);
        }

        return (state, acc);
    }

    private static uint Rotate16(uint value) => (value << 16) | (value >> 16);
}
