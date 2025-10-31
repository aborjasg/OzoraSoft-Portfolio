using Microsoft.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace OzoraSoft.Library.PictureMaker
{
	/// <summary>
	/// a Json converter forDynamicDictionary
	/// </summary>
	public class DynamicConverter : JsonConverter<DynamicDictionary>
	{
		/// <summary>
		/// Read Json. do not support now
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="typeToConvert"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public override DynamicDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
		/// <summary>
		/// Writ Json for DynamicDictionary. The maximum token size in characters (166 MB) and in base 64 (125 MB). so big array cann't be write out. for details see <see href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/migrate-from-newtonsoft?pivots=dotnet-6-0"/>
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="options"></param>
		public override void Write(Utf8JsonWriter writer, DynamicDictionary value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			try
			{
				if (value != null)
				{
					foreach (KeyValuePair<string, object> kvp in value.DataSet)
					{
						writer.WritePropertyName(kvp.Key);
						if (kvp.Value is Array array)
						{
							if (array != null)
							{
								int[] dims = array.GetDimensions();
								if (kvp.Key.Equals("Cc_data"))
								{//this is a big array, so do not write out, please use ArrayExtensions.ToString2() to get all of array elemment
									writer.WriteStringValue($"{array.GetType().GetElementType()!.Name}{dims.ToString2()}, Sinse this is big array, Json cann't support over 166MB token size, so just show its dimensions. Use Cc_data.ToString2() to get all of them");
								}
								else
								{
									int[] Indices = new int[dims.Length];
									Array.Clear(Indices);
									for (int i = 0; i < array.Length; i += dims[^1])
									{
										//find non zero from Idices
										int nNonZeroIndex = Array.FindLastIndex(Indices, value => value != 0);
										//put "[" for each dimemsion to last third dimemsion
										for (int j = nNonZeroIndex; j < Indices.Length - 1; j++)
										{
											writer.WriteStartArray();
										}
										//for all last dimension's values
										for (int j = 0; j < dims[^1]; j++)
										{
											Indices[^1] = j;
											writer.WriteStringValue(array.GetValue(Indices)!.ToString());
										}
										//clear last dimension's index
										Indices[^1] = 0;
										//put "}" for last dimesion
										writer.WriteEndArray();
										// increment Indices from last second to first
										for (int nIndex = Indices.Length - 2; nIndex >= 0; nIndex--)
										{
											Indices[nIndex]++;
											if (Indices[nIndex] < dims[nIndex])
											{
												break;
											}
											//put "}" last
											writer.WriteEndArray();
											Indices[nIndex] = 0;
										}
									}
								}
							}
							else
							{
								writer.WriteStringValue(string.Empty);
							}
						}
						else
						{
							JsonSerializer.Serialize(writer, kvp.Value, options);
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
			writer.WriteEndObject();
		}
	}

	/// <summary>
	/// a dynamic dictionary
	/// </summary>
	/// <example>
	/// var oData = new DynamicDictionary();
	/// oData.Set("myData1", "this is my data");
	/// var oArrayData = Array.CreateInstance(typeof(object), new int[]{2,3})
	/// var oElement1 = new DynamicDictionary();
	/// oArrayData.SetValue(0,oElement1); //oArrayData[0,0] == oElement1
	/// oStruct.Set("myArrayData", oArrayData);
	/// oElement1.Set("aData", 1234);
	/// //get back the data
	/// string strData = oData.myData1; // strData== "this is my data"
	/// int nData = oData.myArrayData[0,0].aData; // nData == 1234
	/// </example>
	[JsonConverter(typeof(DynamicConverter))]
	public class DynamicDictionary : DynamicObject
	{
		#region inner var
		/// <summary>
		/// The inner dictionary
		/// </summary>
		Dictionary<string, object> _dictionary = new Dictionary<string, object>();
		#endregion

		#region Properties
		/// <summary>
		/// count in this data
		/// </summary>
		public int Count
		{
			get
			{
				return _dictionary.Count;
			}
		}

		/// <summary>
		/// current data in dictrionary
		/// </summary>
		public Dictionary<string, object> DataSet => _dictionary;

		/// <summary>
		/// All Json data inside of this object
		/// </summary>
		public string Json
		{
			get
			{
				//test jagged array
				//object[] testRoot = new object[2];
				//testRoot[0] = new int[] { 1, 2, 3 };
				//testRoot[1] = new int[] { 4, 5, 6 };
				//string testJson = JsonSerializer.Serialize(testRoot);
				// using customize Json converter for multi dimensional array
				//var serializeOptions = new JsonSerializerOptions
				//{
				//	MaxDepth = int.MaxValue,
				//	//WriteIndented = true,
				//	//Converters =
				//	//{
				//	//	new NDArrayConverter()
				//	//}
				//};
				string strRcd = string.Empty;
				try
				{
					strRcd = JsonSerializer.Serialize(_dictionary, new JsonSerializerOptions() {WriteIndented=true });
				}
				catch (Exception e)
				{
					strRcd = e.Message;
				}
				return strRcd;
			}
		}
		#endregion

		#region
		/// <summary>
		/// set data with its key
		/// make first letter be an uppercase to avoid use of C# keyword
		/// </summary>
		/// <param name="strName"></param>
		/// <param name="data"></param>
		public void Set(string strName, object data)
		{
			strName = GetPropertyName(strName);
			if (!string.IsNullOrEmpty(strName))
			{
				_dictionary.TryAdd(strName, data);
			}
		}

		/// <summary>
		/// Get a usfull property name from a orignal name
		/// </summary>
		/// <param name="strOrignalName">orignal name. should not be a reserved keyword of C# like param, int long...</param>
		/// <returns>Property name used in this class</returns>
		public static string GetPropertyName(string strOrignalName)
		{
			if (!string.IsNullOrEmpty(strOrignalName))
			{
				strOrignalName = char.ToUpper(strOrignalName[0]) + strOrignalName.Substring(1);
			}
			return strOrignalName;
		}
		#endregion

		#region Overload Methodes
		/// <summary>
		/// If you try to get a value of a property
		/// not defined in the class, this method is called.
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			// Converting the property name to lowercase
			// so that property names become case-insensitive.
			//string name = binder.Name.ToLower();

			// If the property name is found in a dictionary,
			// set the result parameter to the property value and return true.
			// Otherwise, return false.
			return _dictionary.TryGetValue(binder.Name, out result!);
		}

		/// <summary>
		/// If you try to set a value of a property that is
		/// not defined in the class, this method is called.
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool TrySetMember(SetMemberBinder binder, object? value)
		{
			// Converting the property name to lowercase
			// so that property names become case-insensitive.
			//_dictionary[binder.Name.ToLower()] = value;
			_dictionary[binder.Name] = value!;

			// You can always add a value to a dictionary,
			// so this method always returns true.
			return true;
		}
		#endregion

		#region inner method
		#endregion
	}
}
