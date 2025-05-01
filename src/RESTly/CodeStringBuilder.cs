using System.Text;

namespace Restly;

internal sealed class CodeStringBuilder
{
	private readonly StringBuilder _stringBuilder = new ();
	private uint _indent;
	
	public CodeStringBuilder AppendLine(string s)
	{
		_stringBuilder.Append('\t', (int)_indent);
		_stringBuilder.AppendLine(s);
		return this;
	}

	public CodeStringBuilder PushIndent()
	{
		_indent++;
		return this;
	}
	
	public CodeStringBuilder PopIndent()
	{
		_indent--;
		return this;
	}
}