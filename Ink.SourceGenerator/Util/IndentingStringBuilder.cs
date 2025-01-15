// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// Adapted from: https://github.com/dotnet/roslyn/pull/71163
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ink.SourceGenerator.Util;

/// <summary>
/// A helper type for generating C# source code efficiently.  Content written is added to a <see cref="StringBuilder"/>,
/// with default behavior provided for controlling the current indent level of the code.
/// </summary>
/// <remarks>
/// Not thread-safe.
/// </remarks>
public sealed class IndentingStringBuilder
{
    private const string DefaultIndentation = "    ";
    private const int DefaultIndentationCount = 8;

    // On net8 we should use SearchValues here.

    /// <summary>
    /// The new line characters accepted by C#.
    /// </summary>
    private static ReadOnlySpan<char> EndOfLineCharacters => ['\r', '\n', '\f', '\u0085', '\u2028', '\u2029'];

    private StringBuilder _builder = new StringBuilder();

    private readonly string _indentationString;

    /// <summary>
    /// The current indentation level.
    /// </summary>
    private int _currentIndentationLevel = 0;

    public IndentingStringBuilder() : this(DefaultIndentation)
    {
    }

    public IndentingStringBuilder(string indentationString)
        => _indentationString = indentationString;

    /// <summary>
    /// Increases the current indentation level, increasing the amount of indentation written at the start of a
    /// new line when content is written to this.
    /// </summary>
    public void IncreaseIndent()
    {
        _currentIndentationLevel++;
    }

    /// <summary>
    /// Decreases the current indentation level, decreasing the amount of indentation written at the start of a
    /// new line when content is written to it.
    /// </summary>
    public void DecreaseIndent()
    {
        if (_currentIndentationLevel == 0)
            throw new InvalidOperationException($"Current indent is already zero.");

        _currentIndentationLevel--;
    }

    /// <summary>
    /// Appends a single end of line sequence to the underlying buffer.  No indentation is written prior to the end of line.
    /// </summary>
    private IndentingStringBuilder AppendEndOfLine()
    {
        this._builder.AppendLine();
        return this;
    }

    /// <summary>
    /// Appends a single line to the underlying buffer.  Indentation is written out if the underlying buffer
    /// is at the start of a line.
    /// </summary>
    private void AppendSingleLine(ReadOnlySpan<char> line, string? originalLine)
    {
        if (line.Length == 0)
            return;

        if (!IsEndOfLineCharacter(line[0]))
        {
            if (_builder.Length == 0 || IsEndOfLineCharacter(_builder[^1]))
            {
                for(int i = 0; i < _currentIndentationLevel; ++i)
                    _builder.Append(_indentationString);
            }
        }

        _builder.Append(line);
    }

    private static bool IsEndOfLineCharacter(char ch)
        => EndOfLineCharacters.IndexOf(ch) >= 0;

    /// <summary>
    /// Writes content to the underlying buffer.  If the buffer is at the start of a line, then indentation will be
    /// appended first before the content.  By default, for performance reasons, the content is assumed to contain no
    /// end of line characters in it.  If the content may contain end of line characters, then <see langword="true"/>
    /// should be passed in for <paramref name="splitContent"/>.  This will cause the provided content to be split into
    /// constituent lines, with each line being appended one at a time.
    /// </summary>
    public IndentingStringBuilder Write(string content, bool splitContent = false)
        => Write(content.AsSpan(), content, splitContent);

    /// <inheritdoc cref="Write(string, bool)"/>
    public IndentingStringBuilder Write(ReadOnlySpan<char> content, bool splitContent = false)
        => Write(content, originalString: null, splitContent);

    /// <inheritdoc cref="Write(string, bool)"/>
    private IndentingStringBuilder Write(ReadOnlySpan<char> content, string? originalString, bool splitContent)
    {
        if (splitContent)
        {
            while (content.Length > 0)
            {
                var endOfLineIndex = content.IndexOfAny(EndOfLineCharacters);
                if (endOfLineIndex < 0)
                {
                    // no new line, append the rest of the content to the buffer.
                    AppendSingleLine(content, originalLine: null);
                    break;
                }
                else
                {
                    while (endOfLineIndex < content.Length && IsEndOfLineCharacter(content[endOfLineIndex]))
                    {
                        ++endOfLineIndex;
                    }

                    AppendSingleLine(content[0..endOfLineIndex], originalLine: null);
                    content = content[endOfLineIndex..];
                }
            }
        }
        else
        {
            AppendSingleLine(content, originalString);
        }

        return this;
    }

