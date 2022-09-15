namespace FileSorter.MapTextValues
{
    public static class TextValueBufferOperations
    {
        public static IEnumerable<(long, Range)> Split(char[] str, int size)
        {
            bool isNum = true;
            int numStart = 0;
            int numLength = 0;
            int textStart = 0;
            int textLength = 0;

            for (int i = 0; i < size; i++)
            {
                var isDot = str[i] == '.' && str[i + 1] == ' ';
                var isEndOfLine = str[i] == '\r' || str[i] == '\n';
                if (isEndOfLine)
                {
                    if (numLength != 0)
                    {
                        var number = long.Parse(str.AsSpan(numStart, numLength));
                        yield return (number, new Range(textStart, textLength + textStart));
                    }

                    isNum = true;
                    numStart = i + 1;
                    numLength = 0;
                    textStart = 0;
                    textLength = 0;
                }
                else if (isNum && !isDot)
                {
                    numLength++;
                }
                else if (isNum && isDot)
                {
                    isNum = false;
                    i++;
                    textStart = i + 1;
                }
                else if (!isEndOfLine)
                {
                    textLength++;
                }
            }

            if (numLength != 0)
            {
                var number = long.Parse(str.AsSpan(numStart, numLength));
                yield return (number, new Range(textStart, textLength + textStart));
            }
        }
    }
}
