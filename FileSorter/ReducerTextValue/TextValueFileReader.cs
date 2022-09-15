using System.Text;

namespace FileSorter.ReducerTextValue;

public class TextValueFileReader : IAsyncDisposable
{
    public record struct TextValueStruct(char[] Text, Stack<long> Values);

    private readonly FileStream _fileStream;
    private readonly BinaryReader _binaryReader;

    public TextValueFileReader(int num)
    {
        var file = new System.IO.FileInfo(num + ".bin");
        _fileStream = file.OpenRead();
        _binaryReader = new BinaryReader(_fileStream, Encoding.UTF8, false);
        PopulateNextItem();
    }

    public TextValueStruct Current { get; private set; }

    public bool IsEmpty { get; private set; }

    public void PopulateNextItem()
    {
        if (_fileStream.Position == _fileStream.Length)
        {
            IsEmpty = true;
            return;
        }

        var stringSize = _binaryReader.ReadInt32();
        var text = _binaryReader.ReadChars(stringSize);
        Current = new TextValueStruct(text, new Stack<long>());

        var longSize = _binaryReader.ReadInt32();
        for (int i = 0; i < longSize; i++)
        {
            Current.Values.Push(_binaryReader.ReadInt64());
        }
    }

    public ValueTask DisposeAsync()
    {
        _binaryReader.Dispose();
        return _fileStream.DisposeAsync();
    }
}