    /// <summary>
    /// Equivalent to <see cref="Write(string, bool)"/> except that a final end of line sequence will be written after
    /// the content is written.
    /// </summary>
    public IndentingStringBuilder WriteLine(string content = "", bool splitContent = false)
        => WriteLine(content.AsSpan(), content, splitContent);

    /// <inheritdoc cref="WriteLine(string, bool)"/>
    public IndentingStringBuilder WriteLine(ReadOnlySpan<char> content, bool splitContent = false)
        => WriteLine(content, originalContent: null, splitContent);

    /// <inheritdoc cref="WriteLine(string, bool)"/>
    private IndentingStringBuilder WriteLine(ReadOnlySpan<char> content, string? originalContent, bool splitContent = false)
    {
        Write(content, originalContent, splitContent);
        AppendEndOfLine();
        return this;
    }

    /// <summary>
    /// Ensures that the current buffer has at least one blank line between the last written content and the content
    /// that would be written.  Note: a line containing only whitespace/indentation is not considered an empty line.
    /// Only a line with no content on it counts.
    /// </summary>
    /// <returns></returns>
    public IndentingStringBuilder EnsureEmptyLine()
    {
        if (GetLineCount() < 2)
            AppendEndOfLine();

        return this;
    }

    private int GetLineCount()
    {
        var position = _builder.Length - 1;
        var lineCount = 0;
        while (position >= 0)
        {
            if (_builder[position] == '\n')
            {
                if (position >= 1 && _builder[position - 1] == '\r')
                {
                    position -= 2;
                }
                else
                {
                    position--;
                }

                lineCount++;
            }
            else if (IsEndOfLineCharacter(_builder[position]))
            {
                position--;
                lineCount++;
            }
            else
            {
                break;
            }
        }

        return lineCount;
    }

    /// <summary>
    /// Opens a block of code to write new content into.  Can be used like so:
    /// <code>
    /// using (writer.StartBlock())
    /// {
    ///     write.WriteLine("...");
    ///     write.WriteLine("...");
    /// }
    /// </code>
    /// </summary>
    public Region EnterBlock()
        => EnterIndentedRegion("{", "}");

    public Region EnterIndentedRegion()
        => EnterIndentedRegion("", "");

    private Region EnterIndentedRegion(string open, string close)
    {
        this.WriteLine(open);
        this.IncreaseIndent();
        return new Region(this, close);
    }

    public readonly struct Region(IndentingStringBuilder builder, string close) : IDisposable
    {
        public readonly void Dispose()
        {
            builder.DecreaseIndent();
            builder.WriteLine(close);
        }
    }

#pragma warning disable IDE0060 // Remove unused parameter
    public IndentingStringBuilder Write(bool splitContent, [InterpolatedStringHandlerArgument("", nameof(splitContent))] WriteInterpolatedStringHandler handler)
        => this;

    public IndentingStringBuilder Write([InterpolatedStringHandlerArgument("")] WriteInterpolatedStringHandler handler)
        => this;

    public IndentingStringBuilder WriteLine(bool splitContent, [InterpolatedStringHandlerArgument("", nameof(splitContent))] WriteInterpolatedStringHandler handler)
    {
        Write(splitContent, handler);
        AppendEndOfLine();
        return this;
    }

    public IndentingStringBuilder WriteLine([InterpolatedStringHandlerArgument("")] WriteInterpolatedStringHandler handler)
    {
        AppendEndOfLine();
        return this;
    }
#pragma warning restore IDE0060 // Remove unused parameter

    public override string ToString()
        => _builder.ToString();

    public StringBuilder.ChunkEnumerator GetChunks()
        => _builder.GetChunks();

    /// <summary>
    /// Provides a handler used by the language compiler to append interpolated strings into <see cref="IndentedTextWriter"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [InterpolatedStringHandler]
    public readonly ref struct WriteInterpolatedStringHandler
    {
        private readonly IndentingStringBuilder _builder;
        private readonly bool _splitContent;

        public WriteInterpolatedStringHandler(int literalLength, int formattedCount, IndentingStringBuilder builder, bool splitContent = false)
        {
            // Assume that each formatted section adds at least one character.
            _builder = builder;
            _builder._builder.EnsureCapacity(_builder._builder.Length + literalLength + formattedCount);
            _splitContent = splitContent;
        }

        public void AppendLiteral(string literal)
            => _builder.Write(literal, _splitContent);

        public void AppendFormatted<T>(T value)
        {
            var str = value?.ToString();
            if (str is not null)
                _builder.Write(str, _splitContent);
        }

        public void AppendFormatted<T>(T value, string format) where T : IFormattable
        {
            var str = value?.ToString(format, formatProvider: null);
            if (str is not null)
                _builder.Write(str, _splitContent);
        }
    }
}
