using System.Text;

namespace Restly;

internal sealed class CodeStringBuilder
{
	private readonly StringBuilder _stringBuilder = new ();
	private uint _indent;

	public CodeStringBuilder() { }
	
	public CodeStringBuilder(string s) => _stringBuilder.Append(s);

	public CodeStringBuilder AppendLine()
	{
		_stringBuilder.AppendLine();
		return this;
	}
	
	public CodeStringBuilder AppendLine(string l)
	{
		_stringBuilder.Append('\t', (int)_indent);
		_stringBuilder.AppendLine(l);
		return this;
	}

	public CodeStringBuilder AppendLines(string[] ls)
	{
		foreach (var l in ls)
			AppendLine(l);
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

	public override string ToString() => _stringBuilder.ToString();
}