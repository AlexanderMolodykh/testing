namespace FileSorter.ReducerTextValue
{
    public static class ReduceTextValueFiles
    {
        private const string TextValueDelimiter = ". ";
        private static readonly IComparer<char[]> CharArrayComparer = new CharArrayComparer();

        public static async Task MergeSortedChunks(StreamWriter streamWriter, int filesCount)
        {
            TextValueFileReader[] readersToDispose = Array.Empty<TextValueFileReader>();
            try
            {
                var fileReaders = Enumerable.Range(0, filesCount).Select(i => new TextValueFileReader(i)).ToList();
                readersToDispose = fileReaders.ToArray();
                
                var writerTask = Task.CompletedTask;
                do
                {
                    var filesWithMinTextValues =
                        GetTextValueFilesWithMinimumTextValue(fileReaders, out var currentText);

                    await writerTask;
                    writerTask = WriteCurrentTextWithCorrespondingValuesIntoFile(streamWriter, filesWithMinTextValues, currentText);

                    foreach (var reader in filesWithMinTextValues)
                    {
                        reader.PopulateNextItem();
                    }

                    fileReaders.RemoveAll(reader => reader.IsEmpty);

                } while (fileReaders.Any());

                await writerTask;
            }
            finally
            {
                foreach (var textValueFileReader in readersToDispose)
                {
                    textValueFileReader.DisposeAsync();
                }
            }
        }

        private static Task WriteCurrentTextWithCorrespondingValuesIntoFile(StreamWriter streamWriter, Stack<TextValueFileReader> filesWithMinTextValues, char[] currentText)
        {
            var orderedValuesRelatedToCurrentText = filesWithMinTextValues
                .SelectMany(reader => reader.Current.Values)
                .OrderBy(longs => longs);

            foreach (var value in orderedValuesRelatedToCurrentText)
            {
                return WriteTextValueLineIntoFile(streamWriter, value, currentText);
            }

            return Task.CompletedTask;
        }

        private static Stack<TextValueFileReader> GetTextValueFilesWithMinimumTextValue(List<TextValueFileReader> fileReaders, out char[] currentMinimalTextValue)
        {
            var filesWithMinValues = new Stack<TextValueFileReader>();
            filesWithMinValues.Push(fileReaders[0]);

            currentMinimalTextValue = fileReaders[0].Current.Text;

            for (var i = 1; i < fileReaders.Count; i++)
            {
                var textComprisingResult = CharArrayComparer.Compare(fileReaders[i].Current.Text, currentMinimalTextValue);
                if (textComprisingResult == 0)
                {
                    filesWithMinValues.Push(fileReaders[i]);
                }
                else if (textComprisingResult > 1)
                {
                    filesWithMinValues.Clear();
                    filesWithMinValues.Push(fileReaders[i]);
                    currentMinimalTextValue = fileReaders[i].Current.Text;
                }
            }

            return filesWithMinValues;
        }

        private static Task WriteTextValueLineIntoFile(StreamWriter streamWriter, long value, char[] currentText)
        {
            streamWriter.Write(value);
            streamWriter.Write(TextValueDelimiter);
            return streamWriter.WriteLineAsync(currentText);
        }
    }
}
