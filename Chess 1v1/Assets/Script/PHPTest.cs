using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;

public class PHPTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		string url = "http://2tn.nl/api/chess/test.php";

		WWWForm data = new WWWForm();

		data.AddField("id", "nike1");
		data.AddField("req", "points");

		WWW www = new WWW(url, data);
		StartCoroutine(WaitForRequest(www));
	}

	IEnumerator WaitForRequest(WWW www)
	{
		yield return www;
		Debug.Log(www.text);
		Dictionary<string, object> dic = DictionaryFromPhpArray(www.text);

		DebugLogDictionary(dic);
	}

	object DatatypeFromString(string datatype) 
	{
		object returnObject;
		switch(datatype) {
		case "boolean":
			returnObject = false;
			break;
		case "integer":
			returnObject = 1;
			break;
		case "double":
			returnObject = 0.1;
			break;
		case "string":
			returnObject = "0";
			break;
		case "Array":
			returnObject = new Dictionary<string, object>();
			break;
		default:
			returnObject = null;
			break;
		}

		return returnObject;
	}

	void DebugLogDictionary(Dictionary<string, object> dic)
	{
		foreach (string key in dic.Keys) {
			Debug.Log(key);
			if(dic[key].GetType() == typeof(Dictionary<string,object>)) {
				DebugLogDictionary((Dictionary<string,object>)dic[key]);
				continue;
			}
			Debug.Log(dic[key]);
		}
	}

	Dictionary<string,object> DictionaryFromPhpArray(string php, int arrayCount = 0)
	{
		Dictionary<string, object> returnDictionary = new Dictionary<string, object>();
		int index = 0;

		while(index < php.Length) {
			object[] key = GetKeyFromPhpArray(php.Substring(index), arrayCount);
			if(key == null) {
				break;
			}
			index += (int)key[1];

			object[] value = GetValueFromPhpArray(php.Substring(index));
			if(value[0].GetType() == typeof(Dictionary<string, object>)) {
				index += (int)value[1];
				returnDictionary.Add((string)key[0], DictionaryFromPhpArray(php.Substring(index), arrayCount+1));
				index += (int)((Dictionary<string,object>)returnDictionary[(string)key[0]])["index"];
				((Dictionary<string,object>)returnDictionary[(string)key[0]]).Remove("index");
			} else {
				index += (int)value[1];
				returnDictionary.Add((string)key[0], value[0]);
			}
		}
		if(arrayCount != 0) {
			returnDictionary.Add("index", index);
		}
		return returnDictionary;
	}

	object[] GetKeyFromPhpArray(string php, int arrayCount) 
	{
		return GetObjectFromPhpArray(php, arrayCount, true);
	}

	object[] GetObjectFromPhpArray(string php, int arrayCount = 0, bool isKey = false) 
	{
		object value = null;

		int start = 0;
		int length = 0;
		
		int index = 0;
		char x = php[0];
		
		while(x != '(') {
			index++;
			if(index >= php.Length) {
				return null;
			}
			x = php[index];
		}
		index++;
		start = index;
		
		while(x != ')') {
			index++;
			x = php[index];
		}
		if(isKey) {
			int arrayCounter = int.Parse(php.Substring(start, index-start));
			if(arrayCounter < arrayCount) {
				return null;
			}
		} else {
			value = DatatypeFromString(php.Substring(start, index-start));
			if(value.GetType() == typeof(Dictionary<string,object>)) {
				index += 1 + (arrayCount+1).ToString().Length;
				return new object[]{value, index};
			}
		}
		
		index++;
		start = index;
		x = php[index];
		
		while(x != '(') {
			index++;
			if(index == php.Length) {
				break;
			}
			x = php[index];
		}
		length = index-start;

		object[] returnObject;
		if(isKey) {
			returnObject = new object[]{php.Substring(start,length), start + length};
		} else {
			value = StringToDatatype(php.Substring(start,length), value);
			returnObject = new object[]{value, start + length};
		}
		return returnObject;
	}

	object[] GetValueFromPhpArray(string php) 
	{
		return GetObjectFromPhpArray(php);
	}

	object StringToDatatype(string str, object value) 
	{
		System.Type type = value.GetType();
		if(type == typeof(System.Boolean)) {
			if(str == "0") {
				return false;
			} else {
				return true;
			}
		} else if(type == typeof(System.Int32)) {
			return int.Parse(str);
		} else if(type == typeof(System.Double)) {
			return double.Parse(str);
		} else if(type == typeof(System.String)) {
			return str;
		}

		return null;
	}
}
