using System.Text;

/// <summary>
/// Хелпер.
/// </summary>
internal class Helper
{
	/// <summary>
	/// Конвертировать массив в строку, разделённую запятой.
	/// </summary>
	/// <param name="columnNames">Массив строк.</param>
	/// <returns>Строку с элементами массива, разделёнными запятой.</returns>
	public string ConvertToCsv(IEnumerable<string> columnNames)
	{
		ArgumentNullException.ThrowIfNull(columnNames);
		
		var sb = new StringBuilder();
		foreach (var columnName in columnNames)
		{
			sb.AppendFormat("\"{0}\",", columnName);
		}
		var items = sb.ToString();
		if (items != string.Empty)
		{
			items = items.Remove(items.Length - 1);
		}
		return items;
	}
}